using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Payment.Application.Payments;

/// <summary>
/// Faz 4 rewrite: before the extraction this handler called Order synchronously (ISender) both to
/// read the order (ownership + status) and, on success, to mark it paid. Neither call exists anymore —
/// Payment.Service has no project reference to Order.Application at all. Ownership/amount now come
/// from the local Payment record CreatePendingPaymentCommandHandler already created from
/// OrderReadyForPaymentIntegrationEvent; notifying Order of a successful charge now happens through
/// Payment.Attempt() raising PaymentSucceededIntegrationEvent (picked up by the outbox, published to
/// RabbitMQ, consumed by Order — see Payment.Domain/Payment.cs).
/// </summary>
public sealed class ProcessPaymentCommandHandler(
    IPaymentWriteRepository paymentWriteRepository,
    IPaymentGateway paymentGateway) : ICommandHandler<ProcessPaymentCommand>
{
    public async Task<Result> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentWriteRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (payment is null)
        {
            // The order exists in the monolith, but OrderReadyForPaymentIntegrationEvent hasn't been
            // consumed yet — eventual consistency, visible here as a transient, retryable NotFound
            // rather than a permanent error.
            return Result.Failure(Error.NotFound(
                "Payments.OrderNotReady", "Sipariş ödeme için henüz hazır değil. Birkaç saniye içinde tekrar deneyin."));
        }

        if (payment.UserId != request.UserId)
        {
            return Result.Failure(Error.NotFound("Payments.OrderNotFound", "Sipariş bulunamadı."));
        }

        // The mock gateway call below is the ONLY line that changes when a real provider (iyzico)
        // replaces MockPaymentGateway — see IPaymentGateway.cs for the full rationale.
        var chargeResult = await paymentGateway.ChargeAsync(payment.Amount, request.CardNumber, cancellationToken);

        var attemptResult = payment.Attempt(request.IdempotencyKey, chargeResult.IsSuccessful, chargeResult.FailureReason);

        if (attemptResult.IsFailure)
        {
            return attemptResult;
        }

        await paymentWriteRepository.SaveChangesAsync(cancellationToken);

        return chargeResult.IsSuccessful
            ? Result.Success()
            : Result.Failure(Error.Conflict("Payments.ChargeDeclined", chargeResult.FailureReason ?? "Ödeme reddedildi."));
    }
}
