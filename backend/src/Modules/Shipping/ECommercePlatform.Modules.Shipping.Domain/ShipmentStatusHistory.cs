using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Shipping.Domain;

public sealed class ShipmentStatusHistory : BaseEntity
{
    private ShipmentStatusHistory()
    {
    }

    public Guid ShipmentId { get; private set; }

    public ShipmentStatus Status { get; private set; }

    public string? Note { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    internal static ShipmentStatusHistory Create(Guid shipmentId, ShipmentStatus status, string? note)
    {
        return new ShipmentStatusHistory
        {
            ShipmentId = Guard.AgainstEmpty(shipmentId, nameof(shipmentId)),
            Status = status,
            Note = note,
            ChangedAtUtc = DateTime.UtcNow,
        };
    }
}
