namespace ECommercePlatform.Modules.Shipping.Application.Dtos;

public sealed record ShipmentDto(
    Guid Id,
    Guid OrderId,
    string Carrier,
    string TrackingNumber,
    string Status,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<ShipmentStatusHistoryDto> StatusHistory);
