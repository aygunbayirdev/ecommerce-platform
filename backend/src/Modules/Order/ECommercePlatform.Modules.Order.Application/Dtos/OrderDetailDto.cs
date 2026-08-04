namespace ECommercePlatform.Modules.Order.Application.Dtos;

public sealed record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    string Status,
    string ShippingRecipientName,
    string ShippingPhoneNumber,
    string ShippingCity,
    string ShippingDistrict,
    string ShippingFullAddressLine,
    string ShippingPostalCode,
    string? CouponCode,
    decimal DiscountAmount,
    DateTime CreatedAtUtc,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory,
    decimal Total);
