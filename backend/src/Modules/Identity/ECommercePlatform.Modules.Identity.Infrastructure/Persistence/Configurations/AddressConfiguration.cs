using ECommercePlatform.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.Title).HasMaxLength(100).IsRequired();
        builder.Property(a => a.RecipientName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.District).HasMaxLength(100).IsRequired();
        builder.Property(a => a.FullAddressLine).HasMaxLength(500).IsRequired();
        builder.Property(a => a.PostalCode).HasMaxLength(10).IsRequired();
        builder.Property(a => a.IsDefault).IsRequired();
        builder.Property(a => a.CreatedAtUtc).IsRequired();

        builder.Ignore(a => a.DomainEvents);
    }
}
