using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.Application.Orders;

public sealed class GetAllOrdersForAdminQueryHandler(IOrderReadRepository orderReadRepository)
    : IQueryHandler<GetAllOrdersForAdminQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(
        GetAllOrdersForAdminQuery request, CancellationToken cancellationToken)
    {
        var result = await orderReadRepository.GetAllAsync(
            request.Status, request.PageNumber, request.PageSize, cancellationToken);

        return Result.Success(result);
    }
}
