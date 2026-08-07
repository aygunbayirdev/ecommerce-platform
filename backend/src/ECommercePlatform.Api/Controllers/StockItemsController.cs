using ECommercePlatform.Api.Common;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record IncreaseStockRequest(int Quantity, string? Reason);

public sealed record StockItemAdminDto(
    Guid Id,
    Guid ProductVariantId,
    Guid ProductId,
    string ProductName,
    string Sku,
    int AvailableQuantity,
    int ReservedQuantity,
    bool IsVariantActive,
    DateTime CreatedAtUtc);

[ApiController]
[Route("api/stock-items")]
[Authorize(Roles = "Admin")]
public sealed class StockItemsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetAllStockItemsQuery(pageNumber, pageSize), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ToProblemDetails();
        }

        var page = result.Value;

        if (page.Items.Count == 0)
        {
            return Ok(new PagedResult<StockItemAdminDto>
            {
                Items = [],
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
            });
        }

        var variantIds = page.Items.Select(i => i.ProductVariantId).ToList();
        var summariesResult = await sender.Send(new GetProductVariantSummariesQuery(variantIds), cancellationToken);
        var summaryByVariantId = summariesResult.Value.ToDictionary(s => s.ProductVariantId);

        var items = page.Items.Select(i =>
        {
            var summary = summaryByVariantId.GetValueOrDefault(i.ProductVariantId);

            return new StockItemAdminDto(
                i.Id,
                i.ProductVariantId,
                summary?.ProductId ?? Guid.Empty,
                summary?.ProductName ?? "(bilinmiyor)",
                summary?.Sku ?? "-",
                i.AvailableQuantity,
                i.ReservedQuantity,
                summary?.IsActive ?? false,
                i.CreatedAtUtc);
        }).ToList();

        return Ok(new PagedResult<StockItemAdminDto>
        {
            Items = items,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    [HttpGet("{productVariantId:guid}")]
    public async Task<IActionResult> GetByVariantId(Guid productVariantId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStockByVariantIdQuery(productVariantId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost("{productVariantId:guid}/increase")]
    public async Task<IActionResult> Increase(
        Guid productVariantId, IncreaseStockRequest request, CancellationToken cancellationToken)
    {
        var command = new IncreaseStockCommand(productVariantId, request.Quantity, request.Reason);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }
}
