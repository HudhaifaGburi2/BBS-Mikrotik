using BroadbandBilling.Application.Common.Exceptions;
using BroadbandBilling.Application.DTOs.Auth;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Auth.Commands;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<RefreshTokenHandler> logger)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find user by refresh token
        var user = await _context.Users
            .Include(u => u.Admin)
            .Include(u => u.Subscriber)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Invalid refresh token attempted from IP: {IpAddress}", request.IpAddress);
            throw new UnauthorizedException("رمز التحديث غير صالح");
        }

        // Check if refresh token is expired
        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            _logger.LogWarning("Expired refresh token for user {UserId}", user.Id);
            throw new UnauthorizedException("رمز التحديث منتهي الصلاحية");
        }

        // Check if account is locked
        if (user.IsLocked())
        {
            _logger.LogWarning("Refresh token attempted for locked account: {Username}", user.Username);
            throw new UnauthorizedException("الحساب مقفل مؤقتاً");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Refresh token attempted for inactive account: {Username}", user.Username);
            throw new UnauthorizedException("الحساب غير نشط");
        }

        // Generate new tokens
        var accessTokenExpiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");
        var refreshTokenExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7");

        var role = user.Admin?.Role.ToString() ?? string.Empty;
        var accessToken = _jwtTokenService.GenerateAccessToken(user, role);

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        // Update refresh token (rotate)
        user.UpdateRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(refreshTokenExpiryDays));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token successfully for user {UserId}", user.Id);

        // Build response based on user type
        if (user.UserType == UserType.Admin && user.Admin != null)
        {
            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = accessTokenExpiryMinutes * 60,
                UserType = user.UserType.ToString(),
                FullName = user.Admin.FullName,
                Role = user.Admin.Role.ToString()
            };
        }
        else if (user.UserType == UserType.Subscriber && user.Subscriber != null)
        {
            var hasActiveSubscription = await _context.Subscriptions
                .AnyAsync(s => s.SubscriberId == user.Subscriber.Id && 
                              s.Status == SubscriptionStatus.Active, 
                              cancellationToken);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = accessTokenExpiryMinutes * 60,
                UserType = user.UserType.ToString(),
                FullName = user.Subscriber.FullName,
                HasActiveSubscription = hasActiveSubscription
            };
        }

        throw new UnauthorizedException("نوع المستخدم غير صالح");
    }
}
