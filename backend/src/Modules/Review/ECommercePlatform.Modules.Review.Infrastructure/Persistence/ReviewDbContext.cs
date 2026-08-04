using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Review.Infrastructure.Persistence;

public sealed class ReviewDbContext(DbContextOptions<ReviewDbContext> options) : DbContext(options)
{
    public const string Schema = "review";

    public DbSet<Domain.Review> Reviews => Set<Domain.Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReviewDbContext).Assembly);
        modelBuilder.ApplyClientGeneratedKeys();

        base.OnModelCreating(modelBuilder);
    }
}
