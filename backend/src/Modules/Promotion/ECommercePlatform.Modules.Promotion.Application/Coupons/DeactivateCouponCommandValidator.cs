using FluentValidation;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class DeactivateCouponCommandValidator : AbstractValidator<DeactivateCouponCommand>
{
    public DeactivateCouponCommandValidator()
    {
        RuleFor(x => x.CouponId).NotEmpty();
    }
}
