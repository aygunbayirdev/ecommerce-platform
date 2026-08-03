using ECommercePlatform.Modules.Inventory.Domain;

namespace ECommercePlatform.Modules.Inventory.Application.Abstractions;

public interface IStockItemWriteRepository
{
    Task<StockItem?> GetByProductVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken);

    void Add(StockItem stockItem);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
