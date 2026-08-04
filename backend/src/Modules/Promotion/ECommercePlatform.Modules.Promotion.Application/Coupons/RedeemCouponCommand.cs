using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

/// <summary>Internal — not exposed via any controller, only called by Order's checkout orchestration through ISender (mirrors Inventory's ReserveStockCommand).</summary>
public sealed record RedeemCouponCommand(string Code, Guid UserId, Guid OrderId, decimal Subtotal) : ICommand<decimal>;
