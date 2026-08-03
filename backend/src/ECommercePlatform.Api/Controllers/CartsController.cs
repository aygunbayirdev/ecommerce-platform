using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Cart.Application.Carts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record AddCartItemRequest(Guid ProductVariantId, int Quantity);

public sealed record UpdateCartItemQuantityRequest(int Quantity);

[ApiController]
[Route("api/carts")]
public sealed class CartsController(ISender sender) : ControllerBase
{
    // Guest endpoints — addressed by CartId (no session/cookie infrastructure; the client stores
    // the id returned here and passes it in the route on every subsequent call).
    [HttpPost]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCartCommand(UserId: null), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { cartId = result.Value }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpGet("{cartId:guid}")]
    public async Task<IActionResult> GetById(Guid cartId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCartByIdQuery(cartId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost("{cartId:guid}/items")]
    public async Task<IActionResult> AddItem(Guid cartId, AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddItemToCartCommand(cartId, request.ProductVariantId, request.Quantity);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPut("{cartId:guid}/items/{productVariantId:guid}")]
    public async Task<IActionResult> UpdateItemQuantity(
        Guid cartId, Guid productVariantId, UpdateCartItemQuantityRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCartItemQuantityCommand(cartId, productVariantId, request.Quantity);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpDelete("{cartId:guid}/items/{productVariantId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartId, Guid productVariantId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveCartItemCommand(cartId, productVariantId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    // Authenticated endpoints — cart is resolved from the JWT, never from the route (same pattern
    // as AddressesController). GetOrCreateCartForUserCommand transparently creates the user's cart
    // on first use, so "mine" always resolves to something.
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var cartIdResult = await sender.Send(new GetOrCreateCartForUserCommand(CurrentUserId), cancellationToken);

        if (cartIdResult.IsFailure)
        {
            return cartIdResult.ToProblemDetails();
        }

        var result = await sender.Send(new GetCartByIdQuery(cartIdResult.Value), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost("mine/items")]
    [Authorize]
    public async Task<IActionResult> AddItemToMine(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var cartIdResult = await sender.Send(new GetOrCreateCartForUserCommand(CurrentUserId), cancellationToken);

        if (cartIdResult.IsFailure)
        {
            return cartIdResult.ToProblemDetails();
        }

        var command = new AddItemToCartCommand(cartIdResult.Value, request.ProductVariantId, request.Quantity);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPut("mine/items/{productVariantId:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateMineItemQuantity(
        Guid productVariantId, UpdateCartItemQuantityRequest request, CancellationToken cancellationToken)
    {
        var cartIdResult = await sender.Send(new GetOrCreateCartForUserCommand(CurrentUserId), cancellationToken);

        if (cartIdResult.IsFailure)
        {
            return cartIdResult.ToProblemDetails();
        }

        var command = new UpdateCartItemQuantityCommand(cartIdResult.Value, productVariantId, request.Quantity);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpDelete("mine/items/{productVariantId:guid}")]
    [Authorize]
    public async Task<IActionResult> RemoveMineItem(Guid productVariantId, CancellationToken cancellationToken)
    {
        var cartIdResult = await sender.Send(new GetOrCreateCartForUserCommand(CurrentUserId), cancellationToken);

        if (cartIdResult.IsFailure)
        {
            return cartIdResult.ToProblemDetails();
        }

        var result = await sender.Send(new RemoveCartItemCommand(cartIdResult.Value, productVariantId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
