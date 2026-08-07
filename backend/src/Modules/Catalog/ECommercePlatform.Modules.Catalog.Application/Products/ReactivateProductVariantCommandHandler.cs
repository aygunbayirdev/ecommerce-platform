using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class ReactivateProductVariantCommandHandler(IProductWriteRepository productWriteRepository)
    : ICommandHandler<ReactivateProductVariantCommand>
{
    public async Task<Result> Handle(ReactivateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await productWriteRepository.GetByIdWithVariantsAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(Error.NotFound("Products.NotFound", "Ürün bulunamadı."));
        }

        var result = product.ReactivateVariant(request.ProductVariantId);

        if (result.IsFailure)
        {
            return result;
        }

        await productWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
