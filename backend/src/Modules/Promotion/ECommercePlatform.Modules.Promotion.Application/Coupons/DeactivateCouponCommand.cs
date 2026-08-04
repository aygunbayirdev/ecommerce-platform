using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed record DeactivateCouponCommand(Guid CouponId) : ICommand;
