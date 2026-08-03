using ECommercePlatform.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.Url).HasMaxLength(2000).IsRequired();
        builder.Property(i => i.IsPrimary).IsRequired();
        builder.Property(i => i.DisplayOrder).IsRequired();
        builder.Property(i => i.CreatedAtUtc).IsRequired();

        builder.Ignore(i => i.DomainEvents);
    }
}
