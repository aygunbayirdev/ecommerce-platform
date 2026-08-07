using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class ReactivateProductCommandHandler(IProductWriteRepository productWriteRepository)
    : ICommandHandler<ReactivateProductCommand>
{
    public async Task<Result> Handle(ReactivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productWriteRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(Error.NotFound("Products.NotFound", "Ürün bulunamadı."));
        }

        var result = product.Reactivate();

        if (result.IsFailure)
        {
            return result;
        }

        await productWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
