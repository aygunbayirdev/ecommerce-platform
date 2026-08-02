using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Identity.Application.Addresses;

public sealed record UpdateAddressCommand(
    Guid UserId,
    Guid AddressId,
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullAddressLine,
    string PostalCode) : ICommand;
