using ECommercePlatform.Modules.Payment.Application.Dtos;

namespace ECommercePlatform.Modules.Payment.Application.Abstractions;

public interface IPaymentReadRepository
{
    Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
}
