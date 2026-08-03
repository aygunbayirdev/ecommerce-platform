using ECommercePlatform.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ProductId).IsRequired();
        builder.Property(v => v.Sku).HasMaxLength(64).IsRequired();
        builder.HasIndex(v => v.Sku).IsUnique();
        builder.Property(v => v.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(v => v.IsActive).IsRequired();
        builder.Property(v => v.CreatedAtUtc).IsRequired();

        builder.HasMany(v => v.AttributeValues)
            .WithOne()
            .HasForeignKey(a => a.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(v => v.DomainEvents);
    }
}
