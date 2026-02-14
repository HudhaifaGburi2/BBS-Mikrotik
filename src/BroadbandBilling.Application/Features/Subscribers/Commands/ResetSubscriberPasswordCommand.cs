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

        if (!subscriber.UserId.HasValue || subscriber.UserId == Guid.Empty)
            throw new InvalidOperationException("المشترك ليس لديه حساب نظام مرتبط");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == subscriber.UserId.Value, cancellationToken);
        if (user == null)
            throw new NotFoundException("حساب النظام غير موجود");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

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
    private readonly ILogger<ResetMikroTikPasswordCommandHandler> _logger;

    public ResetMikroTikPasswordCommandHandler(IUnitOfWork unitOfWork, ILogger<ResetMikroTikPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
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
            pppoeAccount.UpdatePassword(request.NewPassword);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Reset MikroTik password for subscriber {SubscriberId} ({Count} accounts)", 
            subscriber.Id, subscriber.PppoeAccounts.Count);
        return request.NewPassword;
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
