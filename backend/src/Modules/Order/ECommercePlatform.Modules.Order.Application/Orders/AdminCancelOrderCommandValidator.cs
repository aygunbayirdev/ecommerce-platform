using FluentValidation;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed class AdminCancelOrderCommandValidator : AbstractValidator<AdminCancelOrderCommand>
{
    public AdminCancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
