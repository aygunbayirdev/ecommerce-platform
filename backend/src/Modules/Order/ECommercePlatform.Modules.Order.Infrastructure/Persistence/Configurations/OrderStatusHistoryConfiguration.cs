using ECommercePlatform.Modules.Order.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Order.Infrastructure.Persistence.Configurations;

internal sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.OrderId).IsRequired();
        builder.Property(h => h.Status).IsRequired();
        builder.Property(h => h.Note).HasMaxLength(500);
        builder.Property(h => h.ChangedAtUtc).IsRequired();

        builder.Ignore(h => h.DomainEvents);
    }
}
