using FluentValidation;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class ReactivateProductCommandValidator : AbstractValidator<ReactivateProductCommand>
{
    public ReactivateProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
