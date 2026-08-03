using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Cart.Domain;

public sealed class Cart : BaseEntity
{
    private readonly List<CartItem> _items = [];

    private Cart()
    {
    }

    public Guid? UserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public static Cart Create(Guid? userId)
    {
        var now = DateTime.UtcNow;

        return new Cart
        {
            UserId = userId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
    }

    public CartItem AddItem(Guid productVariantId, int quantity)
    {
        var existing = _items.FirstOrDefault(i => i.ProductVariantId == productVariantId);

        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            UpdatedAtUtc = DateTime.UtcNow;
            return existing;
        }

        var item = CartItem.Create(Id, productVariantId, quantity);
        _items.Add(item);
        UpdatedAtUtc = DateTime.UtcNow;

        return item;
    }

    public Result UpdateItemQuantity(Guid productVariantId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductVariantId == productVariantId);

        if (item is null)
        {
            return Result.Failure(Error.NotFound("Carts.ItemNotFound", "Sepette bu varyanta ait bir ürün bulunamadı."));
        }

        item.SetQuantity(quantity);
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result RemoveItem(Guid productVariantId)
    {
        var item = _items.FirstOrDefault(i => i.ProductVariantId == productVariantId);

        if (item is null)
        {
            return Result.Failure(Error.NotFound("Carts.ItemNotFound", "Sepette bu varyanta ait bir ürün bulunamadı."));
        }

        _items.Remove(item);
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
