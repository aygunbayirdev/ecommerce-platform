using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Identity.Application.Users;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber) : ICommand<Guid>;
