namespace ECommercePlatform.Modules.Order.Application.Dtos;

public sealed record OrderItemDto(
    Guid ProductVariantId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
