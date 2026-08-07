using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Catalog.Application.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record AddProductVariantRequest(
    string Sku, decimal Price, IReadOnlyList<ProductVariantAttributeValueInput> AttributeValues);

public sealed record AddProductImageRequest(string Url, bool IsPrimary);

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetById(Guid productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductByIdQuery(productId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetProductsQuery(categoryId, pageNumber, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    // Admin-only counterpart to GetAll — includes inactive products so they can be found and
    // reactivated (the public listing above deliberately excludes them).
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllForAdmin(
        [FromQuery] Guid? categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetAllProductsForAdminQuery(categoryId, pageNumber, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { productId = result.Value }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPost("{productId:guid}/variants")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddVariant(Guid productId, AddProductVariantRequest request, CancellationToken cancellationToken)
    {
        var command = new AddProductVariantCommand(productId, request.Sku, request.Price, request.AttributeValues);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { productId }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPost("{productId:guid}/images")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddImage(Guid productId, AddProductImageRequest request, CancellationToken cancellationToken)
    {
        var command = new AddProductImageCommand(productId, request.Url, request.IsPrimary);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { productId }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPost("{productId:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateProductCommand(productId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{productId:guid}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reactivate(Guid productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReactivateProductCommand(productId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateVariant(Guid productId, Guid variantId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateProductVariantCommand(productId, variantId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReactivateVariant(Guid productId, Guid variantId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReactivateProductVariantCommand(productId, variantId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpDelete("{productId:guid}/images/{imageId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveImage(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveProductImageCommand(productId, imageId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }
}
