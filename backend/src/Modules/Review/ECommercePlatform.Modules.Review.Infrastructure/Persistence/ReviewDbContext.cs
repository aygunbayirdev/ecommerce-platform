using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Review.Infrastructure.Persistence;

public sealed class ReviewDbContext(DbContextOptions<ReviewDbContext> options) : DbContext(options)
{
    public const string Schema = "review";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReviewDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
