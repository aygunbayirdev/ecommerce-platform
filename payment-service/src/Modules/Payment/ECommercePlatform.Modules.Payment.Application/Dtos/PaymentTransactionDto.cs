namespace ECommercePlatform.Modules.Payment.Application.Dtos;

public sealed record PaymentTransactionDto(string IdempotencyKey, bool IsSuccessful, string? FailureReason, DateTime OccurredAtUtc);
