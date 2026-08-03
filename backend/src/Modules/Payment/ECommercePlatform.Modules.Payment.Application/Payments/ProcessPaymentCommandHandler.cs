using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Payment.Application.Payments;

/// <summary>
/// This is the third instance in the project of the same rule: a user-facing action that needs an
/// immediate success/failure answer talks to other modules with a direct, synchronous ISender call,
/// never a domain event. (The first was Cart -> Catalog for live product data; the second was
/// Order's checkout orchestration across Cart/Identity/Inventory.) A customer submitting a card
/// needs to know NOW whether the charge went through — an eventually-consistent event would mean
/// telling them "we'll let you know" for something that should just work synchronously.
///
/// Note the dependency direction: Payment.Application -> Order.Application only. Order never
/// references Payment. There's no "Order announces it's ready for payment" step here — unlike a
/// real gateway, the mock has no PaymentIntent-style prep step, so a Payment simply gets created
/// lazily the first time the customer tries to pay. That keeps this a one-way dependency with no
/// risk of a circular Order&lt;-&gt;Payment reference (which .NET wouldn't even allow to compile).
/// </summary>
public sealed class ProcessPaymentCommandHandler(
    IPaymentWriteRepository paymentWriteRepository,
    IPaymentGateway paymentGateway,
    ISender sender) : ICommandHandler<ProcessPaymentCommand>
{
    public async Task<Result> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await sender.Send(new GetOrderByIdQuery(request.OrderId), cancellationToken);

        if (orderResult.IsFailure || orderResult.Value.UserId != request.UserId)
        {
            return Result.Failure(Error.NotFound("Payments.OrderNotFound", "Sipariş bulunamadı."));
        }

        var order = orderResult.Value;

        if (order.Status != "PaymentPending")
        {
            return Result.Failure(Error.Conflict(
                "Payments.OrderNotPaymentPending",
                $"Sipariş ödeme bekleyen durumda değil (şu an: {order.Status})."));
        }

        var payment = await paymentWriteRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (payment is null)
        {
            payment = Domain.Payment.Create(request.OrderId, order.Total);
            paymentWriteRepository.Add(payment);
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

        if (!chargeResult.IsSuccessful)
        {
            return Result.Failure(Error.Conflict("Payments.ChargeDeclined", chargeResult.FailureReason ?? "Ödeme reddedildi."));
        }

        await sender.Send(new MarkOrderAsPaidCommand(request.OrderId), cancellationToken);

        return Result.Success();
    }
}
