namespace ECommercePlatform.Modules.Order.Application.Abstractions;

public interface IOrderWriteRepository
{
    /// <summary>Loads the Order aggregate with its Items and StatusHistory included — required for any status transition.</summary>
    Task<Domain.Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken);

    void Add(Domain.Order order);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
