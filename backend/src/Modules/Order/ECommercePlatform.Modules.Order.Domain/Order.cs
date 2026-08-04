using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.Domain;

public sealed class Order : BaseEntity
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];

    private Order()
    {
    }

    public string OrderNumber { get; private set; } = string.Empty;

    public Guid UserId { get; private set; }

    public OrderStatus Status { get; private set; }

    public string ShippingRecipientName { get; private set; } = string.Empty;

    public string ShippingPhoneNumber { get; private set; } = string.Empty;

    public string ShippingCity { get; private set; } = string.Empty;

    public string ShippingDistrict { get; private set; } = string.Empty;

    public string ShippingFullAddressLine { get; private set; } = string.Empty;

    public string ShippingPostalCode { get; private set; } = string.Empty;

    /// <summary>Snapshot of the coupon code applied at checkout, if any — not an FK to Promotion.Coupon (that coupon could be deactivated later without affecting this order).</summary>
    public string? CouponCode { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public decimal Total => Math.Max(0, _items.Sum(i => i.LineTotal) - DiscountAmount);

    public static Order Create(
        Guid userId,
        string shippingRecipientName,
        string shippingPhoneNumber,
        string shippingCity,
        string shippingDistrict,
        string shippingFullAddressLine,
        string shippingPostalCode,
        IReadOnlyList<OrderItemSnapshot> items)
    {
        var now = DateTime.UtcNow;

        var order = new Order
        {
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            Status = OrderStatus.Created,
            ShippingRecipientName = Guard.AgainstNullOrWhiteSpace(shippingRecipientName, nameof(shippingRecipientName)),
            ShippingPhoneNumber = Guard.AgainstNullOrWhiteSpace(shippingPhoneNumber, nameof(shippingPhoneNumber)),
            ShippingCity = Guard.AgainstNullOrWhiteSpace(shippingCity, nameof(shippingCity)),
            ShippingDistrict = Guard.AgainstNullOrWhiteSpace(shippingDistrict, nameof(shippingDistrict)),
            ShippingFullAddressLine = Guard.AgainstNullOrWhiteSpace(shippingFullAddressLine, nameof(shippingFullAddressLine)),
            ShippingPostalCode = Guard.AgainstNullOrWhiteSpace(shippingPostalCode, nameof(shippingPostalCode)),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        // Take the LAST 8 hex chars, not the first: Guid.CreateVersion7()'s leading bits are a
        // millisecond timestamp, so two orders created close together would otherwise collide on
        // the order_number unique index. The trailing bits are UUIDv7's random section.
        order.OrderNumber = $"ORD-{now:yyyyMMdd}-{order.Id.ToString("N")[^8..].ToUpperInvariant()}";

        foreach (var item in items)
        {
            order._items.Add(OrderItem.Create(order.Id, item));
        }

        order._statusHistory.Add(OrderStatusHistory.Create(order.Id, OrderStatus.Created, note: null));

        return order;
    }

    public Result ApplyDiscount(string couponCode, decimal discountAmount)
    {
        if (CouponCode is not null)
        {
            return Result.Failure(Error.Conflict("Orders.CouponAlreadyApplied", "Bu siparişe zaten bir kupon uygulanmış."));
        }

        CouponCode = couponCode;
        DiscountAmount = discountAmount;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkReadyForPayment() => TransitionTo(OrderStatus.PaymentPending, from: OrderStatus.Created, note: null);

    public Result MarkAsPaid() => TransitionTo(OrderStatus.Paid, from: OrderStatus.PaymentPending, note: null);

    public Result MarkAsPreparing() => TransitionTo(OrderStatus.Preparing, from: OrderStatus.Paid, note: null);

    public Result MarkAsShipped() => TransitionTo(OrderStatus.Shipped, from: OrderStatus.Preparing, note: null);

    public Result MarkAsDelivered() => TransitionTo(OrderStatus.Delivered, from: OrderStatus.Shipped, note: null);

    public Result Cancel(string reason)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return Result.Failure(Error.Conflict(
                "Orders.InvalidStatusTransition",
                $"{Status} durumundaki bir sipariş iptal edilemez."));
        }

        Status = OrderStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
        _statusHistory.Add(OrderStatusHistory.Create(Id, OrderStatus.Cancelled, reason));

        return Result.Success();
    }

    private Result TransitionTo(OrderStatus to, OrderStatus from, string? note)
    {
        if (Status != from)
        {
            return Result.Failure(Error.Conflict(
                "Orders.InvalidStatusTransition",
                $"Sipariş {from} durumunda değil (şu an: {Status}), {to} durumuna geçemez."));
        }

        Status = to;
        UpdatedAtUtc = DateTime.UtcNow;
        _statusHistory.Add(OrderStatusHistory.Create(Id, to, note));

        return Result.Success();
    }
}
