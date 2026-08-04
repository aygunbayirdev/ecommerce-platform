using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommercePlatform.Modules.Shipping.Infrastructure.Persistence;

public sealed class ShippingDbContextFactory : IDesignTimeDbContextFactory<ShippingDbContext>
{
    public ShippingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5433;Database=ecommerce;Username=ecommerce_user;Password=change-me";

        var optionsBuilder = new DbContextOptionsBuilder<ShippingDbContext>();
        optionsBuilder
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", ShippingDbContext.Schema))
            .UseSnakeCaseNamingConvention();

        return new ShippingDbContext(optionsBuilder.Options);
    }
}
