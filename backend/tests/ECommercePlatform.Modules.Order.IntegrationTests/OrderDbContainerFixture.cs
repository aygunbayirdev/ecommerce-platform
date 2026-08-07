using ECommercePlatform.Modules.Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace ECommercePlatform.Modules.Order.IntegrationTests;

public sealed class OrderDbContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public OrderDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        optionsBuilder
            .UseNpgsql(ConnectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", OrderDbContext.Schema))
            .UseSnakeCaseNamingConvention();

        return new OrderDbContext(optionsBuilder.Options);
    }
}

[CollectionDefinition(nameof(OrderDbCollection))]
public sealed class OrderDbCollection : ICollectionFixture<OrderDbContainerFixture>
{
}
