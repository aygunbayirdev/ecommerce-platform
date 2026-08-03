using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Dtos;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed record GetCartByIdQuery(Guid CartId) : IQuery<CartViewDto>;
