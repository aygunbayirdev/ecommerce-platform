using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Payment.Infrastructure.Persistence;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public const string Schema = "payment";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
