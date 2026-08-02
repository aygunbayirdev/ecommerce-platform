using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Dtos;

namespace ECommercePlatform.Modules.Identity.Application.Users;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;
