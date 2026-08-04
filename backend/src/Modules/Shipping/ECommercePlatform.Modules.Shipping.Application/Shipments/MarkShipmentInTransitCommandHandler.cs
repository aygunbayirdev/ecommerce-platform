using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Shipping.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed class MarkShipmentInTransitCommandHandler(IShipmentWriteRepository shipmentWriteRepository)
    : ICommandHandler<MarkShipmentInTransitCommand>
{
    public async Task<Result> Handle(MarkShipmentInTransitCommand request, CancellationToken cancellationToken)
    {
        var shipment = await shipmentWriteRepository.GetByIdAsync(request.ShipmentId, cancellationToken);

        if (shipment is null)
        {
            return Result.Failure(Error.NotFound("Shipments.NotFound", "Kargo kaydı bulunamadı."));
        }

        var result = shipment.MarkInTransit();

        if (result.IsFailure)
        {
            return result;
        }

        await shipmentWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
