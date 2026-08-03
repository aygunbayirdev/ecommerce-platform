using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Domain;

public sealed class CategoryAttribute : BaseEntity
{
    private CategoryAttribute()
    {
    }

    public Guid CategoryId { get; private set; }

    public Guid ProductAttributeId { get; private set; }

    /// <summary>
    /// CategoryAttribute is a child entity of the Category aggregate — only Category should call
    /// this, so the "no duplicate attribute assignment" invariant stays enforceable in one place.
    /// </summary>
    internal static CategoryAttribute Create(Guid categoryId, Guid productAttributeId)
    {
        return new CategoryAttribute
        {
            CategoryId = Guard.AgainstEmpty(categoryId, nameof(categoryId)),
            ProductAttributeId = Guard.AgainstEmpty(productAttributeId, nameof(productAttributeId)),
        };
    }
}
