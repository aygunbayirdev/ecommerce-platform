using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Catalog.Application.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record UpdateCategoryRequest(string Name, int DisplayOrder);

public sealed record AssignCategoryAttributeRequest(Guid ProductAttributeId);

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoriesQuery(), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet("{categoryId:guid}/attributes")]
    public async Task<IActionResult> GetAttributes(Guid categoryId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryAttributesQuery(categoryId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { id = result.Value }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPut("{categoryId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(categoryId, request.Name, request.DisplayOrder);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{categoryId:guid}/attributes")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignAttribute(
        Guid categoryId, AssignCategoryAttributeRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignCategoryAttributeCommand(categoryId, request.ProductAttributeId);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpDelete("{categoryId:guid}/attributes/{productAttributeId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveAttribute(
        Guid categoryId, Guid productAttributeId, CancellationToken cancellationToken)
    {
        var command = new RemoveCategoryAttributeCommand(categoryId, productAttributeId);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }
}
