using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Shipping.Infrastructure.Persistence;

public sealed class ShippingDbContext(DbContextOptions<ShippingDbContext> options) : DbContext(options)
{
    public const string Schema = "shipping";

    public DbSet<Domain.Shipment> Shipments => Set<Domain.Shipment>();

    public DbSet<Domain.ShipmentStatusHistory> ShipmentStatusHistory => Set<Domain.ShipmentStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShippingDbContext).Assembly);
        modelBuilder.ApplyClientGeneratedKeys();

        base.OnModelCreating(modelBuilder);
    }
}
