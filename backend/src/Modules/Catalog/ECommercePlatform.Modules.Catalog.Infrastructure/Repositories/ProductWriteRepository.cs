using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Repositories;

internal sealed class ProductWriteRepository(CatalogDbContext dbContext) : IProductWriteRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetByIdWithVariantsAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Products
            .Include(p => p.Variants).ThenInclude(v => v.AttributeValues)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken)
        => dbContext.ProductVariants.AnyAsync(v => v.Sku == sku, cancellationToken);

    public void Add(Product product) => dbContext.Products.Add(product);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
