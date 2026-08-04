using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Promotion.Domain;

public sealed class CouponRedemption : BaseEntity
{
    private CouponRedemption()
    {
    }

    public Guid CouponId { get; private set; }

    /// <summary>Cross-module reference to Identity.User — no DB foreign key by design (modular monolith: no cross-schema FKs).</summary>
    public Guid UserId { get; private set; }

    /// <summary>Cross-module reference to Order.Order — no DB foreign key by design.</summary>
    public Guid OrderId { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public DateTime RedeemedAtUtc { get; private set; }

    public DateTime? ReleasedAtUtc { get; private set; }

    internal static CouponRedemption Create(Guid couponId, Guid userId, Guid orderId, decimal discountAmount)
    {
        return new CouponRedemption
        {
            CouponId = Guard.AgainstEmpty(couponId, nameof(couponId)),
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            OrderId = Guard.AgainstEmpty(orderId, nameof(orderId)),
            DiscountAmount = discountAmount,
            RedeemedAtUtc = DateTime.UtcNow,
        };
    }

    internal void Release()
    {
        ReleasedAtUtc = DateTime.UtcNow;
    }
}
