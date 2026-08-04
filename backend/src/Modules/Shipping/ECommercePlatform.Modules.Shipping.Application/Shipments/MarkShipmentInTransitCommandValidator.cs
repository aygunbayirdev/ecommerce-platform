using FluentValidation;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed class MarkShipmentInTransitCommandValidator : AbstractValidator<MarkShipmentInTransitCommand>
{
    public MarkShipmentInTransitCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
    }
}
