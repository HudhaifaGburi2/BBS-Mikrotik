using BroadbandBilling.Application.DTOs.Auth;
using BroadbandBilling.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("admin/login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> AdminLogin([FromBody] LoginRequest request)
    {
        var command = new AdminLoginCommand
        {
            Username = request.Username,
            Password = request.Password,
            RememberMe = request.RememberMe,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            DeviceName = request.DeviceName,
            Browser = Request.Headers["User-Agent"].ToString(),
            OperatingSystem = request.OperatingSystem
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("subscriber/login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> SubscriberLogin([FromBody] LoginRequest request)
    {
        var command = new SubscriberLoginCommand
        {
            Username = request.Username,
            Password = request.Password,
            RememberMe = request.RememberMe,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            DeviceName = request.DeviceName,
            Browser = Request.Headers["User-Agent"].ToString(),
            OperatingSystem = request.OperatingSystem
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Implement logout logic (clear refresh token)
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // Implement refresh token logic
        return Ok();
    }
}

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
