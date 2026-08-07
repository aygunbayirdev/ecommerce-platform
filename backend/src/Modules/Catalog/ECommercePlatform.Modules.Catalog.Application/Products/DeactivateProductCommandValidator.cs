using FluentValidation;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class DeactivateProductCommandValidator : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
