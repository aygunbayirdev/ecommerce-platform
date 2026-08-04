using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed record MarkShipmentDeliveredCommand(Guid ShipmentId) : ICommand;
