using ECommercePlatform.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("brands");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).HasMaxLength(150).IsRequired();
        builder.Property(b => b.IsActive).IsRequired();
        builder.Property(b => b.CreatedAtUtc).IsRequired();

        builder.Ignore(b => b.DomainEvents);
    }
}
