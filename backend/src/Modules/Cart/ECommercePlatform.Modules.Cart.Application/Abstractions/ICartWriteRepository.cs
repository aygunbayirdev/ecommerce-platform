namespace ECommercePlatform.Modules.Cart.Application.Abstractions;

public interface ICartWriteRepository
{
    /// <summary>Loads the Cart aggregate with its Items collection included — required for any item mutation.</summary>
    Task<Domain.Cart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken);

    Task<Domain.Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Domain.Cart cart);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
