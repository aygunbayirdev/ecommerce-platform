using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.Abstractions;

public interface IProductAttributeReadRepository
{
    Task<IReadOnlyList<ProductAttributeDto>> GetAllAsync(CancellationToken cancellationToken);
}
