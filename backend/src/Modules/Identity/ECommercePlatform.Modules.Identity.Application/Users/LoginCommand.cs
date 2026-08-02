using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Identity.Application.Users;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResult>;

public sealed record LoginResult(Guid UserId, string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken);
