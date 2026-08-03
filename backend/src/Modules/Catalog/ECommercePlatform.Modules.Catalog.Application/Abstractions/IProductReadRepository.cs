using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.Abstractions;

public interface IProductReadRepository
{
    Task<ProductDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<ProductSummaryDto>> GetByCategoryIdAsync(
        Guid categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Used by other modules (e.g. Cart) to enrich a list of variant ids with live product name/price/image data.</summary>
    Task<IReadOnlyList<ProductVariantSummaryDto>> GetVariantSummariesAsync(
        IReadOnlyList<Guid> productVariantIds, CancellationToken cancellationToken);
}
