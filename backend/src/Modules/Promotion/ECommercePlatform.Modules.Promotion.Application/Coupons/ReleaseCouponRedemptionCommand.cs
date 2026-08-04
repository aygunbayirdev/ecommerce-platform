using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

/// <summary>Internal — called unconditionally by Order's cancellation flow (mirrors Inventory's ReleaseStockCommand); a no-op if the order never redeemed a coupon.</summary>
public sealed record ReleaseCouponRedemptionCommand(Guid OrderId) : ICommand;
