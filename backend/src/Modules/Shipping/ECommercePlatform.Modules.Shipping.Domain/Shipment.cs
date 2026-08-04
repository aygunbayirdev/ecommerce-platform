using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Shipping.Domain;

public sealed class Shipment : BaseEntity
{
    private readonly List<ShipmentStatusHistory> _statusHistory = [];

    private Shipment()
    {
    }

    /// <summary>Cross-module reference to Order.Order — no DB foreign key by design (modular monolith: no cross-schema FKs).</summary>
    public Guid OrderId { get; private set; }

    public string Carrier { get; private set; } = string.Empty;

    public string TrackingNumber { get; private set; } = string.Empty;

    public ShipmentStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ShipmentStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public static Shipment Create(Guid orderId, string carrier, string trackingNumber)
    {
        var now = DateTime.UtcNow;

        var shipment = new Shipment
        {
            OrderId = Guard.AgainstEmpty(orderId, nameof(orderId)),
            Carrier = Guard.AgainstNullOrWhiteSpace(carrier, nameof(carrier)),
            TrackingNumber = Guard.AgainstNullOrWhiteSpace(trackingNumber, nameof(trackingNumber)),
            Status = ShipmentStatus.Shipped,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        shipment._statusHistory.Add(ShipmentStatusHistory.Create(shipment.Id, ShipmentStatus.Shipped, note: null));

        return shipment;
    }

    public Result MarkInTransit() => TransitionTo(ShipmentStatus.InTransit, from: ShipmentStatus.Shipped, note: null);

    /// <summary>
    /// Accepted from either Shipped or InTransit — unlike Order's strict single-predecessor state
    /// machine, real carrier tracking doesn't guarantee an InTransit update ever arrives before a
    /// Delivered one.
    /// </summary>
    public Result MarkDelivered()
    {
        if (Status != ShipmentStatus.Shipped && Status != ShipmentStatus.InTransit)
        {
            return Result.Failure(Error.Conflict(
                "Shipments.InvalidStatusTransition",
                $"Kargo {Status} durumunda, teslim edildi olarak işaretlenemez."));
        }

        Status = ShipmentStatus.Delivered;
        UpdatedAtUtc = DateTime.UtcNow;
        _statusHistory.Add(ShipmentStatusHistory.Create(Id, ShipmentStatus.Delivered, note: null));

        return Result.Success();
    }

    public Result MarkFailed(string reason)
    {
        if (Status is ShipmentStatus.Delivered or ShipmentStatus.Failed)
        {
            return Result.Failure(Error.Conflict(
                "Shipments.InvalidStatusTransition",
                $"{Status} durumundaki bir kargo başarısız olarak işaretlenemez."));
        }

        Status = ShipmentStatus.Failed;
        FailureReason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));
        UpdatedAtUtc = DateTime.UtcNow;
        _statusHistory.Add(ShipmentStatusHistory.Create(Id, ShipmentStatus.Failed, reason));

        return Result.Success();
    }

    private Result TransitionTo(ShipmentStatus to, ShipmentStatus from, string? note)
    {
        if (Status != from)
        {
            return Result.Failure(Error.Conflict(
                "Shipments.InvalidStatusTransition",
                $"Kargo {from} durumunda değil (şu an: {Status}), {to} durumuna geçemez."));
        }

        Status = to;
        UpdatedAtUtc = DateTime.UtcNow;
        _statusHistory.Add(ShipmentStatusHistory.Create(Id, to, note));

        return Result.Success();
    }
}
