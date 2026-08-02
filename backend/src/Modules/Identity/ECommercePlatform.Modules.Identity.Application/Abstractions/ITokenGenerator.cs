using ECommercePlatform.Modules.Identity.Domain;

namespace ECommercePlatform.Modules.Identity.Application.Abstractions;

public sealed record AccessTokenResult(string AccessToken, DateTime ExpiresAtUtc);

public sealed record RefreshTokenResult(string Token, DateTime ExpiresAtUtc);

public interface ITokenGenerator
{
    AccessTokenResult GenerateAccessToken(User user);

    RefreshTokenResult GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
