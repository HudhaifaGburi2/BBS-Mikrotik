using BroadbandBilling.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Auth.Commands;

public class LogoutHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(IApplicationDbContext context, ILogger<LogoutHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Logout attempted for non-existent user {UserId}", request.UserId);
            return false;
        }

        // Clear refresh token
        user.ClearRefreshToken();
        
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged out successfully", request.UserId);
        
        return true;
    }
}
