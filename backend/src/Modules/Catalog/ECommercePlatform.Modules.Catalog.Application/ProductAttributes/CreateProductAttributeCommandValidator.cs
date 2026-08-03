using FluentValidation;

namespace ECommercePlatform.Modules.Catalog.Application.ProductAttributes;

public sealed class CreateProductAttributeCommandValidator : AbstractValidator<CreateProductAttributeCommand>
{
    public CreateProductAttributeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
