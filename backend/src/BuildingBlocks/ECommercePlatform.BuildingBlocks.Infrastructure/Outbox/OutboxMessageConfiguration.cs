using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Shared across every module's DbContext (applied explicitly via modelBuilder.ApplyConfiguration,
/// not discovered via ApplyConfigurationsFromAssembly since it lives outside each module's own
/// assembly) — every module that raises domain events maps this same outbox_messages shape into
/// its own schema.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.OccurredOnUtc).IsRequired();
        builder.Property(m => m.Error);
    }
}
