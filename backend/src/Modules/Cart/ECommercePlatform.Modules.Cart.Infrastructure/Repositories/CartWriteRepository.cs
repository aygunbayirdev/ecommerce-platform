using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.Modules.Cart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Cart.Infrastructure.Repositories;

internal sealed class CartWriteRepository(CartDbContext dbContext) : ICartWriteRepository
{
    public Task<Domain.Cart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Domain.Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken)
        => dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public void Add(Domain.Cart cart) => dbContext.Carts.Add(cart);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
