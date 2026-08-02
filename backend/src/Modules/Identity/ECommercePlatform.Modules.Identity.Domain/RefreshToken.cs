using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Domain;

public sealed class RefreshToken : BaseEntity
{
    private RefreshToken()
    {
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

    public static RefreshToken Issue(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        return new RefreshToken
        {
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            TokenHash = Guard.AgainstNullOrWhiteSpace(tokenHash, nameof(tokenHash)),
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void Revoke()
    {
        RevokedAtUtc = DateTime.UtcNow;
    }
}
