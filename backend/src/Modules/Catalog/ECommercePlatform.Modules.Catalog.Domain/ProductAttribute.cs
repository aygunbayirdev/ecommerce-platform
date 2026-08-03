using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Domain;

public sealed class ProductAttribute : BaseEntity
{
    private ProductAttribute()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static ProductAttribute Create(string name)
    {
        return new ProductAttribute
        {
            Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name)),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
