using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class AddProductImageCommandHandler(IProductWriteRepository productWriteRepository)
    : ICommandHandler<AddProductImageCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await productWriteRepository.GetByIdWithVariantsAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Products.NotFound", "Ürün bulunamadı."));
        }

        var image = product.AddImage(request.Url, request.IsPrimary);

        await productWriteRepository.SaveChangesAsync(cancellationToken);

        return image.Id;
    }
}
