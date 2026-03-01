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
        Console.WriteLine($"[AdminLogin] Login attempt for user: {request.Username}");
        
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
        Console.WriteLine($"[AdminLogin] Login successful for: {response.FullName}, Role: {response.Role}");
        Console.WriteLine($"[AdminLogin] Setting cookies - AccessToken length: {response.AccessToken?.Length ?? 0}, RefreshToken length: {response.RefreshToken?.Length ?? 0}");
        
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
    [Consumes("application/json", "application/x-www-form-urlencoded", "text/plain")]
    public async Task<ActionResult> RefreshToken()
    {
        // Log all cookies for debugging
        var allCookies = Request.Cookies.Keys.ToList();
        Console.WriteLine($"[RefreshToken] Cookies received: {string.Join(", ", allCookies)}");
        
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        Console.WriteLine($"[RefreshToken] Refresh token cookie present: {!string.IsNullOrEmpty(refreshToken)}");
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            // Return 400 Bad Request instead of 401 to prevent infinite refresh loop
            return BadRequest(new { message = "لا يوجد رمز تحديث", error = "No refresh token cookie" });
        }

        try
        {
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
        catch (Exception ex)
        {
            Console.WriteLine($"[RefreshToken] Error: {ex.Message}");
            return BadRequest(new { message = "فشل تحديث الرمز", error = ex.Message });
        }
    }

    private void SetAuthCookies(string accessToken, string refreshToken, int expiresInSeconds, bool rememberMe)
    {
        // For development with Vite proxy, cookies need to be set without domain
        // and with SameSite=Lax to work across the proxy
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,  // Must be false for localhost without HTTPS
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds),
            // Don't set Domain - let browser default to request origin
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddDays(7),
        };

        Console.WriteLine($"[SetAuthCookies] Setting access token cookie, expires in {expiresInSeconds}s");
        Console.WriteLine($"[SetAuthCookies] Setting refresh token cookie, expires in {(rememberMe ? 30 : 7)} days");
        
        Response.Cookies.Append(AccessTokenCookie, accessToken, accessCookieOptions);
        Response.Cookies.Append(RefreshTokenCookie, refreshToken, refreshCookieOptions);
        
        Console.WriteLine($"[SetAuthCookies] Cookies appended to response");
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
            Path = "/",  // Match the path used when setting the cookie
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });
    }
}
