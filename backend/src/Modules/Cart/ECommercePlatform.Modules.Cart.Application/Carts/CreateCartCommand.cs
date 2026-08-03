using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed record CreateCartCommand(Guid? UserId) : ICommand<Guid>;
