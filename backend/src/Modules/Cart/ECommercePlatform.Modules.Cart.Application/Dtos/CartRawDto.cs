namespace ECommercePlatform.Modules.Cart.Application.Dtos;

public sealed record CartRawItemDto(Guid ProductVariantId, int Quantity);

public sealed record CartRawDto(Guid Id, Guid? UserId, IReadOnlyList<CartRawItemDto> Items);
