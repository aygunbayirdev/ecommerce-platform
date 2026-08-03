using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Payment.Application.Dtos;

namespace ECommercePlatform.Modules.Payment.Application.Payments;

public sealed record GetPaymentByOrderIdQuery(Guid OrderId) : IQuery<PaymentDto>;
