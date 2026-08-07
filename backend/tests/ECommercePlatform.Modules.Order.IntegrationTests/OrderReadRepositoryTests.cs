using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Order.Domain;
using ECommercePlatform.Modules.Order.Infrastructure.Repositories;

namespace ECommercePlatform.Modules.Order.IntegrationTests;

[Collection(nameof(OrderDbCollection))]
public sealed class OrderReadRepositoryTests(OrderDbContainerFixture fixture)
{
    private static Domain.Order CreateOrder(Guid userId)
    {
        var items = new List<OrderItemSnapshot> { new(Guid.NewGuid(), "Telefon", "SKU-1", 100m, 2) };

        return Domain.Order.Create(
            userId, "Ayşe Yılmaz", "5551234567", "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", items);
    }

    private async Task PersistAsync(Domain.Order order)
    {
        await using var dbContext = fixture.CreateDbContext();
        var repository = new OrderWriteRepository(dbContext);
        repository.Add(order);
        await repository.SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrderWithItemsAndTotal_MatchingWhatWasWrittenViaEfCore()
    {
        var order = CreateOrder(Guid.NewGuid());
        await PersistAsync(order);

        var connectionFactory = new NpgsqlConnectionFactory(fixture.ConnectionString);
        var readRepository = new OrderReadRepository(connectionFactory);

        var result = await readRepository.GetByIdAsync(order.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(order.OrderNumber, result!.OrderNumber);
        Assert.Equal(OrderStatus.Created.ToString(), result.Status);
        var item = Assert.Single(result.Items);
        Assert.Equal("Telefon", item.ProductName);
        Assert.Equal(200m, item.LineTotal);
        Assert.Equal(200m, result.Total);
        Assert.Single(result.StatusHistory);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        var connectionFactory = new NpgsqlConnectionFactory(fixture.ConnectionString);
        var readRepository = new OrderReadRepository(connectionFactory);

        var result = await readRepository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyThatUsersOrders_AsAPagedResult()
    {
        var userId = Guid.NewGuid();
        await PersistAsync(CreateOrder(userId));
        await PersistAsync(CreateOrder(userId));
        await PersistAsync(CreateOrder(Guid.NewGuid()));

        var connectionFactory = new NpgsqlConnectionFactory(fixture.ConnectionString);
        var readRepository = new OrderReadRepository(connectionFactory);

        var result = await readRepository.GetByUserIdAsync(userId, pageNumber: 1, pageSize: 10, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }
}
