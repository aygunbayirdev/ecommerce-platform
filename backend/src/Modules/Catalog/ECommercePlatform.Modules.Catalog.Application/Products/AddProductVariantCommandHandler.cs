using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class AddProductVariantCommandHandler(
    IProductWriteRepository productWriteRepository,
    ICategoryWriteRepository categoryWriteRepository) : ICommandHandler<AddProductVariantCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddProductVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await productWriteRepository.GetByIdWithVariantsAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Products.NotFound", "Ürün bulunamadı."));
        }

        var skuExists = await productWriteRepository.ExistsBySkuAsync(request.Sku, cancellationToken);

        if (skuExists)
        {
            return Result.Failure<Guid>(Error.Conflict("Products.SkuAlreadyExists", "Bu SKU zaten kullanılıyor."));
        }

        var category = await categoryWriteRepository.GetByIdWithAttributesAsync(product.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Products.CategoryNotFound", "Ürünün kategorisi bulunamadı."));
        }

        var allowedAttributeIds = category.Attributes.Select(a => a.ProductAttributeId).ToHashSet();

        var invalidAttributeIds = request.AttributeValues
            .Select(v => v.ProductAttributeId)
            .Where(id => !allowedAttributeIds.Contains(id))
            .ToList();

        if (invalidAttributeIds.Count > 0)
        {
            return Result.Failure<Guid>(Error.Validation(
                "Products.AttributeNotAllowedForCategory",
                $"Şu özellikler ürünün kategorisine atanmamış: {string.Join(", ", invalidAttributeIds)}."));
        }

        var attributeValues = request.AttributeValues.ToDictionary(v => v.ProductAttributeId, v => v.Value);

        var variant = product.AddVariant(request.Sku, request.Price, attributeValues);

        await productWriteRepository.SaveChangesAsync(cancellationToken);

        return variant.Id;
    }
}
