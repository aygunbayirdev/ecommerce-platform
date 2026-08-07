using FluentValidation;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed class RemoveProductImageCommandValidator : AbstractValidator<RemoveProductImageCommand>
{
    public RemoveProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductImageId).NotEmpty();
    }
}
