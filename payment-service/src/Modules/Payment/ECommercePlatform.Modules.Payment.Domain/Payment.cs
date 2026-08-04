using ECommercePlatform.IntegrationEvents;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Payment.Domain;

/// <summary>
/// Aggregate root for a single order's payment. One Payment per Order — created by
/// CreatePendingPaymentCommandHandler when OrderReadyForPaymentIntegrationEvent arrives from the
/// monolith (Faz 4), not lazily on first charge attempt as before the extraction — Payment.Service
/// no longer has any synchronous way to ask Order for the amount/owner at charge time, so it must
/// already know both by then. Holds every attempt made against it in <see cref="Transactions"/>.
///
/// This module is deliberately built against an <c>IPaymentGateway</c> abstraction (see
/// Payment.Application/Abstractions/IPaymentGateway.cs) rather than talking to a real payment
/// provider directly. Nothing in this file — or anywhere else in Payment.Domain/.Application —
/// knows or cares that the current implementation (<c>MockPaymentGateway</c>) is fake. When a real
/// gateway (iyzico) replaces it later, only that one Infrastructure class changes.
/// </summary>
public sealed class Payment : BaseEntity
{
    private readonly List<PaymentTransaction> _transactions = [];

    private Payment()
    {
    }

    /// <summary>Cross-module reference to Order.Order — no DB foreign key by design (modular monolith: no cross-schema FKs). Never queried synchronously; populated once, from OrderReadyForPaymentIntegrationEvent.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>Copied from OrderReadyForPaymentIntegrationEvent at creation time — lets ProcessPaymentCommandHandler check ownership entirely locally, with no call back into the monolith.</summary>
    public Guid UserId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<PaymentTransaction> Transactions => _transactions.AsReadOnly();

    public static Payment Create(Guid orderId, Guid userId, decimal amount)
    {
        var now = DateTime.UtcNow;

        return new Payment
        {
            OrderId = Guard.AgainstEmpty(orderId, nameof(orderId)),
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            Amount = amount,
            Status = PaymentStatus.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
    }

    /// <summary>
    /// Records the outcome of one charge attempt from the gateway.
    ///
    /// Two guards, in order:
    /// 1. Already <see cref="PaymentStatus.Succeeded"/> — an order can't be charged twice, full stop.
    /// 2. <paramref name="idempotencyKey"/> already used — this is the "retry-safety" guarantee real
    ///    gateways make: replaying the exact same logical request must not double-charge or
    ///    re-process. We enforce it here too, at the domain level, not just by trusting the gateway.
    ///
    /// If the attempt is declined (<paramref name="isSuccessful"/> is false), Status stays Pending —
    /// this is intentional, not an oversight. A declined card shouldn't permanently block the order
    /// from being paid; the customer can call ProcessPaymentCommand again with a different card and
    /// a NEW idempotency key. Status only ever becomes Succeeded, never a separate "Failed" — the
    /// failure is fully captured by the recorded PaymentTransaction, not by aggregate-level state.
    ///
    /// A successful attempt raises PaymentSucceededIntegrationEvent (Faz 4) — the outbox picks this
    /// up in the same transaction as the status change and a background publisher pushes it to
    /// RabbitMQ, where the monolith's Order module consumes it and calls MarkOrderAsPaidCommand.
    /// Before the extraction this same notification was a direct, synchronous ISender call; only the
    /// transport changed, not the fact that a successful charge always tells Order about it.
    /// </summary>
    public Result Attempt(string idempotencyKey, bool isSuccessful, string? failureReason)
    {
        if (Status == PaymentStatus.Succeeded)
        {
            return Result.Failure(Error.Conflict("Payments.AlreadyPaid", "Bu sipariş için ödeme zaten tamamlanmış."));
        }

        if (_transactions.Any(t => t.IdempotencyKey == idempotencyKey))
        {
            return Result.Failure(Error.Conflict(
                "Payments.DuplicateIdempotencyKey",
                "Bu işlem daha önce işlendi (aynı idempotency key ile tekrar denenmiş)."));
        }

        _transactions.Add(PaymentTransaction.Create(Id, idempotencyKey, isSuccessful, failureReason));
        UpdatedAtUtc = DateTime.UtcNow;

        if (isSuccessful)
        {
            Status = PaymentStatus.Succeeded;
            AddDomainEvent(new PaymentSucceededIntegrationEvent(OrderId, Id, Amount));
        }

        return Result.Success();
    }
}
