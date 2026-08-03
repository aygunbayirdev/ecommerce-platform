using ECommercePlatform.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductVariantAttributeValueConfiguration : IEntityTypeConfiguration<ProductVariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttributeValue> builder)
    {
        builder.ToTable("product_variant_attribute_values");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ProductVariantId).IsRequired();
        builder.Property(v => v.ProductAttributeId).IsRequired();
        builder.Property(v => v.Value).HasMaxLength(200).IsRequired();

        builder.HasIndex(v => new { v.ProductVariantId, v.ProductAttributeId }).IsUnique();

        builder.HasOne<ProductAttribute>()
            .WithMany()
            .HasForeignKey(v => v.ProductAttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(v => v.DomainEvents);
    }
}
