using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Inventory.Application.Dtos;

namespace ECommercePlatform.Modules.Inventory.Application.Abstractions;

public interface IStockItemReadRepository
{
    Task<StockItemDto?> GetByProductVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken);

    /// <summary>Raw paged rows, no product name/SKU — the Api layer (StockItemsController) enriches those via Catalog.</summary>
    Task<PagedResult<StockItemDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
