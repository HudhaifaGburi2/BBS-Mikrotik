using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscribers.Commands;

public record CreateSubscriberCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Address,
    string? NationalId = null,
    string? City = null,
    string? PostalCode = null,
    Guid? PlanId = null,
    string? PppUsername = null,
    string? PppPassword = null
) : IRequest<SubscriberDto>;

public class CreateSubscriberCommandHandler : IRequestHandler<CreateSubscriberCommand, SubscriberDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<CreateSubscriberCommandHandler> _logger;

    public CreateSubscriberCommandHandler(
        IUnitOfWork unitOfWork,
        IMikroTikService mikroTikService,
        ILogger<CreateSubscriberCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task<SubscriberDto> Handle(CreateSubscriberCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        ValidateRequest(request);

        // Create subscriber
        var subscriber = Subscriber.Create(
            request.FullName,
            request.Email,
            request.PhoneNumber,
            request.Address,
            request.NationalId
        );

        // Add to database
        await _unitOfWork.Subscribers.AddAsync(subscriber, cancellationToken);

        // Create user account (skip for now as Users repository not implemented)
        // var user = User.CreateSubscriber(
        //     subscriber.Id,
        //     request.Email,
        //     GeneratePassword()
        // );
        // await _unitOfWork.Users.AddAsync(user, cancellationToken);

        // subscriber.UpdateUserId(user.Id);

        // Create subscription if plan provided
        if (request.PlanId.HasValue)
        {
            // Update user password (skip for now as Users repository not implemented)
            // var user = await _unitOfWork.Users.GetByIdAsync(subscriber.UserId, cancellationToken);
            // if (user == null)
            //     throw new NotFoundException($"User account for subscriber {request.SubscriberId} not found");

            // user.UpdatePassword(newPassword);

            var plan = await _unitOfWork.Plans.GetByIdAsync(request.PlanId.Value, cancellationToken);
            if (plan == null)
                throw new NotFoundException($"Plan with ID {request.PlanId} not found");

            var subscription = Subscription.Create(
                subscriber.Id,
                plan.Id,
                DateTime.UtcNow,
                plan.BillingCycleDays
            );

            await _unitOfWork.Subscriptions.AddAsync(subscription, cancellationToken);

            // Create PPPoE account if credentials provided
            if (!string.IsNullOrEmpty(request.PppUsername) && !string.IsNullOrEmpty(request.PppPassword))
            {
                // Get first MikroTik device — optional, sync will happen later if not available
                var mikroTikDevices = await _unitOfWork.MikroTikDevices.GetAllAsync(cancellationToken);
                var mikroTikDevice = mikroTikDevices.FirstOrDefault(d => d.IsActive);

                var deviceId = mikroTikDevice?.Id ?? Guid.Empty;

                var pppoeAccount = PppoeAccount.Create(
                    subscriber.Id,
                    subscription.Id,
                    deviceId,
                    request.PppUsername,
                    request.PppPassword,
                    plan.MikroTikProfileName
                );

                await _unitOfWork.PppoeAccounts.AddAsync(pppoeAccount, cancellationToken);

                // Sync with MikroTik if device is available
                if (mikroTikDevice != null)
                {
                    await SyncPppoeAccountWithMikroTik(pppoeAccount, mikroTikDevice, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("No active MikroTik device configured. PPPoE account for subscriber {SubscriberId} saved without sync.", subscriber.Id);
                }
            }
        }

        // Save all changes
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Created new subscriber {SubscriberId} with email {Email}", subscriber.Id, subscriber.Email);

        return MapToDto(subscriber);
    }

    private void ValidateRequest(CreateSubscriberCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required");

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
            throw new ArgumentException("Valid email is required");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new ArgumentException("Phone number is required");

        if (string.IsNullOrWhiteSpace(request.Address))
            throw new ArgumentException("Address is required");
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

    private async Task SyncPppoeAccountWithMikroTik(PppoeAccount pppoeAccount, MikroTikDevice device, CancellationToken cancellationToken)
    {
        try
        {
            var addRequest = new AddPppUserRequest
            {
                Host = device.IpAddress.Value,
                Username = device.Username,
                Password = device.Password,
                PppUsername = pppoeAccount.Username,
                PppPassword = pppoeAccount.Password,
                Profile = pppoeAccount.ProfileName,
                Service = "pppoe"
            };

            var result = await _mikroTikService.AddPppUserAsync(addRequest, cancellationToken);

            if (result.Success)
            {
                pppoeAccount.MarkAsSynced();
                _logger.LogInformation("Successfully synced PPPoE account {Username} with MikroTik", pppoeAccount.Username);
            }
            else
            {
                pppoeAccount.MarkAsSyncFailed();
                _logger.LogWarning("Failed to sync PPPoE account {Username} with MikroTik: {Error}", pppoeAccount.Username, result.Error);
            }
        }
        catch (Exception ex)
        {
            pppoeAccount.MarkAsSyncFailed();
            _logger.LogError(ex, "Error syncing PPPoE account {Username} with MikroTik", pppoeAccount.Username);
        }
    }

    private static SubscriberDto MapToDto(Subscriber subscriber)
    {
        var subscriptions = subscriber.Subscriptions?.Select(s => new SubscriptionDto(
            s.Id,
            s.PlanId,
            s.Plan?.Name ?? "Unknown",
            s.Status,
            s.BillingPeriod.StartDate,
            s.BillingPeriod.EndDate,
            s.ActivatedAt,
            s.SuspendedAt
        )).ToList() ?? new List<SubscriptionDto>();

        var now = DateTime.UtcNow;
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

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
