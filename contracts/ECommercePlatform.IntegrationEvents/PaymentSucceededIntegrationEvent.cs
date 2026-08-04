using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.IntegrationEvents;

/// <summary>
/// Published by Payment.Service once a charge attempt succeeds. The monolith's Order module
/// consumes this and calls its existing MarkOrderAsPaidCommand — the same command Payment used to
/// call synchronously (in-process) before the extraction; only the transport changed.
/// </summary>
public sealed record PaymentSucceededIntegrationEvent(Guid OrderId, Guid PaymentId, decimal Amount) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
