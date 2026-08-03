using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Order.Application.Dtos;

namespace ECommercePlatform.Modules.Order.Application.Abstractions;

public interface IOrderReadRepository
{
    Task<OrderDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<OrderSummaryDto>> GetByUserIdAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
