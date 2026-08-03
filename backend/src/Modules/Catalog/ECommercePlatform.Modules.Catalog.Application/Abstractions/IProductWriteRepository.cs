using ECommercePlatform.Modules.Catalog.Domain;

namespace ECommercePlatform.Modules.Catalog.Application.Abstractions;

public interface IProductWriteRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Loads the Product aggregate with Variants (+ their AttributeValues) and Images included — required for variant/image mutations.</summary>
    Task<Product?> GetByIdWithVariantsAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken);

    void Add(Product product);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
