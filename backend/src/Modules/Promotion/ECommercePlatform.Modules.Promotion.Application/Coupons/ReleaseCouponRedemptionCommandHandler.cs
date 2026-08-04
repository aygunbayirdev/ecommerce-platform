using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Promotion.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class ReleaseCouponRedemptionCommandHandler(ICouponWriteRepository couponWriteRepository)
    : ICommandHandler<ReleaseCouponRedemptionCommand>
{
    public async Task<Result> Handle(ReleaseCouponRedemptionCommand request, CancellationToken cancellationToken)
    {
        var coupon = await couponWriteRepository.GetByRedeemedOrderIdAsync(request.OrderId, cancellationToken);

        if (coupon is null)
        {
            return Result.Success();
        }

        coupon.ReleaseRedemption(request.OrderId);
        await couponWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
