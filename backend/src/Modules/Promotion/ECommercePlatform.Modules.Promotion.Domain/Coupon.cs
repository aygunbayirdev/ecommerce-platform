using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Promotion.Domain;

public sealed class Coupon : BaseEntity
{
    private readonly List<CouponRedemption> _redemptions = [];

    private Coupon()
    {
    }

    public string Code { get; private set; } = string.Empty;

    public CouponDiscountType DiscountType { get; private set; }

    public decimal DiscountValue { get; private set; }

    public DateTime ValidFrom { get; private set; }

    public DateTime ValidTo { get; private set; }

    public int? UsageLimit { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<CouponRedemption> Redemptions => _redemptions.AsReadOnly();

    public static Coupon Create(
        string code,
        CouponDiscountType discountType,
        decimal discountValue,
        DateTime validFrom,
        DateTime validTo,
        int? usageLimit)
    {
        return new Coupon
        {
            Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code)).ToUpperInvariant(),
            DiscountType = discountType,
            DiscountValue = discountValue,
            ValidFrom = validFrom,
            ValidTo = validTo,
            UsageLimit = usageLimit,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(Error.Conflict("Coupons.AlreadyInactive", "Kupon zaten pasif durumda."));
        }

        IsActive = false;

        return Result.Success();
    }

    public Result Reactivate()
    {
        if (IsActive)
        {
            return Result.Failure(Error.Conflict("Coupons.AlreadyActive", "Kupon zaten aktif durumda."));
        }

        IsActive = true;

        return Result.Success();
    }

    /// <summary>
    /// Pure business-rule check: does NOT mutate state or record a redemption. Used both as the
    /// first step of <see cref="Redeem"/> and, on its own, by anything that only needs to know
    /// "would this coupon apply right now" without spending a usage slot.
    /// </summary>
    public Result<decimal> CalculateDiscount(decimal subtotal)
    {
        if (!IsActive)
        {
            return Result.Failure<decimal>(Error.Conflict("Coupons.Inactive", "Kupon aktif değil."));
        }

        var now = DateTime.UtcNow;

        if (now < ValidFrom || now > ValidTo)
        {
            return Result.Failure<decimal>(Error.Conflict("Coupons.OutOfValidityPeriod", "Kupon geçerlilik tarihi dışında."));
        }

        if (UsageLimit is not null && ActiveRedemptionCount() >= UsageLimit)
        {
            return Result.Failure<decimal>(Error.Conflict("Coupons.UsageLimitReached", "Kupon kullanım limitine ulaşıldı."));
        }

        var discount = DiscountType == CouponDiscountType.Percentage
            ? subtotal * DiscountValue / 100m
            : DiscountValue;

        // Never let a coupon push the order total below zero.
        discount = Math.Min(discount, subtotal);

        return Result.Success(discount);
    }

    public Result<decimal> Redeem(Guid userId, Guid orderId, decimal subtotal)
    {
        var discountResult = CalculateDiscount(subtotal);

        if (discountResult.IsFailure)
        {
            return discountResult;
        }

        _redemptions.Add(CouponRedemption.Create(Id, userId, orderId, discountResult.Value));

        return discountResult;
    }

    /// <summary>
    /// Idempotent by design: called unconditionally by order cancellation regardless of whether
    /// that order ever used a coupon (mirrors Inventory's ReleaseStockCommand being safe to call
    /// even when nothing was reserved).
    /// </summary>
    public Result ReleaseRedemption(Guid orderId)
    {
        var redemption = _redemptions.FirstOrDefault(r => r.OrderId == orderId && r.ReleasedAtUtc is null);

        redemption?.Release();

        return Result.Success();
    }

    private int ActiveRedemptionCount() => _redemptions.Count(r => r.ReleasedAtUtc is null);
}
