using FluentValidation;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed class MarkShipmentDeliveredCommandValidator : AbstractValidator<MarkShipmentDeliveredCommand>
{
    public MarkShipmentDeliveredCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
    }
}
