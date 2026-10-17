using Application.Abstraction.Mediator;
using Application.Auth.Commands.Login;
using Application.Auth.Commands.Logout;
using Application.Auth.Commands.RefreshToken;
using Application.Auth.Commands.Register;
using Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(
            request,
            cancellationToken);

        return Ok();
    }


    [HttpPost("register")]
    public async Task<ActionResult<TokenPairDto>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue(
                "Idempotency-Key",
                out var keyHeader)
            || !Guid.TryParse(keyHeader.ToString(), out Guid key))
        {
            return BadRequest(
                "A valid Idempotency-Key header is required.");
        }

        var command = request with
        {
            IdempotencyKey = key
        };
        var result = await mediator.SendAsync(
            command,
            cancellationToken);

        return Ok(result);
    }


    [HttpPost("login")]
    public async Task<ActionResult<TokenPairDto>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue(
                "Idempotency-Key",
                out var keyHeader)
            || !Guid.TryParse(keyHeader.ToString(), out Guid key))
        {
            return BadRequest(
                "A valid Idempotency-Key header is required.");
        }

        var command = request with
        {
            IdempotencyKey = key
        };

        var result = await mediator.SendAsync(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenPairDto>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var tokens = await mediator.SendAsync(
            request,
            cancellationToken);
        return Ok(tokens);
    }
}