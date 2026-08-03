using ECommercePlatform.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.StockItemId).IsRequired();
        builder.Property(m => m.MovementType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.Quantity).IsRequired();
        builder.Property(m => m.Reason).HasMaxLength(500);
        builder.Property(m => m.OccurredAtUtc).IsRequired();

        builder.Ignore(m => m.DomainEvents);
    }
}
