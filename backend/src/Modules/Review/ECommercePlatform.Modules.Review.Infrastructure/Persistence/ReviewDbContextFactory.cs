using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommercePlatform.Modules.Review.Infrastructure.Persistence;

public sealed class ReviewDbContextFactory : IDesignTimeDbContextFactory<ReviewDbContext>
{
    public ReviewDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5433;Database=ecommerce;Username=ecommerce_user;Password=change-me";

        var optionsBuilder = new DbContextOptionsBuilder<ReviewDbContext>();
        optionsBuilder
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", ReviewDbContext.Schema))
            .UseSnakeCaseNamingConvention();

        return new ReviewDbContext(optionsBuilder.Options);
    }
}
