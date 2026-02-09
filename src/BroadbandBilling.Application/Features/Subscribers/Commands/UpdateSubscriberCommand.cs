using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscribers.Commands;

public record UpdateSubscriberCommand(
    Guid SubscriberId,
    string? FullName = null,
    string? Email = null,
    string? PhoneNumber = null,
    string? Address = null,
    string? NationalId = null,
    string? City = null,
    string? PostalCode = null
) : IRequest<SubscriberDto>;

public class UpdateSubscriberCommandHandler : IRequestHandler<UpdateSubscriberCommand, SubscriberDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSubscriberCommandHandler> _logger;

    public UpdateSubscriberCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateSubscriberCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SubscriberDto> Handle(UpdateSubscriberCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        // Update personal info if provided
        if (!string.IsNullOrEmpty(request.FullName) || !string.IsNullOrEmpty(request.NationalId))
        {
            subscriber.UpdatePersonalInfo(
                request.FullName ?? subscriber.FullName,
                request.NationalId
            );
        }

        // Update contact info if provided
        if (!string.IsNullOrEmpty(request.Email) || 
            !string.IsNullOrEmpty(request.PhoneNumber) || 
            !string.IsNullOrEmpty(request.Address))
        {
            subscriber.UpdateContactInfo(
                request.Email ?? subscriber.Email,
                request.PhoneNumber ?? subscriber.PhoneNumber,
                request.Address ?? subscriber.Address
            );
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Updated subscriber {SubscriberId}", subscriber.Id);

        return MapToDto(subscriber);
    }

    private static SubscriberDto MapToDto(Subscriber subscriber)
    {
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
            subscriber.Subscriptions.Select(s => new SubscriptionDto(
                s.Id,
                s.PlanId,
                s.Plan?.Name ?? "Unknown",
                s.Status,
                s.BillingPeriod.StartDate,
                s.BillingPeriod.EndDate,
                s.ActivatedAt,
                s.SuspendedAt
            )).ToList(),
            subscriber.PppoeAccounts.Select(p => new PppoeAccountDto(
                p.Id,
                p.Username,
                p.ProfileName,
                p.IsEnabled,
                p.IsSyncedWithMikroTik,
                p.LastSyncDate,
                p.ValidationStatus,
                p.IsOnline(),
                p.LastConnectedAt
            )).ToList()
        );
    }
}
