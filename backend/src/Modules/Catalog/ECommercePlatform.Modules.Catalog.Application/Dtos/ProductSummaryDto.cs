namespace ECommercePlatform.Modules.Catalog.Application.Dtos;

public sealed record ProductSummaryDto(
    Guid Id,
    string Name,
    Guid CategoryId,
    Guid? BrandId,
    decimal? MinPrice,
    string? PrimaryImageUrl,
    bool IsActive);
