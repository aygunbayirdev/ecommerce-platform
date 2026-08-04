using ECommercePlatform.Modules.Promotion.Domain;

namespace ECommercePlatform.Modules.Promotion.Application.Abstractions;

public interface ICouponWriteRepository
{
    Task<Coupon?> GetByIdWithRedemptionsAsync(Guid id, CancellationToken cancellationToken);

    Task<Coupon?> GetByCodeWithRedemptionsAsync(string code, CancellationToken cancellationToken);

    /// <summary>Used by ReleaseCouponRedemptionCommand — an order cancellation only knows its OrderId, not which coupon (if any) it redeemed.</summary>
    Task<Coupon?> GetByRedeemedOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken);

    void Add(Coupon coupon);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
