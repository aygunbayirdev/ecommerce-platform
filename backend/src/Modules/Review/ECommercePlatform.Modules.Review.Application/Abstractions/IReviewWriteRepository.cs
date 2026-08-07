namespace ECommercePlatform.Modules.Review.Application.Abstractions;

public interface IReviewWriteRepository
{
    Task<Domain.Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsByProductIdAndUserIdAsync(Guid productId, Guid userId, CancellationToken cancellationToken);

    void Add(Domain.Review review);

    void Remove(Domain.Review review);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
