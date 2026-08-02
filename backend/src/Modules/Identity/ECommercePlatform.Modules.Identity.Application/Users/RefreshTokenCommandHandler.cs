using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Application.Users;

public sealed class RefreshTokenCommandHandler(
    IUserWriteRepository userWriteRepository,
    IRefreshTokenWriteRepository refreshTokenWriteRepository,
    ITokenGenerator tokenGenerator) : ICommandHandler<RefreshTokenCommand, LoginResult>
{
    public async Task<Result<LoginResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenGenerator.HashRefreshToken(request.RefreshToken);

        var existingToken = await refreshTokenWriteRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result.Failure<LoginResult>(
                Error.Unauthorized("Users.InvalidRefreshToken", "Refresh token geçersiz veya süresi dolmuş."));
        }

        var user = await userWriteRepository.GetByIdAsync(existingToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Result.Failure<LoginResult>(
                Error.Unauthorized("Users.InvalidRefreshToken", "Kullanıcı bulunamadı veya devre dışı."));
        }

        existingToken.Revoke();

        var accessToken = tokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = tokenGenerator.GenerateRefreshToken();
        var newRefreshTokenHash = tokenGenerator.HashRefreshToken(newRefreshToken.Token);

        var newRefreshTokenEntity = RefreshToken.Issue(user.Id, newRefreshTokenHash, newRefreshToken.ExpiresAtUtc);
        refreshTokenWriteRepository.Add(newRefreshTokenEntity);
        await refreshTokenWriteRepository.SaveChangesAsync(cancellationToken);

        return new LoginResult(user.Id, accessToken.AccessToken, accessToken.ExpiresAtUtc, newRefreshToken.Token);
    }
}
