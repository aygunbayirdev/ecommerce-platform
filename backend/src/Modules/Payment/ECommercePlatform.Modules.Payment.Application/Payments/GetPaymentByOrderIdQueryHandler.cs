using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.Modules.Payment.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Payment.Application.Payments;

public sealed class GetPaymentByOrderIdQueryHandler(IPaymentReadRepository paymentReadRepository)
    : IQueryHandler<GetPaymentByOrderIdQuery, PaymentDto>
{
    public async Task<Result<PaymentDto>> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentReadRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (payment is null)
        {
            return Result.Failure<PaymentDto>(Error.NotFound("Payments.NotFound", "Bu sipariş için ödeme kaydı bulunamadı."));
        }

        return payment;
    }
}
