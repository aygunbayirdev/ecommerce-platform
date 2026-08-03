using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Cart.Domain;

public sealed class CartItem : BaseEntity
{
    private CartItem()
    {
    }

    public Guid CartId { get; private set; }

    public Guid ProductVariantId { get; private set; }

    public int Quantity { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// CartItem is a child entity of the Cart aggregate — only Cart should call this (and the
    /// mutation methods below), so the "one line item per variant" invariant stays enforceable
    /// in one place.
    /// </summary>
    internal static CartItem Create(Guid cartId, Guid productVariantId, int quantity)
    {
        return new CartItem
        {
            CartId = Guard.AgainstEmpty(cartId, nameof(cartId)),
            ProductVariantId = Guard.AgainstEmpty(productVariantId, nameof(productVariantId)),
            Quantity = quantity,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    internal void IncreaseQuantity(int amount) => Quantity += amount;

    internal void SetQuantity(int quantity) => Quantity = quantity;
}
