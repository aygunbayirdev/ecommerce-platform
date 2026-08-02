using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Dtos;

namespace ECommercePlatform.Modules.Identity.Application.Addresses;

public sealed record GetAddressesByUserIdQuery(Guid UserId) : IQuery<IReadOnlyList<AddressDto>>;
