namespace ECommercePlatform.Modules.Catalog.Application.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    Guid? ParentCategoryId,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAtUtc);
