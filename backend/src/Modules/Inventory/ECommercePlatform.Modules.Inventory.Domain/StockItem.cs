using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Inventory.Domain;

public sealed class StockItem : BaseEntity
{
    private readonly List<StockMovement> _movements = [];

    private StockItem()
    {
    }

    /// <summary>Cross-module reference to Catalog.ProductVariant — no DB foreign key by design (modular monolith: no cross-schema FKs).</summary>
    public Guid ProductVariantId { get; private set; }

    public int AvailableQuantity { get; private set; }

    public int ReservedQuantity { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();

    public static StockItem Create(Guid productVariantId)
    {
        return new StockItem
        {
            ProductVariantId = Guard.AgainstEmpty(productVariantId, nameof(productVariantId)),
            AvailableQuantity = 0,
            ReservedQuantity = 0,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public Result IncreaseStock(int quantity, string? reason)
    {
        if (quantity <= 0)
        {
            return Result.Failure(Error.Validation("StockItems.InvalidQuantity", "Miktar 0'dan büyük olmalı."));
        }

        AvailableQuantity += quantity;
        _movements.Add(StockMovement.Create(Id, StockMovementType.Inbound, quantity, reason));

        return Result.Success();
    }

    public Result Reserve(int quantity)
    {
        if (quantity > AvailableQuantity)
        {
            return Result.Failure(Error.Conflict("StockItems.InsufficientStock", "Yetersiz stok."));
        }

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
        _movements.Add(StockMovement.Create(Id, StockMovementType.Reserved, quantity, reason: null));

        return Result.Success();
    }

    public Result Release(int quantity)
    {
        if (quantity > ReservedQuantity)
        {
            return Result.Failure(Error.Conflict("StockItems.InsufficientReservedStock", "Bırakılmak istenen miktar rezerve edilenden fazla."));
        }

        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;
        _movements.Add(StockMovement.Create(Id, StockMovementType.Released, quantity, reason: null));

        return Result.Success();
    }

    public Result Commit(int quantity)
    {
        if (quantity > ReservedQuantity)
        {
            return Result.Failure(Error.Conflict("StockItems.InsufficientReservedStock", "Düşülmek istenen miktar rezerve edilenden fazla."));
        }

        ReservedQuantity -= quantity;
        _movements.Add(StockMovement.Create(Id, StockMovementType.Committed, quantity, reason: null));

        return Result.Success();
    }
}
