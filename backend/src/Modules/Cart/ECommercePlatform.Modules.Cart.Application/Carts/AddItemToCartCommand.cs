using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed record AddItemToCartCommand(Guid CartId, Guid ProductVariantId, int Quantity) : ICommand;
