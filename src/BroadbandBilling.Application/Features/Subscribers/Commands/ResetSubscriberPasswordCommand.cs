using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscribers.Commands;

public record ResetSubscriberPasswordCommand(
    Guid SubscriberId,
    string? NewPassword = null
) : IRequest<string>;

public class ResetSubscriberPasswordCommandHandler : IRequestHandler<ResetSubscriberPasswordCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<ResetSubscriberPasswordCommandHandler> _logger;

    public ResetSubscriberPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IMikroTikService mikroTikService,
        ILogger<ResetSubscriberPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task<string> Handle(ResetSubscriberPasswordCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        // Generate new password if not provided
        var newPassword = request.NewPassword ?? GeneratePassword();

        // Update user password (skip for now as Users repository not implemented)
        // var user = await _unitOfWork.Users.GetByIdAsync(subscriber.UserId, cancellationToken);
        // if (user == null)
        //     throw new NotFoundException($"User account for subscriber {request.SubscriberId} not found");

        // user.UpdatePassword(newPassword);

        // Update PPPoE account passwords
        foreach (var pppoeAccount in subscriber.PppoeAccounts)
        {
            pppoeAccount.UpdatePassword(newPassword);

            // Sync with MikroTik
            await UpdatePppoeAccountPasswordInMikroTik(pppoeAccount, newPassword, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Reset password for subscriber {SubscriberId}", subscriber.Id);

        return newPassword;
    }

    private string GeneratePassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var password = new char[12];

        for (int i = 0; i < password.Length; i++)
        {
            password[i] = chars[random.Next(chars.Length)];
        }

        return new string(password);
    }

    private async Task UpdatePppoeAccountPasswordInMikroTik(PppoeAccount pppoeAccount, string newPassword, CancellationToken cancellationToken)
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
                NewPassword = newPassword,
                NewProfile = pppoeAccount.ProfileName
            };

            var result = await _mikroTikService.UpdateUserProfileAsync(updateRequest, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully updated password for PPPoE user {Username} in MikroTik", pppoeAccount.Username);
            }
            else
            {
                _logger.LogWarning("Failed to update password for PPPoE user {Username} in MikroTik: {Error}", 
                    pppoeAccount.Username, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for PPPoE account {Username} in MikroTik", pppoeAccount.Username);
        }
    }
}
