using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Payment.Application.Payments;

public sealed record ProcessPaymentCommand(Guid UserId, Guid OrderId, string CardNumber, string IdempotencyKey) : ICommand;
