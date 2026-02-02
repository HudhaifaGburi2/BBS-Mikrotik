using BroadbandBilling.Application.Common.Exceptions;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.DTOs.Auth;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BroadbandBilling.Application.Features.Auth.Commands;

public class SubscriberLoginHandler : IRequestHandler<SubscriberLoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IMikroTikService _mikroTikService;
    private readonly IConfiguration _configuration;

    public SubscriberLoginHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtService,
        IMikroTikService mikroTikService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _mikroTikService = mikroTikService;
        _configuration = configuration;
    }

    public async Task<LoginResponse> Handle(SubscriberLoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by username or email
        var user = await _context.Users
            .Include(u => u.Subscriber)
            .FirstOrDefaultAsync(u => 
                (u.Username == request.Username || u.Email == request.Username) &&
                u.UserType == UserType.Subscriber, 
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

            var failedLogin = LoginHistory.CreateFailure(
                user.Id,
                UserType.Subscriber,
                "كلمة مرور خاطئة",
                request.IpAddress ?? "Unknown",
                request.DeviceName);
            
            _context.LoginHistory.Add(failedLogin);
            await _context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        // Check if user is active
        if (!user.IsActive || user.Subscriber == null || !user.Subscriber.IsActive)
        {
            throw new UnauthorizedException("الحساب غير نشط");
        }

        // Get PPPoE account for MikroTik validation
        var pppoeAccount = await _context.PppoeAccounts
            .Include(p => p.MikroTikDevice)
            .FirstOrDefaultAsync(p => p.SubscriberId == user.Subscriber.Id, cancellationToken);

        // Validate MikroTik credentials if PPPoE account exists
        if (pppoeAccount != null && pppoeAccount.IsEnabled)
        {
            var isValid = await _mikroTikService.ValidateCredentialsAsync(
                pppoeAccount.MikroTikDevice.IpAddress.Value,
                pppoeAccount.MikroTikDevice.Port,
                pppoeAccount.MikroTikDevice.Username,
                pppoeAccount.MikroTikDevice.Password,
                pppoeAccount.Username,
                pppoeAccount.Password);

            pppoeAccount.MarkAsValidated(isValid);

            if (!isValid)
            {
                await _context.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedException("بيانات اعتماد الإنترنت غير صحيحة. يرجى الاتصال بالدعم");
            }
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user, "Subscriber");
        var refreshToken = _jwtService.GenerateRefreshToken();

        var expiryInMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");
        var refreshTokenExpiryDays = request.RememberMe ? 30 : 7;

        // Update login info
        user.RecordSuccessfulLogin(request.IpAddress ?? "Unknown");
        user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(refreshTokenExpiryDays));

        // Record login history
        var loginHistory = LoginHistory.CreateSuccess(
            user.Id,
            UserType.Subscriber,
            request.IpAddress ?? "Unknown",
            request.DeviceName,
            request.Browser,
            request.OperatingSystem);

        _context.LoginHistory.Add(loginHistory);
        await _context.SaveChangesAsync(cancellationToken);

        // Check for active subscription
        var hasActiveSubscription = await _context.Subscriptions
            .AnyAsync(s => s.SubscriberId == user.Subscriber.Id && 
                          s.Status == SubscriptionStatus.Active, 
                          cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiryInMinutes * 60,
            UserType = "Subscriber",
            FullName = user.Subscriber.FullName,
            HasActiveSubscription = hasActiveSubscription
        };
    }
}
