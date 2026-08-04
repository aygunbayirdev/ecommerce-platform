using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.Modules.Payment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Payment.Infrastructure.Repositories;

internal sealed class PaymentWriteRepository(PaymentDbContext dbContext) : IPaymentWriteRepository
{
    public Task<Domain.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        => dbContext.Payments.Include(p => p.Transactions).FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public void Add(Domain.Payment payment) => dbContext.Payments.Add(payment);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
