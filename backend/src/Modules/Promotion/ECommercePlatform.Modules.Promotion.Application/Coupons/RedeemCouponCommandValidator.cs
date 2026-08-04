using FluentValidation;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class RedeemCouponCommandValidator : AbstractValidator<RedeemCouponCommand>
{
    public RedeemCouponCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Subtotal).GreaterThan(0);
    }
}
