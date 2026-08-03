using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.Modules.Inventory.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

public sealed class GetStockByVariantIdQueryHandler(IStockItemReadRepository stockItemReadRepository)
    : IQueryHandler<GetStockByVariantIdQuery, StockItemDto>
{
    public async Task<Result<StockItemDto>> Handle(GetStockByVariantIdQuery request, CancellationToken cancellationToken)
    {
        var stockItem = await stockItemReadRepository.GetByProductVariantIdAsync(request.ProductVariantId, cancellationToken);

        if (stockItem is null)
        {
            return Result.Failure<StockItemDto>(Error.NotFound("StockItems.NotFound", "Bu varyant için stok kaydı bulunamadı."));
        }

        return stockItem;
    }
}
