namespace ECommercePlatform.Modules.Catalog.Application.Dtos;

public sealed record ProductDetailDto(
    Guid Id,
    Guid CategoryId,
    Guid? BrandId,
    string Name,
    string Description,
    bool IsActive,
    DateTime CreatedAtUtc,
    IReadOnlyList<ProductVariantDto> Variants,
    IReadOnlyList<ProductImageDto> Images);
