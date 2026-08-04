using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Shipping.Application.Abstractions;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed class MarkShipmentDeliveredCommandHandler(
    IShipmentWriteRepository shipmentWriteRepository,
    ISender sender) : ICommandHandler<MarkShipmentDeliveredCommand>
{
    public async Task<Result> Handle(MarkShipmentDeliveredCommand request, CancellationToken cancellationToken)
    {
        var shipment = await shipmentWriteRepository.GetByIdAsync(request.ShipmentId, cancellationToken);

        if (shipment is null)
        {
            return Result.Failure(Error.NotFound("Shipments.NotFound", "Kargo kaydı bulunamadı."));
        }

        var result = shipment.MarkDelivered();

        if (result.IsFailure)
        {
            return result;
        }

        await shipmentWriteRepository.SaveChangesAsync(cancellationToken);

        await sender.Send(new MarkOrderAsDeliveredCommand(shipment.OrderId), cancellationToken);

        return Result.Success();
    }
}
