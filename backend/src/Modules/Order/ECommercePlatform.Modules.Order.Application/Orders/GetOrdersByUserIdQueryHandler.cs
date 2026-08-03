using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed class GetOrdersByUserIdQueryHandler(IOrderReadRepository orderReadRepository)
    : IQueryHandler<GetOrdersByUserIdQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(
        GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        var result = await orderReadRepository.GetByUserIdAsync(
            request.UserId, request.PageNumber, request.PageSize, cancellationToken);

        return Result.Success(result);
    }
}
