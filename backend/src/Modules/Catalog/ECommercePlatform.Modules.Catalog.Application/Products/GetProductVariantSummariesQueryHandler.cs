using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class GetProductVariantSummariesQueryHandler(IProductReadRepository productReadRepository)
    : IQueryHandler<GetProductVariantSummariesQuery, IReadOnlyList<ProductVariantSummaryDto>>
{
    public async Task<Result<IReadOnlyList<ProductVariantSummaryDto>>> Handle(
        GetProductVariantSummariesQuery request, CancellationToken cancellationToken)
    {
        var summaries = await productReadRepository.GetVariantSummariesAsync(request.ProductVariantIds, cancellationToken);

        return Result.Success(summaries);
    }
}
