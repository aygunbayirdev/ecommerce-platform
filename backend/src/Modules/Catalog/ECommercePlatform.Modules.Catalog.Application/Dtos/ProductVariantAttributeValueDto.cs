namespace ECommercePlatform.Modules.Catalog.Application.Dtos;

public sealed record ProductVariantAttributeValueDto(Guid ProductAttributeId, string ProductAttributeName, string Value);
