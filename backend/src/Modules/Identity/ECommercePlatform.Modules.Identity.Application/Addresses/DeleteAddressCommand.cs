using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Identity.Application.Addresses;

public sealed record DeleteAddressCommand(Guid UserId, Guid AddressId) : ICommand;
