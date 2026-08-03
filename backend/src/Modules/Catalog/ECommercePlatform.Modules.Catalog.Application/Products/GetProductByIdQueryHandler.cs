using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class GetProductByIdQueryHandler(IProductReadRepository productReadRepository)
    : IQueryHandler<GetProductByIdQuery, Dtos.ProductDetailDto>
{
    public async Task<Result<Dtos.ProductDetailDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productReadRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure<Dtos.ProductDetailDto>(Error.NotFound("Products.NotFound", "Ürün bulunamadı."));
        }

        return product;
    }
}
