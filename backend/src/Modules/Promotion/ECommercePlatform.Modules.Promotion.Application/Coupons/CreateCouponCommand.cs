using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Promotion.Domain;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed record CreateCouponCommand(
    string Code,
    CouponDiscountType DiscountType,
    decimal DiscountValue,
    DateTime ValidFrom,
    DateTime ValidTo,
    int? UsageLimit) : ICommand<Guid>;
