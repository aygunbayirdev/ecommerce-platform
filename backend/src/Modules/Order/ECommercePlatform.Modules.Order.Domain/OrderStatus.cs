namespace ECommercePlatform.Modules.Order.Domain;

public enum OrderStatus
{
    Created = 0,
    PaymentPending = 1,
    Paid = 2,
    Preparing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
}
