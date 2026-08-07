using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Order.Application.Dtos;
using ECommercePlatform.Modules.Order.Domain;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed record GetAllOrdersForAdminQuery(OrderStatus? Status, int PageNumber, int PageSize)
    : IQuery<PagedResult<OrderSummaryDto>>;
