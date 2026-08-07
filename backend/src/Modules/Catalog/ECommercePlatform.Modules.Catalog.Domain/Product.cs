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

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(Error.Conflict("Products.AlreadyInactive", "Ürün zaten pasif durumda."));
        }

        IsActive = false;

        return Result.Success();
    }

    public Result Reactivate()
    {
        if (IsActive)
        {
            return Result.Failure(Error.Conflict("Products.AlreadyActive", "Ürün zaten aktif durumda."));
        }

        IsActive = true;

        return Result.Success();
    }

    public Result DeactivateVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);

        if (variant is null)
        {
            return Result.Failure(Error.NotFound("Products.VariantNotFound", "Varyant bulunamadı."));
        }

        if (!variant.IsActive)
        {
            return Result.Failure(Error.Conflict("Products.VariantAlreadyInactive", "Varyant zaten pasif durumda."));
        }

        variant.Deactivate();

        return Result.Success();
    }

    public Result ReactivateVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);

        if (variant is null)
        {
            return Result.Failure(Error.NotFound("Products.VariantNotFound", "Varyant bulunamadı."));
        }

        if (variant.IsActive)
        {
            return Result.Failure(Error.Conflict("Products.VariantAlreadyActive", "Varyant zaten aktif durumda."));
        }

        variant.Reactivate();

        return Result.Success();
    }

    public Result RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);

        if (image is null)
        {
            return Result.Failure(Error.NotFound("Products.ImageNotFound", "Görsel bulunamadı."));
        }

        var wasPrimary = image.IsPrimary;
        _images.Remove(image);

        // Preserve AddImage's invariant: if any images remain, exactly one must be primary.
        if (wasPrimary && _images.Count > 0 && !_images.Any(i => i.IsPrimary))
        {
            _images.OrderBy(i => i.DisplayOrder).First().MarkAsPrimary();
        }

        return Result.Success();
    }
}
