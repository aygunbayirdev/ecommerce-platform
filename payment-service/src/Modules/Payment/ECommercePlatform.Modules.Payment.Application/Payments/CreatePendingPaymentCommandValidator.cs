using FluentValidation;

namespace ECommercePlatform.Modules.Payment.Application.Payments;

public sealed class CreatePendingPaymentCommandValidator : AbstractValidator<CreatePendingPaymentCommand>
{
    public CreatePendingPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
