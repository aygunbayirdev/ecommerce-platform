using ECommercePlatform.Modules.Inventory.Application.Dtos;

namespace ECommercePlatform.Modules.Inventory.Application.Abstractions;

public interface IStockItemReadRepository
{
    Task<StockItemDto?> GetByProductVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken);
}
