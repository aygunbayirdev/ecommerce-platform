using ECommercePlatform.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ProductVariantId).IsRequired();
        builder.HasIndex(s => s.ProductVariantId).IsUnique();
        builder.Property(s => s.AvailableQuantity).IsRequired();
        builder.Property(s => s.ReservedQuantity).IsRequired();
        builder.Property(s => s.CreatedAtUtc).IsRequired();

        builder.HasMany(s => s.Movements)
            .WithOne()
            .HasForeignKey(m => m.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.DomainEvents);
    }
}
