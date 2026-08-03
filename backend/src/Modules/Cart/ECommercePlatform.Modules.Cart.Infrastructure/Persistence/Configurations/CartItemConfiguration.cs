using ECommercePlatform.Modules.Cart.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Cart.Infrastructure.Persistence.Configurations;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.CartId).IsRequired();
        builder.Property(i => i.ProductVariantId).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.CreatedAtUtc).IsRequired();

        builder.HasIndex(i => new { i.CartId, i.ProductVariantId }).IsUnique();

        builder.Ignore(i => i.DomainEvents);
    }
}
