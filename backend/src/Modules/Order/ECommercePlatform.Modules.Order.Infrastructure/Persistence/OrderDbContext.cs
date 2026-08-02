using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Order.Infrastructure.Persistence;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public const string Schema = "order";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
