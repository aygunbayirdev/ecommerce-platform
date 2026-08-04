using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Shipping.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed class MarkShipmentFailedCommandHandler(IShipmentWriteRepository shipmentWriteRepository)
    : ICommandHandler<MarkShipmentFailedCommand>
{
    public async Task<Result> Handle(MarkShipmentFailedCommand request, CancellationToken cancellationToken)
    {
        var shipment = await shipmentWriteRepository.GetByIdAsync(request.ShipmentId, cancellationToken);

        if (shipment is null)
        {
            return Result.Failure(Error.NotFound("Shipments.NotFound", "Kargo kaydı bulunamadı."));
        }

        var result = shipment.MarkFailed(request.Reason);

        if (result.IsFailure)
        {
            return result;
        }

        await shipmentWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
