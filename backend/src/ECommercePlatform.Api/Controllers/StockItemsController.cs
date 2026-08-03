using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record IncreaseStockRequest(int Quantity, string? Reason);

[ApiController]
[Route("api/stock-items")]
[Authorize(Roles = "Admin")]
public sealed class StockItemsController(ISender sender) : ControllerBase
{
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
