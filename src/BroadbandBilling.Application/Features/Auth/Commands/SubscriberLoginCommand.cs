using BroadbandBilling.Application.DTOs.Auth;
using MediatR;

namespace BroadbandBilling.Application.Features.Auth.Commands;

public record SubscriberLoginCommand : IRequest<LoginResponse>
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public bool RememberMe { get; init; }
    public string? IpAddress { get; init; }
    public string? DeviceName { get; init; }
    public string? Browser { get; init; }
    public string? OperatingSystem { get; init; }
}
