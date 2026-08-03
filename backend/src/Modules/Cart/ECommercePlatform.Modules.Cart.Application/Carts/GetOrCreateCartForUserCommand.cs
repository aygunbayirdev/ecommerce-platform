using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed record GetOrCreateCartForUserCommand(Guid UserId) : ICommand<Guid>;
