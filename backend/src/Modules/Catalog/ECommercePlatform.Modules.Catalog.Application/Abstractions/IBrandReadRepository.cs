using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.Abstractions;

public interface IBrandReadRepository
{
    Task<IReadOnlyList<BrandDto>> GetAllAsync(CancellationToken cancellationToken);
}
