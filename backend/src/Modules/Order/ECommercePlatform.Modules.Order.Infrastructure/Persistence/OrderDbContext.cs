using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Order.Infrastructure.Persistence;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public const string Schema = "order";

    public DbSet<Domain.Order> Orders => Set<Domain.Order>();

    public DbSet<Domain.OrderItem> OrderItems => Set<Domain.OrderItem>();

    public DbSet<Domain.OrderStatusHistory> OrderStatusHistories => Set<Domain.OrderStatusHistory>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyClientGeneratedKeys();

        base.OnModelCreating(modelBuilder);
    }
}
