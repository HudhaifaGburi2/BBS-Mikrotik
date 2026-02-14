using BroadbandBilling.Application.DTOs.Auth;
using BroadbandBilling.Application.Features.Auth.Commands;
using BroadbandBilling.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    private const string AccessTokenCookie = "doshi_access_token";
    private const string RefreshTokenCookie = "doshi_refresh_token";

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("admin/login")]
    [AllowAnonymous]
    public async Task<ActionResult> AdminLogin([FromBody] LoginRequest request)
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
        SetAuthCookies(response.AccessToken, response.RefreshToken, response.ExpiresIn, request.RememberMe);

        return Ok(new
        {
            userType = response.UserType,
            fullName = response.FullName,
            role = response.Role,
            hasActiveSubscription = response.HasActiveSubscription
        });
    }

    [HttpPost("subscriber/login")]
    [AllowAnonymous]
    public async Task<ActionResult> SubscriberLogin([FromBody] LoginRequest request)
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
        SetAuthCookies(response.AccessToken, response.RefreshToken, response.ExpiresIn, request.RememberMe);

        return Ok(new
        {
            userType = response.UserType,
            fullName = response.FullName,
            role = response.Role,
            hasActiveSubscription = response.HasActiveSubscription
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            ClearAuthCookies();
            return Ok(new { message = "تم تسجيل الخروج" });
        }

        var command = new LogoutCommand { UserId = userId };
        await _mediator.Send(command);

        ClearAuthCookies();
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _mediator.Send(new GetCurrentUserQuery { UserId = userId });
        return Ok(user);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "لا يوجد رمز تحديث" });
        }

        var command = new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var response = await _mediator.Send(command);
        SetAuthCookies(response.AccessToken, response.RefreshToken, response.ExpiresIn, false);

        return Ok(new
        {
            userType = response.UserType,
            fullName = response.FullName,
            role = response.Role,
            hasActiveSubscription = response.HasActiveSubscription
        });
    }

    private void SetAuthCookies(string accessToken, string refreshToken, int expiresInSeconds, bool rememberMe)
    {
        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append(AccessTokenCookie, accessToken, accessCookieOptions);
        Response.Cookies.Append(RefreshTokenCookie, refreshToken, refreshCookieOptions);
    }

    private void ClearAuthCookies()
    {
        var expiredOptions = new CookieOptions
        {
            HttpOnly = true,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        Response.Cookies.Append(AccessTokenCookie, "", expiredOptions);
        Response.Cookies.Append(RefreshTokenCookie, "", new CookieOptions
        {
            HttpOnly = true,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });
    }
}
