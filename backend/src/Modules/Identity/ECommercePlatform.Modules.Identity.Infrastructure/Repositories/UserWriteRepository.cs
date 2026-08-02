using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Domain;
using ECommercePlatform.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Identity.Infrastructure.Repositories;

internal sealed class UserWriteRepository(IdentityDbContext dbContext) : IUserWriteRepository
{
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        => dbContext.Users.AnyAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByIdWithAddressesAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Users.Include(u => u.Addresses).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
