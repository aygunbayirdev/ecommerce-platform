using ECommercePlatform.IntegrationEvents;
using ECommercePlatform.Modules.Payment.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Payment.UnitTests;

public sealed class PaymentTests
{
    [Fact]
    public void Create_ShouldStartAsPending()
    {
        var payment = Domain.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Empty(payment.Transactions);
    }

    [Fact]
    public void Attempt_ShouldMarkSucceeded_AndRaisePaymentSucceededIntegrationEvent_WhenChargeSucceeds()
    {
        var orderId = Guid.NewGuid();
        var payment = Domain.Payment.Create(orderId, Guid.NewGuid(), 100m);

        var result = payment.Attempt("key-1", isSuccessful: true, failureReason: null);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Single(payment.Transactions);
        var domainEvent = Assert.Single(payment.DomainEvents);
        var integrationEvent = Assert.IsType<PaymentSucceededIntegrationEvent>(domainEvent);
        Assert.Equal(orderId, integrationEvent.OrderId);
        Assert.Equal(payment.Id, integrationEvent.PaymentId);
        Assert.Equal(100m, integrationEvent.Amount);
    }

    [Fact]
    public void Attempt_ShouldStayPendingAndRecordTransaction_AndRaiseNoEvent_WhenChargeDeclined()
    {
        var payment = Domain.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);

        var result = payment.Attempt("key-1", isSuccessful: false, failureReason: "Kart reddedildi.");

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        var transaction = Assert.Single(payment.Transactions);
        Assert.False(transaction.IsSuccessful);
        Assert.Equal("Kart reddedildi.", transaction.FailureReason);
        Assert.Empty(payment.DomainEvents);
    }

    [Fact]
    public void Attempt_ShouldAllowRetryWithNewIdempotencyKey_AfterADecline()
    {
        var payment = Domain.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        payment.Attempt("key-1", isSuccessful: false, failureReason: "Kart reddedildi.");

        var result = payment.Attempt("key-2", isSuccessful: true, failureReason: null);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(2, payment.Transactions.Count);
    }

    [Fact]
    public void Attempt_ShouldReturnConflict_WhenIdempotencyKeyReused()
    {
        var payment = Domain.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        payment.Attempt("key-1", isSuccessful: false, failureReason: "Kart reddedildi.");

        var result = payment.Attempt("key-1", isSuccessful: true, failureReason: null);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Single(payment.Transactions);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Attempt_ShouldReturnConflict_WhenAlreadySucceeded()
    {
        var payment = Domain.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        payment.Attempt("key-1", isSuccessful: true, failureReason: null);

        var result = payment.Attempt("key-2", isSuccessful: true, failureReason: null);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Single(payment.Transactions);
    }
}
