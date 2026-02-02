using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, string role);
    string GenerateRefreshToken();
    bool ValidateRefreshToken(string refreshToken, User user);
}
