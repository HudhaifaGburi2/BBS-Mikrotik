using BroadbandBilling.Application.DTOs.Auth;
using MediatR;

namespace BroadbandBilling.Application.Features.Auth.Commands;

public record RefreshTokenCommand : IRequest<LoginResponse>
{
    public required string RefreshToken { get; init; }
    public string? IpAddress { get; init; }
}
