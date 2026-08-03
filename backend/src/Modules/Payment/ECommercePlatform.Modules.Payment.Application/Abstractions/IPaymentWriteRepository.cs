namespace ECommercePlatform.Modules.Payment.Application.Abstractions;

public interface IPaymentWriteRepository
{
    /// <summary>Loads the Payment aggregate with its Transactions included — required to check IdempotencyKey reuse.</summary>
    Task<Domain.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    void Add(Domain.Payment payment);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
