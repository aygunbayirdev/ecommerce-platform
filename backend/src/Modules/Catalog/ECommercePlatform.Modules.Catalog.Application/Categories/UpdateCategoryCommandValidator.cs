using FluentValidation;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
