using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Promotion.Application.Abstractions;
using ECommercePlatform.Modules.Promotion.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class CreateCouponCommandHandler(ICouponWriteRepository couponWriteRepository)
    : ICommandHandler<CreateCouponCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        if (await couponWriteRepository.ExistsByCodeAsync(request.Code, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Coupons.CodeAlreadyExists", "Bu kupon kodu zaten kullanılıyor."));
        }

        var coupon = Coupon.Create(
            request.Code,
            request.DiscountType,
            request.DiscountValue,
            request.ValidFrom,
            request.ValidTo,
            request.UsageLimit);

        couponWriteRepository.Add(coupon);
        await couponWriteRepository.SaveChangesAsync(cancellationToken);

        return coupon.Id;
    }
}
