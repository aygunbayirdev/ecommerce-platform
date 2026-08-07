using ECommercePlatform.Modules.Order.Domain;
using ECommercePlatform.Modules.Order.Infrastructure.Repositories;

namespace ECommercePlatform.Modules.Order.IntegrationTests;

[Collection(nameof(OrderDbCollection))]
public sealed class OrderWriteRepositoryTests(OrderDbContainerFixture fixture)
{
    private static Domain.Order CreateOrder()
    {
        var items = new List<OrderItemSnapshot>
        {
            new(Guid.NewGuid(), "Telefon", "SKU-1", 100m, 2),
            new(Guid.NewGuid(), "Kılıf", "SKU-2", 20m, 1),
        };

        return Domain.Order.Create(
            Guid.NewGuid(), "Ayşe Yılmaz", "5551234567", "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", items);
    }

    [Fact]
    public async Task AddAndSave_ShouldPersistOrderWithItemsAndStatusHistory_ToRealPostgres()
    {
        var order = CreateOrder();
        order.MarkReadyForPayment();

        await using (var writeDbContext = fixture.CreateDbContext())
        {
            var repository = new OrderWriteRepository(writeDbContext);
            repository.Add(order);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var readDbContext = fixture.CreateDbContext();
        var readRepository = new OrderWriteRepository(readDbContext);
        var persisted = await readRepository.GetByIdWithItemsAsync(order.Id, CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Equal(order.OrderNumber, persisted!.OrderNumber);
        Assert.Equal(OrderStatus.PaymentPending, persisted.Status);
        Assert.Equal(2, persisted.Items.Count);
        Assert.Contains(persisted.Items, i => i.ProductName == "Telefon" && i.Quantity == 2);
        Assert.Equal(2, persisted.StatusHistory.Count);
        Assert.Contains(persisted.StatusHistory, h => h.Status == OrderStatus.Created);
        Assert.Contains(persisted.StatusHistory, h => h.Status == OrderStatus.PaymentPending);
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        await using var dbContext = fixture.CreateDbContext();
        var repository = new OrderWriteRepository(dbContext);

        var result = await repository.GetByIdWithItemsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }
}
