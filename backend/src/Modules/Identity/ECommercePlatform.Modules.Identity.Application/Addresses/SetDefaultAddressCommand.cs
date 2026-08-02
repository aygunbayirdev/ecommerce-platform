using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Identity.Application.Addresses;

public sealed record SetDefaultAddressCommand(Guid UserId, Guid AddressId) : ICommand;
