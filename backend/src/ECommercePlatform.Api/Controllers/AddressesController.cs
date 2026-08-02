using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Identity.Application.Addresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record AddAddressRequest(
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullAddressLine,
    string PostalCode,
    bool IsDefault);

public sealed record UpdateAddressRequest(
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullAddressLine,
    string PostalCode);

[ApiController]
[Route("api/addresses")]
[Authorize]
public sealed class AddressesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAddressesByUserIdQuery(CurrentUserId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddAddressRequest request, CancellationToken cancellationToken)
    {
        var command = new AddAddressCommand(
            CurrentUserId,
            request.Title,
            request.RecipientName,
            request.PhoneNumber,
            request.City,
            request.District,
            request.FullAddressLine,
            request.PostalCode,
            request.IsDefault);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMine), new { id = result.Value }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPut("{addressId:guid}")]
    public async Task<IActionResult> Update(Guid addressId, UpdateAddressRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAddressCommand(
            CurrentUserId,
            addressId,
            request.Title,
            request.RecipientName,
            request.PhoneNumber,
            request.City,
            request.District,
            request.FullAddressLine,
            request.PostalCode);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpDelete("{addressId:guid}")]
    public async Task<IActionResult> Delete(Guid addressId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteAddressCommand(CurrentUserId, addressId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPut("{addressId:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid addressId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetDefaultAddressCommand(CurrentUserId, addressId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
