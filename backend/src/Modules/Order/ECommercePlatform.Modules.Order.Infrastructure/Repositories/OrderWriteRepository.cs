using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Order.Infrastructure.Repositories;

internal sealed class OrderWriteRepository(OrderDbContext dbContext) : IOrderWriteRepository
{
    public Task<Domain.Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public void Add(Domain.Order order) => dbContext.Orders.Add(order);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
