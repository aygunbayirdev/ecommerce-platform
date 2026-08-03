using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

public sealed class IncreaseStockCommandHandler(IStockItemWriteRepository stockItemWriteRepository)
    : ICommandHandler<IncreaseStockCommand>
{
    public async Task<Result> Handle(IncreaseStockCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await stockItemWriteRepository.GetByProductVariantIdAsync(request.ProductVariantId, cancellationToken);

        if (stockItem is null)
        {
            return Result.Failure(Error.NotFound("StockItems.NotFound", "Bu varyant için stok kaydı bulunamadı."));
        }

        var result = stockItem.IncreaseStock(request.Quantity, request.Reason);

        if (result.IsFailure)
        {
            return result;
        }

        await stockItemWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
