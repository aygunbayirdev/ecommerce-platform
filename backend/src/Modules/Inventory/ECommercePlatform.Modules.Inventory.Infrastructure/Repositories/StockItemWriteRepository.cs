using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.Modules.Inventory.Domain;
using ECommercePlatform.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Inventory.Infrastructure.Repositories;

internal sealed class StockItemWriteRepository(InventoryDbContext dbContext) : IStockItemWriteRepository
{
    public Task<StockItem?> GetByProductVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken)
        => dbContext.StockItems.FirstOrDefaultAsync(s => s.ProductVariantId == productVariantId, cancellationToken);

    public void Add(StockItem stockItem) => dbContext.StockItems.Add(stockItem);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
