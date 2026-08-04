using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Payment.Application.Payments;

/// <summary>Internal — not exposed via any controller, only sent by the RabbitMQ consumer that handles OrderReadyForPaymentIntegrationEvent.</summary>
public sealed record CreatePendingPaymentCommand(Guid OrderId, Guid UserId, decimal Amount) : ICommand<Guid>;
