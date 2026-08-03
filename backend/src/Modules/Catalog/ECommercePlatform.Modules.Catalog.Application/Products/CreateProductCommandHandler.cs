using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class CreateProductCommandHandler(
    IProductWriteRepository productWriteRepository,
    ICategoryWriteRepository categoryWriteRepository) : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryWriteRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Products.CategoryNotFound", "Kategori bulunamadı."));
        }

        var product = Product.Create(request.CategoryId, request.BrandId, request.Name, request.Description);

        productWriteRepository.Add(product);
        await productWriteRepository.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
