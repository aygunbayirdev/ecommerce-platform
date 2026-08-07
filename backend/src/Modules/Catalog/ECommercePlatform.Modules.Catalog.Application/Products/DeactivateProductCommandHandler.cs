using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class DeactivateProductCommandHandler(IProductWriteRepository productWriteRepository)
    : ICommandHandler<DeactivateProductCommand>
{
    public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productWriteRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(Error.NotFound("Products.NotFound", "Ürün bulunamadı."));
        }

        var result = product.Deactivate();

        if (result.IsFailure)
        {
            return result;
        }

        await productWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
