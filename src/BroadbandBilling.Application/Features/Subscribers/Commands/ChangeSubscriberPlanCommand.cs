using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscribers.Commands;

public record ChangeSubscriberPlanCommand(
    Guid SubscriberId,
    Guid NewPlanId,
    bool Prorate = false
) : IRequest<SubscriptionDto>;

public class ChangeSubscriberPlanCommandHandler : IRequestHandler<ChangeSubscriberPlanCommand, SubscriptionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<ChangeSubscriberPlanCommandHandler> _logger;

    public ChangeSubscriberPlanCommandHandler(
        IUnitOfWork unitOfWork,
        IMikroTikService mikroTikService,
        ILogger<ChangeSubscriberPlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task<SubscriptionDto> Handle(ChangeSubscriberPlanCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        var newPlan = await _unitOfWork.Plans.GetByIdAsync(request.NewPlanId, cancellationToken);
        if (newPlan == null)
            throw new NotFoundException($"Plan with ID {request.NewPlanId} not found");

        // Get active subscription
        var activeSubscription = subscriber.GetActiveSubscription();
        if (activeSubscription == null)
            throw new InvalidOperationException("Subscriber has no active subscription to change");

        // Create new subscription with new plan
        var startDate = request.Prorate ? DateTime.UtcNow : activeSubscription.BillingPeriod.EndDate.AddDays(1);
        var newSubscription = Subscription.Create(
            subscriber.Id,
            newPlan.Id,
            startDate,
            newPlan.BillingCycleDays
        );

        await _unitOfWork.Subscriptions.AddAsync(newSubscription, cancellationToken);

        // Suspend old subscription
        activeSubscription.Suspend($"Changed to plan {newPlan.Name}");

        // Update PPPoE accounts to use new profile
        foreach (var pppoeAccount in subscriber.PppoeAccounts.Where(p => p.IsEnabled))
        {
            pppoeAccount.UpdateProfile(newPlan.MikroTikProfileName);

            // Sync with MikroTik
            await UpdatePppoeAccountProfileInMikroTik(pppoeAccount, newPlan.MikroTikProfileName, cancellationToken);
        }

        // Activate new subscription immediately if not prorating
        if (!request.Prorate)
        {
            newSubscription.Activate();
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Changed plan for subscriber {SubscriberId} from {OldPlan} to {NewPlan}", 
            subscriber.Id, activeSubscription.Plan?.Name, newPlan.Name);

        return MapToDto(newSubscription, newPlan);
    }

    private async Task UpdatePppoeAccountProfileInMikroTik(PppoeAccount pppoeAccount, string newProfileName, CancellationToken cancellationToken)
    {
        try
        {
            var device = await _unitOfWork.MikroTikDevices.GetByIdAsync(pppoeAccount.MikroTikDeviceId, cancellationToken);
            if (device == null)
            {
                _logger.LogWarning("MikroTik device {DeviceId} not found for PPPoE account {Username}", 
                    pppoeAccount.MikroTikDeviceId, pppoeAccount.Username);
                return;
            }

            var updateRequest = new UpdateProfileRequest
            {
                Host = device.IpAddress.Value,
                Username = device.Username,
                Password = device.Password,
                PppUsername = pppoeAccount.Username,
                NewProfile = newProfileName
            };

            var result = await _mikroTikService.UpdateUserProfileAsync(updateRequest, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully updated profile for PPPoE user {Username} to {Profile} in MikroTik", 
                    pppoeAccount.Username, newProfileName);
            }
            else
            {
                _logger.LogWarning("Failed to update profile for PPPoE user {Username} in MikroTik: {Error}", 
                    pppoeAccount.Username, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for PPPoE account {Username} in MikroTik", pppoeAccount.Username);
        }
    }

    private static SubscriptionDto MapToDto(Subscription subscription, Plan plan)
    {
        return new SubscriptionDto(
            subscription.Id,
            subscription.PlanId,
            plan.Name,
            subscription.Status,
            subscription.BillingPeriod.StartDate,
            subscription.BillingPeriod.EndDate,
            subscription.ActivatedAt,
            subscription.SuspendedAt
        );
    }
}
