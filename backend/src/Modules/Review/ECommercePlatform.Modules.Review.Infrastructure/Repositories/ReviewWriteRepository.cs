using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.Modules.Review.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Review.Infrastructure.Repositories;

internal sealed class ReviewWriteRepository(ReviewDbContext dbContext) : IReviewWriteRepository
{
    public Task<Domain.Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ExistsByProductIdAndUserIdAsync(Guid productId, Guid userId, CancellationToken cancellationToken)
        => dbContext.Reviews.AnyAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);

    public void Add(Domain.Review review) => dbContext.Reviews.Add(review);

    public void Remove(Domain.Review review) => dbContext.Reviews.Remove(review);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
