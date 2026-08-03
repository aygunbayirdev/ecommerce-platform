using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Catalog.Application.Brands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record UpdateBrandRequest(string Name);

[ApiController]
[Route("api/brands")]
public sealed class BrandsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBrandsQuery(), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateBrandCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { id = result.Value }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPut("{brandId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid brandId, UpdateBrandRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBrandCommand(brandId, request.Name);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }
}
