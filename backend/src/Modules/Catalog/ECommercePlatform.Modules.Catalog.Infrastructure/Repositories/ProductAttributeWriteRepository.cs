using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Repositories;

internal sealed class ProductAttributeWriteRepository(CatalogDbContext dbContext) : IProductAttributeWriteRepository
{
    public Task<ProductAttribute?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.ProductAttributes.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public void Add(ProductAttribute productAttribute) => dbContext.ProductAttributes.Add(productAttribute);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
