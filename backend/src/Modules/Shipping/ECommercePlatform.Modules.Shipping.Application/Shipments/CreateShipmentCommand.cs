using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed record CreateShipmentCommand(Guid OrderId, string Carrier, string TrackingNumber) : ICommand<Guid>;
