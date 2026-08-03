using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed record RemoveCartItemCommand(Guid CartId, Guid ProductVariantId) : ICommand;
