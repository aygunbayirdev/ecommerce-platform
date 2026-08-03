namespace ECommercePlatform.Modules.Payment.Application.Dtos;

public sealed record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Status,
    IReadOnlyList<PaymentTransactionDto> Transactions);
