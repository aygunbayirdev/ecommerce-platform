using ECommercePlatform.Modules.Promotion.Domain;

namespace ECommercePlatform.Modules.Promotion.Application.Dtos;

public sealed record CouponDto(
    Guid Id,
    string Code,
    CouponDiscountType DiscountType,
    decimal DiscountValue,
    DateTime ValidFrom,
    DateTime ValidTo,
    int? UsageLimit,
    int UsedCount,
    bool IsActive,
    DateTime CreatedAtUtc);
