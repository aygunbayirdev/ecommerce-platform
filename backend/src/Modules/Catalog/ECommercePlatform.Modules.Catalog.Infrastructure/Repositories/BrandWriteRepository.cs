using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Repositories;

internal sealed class BrandWriteRepository(CatalogDbContext dbContext) : IBrandWriteRepository
{
    public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public void Add(Brand brand) => dbContext.Brands.Add(brand);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
