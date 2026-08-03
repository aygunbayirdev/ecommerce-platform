using ECommercePlatform.Modules.Catalog.Domain;

namespace ECommercePlatform.Modules.Catalog.Application.Abstractions;

public interface IBrandWriteRepository
{
    Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(Brand brand);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
