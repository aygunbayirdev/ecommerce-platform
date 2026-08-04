using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed record MarkShipmentInTransitCommand(Guid ShipmentId) : ICommand;
