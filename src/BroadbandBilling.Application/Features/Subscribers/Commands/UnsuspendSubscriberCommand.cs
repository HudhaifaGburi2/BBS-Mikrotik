using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscribers.Commands;

public record UnsuspendSubscriberCommand(
    Guid SubscriberId,
    string? Reason = null
) : IRequest<bool>;

public class UnsuspendSubscriberCommandHandler : IRequestHandler<UnsuspendSubscriberCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<UnsuspendSubscriberCommandHandler> _logger;

    public UnsuspendSubscriberCommandHandler(
        IUnitOfWork unitOfWork,
        IMikroTikService mikroTikService,
        ILogger<UnsuspendSubscriberCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task<bool> Handle(UnsuspendSubscriberCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        // Activate subscriber
        subscriber.Activate();

        // Reactivate suspended subscriptions
        var suspendedSubscriptions = subscriber.Subscriptions.Where(s => s.Status == Domain.Enums.SubscriptionStatus.Suspended).ToList();
        foreach (var subscription in suspendedSubscriptions)
        {
            // Only reactivate if not expired
            if (!subscription.IsExpired())
            {
                subscription.Activate();
            }
        }

        // Enable PPPoE accounts
        foreach (var pppoeAccount in subscriber.PppoeAccounts.Where(p => !p.IsEnabled))
        {
            pppoeAccount.Enable();

            // Sync with MikroTik if device is available
            await EnablePppoeAccountInMikroTik(pppoeAccount, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Unsuspended subscriber {SubscriberId}", subscriber.Id);

        return true;
    }

    private async Task EnablePppoeAccountInMikroTik(PppoeAccount pppoeAccount, CancellationToken cancellationToken)
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

            var activateRequest = new DeletePppUserRequest
            {
                Host = device.IpAddress.Value,
                Username = device.Username,
                Password = device.Password,
                PppUsername = pppoeAccount.Username
            };

            var result = await _mikroTikService.ActivateUserAsync(activateRequest, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully enabled PPPoE user {Username} in MikroTik", pppoeAccount.Username);
            }
            else
            {
                _logger.LogWarning("Failed to enable PPPoE user {Username} in MikroTik: {Error}", 
                    pppoeAccount.Username, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling PPPoE account {Username} in MikroTik", pppoeAccount.Username);
        }
    }
}
