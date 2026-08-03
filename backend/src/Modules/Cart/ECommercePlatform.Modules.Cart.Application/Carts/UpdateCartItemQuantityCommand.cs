using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed record UpdateCartItemQuantityCommand(Guid CartId, Guid ProductVariantId, int Quantity) : ICommand;
