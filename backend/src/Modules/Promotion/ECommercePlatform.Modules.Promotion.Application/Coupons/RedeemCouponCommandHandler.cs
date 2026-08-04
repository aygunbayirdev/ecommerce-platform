using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Promotion.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class RedeemCouponCommandHandler(ICouponWriteRepository couponWriteRepository)
    : ICommandHandler<RedeemCouponCommand, decimal>
{
    public async Task<Result<decimal>> Handle(RedeemCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await couponWriteRepository.GetByCodeWithRedemptionsAsync(request.Code, cancellationToken);

        if (coupon is null)
        {
            return Result.Failure<decimal>(Error.NotFound("Coupons.NotFound", "Kupon kodu bulunamadı."));
        }

        var redeemResult = coupon.Redeem(request.UserId, request.OrderId, request.Subtotal);

        if (redeemResult.IsFailure)
        {
            return redeemResult;
        }

        await couponWriteRepository.SaveChangesAsync(cancellationToken);

        return redeemResult;
    }
}
