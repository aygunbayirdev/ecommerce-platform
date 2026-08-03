using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed class MarkOrderAsDeliveredCommandHandler(IOrderWriteRepository orderWriteRepository)
    : ICommandHandler<MarkOrderAsDeliveredCommand>
{
    public async Task<Result> Handle(MarkOrderAsDeliveredCommand request, CancellationToken cancellationToken)
    {
        var order = await orderWriteRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(Error.NotFound("Orders.NotFound", "Sipariş bulunamadı."));
        }

        var result = order.MarkAsDelivered();

        if (result.IsFailure)
        {
            return result;
        }

        await orderWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
