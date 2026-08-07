using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed record ReactivateCouponCommand(Guid CouponId) : ICommand;
