using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Payment.Application.Payments;

/// <summary>
/// Idempotent by design (mirrors CreateStockItemCommandHandler in Catalog -> Inventory): this
/// command is the target of an at-least-once RabbitMQ delivery (OrderReadyForPaymentIntegrationEvent),
/// so a redelivered message must be a harmless no-op rather than an error that burns through the
/// consumer's retry budget.
/// </summary>
public sealed class CreatePendingPaymentCommandHandler(IPaymentWriteRepository paymentWriteRepository)
    : ICommandHandler<CreatePendingPaymentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePendingPaymentCommand request, CancellationToken cancellationToken)
    {
        var existing = await paymentWriteRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var payment = Domain.Payment.Create(request.OrderId, request.UserId, request.Amount);

        paymentWriteRepository.Add(payment);
        await paymentWriteRepository.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
