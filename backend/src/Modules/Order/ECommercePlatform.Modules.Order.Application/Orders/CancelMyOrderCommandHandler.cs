using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Dtos;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Domain;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Order.Application.Orders;

/// <summary>
/// Only releases the stock reservation (<c>ReleaseStockCommand</c>) when the order was
/// PaymentPending at the moment of cancellation — that's the only state where a reservation
/// exists but hasn't been committed yet. An order auto-cancelled for insufficient stock (from
/// Created, in CreateOrderCommandHandler) never had anything reserved in the first place, and an
/// order cancelled after Paid/Preparing already had its stock permanently committed — releasing it
/// here would be wrong in both cases. Cancelling a Paid order (a refund scenario) isn't handled by
/// this MVP; see TASKS.md.
/// </summary>
public sealed class CancelMyOrderCommandHandler(IOrderWriteRepository orderWriteRepository, ISender sender)
    : ICommandHandler<CancelMyOrderCommand>
{
    public async Task<Result> Handle(CancelMyOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderWriteRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null || order.UserId != request.UserId)
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
        }

        return Result.Success();
    }
}
