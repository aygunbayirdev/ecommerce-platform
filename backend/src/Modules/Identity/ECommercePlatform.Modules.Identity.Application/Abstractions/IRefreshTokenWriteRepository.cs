using ECommercePlatform.Modules.Identity.Domain;

namespace ECommercePlatform.Modules.Identity.Application.Abstractions;

public interface IRefreshTokenWriteRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    void Add(RefreshToken refreshToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
