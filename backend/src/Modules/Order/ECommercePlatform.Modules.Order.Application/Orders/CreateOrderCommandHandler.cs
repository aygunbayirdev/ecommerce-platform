using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Carts;
using ECommercePlatform.Modules.Identity.Application.Addresses;
using ECommercePlatform.Modules.Inventory.Application.Dtos;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Promotion.Application.Coupons;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Order.Application.Orders;

/// <summary>
/// Orchestrates checkout across Cart, Identity, Inventory, and (optionally) Promotion via
/// synchronous ISender calls — a user-facing action needs an immediate success/failure answer,
/// unlike Catalog → Inventory's eventual-consistency domain event.
///
/// Cross-module atomicity is handled by sequencing, not a distributed transaction: the Order is
/// created and saved FIRST (the commit point), then stock is reserved, then — if a coupon code
/// was supplied — the coupon is redeemed. Each step that fails compensates every step that
/// already succeeded (release stock, cancel the order) instead of rolling back — so a failure
/// always leaves an auditable record (a cancelled order) rather than an invisible resource leak.
/// </summary>
public sealed class CreateOrderCommandHandler(
    IOrderWriteRepository orderWriteRepository,
    ISender sender) : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var cartIdResult = await sender.Send(new GetOrCreateCartForUserCommand(request.UserId), cancellationToken);

        if (cartIdResult.IsFailure)
        {
            return Result.Failure<Guid>(cartIdResult.Error);
        }

        var cartViewResult = await sender.Send(new GetCartByIdQuery(cartIdResult.Value), cancellationToken);

        if (cartViewResult.IsFailure)
        {
            return Result.Failure<Guid>(cartViewResult.Error);
        }

        var cart = cartViewResult.Value;

        if (cart.Items.Count == 0)
        {
            return Result.Failure<Guid>(Error.Validation("Orders.CartEmpty", "Sepetiniz boş."));
        }

        var addressesResult = await sender.Send(new GetAddressesByUserIdQuery(request.UserId), cancellationToken);

        if (addressesResult.IsFailure)
        {
            return Result.Failure<Guid>(addressesResult.Error);
        }

        var address = addressesResult.Value.FirstOrDefault(a => a.Id == request.AddressId);

        if (address is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Orders.AddressNotFound", "Adres bulunamadı."));
        }

        var items = cart.Items
            .Select(i => new Domain.OrderItemSnapshot(i.ProductVariantId, i.ProductName, i.Sku, i.UnitPrice, i.Quantity))
            .ToList();

        var order = Domain.Order.Create(
            request.UserId,
            address.RecipientName,
            address.PhoneNumber,
            address.City,
            address.District,
            address.FullAddressLine,
            address.PostalCode,
            items);

        orderWriteRepository.Add(order);
        await orderWriteRepository.SaveChangesAsync(cancellationToken);

        var reservationItems = items.Select(i => new StockReservationItem(i.ProductVariantId, i.Quantity)).ToList();
        var reserveResult = await sender.Send(new ReserveStockCommand(reservationItems), cancellationToken);

        if (reserveResult.IsFailure)
        {
            order.Cancel($"Yetersiz stok: {reserveResult.Error.Message}");
            await orderWriteRepository.SaveChangesAsync(cancellationToken);

            return Result.Failure<Guid>(reserveResult.Error);
        }

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
            var redeemResult = await sender.Send(
                new RedeemCouponCommand(request.CouponCode, request.UserId, order.Id, subtotal), cancellationToken);

            if (redeemResult.IsFailure)
            {
                await sender.Send(new ReleaseStockCommand(reservationItems), cancellationToken);

                order.Cancel($"Kupon uygulanamadı: {redeemResult.Error.Message}");
                await orderWriteRepository.SaveChangesAsync(cancellationToken);

                return Result.Failure<Guid>(redeemResult.Error);
            }

            order.ApplyDiscount(request.CouponCode, redeemResult.Value);
        }

        order.MarkReadyForPayment();
        await orderWriteRepository.SaveChangesAsync(cancellationToken);

        await sender.Send(new ClearCartCommand(cartIdResult.Value), cancellationToken);

        return order.Id;
    }
}
