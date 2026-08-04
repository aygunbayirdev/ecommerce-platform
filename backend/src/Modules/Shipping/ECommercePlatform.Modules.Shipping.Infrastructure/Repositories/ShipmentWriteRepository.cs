using ECommercePlatform.Modules.Shipping.Application.Abstractions;
using ECommercePlatform.Modules.Shipping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Shipping.Infrastructure.Repositories;

internal sealed class ShipmentWriteRepository(ShippingDbContext dbContext) : IShipmentWriteRepository
{
    public Task<Domain.Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Shipments.Include(s => s.StatusHistory).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Domain.Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        => dbContext.Shipments.Include(s => s.StatusHistory).FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);

    public Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        => dbContext.Shipments.AnyAsync(s => s.OrderId == orderId, cancellationToken);

    public void Add(Domain.Shipment shipment) => dbContext.Shipments.Add(shipment);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
