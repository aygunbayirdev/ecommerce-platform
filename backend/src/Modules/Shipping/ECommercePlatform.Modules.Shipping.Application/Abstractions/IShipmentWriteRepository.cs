namespace ECommercePlatform.Modules.Shipping.Application.Abstractions;

public interface IShipmentWriteRepository
{
    Task<Domain.Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Domain.Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    void Add(Domain.Shipment shipment);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
