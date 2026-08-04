using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Shipping.Application.Abstractions;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

/// <summary>
/// The fourth instance in the project of a source module driving a target module's existing
/// command via a direct, synchronous ISender call (Cart -> Catalog was a read; Order's checkout
/// was a multi-module write orchestration; Payment -> Order was the first single-command case this
/// mirrors exactly). MarkOrderAsShippedCommand already existed as an admin-only stand-in — no new
/// Order code was needed, only a real caller for it.
///
/// Requires the order to already be Preparing, because Order.MarkAsShipped() only accepts that as
/// its predecessor state — creating a shipment for a Paid order fails here with a clear message
/// rather than silently trying (and failing) to skip the Preparing step in Order.
/// </summary>
public sealed class CreateShipmentCommandHandler(
    IShipmentWriteRepository shipmentWriteRepository,
    ISender sender) : ICommandHandler<CreateShipmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await sender.Send(new GetOrderByIdQuery(request.OrderId), cancellationToken);

        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(Error.NotFound("Shipments.OrderNotFound", "Sipariş bulunamadı."));
        }

        if (orderResult.Value.Status != "Preparing")
        {
            return Result.Failure<Guid>(Error.Conflict(
                "Shipments.OrderNotReadyToShip",
                $"Sipariş 'Preparing' durumunda değil (şu an: {orderResult.Value.Status}), kargoya verilemez."));
        }

        if (await shipmentWriteRepository.ExistsByOrderIdAsync(request.OrderId, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Shipments.AlreadyExists", "Bu sipariş için zaten bir kargo kaydı var."));
        }

        var shipment = Domain.Shipment.Create(request.OrderId, request.Carrier, request.TrackingNumber);

        shipmentWriteRepository.Add(shipment);
        await shipmentWriteRepository.SaveChangesAsync(cancellationToken);

        await sender.Send(new MarkOrderAsShippedCommand(request.OrderId), cancellationToken);

        return shipment.Id;
    }
}
