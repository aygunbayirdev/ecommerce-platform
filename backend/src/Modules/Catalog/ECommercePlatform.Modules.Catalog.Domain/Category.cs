using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Domain;

public sealed class Category : BaseEntity
{
    private readonly List<CategoryAttribute> _attributes = [];

    private Category()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public Guid? ParentCategoryId { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<CategoryAttribute> Attributes => _attributes.AsReadOnly();

    public static Category Create(string name, Guid? parentCategoryId, int displayOrder)
    {
        return new Category
        {
            Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name)),
            ParentCategoryId = parentCategoryId,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void Rename(string name)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public Result AssignAttribute(Guid productAttributeId)
    {
        if (_attributes.Any(a => a.ProductAttributeId == productAttributeId))
        {
            return Result.Failure(Error.Conflict(
                "Categories.AttributeAlreadyAssigned",
                "Bu özellik zaten bu kategoriye atanmış."));
        }

        _attributes.Add(CategoryAttribute.Create(Id, productAttributeId));

        return Result.Success();
    }

    public Result RemoveAttribute(Guid productAttributeId)
    {
        var attribute = _attributes.FirstOrDefault(a => a.ProductAttributeId == productAttributeId);

        if (attribute is null)
        {
            return Result.Failure(Error.NotFound(
                "Categories.AttributeNotAssigned",
                "Bu özellik bu kategoriye atanmamış."));
        }

        _attributes.Remove(attribute);

        return Result.Success();
    }
}
