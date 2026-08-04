using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Shipping.Application.Dtos;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed record GetShipmentByOrderIdQuery(Guid OrderId) : IQuery<ShipmentDto>;
