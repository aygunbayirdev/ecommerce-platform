namespace ECommercePlatform.Modules.Identity.Application.Dtos;

public sealed record AddressDto(
    Guid Id,
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullAddressLine,
    string PostalCode,
    bool IsDefault,
    DateTime CreatedAtUtc);
