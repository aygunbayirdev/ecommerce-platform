using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Shipping.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentStatusHistoryConfiguration : IEntityTypeConfiguration<Domain.ShipmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<Domain.ShipmentStatusHistory> builder)
    {
        builder.ToTable("shipment_status_history");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ShipmentId).IsRequired();
        builder.Property(h => h.Status).IsRequired();
        builder.Property(h => h.Note).HasMaxLength(500);
        builder.Property(h => h.ChangedAtUtc).IsRequired();

        builder.Ignore(h => h.DomainEvents);
    }
}
