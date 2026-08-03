using ECommercePlatform.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class CategoryAttributeConfiguration : IEntityTypeConfiguration<CategoryAttribute>
{
    public void Configure(EntityTypeBuilder<CategoryAttribute> builder)
    {
        builder.ToTable("category_attributes");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CategoryId).IsRequired();
        builder.Property(a => a.ProductAttributeId).IsRequired();

        builder.HasIndex(a => new { a.CategoryId, a.ProductAttributeId }).IsUnique();

        builder.HasOne<ProductAttribute>()
            .WithMany()
            .HasForeignKey(a => a.ProductAttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(a => a.DomainEvents);
    }
}
