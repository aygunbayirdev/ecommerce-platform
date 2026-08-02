using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Identity.Application.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Register), new { id = result.Value }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }
}
