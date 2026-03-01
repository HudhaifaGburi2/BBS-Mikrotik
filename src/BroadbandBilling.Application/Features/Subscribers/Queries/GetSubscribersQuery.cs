using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.Commands;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscribers.Queries;

public record GetSubscribersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null,
    bool? HasActiveSubscription = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<PagedResult<SubscriberDto>>;

public class GetSubscribersQueryHandler : IRequestHandler<GetSubscribersQuery, PagedResult<SubscriberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubscribersQueryHandler> _logger;

    public GetSubscribersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetSubscribersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<SubscriberDto>> Handle(GetSubscribersQuery request, CancellationToken cancellationToken)
    {
        var subscribers = await _unitOfWork.Subscribers.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.IsActive,
            request.HasActiveSubscription,
            request.SortBy,
            request.SortDescending,
            cancellationToken
        );

        var subscriberDtos = subscribers.Items.Select(MapToDto).ToList();

        _logger.LogInformation("Retrieved {Count} subscribers (page {Page})", subscriberDtos.Count, request.Page);

        return new PagedResult<SubscriberDto>(
            subscriberDtos,
            subscribers.TotalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling((double)subscribers.TotalCount / request.PageSize)
        );
    }

    private static SubscriberDto MapToDto(Subscriber subscriber)
    {
        var now = DateTime.UtcNow;
        
        var subscriptions = subscriber.Subscriptions?.Select(s => new SubscriptionDto(
            s.Id,
            s.PlanId,
            s.Plan?.Name ?? "Unknown",
            s.Status,
            s.BillingPeriod?.StartDate ?? DateTime.MinValue,
            s.BillingPeriod?.EndDate ?? DateTime.MinValue,
            s.ActivatedAt,
            s.SuspendedAt
        )).ToList() ?? new List<SubscriptionDto>();

        var pppoeAccounts = subscriber.PppoeAccounts?.Select(p => new PppoeAccountDto(
            p.Id,
            p.Username,
            p.ProfileName,
            p.IsEnabled,
            p.IsSyncedWithMikroTik,
            p.LastSyncDate,
            p.ValidationStatus,
            p.LastConnectedAt.HasValue && p.LastConnectedAt.Value > now.AddMinutes(-5),
            p.LastConnectedAt
        )).ToList() ?? new List<PppoeAccountDto>();

        return new SubscriberDto(
            subscriber.Id,
            subscriber.FullName,
            subscriber.Email,
            subscriber.PhoneNumber,
            subscriber.Address,
            subscriber.NationalId,
            subscriber.IsActive,
            subscriber.CreatedAt,
            subscriber.UpdatedAt,
            subscriptions,
            pppoeAccounts
        );
    }
}

public record GetSubscriberByIdQuery(Guid SubscriberId) : IRequest<SubscriberDto>;

public class GetSubscriberByIdQueryHandler : IRequestHandler<GetSubscriberByIdQuery, SubscriberDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubscriberByIdQueryHandler> _logger;

    public GetSubscriberByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetSubscriberByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SubscriberDto> Handle(GetSubscriberByIdQuery request, CancellationToken cancellationToken)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        _logger.LogInformation("Retrieved subscriber {SubscriberId}", subscriber.Id);

        return MapToDto(subscriber);
    }

    private static SubscriberDto MapToDto(Subscriber subscriber)
    {
        var now = DateTime.UtcNow;
        
        var subscriptions = subscriber.Subscriptions?.Select(s => new SubscriptionDto(
            s.Id,
            s.PlanId,
            s.Plan?.Name ?? "Unknown",
            s.Status,
            s.BillingPeriod?.StartDate ?? DateTime.MinValue,
            s.BillingPeriod?.EndDate ?? DateTime.MinValue,
            s.ActivatedAt,
            s.SuspendedAt
        )).ToList() ?? new List<SubscriptionDto>();

        var pppoeAccounts = subscriber.PppoeAccounts?.Select(p => new PppoeAccountDto(
            p.Id,
            p.Username,
            p.ProfileName,
            p.IsEnabled,
            p.IsSyncedWithMikroTik,
            p.LastSyncDate,
            p.ValidationStatus,
            p.LastConnectedAt.HasValue && p.LastConnectedAt.Value > now.AddMinutes(-5),
            p.LastConnectedAt
        )).ToList() ?? new List<PppoeAccountDto>();

        return new SubscriberDto(
            subscriber.Id,
            subscriber.FullName,
            subscriber.Email,
            subscriber.PhoneNumber,
            subscriber.Address,
            subscriber.NationalId,
            subscriber.IsActive,
            subscriber.CreatedAt,
            subscriber.UpdatedAt,
            subscriptions,
            pppoeAccounts
        );
    }
}
