using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Payment.Domain;

/// <summary>
/// One row per charge attempt against the gateway (successful or declined) — an audit log, the
/// same role <c>StockMovement</c> plays for <c>StockItem</c> in the Inventory module. Keeping every
/// attempt (not just the final outcome) is what makes the IdempotencyKey replay-protection in
/// <see cref="Payment.Attempt"/> possible: we need to know whether a given key was already used.
/// </summary>
public sealed class PaymentTransaction : BaseEntity
{
    private PaymentTransaction()
    {
    }

    public Guid PaymentId { get; private set; }

    /// <summary>
    /// A caller-generated key identifying one logical charge attempt. Real gateways (Stripe,
    /// iyzico) use the same concept: if a request times out and the client retries with the SAME
    /// key, the gateway returns the original result instead of charging the card twice. A genuinely
    /// new attempt (e.g. a different card) must use a new key.
    /// </summary>
    public string IdempotencyKey { get; private set; } = string.Empty;

    public bool IsSuccessful { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    internal static PaymentTransaction Create(Guid paymentId, string idempotencyKey, bool isSuccessful, string? failureReason)
    {
        return new PaymentTransaction
        {
            PaymentId = Guard.AgainstEmpty(paymentId, nameof(paymentId)),
            IdempotencyKey = Guard.AgainstNullOrWhiteSpace(idempotencyKey, nameof(idempotencyKey)),
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            OccurredAtUtc = DateTime.UtcNow,
        };
    }
}
