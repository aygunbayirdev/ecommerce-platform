using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Domain;
using ECommercePlatform.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Identity.Infrastructure.Repositories;

internal sealed class RefreshTokenWriteRepository(IdentityDbContext dbContext) : IRefreshTokenWriteRepository
{
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        => dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);

    public void Add(RefreshToken refreshToken) => dbContext.RefreshTokens.Add(refreshToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
