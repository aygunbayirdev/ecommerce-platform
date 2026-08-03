using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Domain;

public sealed class Product : BaseEntity
{
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductImage> _images = [];

    private Product()
    {
    }

    public Guid CategoryId { get; private set; }

    public Guid? BrandId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    public static Product Create(Guid categoryId, Guid? brandId, string name, string description)
    {
        return new Product
        {
            CategoryId = Guard.AgainstEmpty(categoryId, nameof(categoryId)),
            BrandId = brandId,
            Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name)),
            Description = description ?? string.Empty,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public ProductVariant AddVariant(string sku, decimal price, IReadOnlyDictionary<Guid, string> attributeValues)
    {
        var variant = ProductVariant.Create(Id, sku, price);

        foreach (var (attributeId, value) in attributeValues)
        {
            variant.SetAttributeValue(attributeId, value);
        }

        _variants.Add(variant);

        return variant;
    }

    public ProductImage AddImage(string url, bool isPrimary)
    {
        var shouldBePrimary = isPrimary || _images.Count == 0;

        if (shouldBePrimary)
        {
            foreach (var image in _images.Where(i => i.IsPrimary))
            {
                image.UnmarkAsPrimary();
            }
        }

        var newImage = ProductImage.Create(Id, url, shouldBePrimary, _images.Count);
        _images.Add(newImage);

        return newImage;
    }
}
