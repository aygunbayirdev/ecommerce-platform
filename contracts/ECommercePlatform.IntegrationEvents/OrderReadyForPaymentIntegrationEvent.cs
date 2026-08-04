using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.IntegrationEvents;

/// <summary>
/// Published by the monolith (Order module) once an order's checkout has reserved stock and moved
/// to PaymentPending. Payment.Service consumes this to create a local, Pending Payment record —
/// event-carried state transfer: Payment never calls back into Order to learn who owns the order or
/// how much it costs, it keeps its own copy, populated from this event.
/// </summary>
public sealed record OrderReadyForPaymentIntegrationEvent(Guid OrderId, Guid UserId, decimal Amount) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
