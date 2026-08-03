using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.Abstractions;

public interface ICategoryReadRepository
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductAttributeDto>> GetAttributesByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken);
}
