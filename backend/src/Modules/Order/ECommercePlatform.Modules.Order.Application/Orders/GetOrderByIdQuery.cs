using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Order.Application.Dtos;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDetailDto>;
