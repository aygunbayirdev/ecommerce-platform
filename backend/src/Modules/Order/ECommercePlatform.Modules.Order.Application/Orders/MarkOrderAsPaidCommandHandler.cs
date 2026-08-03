using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.Application.Orders;

/// <summary>
/// Stand-in for the Payment module, which doesn't exist yet — this admin-only endpoint lets the
/// order status machine be exercised end to end. Once Payment lands, an Order domain event will
/// drive this transition instead (mirrors the Catalog → Inventory event pattern).
/// </summary>
public sealed class MarkOrderAsPaidCommandHandler(IOrderWriteRepository orderWriteRepository)
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

        return Result.Success();
    }
}
