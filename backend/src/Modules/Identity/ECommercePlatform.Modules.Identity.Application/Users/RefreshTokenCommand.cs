using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Identity.Application.Users;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResult>;
