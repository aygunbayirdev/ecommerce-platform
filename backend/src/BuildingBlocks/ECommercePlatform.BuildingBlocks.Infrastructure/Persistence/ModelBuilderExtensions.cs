using ECommercePlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// BaseEntity.Id is always generated client-side (Guid v7, set at construction), never by the
    /// database. Without this, EF Core's default heuristic — "a non-default key value discovered
    /// via graph traversal implies an existing row" — marks a brand-new child entity added to an
    /// already-tracked parent's collection (e.g. a fresh Address added to a loaded User.Addresses)
    /// as Modified instead of Added, producing a DbUpdateConcurrencyException on save (an UPDATE
    /// affecting 0 rows instead of an INSERT).
    /// </summary>
    public static void ApplyClientGeneratedKeys(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty(nameof(BaseEntity.Id));

            if (idProperty is not null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.ValueGenerated = ValueGenerated.Never;
            }
        }
    }
}
