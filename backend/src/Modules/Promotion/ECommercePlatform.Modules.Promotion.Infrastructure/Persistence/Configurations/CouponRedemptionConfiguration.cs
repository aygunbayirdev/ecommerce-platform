using ECommercePlatform.Modules.Promotion.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Promotion.Infrastructure.Persistence.Configurations;

internal sealed class CouponRedemptionConfiguration : IEntityTypeConfiguration<CouponRedemption>
{
    public void Configure(EntityTypeBuilder<CouponRedemption> builder)
    {
        builder.ToTable("coupon_redemptions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.CouponId).IsRequired();
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.OrderId).IsRequired();

        // A single order can redeem at most one coupon (Order.ApplyDiscount enforces one-coupon-
        // per-order too) — this is the DB-level backstop, same role as Payment's unique OrderId index.
        builder.HasIndex(r => r.OrderId).IsUnique();

        builder.Property(r => r.DiscountAmount).IsRequired();
        builder.Property(r => r.RedeemedAtUtc).IsRequired();
        builder.Property(r => r.ReleasedAtUtc);

        builder.Ignore(r => r.DomainEvents);
    }
}
