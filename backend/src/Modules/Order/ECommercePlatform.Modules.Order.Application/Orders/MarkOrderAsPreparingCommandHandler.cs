using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed class MarkOrderAsPreparingCommandHandler(IOrderWriteRepository orderWriteRepository)
    : ICommandHandler<MarkOrderAsPreparingCommand>
{
    public async Task<Result> Handle(MarkOrderAsPreparingCommand request, CancellationToken cancellationToken)
    {
        var order = await orderWriteRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(Error.NotFound("Orders.NotFound", "Sipariş bulunamadı."));
        }

        var result = order.MarkAsPreparing();

        if (result.IsFailure)
        {
            return result;
        }

        await orderWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
