using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class GetAllProductsForAdminQueryHandler(IProductReadRepository productReadRepository)
    : IQueryHandler<GetAllProductsForAdminQuery, PagedResult<ProductSummaryDto>>
{
    public async Task<Result<PagedResult<ProductSummaryDto>>> Handle(
        GetAllProductsForAdminQuery request, CancellationToken cancellationToken)
    {
        var result = await productReadRepository.GetAllForAdminAsync(
            request.CategoryId, request.PageNumber, request.PageSize, cancellationToken);

        return Result.Success(result);
    }
}
