using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Shipping.Application.Abstractions;
using ECommercePlatform.Modules.Shipping.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed class GetShipmentByOrderIdQueryHandler(IShipmentReadRepository shipmentReadRepository)
    : IQueryHandler<GetShipmentByOrderIdQuery, ShipmentDto>
{
    public async Task<Result<ShipmentDto>> Handle(GetShipmentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var shipment = await shipmentReadRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (shipment is null)
        {
            return Result.Failure<ShipmentDto>(Error.NotFound("Shipments.NotFound", "Bu sipariş için kargo kaydı bulunamadı."));
        }

        return Result.Success(shipment);
    }
}
