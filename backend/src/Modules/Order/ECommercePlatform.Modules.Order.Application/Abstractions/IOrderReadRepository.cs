using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Order.Application.Dtos;
using ECommercePlatform.Modules.Order.Domain;

namespace ECommercePlatform.Modules.Order.Application.Abstractions;

public interface IOrderReadRepository
{
    Task<OrderDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<OrderSummaryDto>> GetByUserIdAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Admin listing — not scoped to a single user, unlike GetByUserIdAsync.</summary>
    Task<PagedResult<OrderSummaryDto>> GetAllAsync(
        OrderStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
