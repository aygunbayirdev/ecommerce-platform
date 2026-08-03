using FluentValidation;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

public sealed class CommitStockCommandValidator : AbstractValidator<CommitStockCommand>
{
    public CommitStockCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductVariantId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
