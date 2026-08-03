namespace ECommercePlatform.Modules.Order.Application.Dtos;

public sealed record OrderSummaryDto(Guid Id, string OrderNumber, string Status, decimal Total, DateTime CreatedAtUtc);
