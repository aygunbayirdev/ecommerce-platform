namespace ECommercePlatform.Modules.Payment.Domain;

/// <summary>
/// A payment only ever moves forward one way: Pending -> Succeeded is terminal (an order can't be
/// charged twice). Pending -> Failed does NOT exist as a status here on purpose — a declined charge
/// attempt is recorded as a failed <see cref="PaymentTransaction"/>, but the Payment itself stays
/// Pending so the customer can retry with a different card (a new IdempotencyKey) without needing
/// any "reset" step. See <see cref="Payment.Attempt"/> for the exact rule.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
}
