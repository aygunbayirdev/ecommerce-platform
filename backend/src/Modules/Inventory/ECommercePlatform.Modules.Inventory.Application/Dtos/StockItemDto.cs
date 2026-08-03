namespace ECommercePlatform.Modules.Inventory.Application.Dtos;

public sealed record StockItemDto(
    Guid Id,
    Guid ProductVariantId,
    int AvailableQuantity,
    int ReservedQuantity,
    DateTime CreatedAtUtc);
