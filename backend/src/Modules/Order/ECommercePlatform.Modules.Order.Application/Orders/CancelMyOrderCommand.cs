using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed record CancelMyOrderCommand(Guid UserId, Guid OrderId, string Reason) : ICommand;
