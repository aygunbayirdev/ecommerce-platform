using ECommercePlatform.Modules.Shipping.Application.Dtos;

namespace ECommercePlatform.Modules.Shipping.Application.Abstractions;

public interface IShipmentReadRepository
{
    Task<ShipmentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
}
