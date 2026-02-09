using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscribers.Commands;

public record SuspendSubscriberCommand(
    Guid SubscriberId,
    string? Reason = null
) : IRequest<bool>;

public class SuspendSubscriberCommandHandler : IRequestHandler<SuspendSubscriberCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<SuspendSubscriberCommandHandler> _logger;

    public SuspendSubscriberCommandHandler(
        IUnitOfWork unitOfWork,
        IMikroTikService mikroTikService,
        ILogger<SuspendSubscriberCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task<bool> Handle(SuspendSubscriberCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        // Deactivate subscriber
        subscriber.Deactivate();

        // Suspend active subscriptions
        var activeSubscriptions = subscriber.Subscriptions.Where(s => s.IsActive()).ToList();
        foreach (var subscription in activeSubscriptions)
        {
            subscription.Suspend(request.Reason ?? "Suspended by admin");
        }

        // Disable PPPoE accounts
        foreach (var pppoeAccount in subscriber.PppoeAccounts.Where(p => p.IsEnabled))
        {
            pppoeAccount.Disable();

            // Sync with MikroTik if device is available
            await DisablePppoeAccountInMikroTik(pppoeAccount, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Suspended subscriber {SubscriberId} with reason: {Reason}", 
            subscriber.Id, request.Reason ?? "No reason provided");

        return true;
    }

    private async Task DisablePppoeAccountInMikroTik(PppoeAccount pppoeAccount, CancellationToken cancellationToken)
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

            var deactivateRequest = new DeletePppUserRequest
            {
                Host = device.IpAddress.Value,
                Username = device.Username,
                Password = device.Password,
                PppUsername = pppoeAccount.Username
            };

            var result = await _mikroTikService.DeactivateUserAsync(deactivateRequest, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully disabled PPPoE user {Username} in MikroTik", pppoeAccount.Username);
            }
            else
            {
                _logger.LogWarning("Failed to disable PPPoE user {Username} in MikroTik: {Error}", 
                    pppoeAccount.Username, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling PPPoE account {Username} in MikroTik", pppoeAccount.Username);
        }
    }
}
