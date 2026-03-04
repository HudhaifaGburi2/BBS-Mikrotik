using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BroadbandBilling.Infrastructure.BackgroundJobs;

public class DataUsageCheckJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<DataUsageCheckJob> _logger;
    
    // Track last known session bytes to calculate delta (handles reconnects)
    private static readonly ConcurrentDictionary<string, long> _lastSessionBytes = new();

    public DataUsageCheckJob(
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IMikroTikService mikroTikService,
        ILogger<DataUsageCheckJob> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting data usage check job");

        try
        {
            // Get all active sessions from MikroTik first (single API call)
            var activeSessionsResult = await _mikroTikService.GetActiveSessionsAsync(
                new MikroTikConnectionRequest(), cancellationToken);
            
            var activeSessions = activeSessionsResult.Success && activeSessionsResult.Data != null 
                ? activeSessionsResult.Data.ToDictionary(s => s.Name, s => s)
                : new Dictionary<string, ActiveSessionDto>();

            _logger.LogInformation("Found {Count} active sessions on MikroTik", activeSessions.Count);

            var activeSubscriptions = await _dbContext.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.Subscriber)
                .Where(s => s.Status == SubscriptionStatus.Active && !s.DataLimitExceeded)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} active subscriptions to check", activeSubscriptions.Count);

            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    if (subscription.Plan == null || !subscription.Plan.HasDataLimit())
                    {
                        continue;
                    }

                    var pppoeAccount = await _dbContext.PppoeAccounts
                        .FirstOrDefaultAsync(p => p.SubscriberId == subscription.SubscriberId, cancellationToken);

                    if (pppoeAccount == null)
                    {
                        continue;
                    }

                    // Check if user is currently online
                    if (activeSessions.TryGetValue(pppoeAccount.Username, out var session))
                    {
                        var currentSessionBytes = session.BytesUsed;
                        var lastKnownBytes = _lastSessionBytes.GetValueOrDefault(pppoeAccount.Username, 0);
                        
                        // Calculate delta (new bytes since last check)
                        // If current < last, user reconnected (session reset), so current is the delta
                        long deltaBytes;
                        if (currentSessionBytes < lastKnownBytes)
                        {
                            // User reconnected - session bytes reset
                            // Add the current session bytes as new usage
                            deltaBytes = currentSessionBytes;
                            _logger.LogDebug("User {Username} reconnected. Adding {Bytes} bytes", 
                                pppoeAccount.Username, deltaBytes);
                        }
                        else
                        {
                            // Normal case - calculate delta
                            deltaBytes = currentSessionBytes - lastKnownBytes;
                        }
                        
                        // Update last known bytes
                        _lastSessionBytes[pppoeAccount.Username] = currentSessionBytes;
                        
                        // Accumulate usage in SQL
                        if (deltaBytes > 0)
                        {
                            subscription.AccumulateDataUsage(deltaBytes);
                            _logger.LogDebug("Accumulated {DeltaBytes} bytes for {Username}. Total: {TotalBytes}",
                                deltaBytes, pppoeAccount.Username, subscription.DataUsedBytes);
                        }

                        // Check if quota exceeded
                        if (subscription.HasExceededDataLimit(subscription.Plan.DataLimitGB))
                        {
                            _logger.LogWarning("Subscription {SubscriptionId} exceeded data limit ({UsedGB:F2} GB / {LimitGB} GB). Suspending user.",
                                subscription.Id, subscription.GetDataUsedGB(), subscription.Plan.DataLimitGB);

                            subscription.MarkDataLimitExceeded();
                            subscription.Suspend("تجاوز حد البيانات المسموح");

                            // Disable PPP secret on MikroTik
                            var deactivateResult = await _mikroTikService.DeactivateUserAsync(
                                new DeletePppUserRequest { PppUsername = pppoeAccount.Username },
                                cancellationToken);

                            if (deactivateResult.Success)
                            {
                                _logger.LogInformation("Deactivated MikroTik user {Username} due to data limit exceeded",
                                    pppoeAccount.Username);
                                
                                pppoeAccount.Disable();
                                _unitOfWork.PppoeAccounts.Update(pppoeAccount);
                                
                                // Disconnect active session
                                await _mikroTikService.DisconnectUserAsync(
                                    new DeletePppUserRequest { PppUsername = pppoeAccount.Username },
                                    cancellationToken);
                            }
                            else
                            {
                                _logger.LogError("Failed to deactivate MikroTik user {Username}: {Error}",
                                    pppoeAccount.Username, deactivateResult.Message);
                            }
                        }

                        _unitOfWork.Subscriptions.Update(subscription);
                    }
                    // User is offline - no action needed, SQL already has accumulated usage
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking data usage for subscription {SubscriptionId}",
                        subscription.Id);
                }
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Data usage check job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during data usage check job");
            throw;
        }
    }
    
    /// <summary>
    /// Clear cached session bytes for a user (call when subscription is renewed)
    /// </summary>
    public static void ClearUserSessionCache(string username)
    {
        _lastSessionBytes.TryRemove(username, out _);
    }
}
