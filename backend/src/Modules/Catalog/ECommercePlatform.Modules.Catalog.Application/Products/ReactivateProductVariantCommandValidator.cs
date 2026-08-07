using FluentValidation;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class ReactivateProductVariantCommandValidator : AbstractValidator<ReactivateProductVariantCommand>
{
    public ReactivateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductVariantId).NotEmpty();
    }
}
