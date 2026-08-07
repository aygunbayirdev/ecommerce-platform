using FluentValidation;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class DeactivateProductVariantCommandValidator : AbstractValidator<DeactivateProductVariantCommand>
{
    public DeactivateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductVariantId).NotEmpty();
    }
}
