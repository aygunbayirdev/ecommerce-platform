namespace ECommercePlatform.Modules.Catalog.Application.Dtos;

public sealed record BrandDto(Guid Id, string Name, bool IsActive, DateTime CreatedAtUtc);
