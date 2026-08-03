using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Dtos;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Order.Application.Orders;

/// <summary>
/// Called by ProcessPaymentCommand (Payment module) on a successful charge, and available as an
/// admin-only endpoint for anything Payment doesn't cover yet.
///
/// Marking the order Paid is also the moment the stock reserved for it during checkout is
/// permanently committed (<c>CommitStockCommand</c>) — it moves from "reserved, might still be
/// released" to "gone for good," matching <c>StockItem.Commit</c>'s semantics in the Inventory
/// module. See CancelMyOrderCommandHandler for the mirror-image case (releasing, not committing).
/// </summary>
public sealed class MarkOrderAsPaidCommandHandler(IOrderWriteRepository orderWriteRepository, ISender sender)
    : ICommandHandler<MarkOrderAsPaidCommand>
{
    public async Task<Result> Handle(MarkOrderAsPaidCommand request, CancellationToken cancellationToken)
    {
        var order = await orderWriteRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(Error.NotFound("Orders.NotFound", "Sipariş bulunamadı."));
        }

        var result = order.MarkAsPaid();

        if (result.IsFailure)
        {
            return result;
        }

        await orderWriteRepository.SaveChangesAsync(cancellationToken);

        var items = order.Items.Select(i => new StockReservationItem(i.ProductVariantId, i.Quantity)).ToList();
        await sender.Send(new CommitStockCommand(items), cancellationToken);

        return Result.Success();
    }
}
