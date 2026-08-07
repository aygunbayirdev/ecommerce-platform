using FluentValidation;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class ReactivateCouponCommandValidator : AbstractValidator<ReactivateCouponCommand>
{
    public ReactivateCouponCommandValidator()
    {
        RuleFor(x => x.CouponId).NotEmpty();
    }
}
