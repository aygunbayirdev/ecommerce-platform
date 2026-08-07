using ECommercePlatform.Modules.Promotion.Application.Abstractions;
using ECommercePlatform.Modules.Promotion.Application.Coupons;
using ECommercePlatform.Modules.Promotion.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Promotion.UnitTests;

public sealed class ReactivateCouponCommandHandlerTests
{
    private readonly Mock<ICouponWriteRepository> _couponWriteRepository = new();
    private readonly ReactivateCouponCommandHandler _handler;

    public ReactivateCouponCommandHandlerTests()
    {
        _handler = new ReactivateCouponCommandHandler(_couponWriteRepository.Object);
    }

    private static Coupon CreateCoupon()
    {
        return Coupon.Create("SAVE10", CouponDiscountType.Percentage, 10m, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenCouponIsInactive()
    {
        var coupon = CreateCoupon();
        coupon.Deactivate();
        _couponWriteRepository
            .Setup(r => r.GetByIdWithRedemptionsAsync(coupon.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _handler.Handle(new ReactivateCouponCommand(coupon.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(coupon.IsActive);
        _couponWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCouponDoesNotExist()
    {
        var couponId = Guid.NewGuid();
        _couponWriteRepository
            .Setup(r => r.GetByIdWithRedemptionsAsync(couponId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Coupon?)null);

        var result = await _handler.Handle(new ReactivateCouponCommand(couponId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenCouponAlreadyActive()
    {
        var coupon = CreateCoupon();
        _couponWriteRepository
            .Setup(r => r.GetByIdWithRedemptionsAsync(coupon.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _handler.Handle(new ReactivateCouponCommand(coupon.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _couponWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
