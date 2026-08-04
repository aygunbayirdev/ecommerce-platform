using FluentValidation;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class ReleaseCouponRedemptionCommandValidator : AbstractValidator<ReleaseCouponRedemptionCommand>
{
    public ReleaseCouponRedemptionCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
