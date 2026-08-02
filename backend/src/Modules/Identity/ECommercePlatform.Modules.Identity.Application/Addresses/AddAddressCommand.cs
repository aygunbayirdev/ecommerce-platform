using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Identity.Application.Addresses;

public sealed record AddAddressCommand(
    Guid UserId,
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullAddressLine,
    string PostalCode,
    bool IsDefault) : ICommand<Guid>;
