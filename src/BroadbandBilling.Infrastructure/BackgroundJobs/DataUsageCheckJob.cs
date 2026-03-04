using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.BackgroundJobs;

public class DataUsageCheckJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<DataUsageCheckJob> _logger;

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

                    var sessionResult = await _mikroTikService.GetUserSessionAsync(
                        new DeletePppUserRequest { PppUsername = pppoeAccount.Username },
                        cancellationToken);

                    if (!sessionResult.Success || sessionResult.Data == null)
                    {
                        continue;
                    }

                    var session = sessionResult.Data;
                    var totalBytes = (session.LimitBytesIn ?? 0) + (session.LimitBytesOut ?? 0);

                    subscription.UpdateDataUsage(totalBytes);

                    if (subscription.HasExceededDataLimit(subscription.Plan.DataLimitGB))
                    {
                        _logger.LogWarning("Subscription {SubscriptionId} exceeded data limit ({UsedGB:F2} GB / {LimitGB} GB). Deactivating MikroTik user.",
                            subscription.Id, subscription.GetDataUsedGB(), subscription.Plan.DataLimitGB);

                        subscription.MarkDataLimitExceeded();

                        var deactivateResult = await _mikroTikService.DeactivateUserAsync(
                            new DeletePppUserRequest { PppUsername = pppoeAccount.Username },
                            cancellationToken);

                        if (deactivateResult.Success)
                        {
                            _logger.LogInformation("Deactivated MikroTik user {Username} due to data limit exceeded",
                                pppoeAccount.Username);
                            
                            pppoeAccount.Disable();
                            _unitOfWork.PppoeAccounts.Update(pppoeAccount);
                        }
                        else
                        {
                            _logger.LogError("Failed to deactivate MikroTik user {Username}: {Error}",
                                pppoeAccount.Username, deactivateResult.Message);
                        }
                    }

                    _unitOfWork.Subscriptions.Update(subscription);
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
}
