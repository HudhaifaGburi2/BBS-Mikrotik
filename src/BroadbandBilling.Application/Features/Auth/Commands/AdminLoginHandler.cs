using BroadbandBilling.Application.Common.Exceptions;
using BroadbandBilling.Application.DTOs.Auth;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BroadbandBilling.Application.Features.Auth.Commands;

public class AdminLoginHandler : IRequestHandler<AdminLoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IConfiguration _configuration;

    public AdminLoginHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<LoginResponse> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by username or email
        var user = await _context.Users
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => 
                (u.Username == request.Username || u.Email == request.Username) &&
                u.UserType == UserType.Admin, 
                cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedException("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        // Check if account is locked
        if (user.IsLockedOut())
        {
            var lockoutMinutesRemaining = (int)(user.LockoutEnd!.Value - DateTime.UtcNow).TotalMinutes;
            throw new UnauthorizedException($"الحساب مقفل. يرجى المحاولة بعد {lockoutMinutesRemaining} دقيقة");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _context.SaveChangesAsync(cancellationToken);

            // Record failed login
            var failedLogin = LoginHistory.CreateFailure(
                user.Id,
                UserType.Admin,
                "كلمة مرور خاطئة",
                request.IpAddress ?? "Unknown",
                request.DeviceName);
            
            _context.LoginHistory.Add(failedLogin);
            await _context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedException("الحساب غير نشط");
        }

        // Check admin details
        if (user.Admin == null || !user.Admin.IsActive)
        {
            throw new UnauthorizedException("حساب المسؤول غير نشط");
        }

        // Generate JWT tokens
        var accessToken = _jwtService.GenerateAccessToken(user, user.Admin.Role.ToString());
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Calculate expiry
        var expiryInMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");
        var refreshTokenExpiryDays = request.RememberMe ? 30 : 7;

        // Update user login info
        user.RecordSuccessfulLogin(request.IpAddress ?? "Unknown");
        user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(refreshTokenExpiryDays));

        // Record successful login
        var loginHistory = LoginHistory.CreateSuccess(
            user.Id,
            UserType.Admin,
            request.IpAddress ?? "Unknown",
            request.DeviceName,
            request.Browser,
            request.OperatingSystem);

        _context.LoginHistory.Add(loginHistory);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiryInMinutes * 60,
            UserType = "Admin",
            FullName = user.Admin.FullName,
            Role = user.Admin.Role.ToString()
        };
    }
}
