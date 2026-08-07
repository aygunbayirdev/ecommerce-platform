using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Dtos;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Domain;
using ECommercePlatform.Modules.Promotion.Application.Coupons;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Order.Application.Orders;

/// <summary>
/// CancelMyOrderCommandHandler's twin, minus the ownership check — an admin can cancel any
/// customer's order, not just their own. See that handler for why stock is only released when
/// the order was PaymentPending at cancellation time.
/// </summary>
public sealed class AdminCancelOrderCommandHandler(IOrderWriteRepository orderWriteRepository, ISender sender)
    : ICommandHandler<AdminCancelOrderCommand>
{
    public async Task<Result> Handle(AdminCancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderWriteRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(Error.NotFound("Orders.NotFound", "Sipariş bulunamadı."));
        }

        var hadReservedStock = order.Status == OrderStatus.PaymentPending;

        var result = order.Cancel(request.Reason);

        if (result.IsFailure)
        {
            return result;
        }

        await orderWriteRepository.SaveChangesAsync(cancellationToken);

        if (hadReservedStock)
        {
            var items = order.Items.Select(i => new StockReservationItem(i.ProductVariantId, i.Quantity)).ToList();
            await sender.Send(new ReleaseStockCommand(items), cancellationToken);
            await sender.Send(new ReleaseCouponRedemptionCommand(order.Id), cancellationToken);
        }

        return Result.Success();
    }
}
