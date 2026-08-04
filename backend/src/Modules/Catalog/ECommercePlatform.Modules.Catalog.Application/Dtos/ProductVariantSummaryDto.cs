namespace ECommercePlatform.Modules.Catalog.Application.Dtos;

public sealed record ProductVariantSummaryDto(
    Guid ProductVariantId,
    string ProductName,
    string Sku,
    decimal Price,
    string? ImageUrl,
    bool IsActive,
    Guid ProductId);
