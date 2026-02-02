namespace BroadbandBilling.Application.DTOs.Auth;

public record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required string UserType { get; init; }
    public required string FullName { get; init; }
    public string? Role { get; init; }
    public bool HasActiveSubscription { get; init; }
}
