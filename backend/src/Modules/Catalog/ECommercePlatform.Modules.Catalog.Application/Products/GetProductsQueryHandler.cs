using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class GetProductsQueryHandler(IProductReadRepository productReadRepository)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductSummaryDto>>
{
    public async Task<Result<PagedResult<ProductSummaryDto>>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken)
    {
        var result = await productReadRepository.GetAsync(
            request.CategoryId, request.PageNumber, request.PageSize, cancellationToken);

        return Result.Success(result);
    }
}
