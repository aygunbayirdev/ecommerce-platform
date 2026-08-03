namespace ECommercePlatform.Modules.Catalog.Application.Dtos;

public sealed record ProductVariantDto(
    Guid Id,
    string Sku,
    decimal Price,
    bool IsActive,
    IReadOnlyList<ProductVariantAttributeValueDto> AttributeValues);
