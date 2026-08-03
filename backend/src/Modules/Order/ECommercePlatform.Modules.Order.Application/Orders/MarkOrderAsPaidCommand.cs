using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed record MarkOrderAsPaidCommand(Guid OrderId) : ICommand;
