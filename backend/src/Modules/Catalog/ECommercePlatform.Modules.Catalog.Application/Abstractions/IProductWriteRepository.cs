using ECommercePlatform.Modules.Catalog.Domain;

namespace ECommercePlatform.Modules.Catalog.Application.Abstractions;

public interface IProductWriteRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Loads the Product aggregate with Variants (+ their AttributeValues) and Images included — required for variant/image mutations.</summary>
    Task<Product?> GetByIdWithVariantsAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken);

    /// <summary>Used to guard against removing a CategoryAttribute while a variant of a product in that category still has a value set for it.</summary>
    Task<bool> IsAttributeUsedByAnyVariantInCategoryAsync(Guid categoryId, Guid productAttributeId, CancellationToken cancellationToken);

    void Add(Product product);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
