using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed class GetCategoryAttributesQueryHandler(ICategoryReadRepository categoryReadRepository)
    : IQueryHandler<GetCategoryAttributesQuery, IReadOnlyList<ProductAttributeDto>>
{
    public async Task<Result<IReadOnlyList<ProductAttributeDto>>> Handle(
        GetCategoryAttributesQuery request, CancellationToken cancellationToken)
    {
        var attributes = await categoryReadRepository.GetAttributesByCategoryIdAsync(request.CategoryId, cancellationToken);

        return Result.Success(attributes);
    }
}
