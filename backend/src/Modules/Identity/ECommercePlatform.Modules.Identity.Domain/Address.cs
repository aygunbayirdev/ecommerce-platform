using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Domain;

public sealed class Address : BaseEntity
{
    private Address()
    {
    }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string RecipientName { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string District { get; private set; } = string.Empty;

    public string FullAddressLine { get; private set; } = string.Empty;

    public string PostalCode { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Address is a child entity of the User aggregate — only User should call this (and the
    /// mutation methods below), so the "at most one default address" invariant stays enforceable
    /// in one place. Internal keeps the Application layer from bypassing User and mutating directly.
    /// </summary>
    internal static Address Create(
        Guid userId,
        string title,
        string recipientName,
        string phoneNumber,
        string city,
        string district,
        string fullAddressLine,
        string postalCode)
    {
        return new Address
        {
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title)),
            RecipientName = Guard.AgainstNullOrWhiteSpace(recipientName, nameof(recipientName)),
            PhoneNumber = Guard.AgainstNullOrWhiteSpace(phoneNumber, nameof(phoneNumber)),
            City = Guard.AgainstNullOrWhiteSpace(city, nameof(city)),
            District = Guard.AgainstNullOrWhiteSpace(district, nameof(district)),
            FullAddressLine = Guard.AgainstNullOrWhiteSpace(fullAddressLine, nameof(fullAddressLine)),
            PostalCode = Guard.AgainstNullOrWhiteSpace(postalCode, nameof(postalCode)),
            IsDefault = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    internal void UpdateDetails(
        string title,
        string recipientName,
        string phoneNumber,
        string city,
        string district,
        string fullAddressLine,
        string postalCode)
    {
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title));
        RecipientName = Guard.AgainstNullOrWhiteSpace(recipientName, nameof(recipientName));
        PhoneNumber = Guard.AgainstNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));
        City = Guard.AgainstNullOrWhiteSpace(city, nameof(city));
        District = Guard.AgainstNullOrWhiteSpace(district, nameof(district));
        FullAddressLine = Guard.AgainstNullOrWhiteSpace(fullAddressLine, nameof(fullAddressLine));
        PostalCode = Guard.AgainstNullOrWhiteSpace(postalCode, nameof(postalCode));
    }

    internal void MarkAsDefault() => IsDefault = true;

    internal void UnmarkAsDefault() => IsDefault = false;
}
