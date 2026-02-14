using BroadbandBilling.Application.Features.Subscribers.Commands;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BroadbandBilling.Application.Features.Auth.Queries;

public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IApplicationDbContext _context;

    public GetCurrentUserHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Admin)
            .Include(u => u.Subscriber)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("المستخدم غير موجود");
        }

        if (user.UserType == UserType.Admin && user.Admin != null)
        {
            return new CurrentUserDto
            {
                UserType = "Admin",
                FullName = user.Admin.FullName,
                Role = user.Admin.Role.ToString(),
                HasActiveSubscription = false
            };
        }

        if (user.UserType == UserType.Subscriber && user.Subscriber != null)
        {
            var hasActiveSubscription = await _context.Subscriptions
                .AnyAsync(s => s.SubscriberId == user.Subscriber.Id &&
                              s.Status == SubscriptionStatus.Active,
                              cancellationToken);

            return new CurrentUserDto
            {
                UserType = "Subscriber",
                FullName = user.Subscriber.FullName,
                Role = "Subscriber",
                HasActiveSubscription = hasActiveSubscription
            };
        }

        throw new NotFoundException("بيانات المستخدم غير مكتملة");
    }
}
