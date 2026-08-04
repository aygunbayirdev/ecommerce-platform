using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed record MarkShipmentFailedCommand(Guid ShipmentId, string Reason) : ICommand;
