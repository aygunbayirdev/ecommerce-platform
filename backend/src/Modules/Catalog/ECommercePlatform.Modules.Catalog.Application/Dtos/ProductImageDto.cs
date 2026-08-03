namespace ECommercePlatform.Modules.Catalog.Application.Dtos;

public sealed record ProductImageDto(Guid Id, string Url, bool IsPrimary, int DisplayOrder);
