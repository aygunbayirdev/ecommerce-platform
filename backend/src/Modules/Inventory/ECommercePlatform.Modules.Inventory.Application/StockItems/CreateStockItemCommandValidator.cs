using FluentValidation;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

public sealed class CreateStockItemCommandValidator : AbstractValidator<CreateStockItemCommand>
{
    public CreateStockItemCommandValidator()
    {
        RuleFor(x => x.ProductVariantId).NotEmpty();
    }
}
