using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public const string Schema = "catalog";

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<CategoryAttribute> CategoryAttributes => Set<CategoryAttribute>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        modelBuilder.ApplyClientGeneratedKeys();

        base.OnModelCreating(modelBuilder);
    }
}
