using ECommercePlatform.Modules.Catalog.Domain;

namespace ECommercePlatform.Modules.Catalog.Application.Abstractions;

public interface IProductAttributeWriteRepository
{
    Task<ProductAttribute?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(ProductAttribute productAttribute);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
