using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BroadbandBilling.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role),
            new Claim("UserType", user.UserType.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.SubscriberId.HasValue)
        {
            claims.Add(new Claim("SubscriberId", user.SubscriberId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public bool ValidateRefreshToken(string refreshToken, User user)
    {
        if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(user.RefreshToken))
            return false;

        if (user.RefreshToken != refreshToken)
            return false;

        if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return false;

        return true;
    }
}
