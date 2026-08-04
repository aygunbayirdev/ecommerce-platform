using ECommercePlatform.Modules.Promotion.Domain;
using FluentValidation;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.DiscountValue).GreaterThan(0);
        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == CouponDiscountType.Percentage)
            .WithMessage("Yüzdesel indirim 100'den büyük olamaz.");
        RuleFor(x => x.ValidTo).GreaterThan(x => x.ValidFrom);
        RuleFor(x => x.UsageLimit).GreaterThan(0).When(x => x.UsageLimit is not null);
    }
}
