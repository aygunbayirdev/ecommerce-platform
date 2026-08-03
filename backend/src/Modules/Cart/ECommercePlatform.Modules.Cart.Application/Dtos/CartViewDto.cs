namespace ECommercePlatform.Modules.Cart.Application.Dtos;

public sealed record CartItemViewDto(
    Guid ProductVariantId,
    int Quantity,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    string? ImageUrl,
    decimal LineTotal);

public sealed record CartViewDto(Guid Id, Guid? UserId, IReadOnlyList<CartItemViewDto> Items, decimal Total);
