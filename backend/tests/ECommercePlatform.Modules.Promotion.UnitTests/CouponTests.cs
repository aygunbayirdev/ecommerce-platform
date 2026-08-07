using ECommercePlatform.Modules.Promotion.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Promotion.UnitTests;

public sealed class CouponTests
{
    private static Coupon CreateCoupon(
        CouponDiscountType discountType = CouponDiscountType.Percentage,
        decimal discountValue = 10m,
        int? usageLimit = null,
        DateTime? validFrom = null,
        DateTime? validTo = null)
    {
        return Coupon.Create(
            "SAVE10",
            discountType,
            discountValue,
            validFrom ?? DateTime.UtcNow.AddDays(-1),
            validTo ?? DateTime.UtcNow.AddDays(1),
            usageLimit);
    }

    [Fact]
    public void CalculateDiscount_ShouldComputePercentageDiscount()
    {
        var coupon = CreateCoupon(CouponDiscountType.Percentage, 10m);

        var result = coupon.CalculateDiscount(200m);

        Assert.True(result.IsSuccess);
        Assert.Equal(20m, result.Value);
    }

    [Fact]
    public void CalculateDiscount_ShouldComputeFixedAmountDiscount()
    {
        var coupon = CreateCoupon(CouponDiscountType.FixedAmount, 30m);

        var result = coupon.CalculateDiscount(200m);

        Assert.True(result.IsSuccess);
        Assert.Equal(30m, result.Value);
    }

    [Fact]
    public void CalculateDiscount_ShouldClampToSubtotal_WhenDiscountExceedsIt()
    {
        var coupon = CreateCoupon(CouponDiscountType.FixedAmount, 500m);

        var result = coupon.CalculateDiscount(200m);

        Assert.True(result.IsSuccess);
        Assert.Equal(200m, result.Value);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnConflict_WhenInactive()
    {
        var coupon = CreateCoupon();
        coupon.Deactivate();

        var result = coupon.CalculateDiscount(200m);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnConflict_WhenOutsideValidityPeriod()
    {
        var coupon = CreateCoupon(validFrom: DateTime.UtcNow.AddDays(-10), validTo: DateTime.UtcNow.AddDays(-1));

        var result = coupon.CalculateDiscount(200m);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnConflict_WhenUsageLimitReached()
    {
        var coupon = CreateCoupon(usageLimit: 1);
        coupon.Redeem(Guid.NewGuid(), Guid.NewGuid(), 100m);

        var result = coupon.CalculateDiscount(100m);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Redeem_ShouldRecordRedemption()
    {
        var coupon = CreateCoupon();
        var orderId = Guid.NewGuid();

        var result = coupon.Redeem(Guid.NewGuid(), orderId, 200m);

        Assert.True(result.IsSuccess);
        var redemption = Assert.Single(coupon.Redemptions);
        Assert.Equal(orderId, redemption.OrderId);
        Assert.Null(redemption.ReleasedAtUtc);
    }

    [Fact]
    public void ReleaseRedemption_ShouldFreeUpUsageSlot_AllowingReRedemption()
    {
        var coupon = CreateCoupon(usageLimit: 1);
        var orderId = Guid.NewGuid();
        coupon.Redeem(Guid.NewGuid(), orderId, 100m);

        var releaseResult = coupon.ReleaseRedemption(orderId);
        var redeemAgainResult = coupon.Redeem(Guid.NewGuid(), Guid.NewGuid(), 100m);

        Assert.True(releaseResult.IsSuccess);
        Assert.True(redeemAgainResult.IsSuccess);
        Assert.NotNull(coupon.Redemptions.First(r => r.OrderId == orderId).ReleasedAtUtc);
    }

    [Fact]
    public void ReleaseRedemption_ShouldBeNoOp_WhenOrderNeverRedeemedThisCoupon()
    {
        var coupon = CreateCoupon();

        var result = coupon.ReleaseRedemption(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Empty(coupon.Redemptions);
    }

    [Fact]
    public void Reactivate_ShouldSucceed_WhenCouponIsInactive()
    {
        var coupon = CreateCoupon();
        coupon.Deactivate();

        var result = coupon.Reactivate();

        Assert.True(result.IsSuccess);
        Assert.True(coupon.IsActive);
    }

    [Fact]
    public void Reactivate_ShouldReturnConflict_WhenCouponAlreadyActive()
    {
        var coupon = CreateCoupon();

        var result = coupon.Reactivate();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
