using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    string? MacAddress = null,
    string? IpAddress = null,
    Guid? PlanId = null,
    DateTime? StartDate = null,
    bool AutoRenew = false,
    string? PppUsername = null,
    string? PppPassword = null,
    bool AutoCreateMikroTik = false,
    bool CreateSystemAccount = false,
    string? SystemUsername = null,
    string? SystemPassword = null
) : IRequest<SubscriberDto>;

public class CreateSubscriberCommandHandler : IRequestHandler<CreateSubscriberCommand, SubscriberDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMikroTikService _mikroTikService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateSubscriberCommandHandler> _logger;

    public CreateSubscriberCommandHandler(
        IUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        IMikroTikService mikroTikService,
        IPasswordHasher passwordHasher,
        ILogger<CreateSubscriberCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _mikroTikService = mikroTikService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<SubscriberDto> Handle(CreateSubscriberCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        ValidateRequest(request);

        // Check if email already exists in Users table
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new ArgumentException($"البريد الإلكتروني '{request.Email}' مستخدم بالفعل. الرجاء استخدام بريد إلكتروني آخر.");
        }

        // Check if email already exists in Subscribers table
        var existingSubscriber = await _dbContext.Subscribers.FirstOrDefaultAsync(s => s.Email == request.Email, cancellationToken);
        if (existingSubscriber != null)
        {
            throw new ArgumentException($"البريد الإلكتروني '{request.Email}' مستخدم بالفعل لمشترك آخر.");
        }

        // Check if system username already exists
        if (request.CreateSystemAccount && !string.IsNullOrEmpty(request.SystemUsername))
        {
            var existingUsername = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.SystemUsername, cancellationToken);
            if (existingUsername != null)
            {
                throw new ArgumentException($"اسم المستخدم '{request.SystemUsername}' مستخدم بالفعل. الرجاء اختيار اسم آخر.");
            }
        }

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

        // Create system user account if requested
        if (request.CreateSystemAccount && !string.IsNullOrEmpty(request.SystemUsername) && !string.IsNullOrEmpty(request.SystemPassword))
        {
            var passwordHash = _passwordHasher.HashPassword(request.SystemPassword);
            var user = User.CreateSubscriber(request.SystemUsername, request.Email, passwordHash, subscriber.Id);
            _dbContext.Users.Add(user);
            
            // Link subscriber to user (bidirectional)
            subscriber.SetUserId(user.Id);
            
            _logger.LogInformation("Created system user account {Username} for subscriber {SubscriberId}", request.SystemUsername, subscriber.Id);
        }

        // Set MAC and IP if provided
        if (!string.IsNullOrEmpty(request.MacAddress) || !string.IsNullOrEmpty(request.IpAddress))
        {
            subscriber.UpdateNetworkInfo(request.MacAddress, request.IpAddress, null);
        }

        // Create subscription if plan provided
        if (request.PlanId.HasValue)
        {
            var plan = await _unitOfWork.Plans.GetByIdAsync(request.PlanId.Value, cancellationToken);
            if (plan == null)
                throw new NotFoundException($"Plan with ID {request.PlanId} not found");

            // Use provided start date or default to now
            var startDate = request.StartDate ?? DateTime.UtcNow;

            var subscription = Subscription.Create(
                subscriber.Id,
                plan.Id,
                startDate,
                plan.BillingCycleDays
            );

            await _unitOfWork.Subscriptions.AddAsync(subscription, cancellationToken);

            // Get first MikroTik device
            var mikroTikDevices = await _unitOfWork.MikroTikDevices.GetAllAsync(cancellationToken);
            var mikroTikDevice = mikroTikDevices.FirstOrDefault(d => d.IsActive);

            // Auto-create MikroTik credentials if requested and plan selected
            var pppUsername = request.PppUsername;
            var pppPassword = request.PppPassword;

            if (request.AutoCreateMikroTik && string.IsNullOrEmpty(pppUsername))
            {
                // Generate username from subscriber name (sanitized) + random suffix
                pppUsername = GeneratePppUsername(request.FullName);
                pppPassword = GeneratePassword();
                _logger.LogInformation("Auto-generated MikroTik credentials for subscriber: {Username}", pppUsername);
            }

            // Create PPPoE account if credentials available
            if (!string.IsNullOrEmpty(pppUsername) && !string.IsNullOrEmpty(pppPassword))
            {
                var deviceId = mikroTikDevice?.Id ?? Guid.Empty;

                var pppoeAccount = PppoeAccount.Create(
                    subscriber.Id,
                    subscription.Id,
                    deviceId,
                    pppUsername,
                    pppPassword,
                    plan.MikroTikProfileName
                );

                await _unitOfWork.PppoeAccounts.AddAsync(pppoeAccount, cancellationToken);
                
                // Update subscriber with MikroTik username for easy reference
                subscriber.UpdateNetworkInfo(subscriber.MacAddress, subscriber.IpAddress, pppUsername);

                // Sync with MikroTik synchronously to ensure user is created
                // Use appsettings.json MikroTik settings (pass null to use defaults)
                try
                {
                    // Ensure profile exists on MikroTik before adding user
                    await EnsureProfileExistsOnMikroTik(plan, cancellationToken);
                    
                    // Add PPPoE user to MikroTik
                    await SyncPppoeAccountWithMikroTik(pppoeAccount, cancellationToken);
                    
                    _logger.LogInformation("MikroTik sync completed for PPPoE account {Username}", pppoeAccount.Username);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MikroTik sync failed for subscriber PPPoE account {Username}. Account saved to DB but not synced.", pppoeAccount.Username);
                    // Don't throw - allow subscriber creation to succeed even if MikroTik sync fails
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

    private string GeneratePppUsername(string fullName)
    {
        // Sanitize name: remove special chars, replace spaces with underscores, lowercase
        var sanitized = new string(fullName
            .Where(c => char.IsLetterOrDigit(c) || c == ' ')
            .ToArray())
            .Replace(" ", "_")
            .ToLowerInvariant();

        // Limit length and add random suffix for uniqueness
        if (sanitized.Length > 12) sanitized = sanitized.Substring(0, 12);
        var suffix = new Random().Next(1000, 9999);
        return $"{sanitized}_{suffix}";
    }

    private async Task EnsureProfileExistsOnMikroTik(Plan plan, CancellationToken cancellationToken)
    {
        try
        {
            // Use empty connection request - MikroTikService will use appsettings.json defaults
            var connectionRequest = new MikroTikConnectionRequest();

            // Check if profile exists
            var profilesResult = await _mikroTikService.GetPppProfilesAsync(connectionRequest, cancellationToken);
            if (profilesResult.Success && profilesResult.Data != null)
            {
                var profileExists = profilesResult.Data.Any(p => p.Name == plan.MikroTikProfileName);
                if (!profileExists)
                {
                    // Create the profile with rate limit based on plan speed
                    var rateLimit = $"{plan.SpeedMbps}M/{plan.SpeedMbps}M";
                    var addProfileRequest = new AddProfileRequest
                    {
                        ProfileName = plan.MikroTikProfileName,
                        RateLimit = rateLimit
                    };

                    var addResult = await _mikroTikService.AddPppProfileAsync(addProfileRequest, cancellationToken);
                    if (addResult.Success)
                    {
                        _logger.LogInformation("Created MikroTik profile '{ProfileName}' with rate limit {RateLimit}", 
                            plan.MikroTikProfileName, rateLimit);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create MikroTik profile '{ProfileName}': {Error}", 
                            plan.MikroTikProfileName, addResult.Error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring MikroTik profile exists for plan {PlanId}", plan.Id);
        }
    }

    private async Task SyncPppoeAccountWithMikroTik(PppoeAccount pppoeAccount, CancellationToken cancellationToken)
    {
        try
        {
            // Use empty connection request - MikroTikService will use appsettings.json defaults
            var addRequest = new AddPppUserRequest
            {
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
            subscriber.MikroTikUsername,
            subscriber.MacAddress,
            subscriber.IpAddress,
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
