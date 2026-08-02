namespace ECommercePlatform.Modules.Identity.Application.Dtos;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc);
