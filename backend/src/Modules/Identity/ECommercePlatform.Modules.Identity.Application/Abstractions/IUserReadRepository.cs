using ECommercePlatform.Modules.Identity.Application.Dtos;

namespace ECommercePlatform.Modules.Identity.Application.Abstractions;

public interface IUserReadRepository
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
