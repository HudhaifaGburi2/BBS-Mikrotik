using MediatR;

namespace BroadbandBilling.Application.Features.Auth.Queries;

public class GetCurrentUserQuery : IRequest<CurrentUserDto>
{
    public Guid UserId { get; set; }
}

public record CurrentUserDto
{
    public string UserType { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool HasActiveSubscription { get; init; }
}
