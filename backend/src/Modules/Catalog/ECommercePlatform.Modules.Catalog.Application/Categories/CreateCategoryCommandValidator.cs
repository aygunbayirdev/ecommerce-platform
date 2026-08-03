using FluentValidation;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
