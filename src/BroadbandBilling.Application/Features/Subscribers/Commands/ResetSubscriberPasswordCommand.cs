using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscribers.Commands;

// ============ Reset System (Login) Password ============
public record ResetSystemPasswordCommand(
    Guid SubscriberId,
    string NewPassword
) : IRequest<string>;

public class ResetSystemPasswordCommandHandler : IRequestHandler<ResetSystemPasswordCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ResetSystemPasswordCommandHandler> _logger;

    public ResetSystemPasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<ResetSystemPasswordCommandHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<string> Handle(ResetSystemPasswordCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Id == request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        User? user = null;

        if (subscriber.UserId.HasValue && subscriber.UserId != Guid.Empty)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == subscriber.UserId.Value, cancellationToken);
        }

        if (user == null)
        {
            // Check if a user with this email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == subscriber.Email, cancellationToken);
            if (existingUser != null)
            {
                // Link existing user to subscriber and update password
                user = existingUser;
                user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                subscriber.SetUserId(user.Id);
                _logger.LogInformation("Linked existing user {Username} to subscriber {SubscriberId}", user.Username, subscriber.Id);
            }
            else
            {
                // Auto-create a system User account for this subscriber (use email as username)
                var username = subscriber.Email;
                user = User.CreateSubscriber(
                    username,
                    subscriber.Email,
                    _passwordHasher.HashPassword(request.NewPassword),
                    subscriber.Id
                );
                _context.Users.Add(user);
                subscriber.SetUserId(user.Id);
                _logger.LogInformation("Auto-created system user {Username} for subscriber {SubscriberId}", username, subscriber.Id);
            }
        }
        else
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reset system password for subscriber {SubscriberId}", subscriber.Id);
        return request.NewPassword;
    }
}

// ============ Reset MikroTik (PPPoE) Password ============
public record ResetMikroTikPasswordCommand(
    Guid SubscriberId,
    string NewPassword
) : IRequest<string>;

public class ResetMikroTikPasswordCommandHandler : IRequestHandler<ResetMikroTikPasswordCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<ResetMikroTikPasswordCommandHandler> _logger;

    public ResetMikroTikPasswordCommandHandler(
        IUnitOfWork unitOfWork, 
        IMikroTikService mikroTikService,
        ILogger<ResetMikroTikPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task<string> Handle(ResetMikroTikPasswordCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        if (!subscriber.PppoeAccounts.Any())
            throw new InvalidOperationException("المشترك ليس لديه حساب MikroTik");

        foreach (var pppoeAccount in subscriber.PppoeAccounts)
        {
            // Update password in database
            pppoeAccount.UpdatePassword(request.NewPassword);

            // Sync with MikroTik
            await SyncPasswordWithMikroTik(pppoeAccount, request.NewPassword, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Reset MikroTik password for subscriber {SubscriberId} ({Count} accounts)", 
            subscriber.Id, subscriber.PppoeAccounts.Count);
        return request.NewPassword;
    }

    private async Task SyncPasswordWithMikroTik(PppoeAccount pppoeAccount, string newPassword, CancellationToken cancellationToken)
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

            // Delete and re-add user with new password (MikroTik doesn't support password update directly)
            var deleteRequest = new DeletePppUserRequest
            {
                Host = device.IpAddress.Value,
                Username = device.Username,
                Password = device.Password,
                PppUsername = pppoeAccount.Username
            };

            await _mikroTikService.DeletePppUserAsync(deleteRequest, cancellationToken);

            var addRequest = new AddPppUserRequest
            {
                Host = device.IpAddress.Value,
                Username = device.Username,
                Password = device.Password,
                PppUsername = pppoeAccount.Username,
                PppPassword = newPassword,
                Profile = pppoeAccount.ProfileName,
                Service = "pppoe"
            };

            var result = await _mikroTikService.AddPppUserAsync(addRequest, cancellationToken);

            if (result.Success)
            {
                pppoeAccount.MarkAsSynced();
                _logger.LogInformation("Successfully synced password change for PPPoE user {Username} with MikroTik", pppoeAccount.Username);
            }
            else
            {
                pppoeAccount.MarkAsSyncFailed();
                _logger.LogWarning("Failed to sync password change for PPPoE user {Username} with MikroTik: {Error}", 
                    pppoeAccount.Username, result.Error);
            }
        }
        catch (Exception ex)
        {
            pppoeAccount.MarkAsSyncFailed();
            _logger.LogError(ex, "Error syncing password change for PPPoE account {Username} with MikroTik", pppoeAccount.Username);
        }
    }
}

// ============ Legacy combined reset (kept for backward compat) ============
public record ResetSubscriberPasswordCommand(
    Guid SubscriberId,
    string? NewPassword = null
) : IRequest<string>;

public class ResetSubscriberPasswordCommandHandler : IRequestHandler<ResetSubscriberPasswordCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetSubscriberPasswordCommandHandler> _logger;

    public ResetSubscriberPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ResetSubscriberPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> Handle(ResetSubscriberPasswordCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(request.SubscriberId, cancellationToken);
        if (subscriber == null)
            throw new NotFoundException($"Subscriber with ID {request.SubscriberId} not found");

        var newPassword = request.NewPassword ?? GeneratePassword();

        // Update PPPoE account passwords in DB
        foreach (var pppoeAccount in subscriber.PppoeAccounts)
        {
            pppoeAccount.UpdatePassword(newPassword);
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
            password[i] = chars[random.Next(chars.Length)];
        return new string(password);
    }
}
