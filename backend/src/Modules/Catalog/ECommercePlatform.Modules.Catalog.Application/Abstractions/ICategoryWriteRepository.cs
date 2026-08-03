using ECommercePlatform.Modules.Catalog.Domain;

namespace ECommercePlatform.Modules.Catalog.Application.Abstractions;

public interface ICategoryWriteRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Loads the Category aggregate with its Attributes collection included — required for attribute assignment.</summary>
    Task<Category?> GetByIdWithAttributesAsync(Guid id, CancellationToken cancellationToken);

    void Add(Category category);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
