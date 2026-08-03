using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed class CancelMyOrderCommandHandler(IOrderWriteRepository orderWriteRepository)
    : ICommandHandler<CancelMyOrderCommand>
{
    public async Task<Result> Handle(CancelMyOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderWriteRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null || order.UserId != request.UserId)
        {
            return Result.Failure(Error.NotFound("Orders.NotFound", "Sipariş bulunamadı."));
        }

        var result = order.Cancel(request.Reason);

        if (result.IsFailure)
        {
            return result;
        }

        await orderWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
