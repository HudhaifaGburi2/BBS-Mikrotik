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
        // Extract user ID from JWT claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "معرف المستخدم غير صالح" });
        }

        var command = new LogoutCommand { UserId = userId };
        var success = await _mediator.Send(command);

        if (!success)
        {
            return BadRequest(new { message = "فشل تسجيل الخروج" });
        }

        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }
}

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
