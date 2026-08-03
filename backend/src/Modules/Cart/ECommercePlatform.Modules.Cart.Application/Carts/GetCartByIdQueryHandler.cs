using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.Modules.Cart.Application.Dtos;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

/// <summary>
/// Cross-module read composition: CartItem stores no price/name (it must always reflect the
/// live catalog, the same way a cart is supposed to work), so rendering a cart calls Catalog's
/// GetProductVariantSummariesQuery synchronously via ISender and joins in-memory. This is
/// deliberately not the event+outbox pattern used for Catalog → Inventory — that pattern accepts
/// eventual consistency for a state change, this is a read that needs an immediately correct answer.
/// </summary>
public sealed class GetCartByIdQueryHandler(ICartReadRepository cartReadRepository, ISender sender)
    : IQueryHandler<GetCartByIdQuery, CartViewDto>
{
    public async Task<Result<CartViewDto>> Handle(GetCartByIdQuery request, CancellationToken cancellationToken)
    {
        var cart = await cartReadRepository.GetByIdAsync(request.CartId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure<CartViewDto>(Error.NotFound("Carts.NotFound", "Sepet bulunamadı."));
        }

        if (cart.Items.Count == 0)
        {
            return new CartViewDto(cart.Id, cart.UserId, [], 0m);
        }

        var variantIds = cart.Items.Select(i => i.ProductVariantId).ToList();

        var summariesResult = await sender.Send(new GetProductVariantSummariesQuery(variantIds), cancellationToken);

        var summariesByVariantId = summariesResult.Value.ToDictionary(s => s.ProductVariantId);

        var items = cart.Items
            .Where(i => summariesByVariantId.ContainsKey(i.ProductVariantId))
            .Select(i =>
            {
                var summary = summariesByVariantId[i.ProductVariantId];
                return new CartItemViewDto(
                    i.ProductVariantId,
                    i.Quantity,
                    summary.ProductName,
                    summary.Sku,
                    summary.Price,
                    summary.ImageUrl,
                    summary.Price * i.Quantity);
            })
            .ToList();

        var total = items.Sum(i => i.LineTotal);

        return new CartViewDto(cart.Id, cart.UserId, items, total);
    }
}
