using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class RemoveProductImageCommandHandler(IProductWriteRepository productWriteRepository)
    : ICommandHandler<RemoveProductImageCommand>
{
    public async Task<Result> Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await productWriteRepository.GetByIdWithVariantsAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(Error.NotFound("Products.NotFound", "Ürün bulunamadı."));
        }

        var result = product.RemoveImage(request.ProductImageId);

        if (result.IsFailure)
        {
            return result;
        }

        await productWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
