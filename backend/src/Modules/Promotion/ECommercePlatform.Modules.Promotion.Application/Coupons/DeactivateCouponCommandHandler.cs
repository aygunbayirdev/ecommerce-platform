using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Promotion.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class DeactivateCouponCommandHandler(ICouponWriteRepository couponWriteRepository)
    : ICommandHandler<DeactivateCouponCommand>
{
    public async Task<Result> Handle(DeactivateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await couponWriteRepository.GetByIdWithRedemptionsAsync(request.CouponId, cancellationToken);

        if (coupon is null)
        {
            return Result.Failure(Error.NotFound("Coupons.NotFound", "Kupon bulunamadı."));
        }

        var result = coupon.Deactivate();

        if (result.IsFailure)
        {
            return result;
        }

        await couponWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
