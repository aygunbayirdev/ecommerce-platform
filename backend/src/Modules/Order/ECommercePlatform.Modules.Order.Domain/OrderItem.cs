using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.Domain;

public sealed class OrderItem : BaseEntity
{
    private OrderItem()
    {
    }

    public Guid OrderId { get; private set; }

    /// <summary>Cross-module reference to Catalog.ProductVariant — no DB foreign key by design (modular monolith: no cross-schema FKs).</summary>
    public Guid ProductVariantId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public string Sku { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    internal static OrderItem Create(Guid orderId, OrderItemSnapshot snapshot)
    {
        return new OrderItem
        {
            OrderId = Guard.AgainstEmpty(orderId, nameof(orderId)),
            ProductVariantId = Guard.AgainstEmpty(snapshot.ProductVariantId, nameof(snapshot.ProductVariantId)),
            ProductName = Guard.AgainstNullOrWhiteSpace(snapshot.ProductName, nameof(snapshot.ProductName)),
            Sku = Guard.AgainstNullOrWhiteSpace(snapshot.Sku, nameof(snapshot.Sku)),
            UnitPrice = snapshot.UnitPrice,
            Quantity = snapshot.Quantity,
        };
    }
}
