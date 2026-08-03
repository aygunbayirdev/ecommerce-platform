using ECommercePlatform.Modules.Payment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Modules.Payment.Infrastructure.Persistence.Configurations;

internal sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.PaymentId).IsRequired();
        builder.Property(t => t.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.HasIndex(t => t.IdempotencyKey).IsUnique();
        builder.Property(t => t.IsSuccessful).IsRequired();
        builder.Property(t => t.FailureReason).HasMaxLength(500);
        builder.Property(t => t.OccurredAtUtc).IsRequired();

        builder.Ignore(t => t.DomainEvents);
    }
}
