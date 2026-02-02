using MediatR;

namespace BroadbandBilling.Application.Features.Auth.Commands;

public record LogoutCommand : IRequest<bool>
{
    public Guid UserId { get; init; }
}
