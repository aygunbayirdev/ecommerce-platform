using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed record CreateOrderCommand(Guid UserId, Guid AddressId) : ICommand<Guid>;
