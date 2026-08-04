using ECommercePlatform.Modules.Promotion.Application.Abstractions;
using ECommercePlatform.Modules.Promotion.Application.Coupons;
using ECommercePlatform.Modules.Promotion.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Promotion.UnitTests;

public sealed class RedeemCouponCommandHandlerTests
{
    private readonly Mock<ICouponWriteRepository> _couponWriteRepository = new();
    private readonly RedeemCouponCommandHandler _handler;

    public RedeemCouponCommandHandlerTests()
    {
        _handler = new RedeemCouponCommandHandler(_couponWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCouponCodeDoesNotExist()
    {
        _couponWriteRepository
            .Setup(r => r.GetByCodeWithRedemptionsAsync("MISSING", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Coupon?)null);

        var command = new RedeemCouponCommand("MISSING", Guid.NewGuid(), Guid.NewGuid(), 100m);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _couponWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRedeemAndSave_OnHappyPath()
    {
        var coupon = Coupon.Create(
            "SAVE10", CouponDiscountType.Percentage, 10m, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null);

        _couponWriteRepository
            .Setup(r => r.GetByCodeWithRedemptionsAsync("SAVE10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var command = new RedeemCouponCommand("SAVE10", Guid.NewGuid(), Guid.NewGuid(), 200m);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(20m, result.Value);
        Assert.Single(coupon.Redemptions);
        _couponWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflictWithoutSaving_WhenCouponIsExpired()
    {
        var coupon = Coupon.Create(
            "OLD", CouponDiscountType.Percentage, 10m, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1), null);

        _couponWriteRepository
            .Setup(r => r.GetByCodeWithRedemptionsAsync("OLD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var command = new RedeemCouponCommand("OLD", Guid.NewGuid(), Guid.NewGuid(), 100m);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _couponWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
