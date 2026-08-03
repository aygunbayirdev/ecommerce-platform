using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Inventory.Domain;

public sealed class StockMovement : BaseEntity
{
    private StockMovement()
    {
    }

    public Guid StockItemId { get; private set; }

    public StockMovementType MovementType { get; private set; }

    public int Quantity { get; private set; }

    public string? Reason { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    internal static StockMovement Create(Guid stockItemId, StockMovementType movementType, int quantity, string? reason)
    {
        return new StockMovement
        {
            StockItemId = Guard.AgainstEmpty(stockItemId, nameof(stockItemId)),
            MovementType = movementType,
            Quantity = quantity,
            Reason = reason,
            OccurredAtUtc = DateTime.UtcNow,
        };
    }
}
