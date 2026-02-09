using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscribers.Commands;

public record DeleteSubscriberCommand(
    Guid SubscriberId,
    bool ForceDelete = false
) : IRequest<bool>;

public class DeleteSubscriberCommandHandler : IRequestHandler<DeleteSubscriberCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<DeleteSubscriberCommandHandler> _logger;

    public DeleteSubscriberCommandHandler(
        IUnitOfWork unitOfWork,
        IMikroTikService mikroTikService,
        ILogger<DeleteSubscriberCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteSubscriberCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        // Check if subscriber has active subscriptions (unless force delete)
        if (!request.ForceDelete && subscriber.HasActiveSubscription())
        {
            throw new InvalidOperationException("Cannot delete subscriber with active subscriptions. Use ForceDelete=true to override.");
        }

        // Cancel all subscriptions
        foreach (var subscription in subscriber.Subscriptions)
        {
            subscription.Cancel("Subscriber deleted");
        }

        // Delete PPPoE accounts from MikroTik
        foreach (var pppoeAccount in subscriber.PppoeAccounts)
        {
            await DeletePppoeAccountFromMikroTik(pppoeAccount, cancellationToken);
        }

        // Get user account to delete (skip for now as Users repository not implemented)
        // var user = await _unitOfWork.Users.GetByIdAsync(subscriber.UserId, cancellationToken);
        // if (user != null)
        // {
        //     _unitOfWork.Users.Delete(user);
        // }

        // Delete subscriber (this will cascade delete subscriptions and PPPoE accounts)
        _unitOfWork.Subscribers.Delete(subscriber);

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Deleted subscriber {SubscriberId}", subscriber.Id);

        return true;
    }

    private async Task DeletePppoeAccountFromMikroTik(PppoeAccount pppoeAccount, CancellationToken cancellationToken)
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

            var deleteRequest = new DeletePppUserRequest
            {
                Host = device.IpAddress.Value,
                Username = device.Username,
                Password = device.Password,
                PppUsername = pppoeAccount.Username
            };

            var result = await _mikroTikService.DeletePppUserAsync(deleteRequest, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully deleted PPPoE user {Username} from MikroTik", pppoeAccount.Username);
            }
            else
            {
                _logger.LogWarning("Failed to delete PPPoE user {Username} from MikroTik: {Error}", 
                    pppoeAccount.Username, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting PPPoE account {Username} from MikroTik", pppoeAccount.Username);
        }
    }
}
