namespace ECommercePlatform.Modules.Shipping.Application.Dtos;

public sealed record ShipmentStatusHistoryDto(string Status, string? Note, DateTime ChangedAtUtc);
