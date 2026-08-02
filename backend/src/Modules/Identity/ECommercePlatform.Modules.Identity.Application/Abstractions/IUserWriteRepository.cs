using ECommercePlatform.Modules.Identity.Domain;

namespace ECommercePlatform.Modules.Identity.Application.Abstractions;

public interface IUserWriteRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    void Add(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
