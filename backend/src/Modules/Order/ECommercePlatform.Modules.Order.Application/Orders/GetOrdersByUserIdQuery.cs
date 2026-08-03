using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Order.Application.Dtos;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed record GetOrdersByUserIdQuery(Guid UserId, int PageNumber, int PageSize) : IQuery<PagedResult<OrderSummaryDto>>;
