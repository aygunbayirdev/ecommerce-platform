using FluentValidation;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class AddProductVariantCommandValidator : AbstractValidator<AddProductVariantCommand>
{
    public AddProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Price).GreaterThan(0);

        RuleForEach(x => x.AttributeValues).ChildRules(input =>
        {
            input.RuleFor(i => i.ProductAttributeId).NotEmpty();
            input.RuleFor(i => i.Value).NotEmpty().MaximumLength(200);
        });
    }
}
