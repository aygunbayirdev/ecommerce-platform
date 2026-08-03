using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.ProductAttributes;

public sealed class GetProductAttributesQueryHandler(IProductAttributeReadRepository productAttributeReadRepository)
    : IQueryHandler<GetProductAttributesQuery, IReadOnlyList<ProductAttributeDto>>
{
    public async Task<Result<IReadOnlyList<ProductAttributeDto>>> Handle(
        GetProductAttributesQuery request, CancellationToken cancellationToken)
    {
        var attributes = await productAttributeReadRepository.GetAllAsync(cancellationToken);

        return Result.Success(attributes);
    }
}
