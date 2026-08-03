using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Repositories;

internal sealed class CategoryWriteRepository(CatalogDbContext dbContext) : ICategoryWriteRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Category?> GetByIdWithAttributesAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Categories.Include(c => c.Attributes).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public void Add(Category category) => dbContext.Categories.Add(category);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
