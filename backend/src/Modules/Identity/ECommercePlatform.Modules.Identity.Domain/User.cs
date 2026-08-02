using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Domain;

public sealed class User : BaseEntity
{
    private readonly List<Address> _addresses = [];

    private User()
    {
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    public static User Register(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string? phoneNumber,
        UserRole role = UserRole.Customer)
    {
        Guard.AgainstNullOrWhiteSpace(email, nameof(email));
        Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        Guard.AgainstNullOrWhiteSpace(firstName, nameof(firstName));
        Guard.AgainstNullOrWhiteSpace(lastName, nameof(lastName));

        var user = new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = phoneNumber,
            Role = role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };

        user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id, user.Email));

        return user;
    }

    public void ChangePasswordHash(string newPasswordHash)
    {
        PasswordHash = Guard.AgainstNullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
