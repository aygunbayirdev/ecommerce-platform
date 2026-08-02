using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Application.Users;

public sealed class LoginCommandHandler(
    IUserWriteRepository userWriteRepository,
    IRefreshTokenWriteRepository refreshTokenWriteRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator) : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userWriteRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResult>(Error.Unauthorized("Users.InvalidCredentials", "E-posta veya şifre hatalı."));
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResult>(Error.Unauthorized("Users.Deactivated", "Hesap devre dışı bırakılmış."));
        }

        var accessToken = tokenGenerator.GenerateAccessToken(user);
        var refreshToken = tokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = tokenGenerator.HashRefreshToken(refreshToken.Token);

        var refreshTokenEntity = RefreshToken.Issue(user.Id, refreshTokenHash, refreshToken.ExpiresAtUtc);
        refreshTokenWriteRepository.Add(refreshTokenEntity);
        await refreshTokenWriteRepository.SaveChangesAsync(cancellationToken);

        return new LoginResult(user.Id, accessToken.AccessToken, accessToken.ExpiresAtUtc, refreshToken.Token);
    }
}
