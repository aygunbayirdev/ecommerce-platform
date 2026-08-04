using FluentValidation;

namespace ECommercePlatform.Modules.Shipping.Application.Shipments;

public sealed class MarkShipmentFailedCommandValidator : AbstractValidator<MarkShipmentFailedCommand>
{
    public MarkShipmentFailedCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
