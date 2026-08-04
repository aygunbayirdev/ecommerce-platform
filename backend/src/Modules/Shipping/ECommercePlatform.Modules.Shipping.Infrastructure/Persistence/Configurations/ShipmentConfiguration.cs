using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Shipping.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentConfiguration : IEntityTypeConfiguration<Domain.Shipment>
{
    public void Configure(EntityTypeBuilder<Domain.Shipment> builder)
    {
        builder.ToTable("shipments");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.OrderId).IsRequired();
        builder.HasIndex(s => s.OrderId).IsUnique();
        builder.Property(s => s.Carrier).HasMaxLength(100).IsRequired();
        builder.Property(s => s.TrackingNumber).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Status).IsRequired();
        builder.Property(s => s.FailureReason).HasMaxLength(500);
        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedAtUtc).IsRequired();

        builder.HasMany(s => s.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.DomainEvents);
    }
}
