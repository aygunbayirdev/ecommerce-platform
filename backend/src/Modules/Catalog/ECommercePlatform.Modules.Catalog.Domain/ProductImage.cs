using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Domain;

public sealed class ProductImage : BaseEntity
{
    private ProductImage()
    {
    }

    public Guid ProductId { get; private set; }

    public string Url { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public int DisplayOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    internal static ProductImage Create(Guid productId, string url, bool isPrimary, int displayOrder)
    {
        return new ProductImage
        {
            ProductId = Guard.AgainstEmpty(productId, nameof(productId)),
            Url = Guard.AgainstNullOrWhiteSpace(url, nameof(url)),
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    internal void UnmarkAsPrimary() => IsPrimary = false;

    internal void MarkAsPrimary() => IsPrimary = true;
}
