using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.Domain;

public sealed class OrderStatusHistory : BaseEntity
{
    private OrderStatusHistory()
    {
    }

    public Guid OrderId { get; private set; }

    public OrderStatus Status { get; private set; }

    public string? Note { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    internal static OrderStatusHistory Create(Guid orderId, OrderStatus status, string? note)
    {
        return new OrderStatusHistory
        {
            OrderId = Guard.AgainstEmpty(orderId, nameof(orderId)),
            Status = status,
            Note = note,
            ChangedAtUtc = DateTime.UtcNow,
        };
    }
}
