namespace ECommercePlatform.Modules.Order.Application.Dtos;

public sealed record OrderStatusHistoryDto(string Status, string? Note, DateTime ChangedAtUtc);
