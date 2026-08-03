using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Domain;

public sealed class Brand : BaseEntity
{
    private Brand()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAtUtc { get; private set; }

    public static Brand Create(string name)
    {
        return new Brand
        {
            Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name)),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void Rename(string name)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
    }
}
