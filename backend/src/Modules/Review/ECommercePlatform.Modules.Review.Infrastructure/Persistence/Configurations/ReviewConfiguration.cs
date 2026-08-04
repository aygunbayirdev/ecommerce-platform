using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Review.Infrastructure.Persistence.Configurations;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Domain.Review>
{
    public void Configure(EntityTypeBuilder<Domain.Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ProductId).IsRequired();
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.OrderId).IsRequired();
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.IsApproved).IsRequired();
        builder.Property(r => r.CreatedAtUtc).IsRequired();

        // One review per product per user — also serves as the index for "reviews by product" queries.
        builder.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();

        builder.Ignore(r => r.DomainEvents);
    }
}
