using BroadbandBilling.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.BackgroundJobs;

public class SuspendExpiredSubscriptionsJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikrotikService;
    private readonly ILogger<SuspendExpiredSubscriptionsJob> _logger;

    public SuspendExpiredSubscriptionsJob(
        IUnitOfWork unitOfWork,
        IMikroTikService mikrotikService,
        ILogger<SuspendExpiredSubscriptionsJob> logger)
    {
        _unitOfWork = unitOfWork;
        _mikrotikService = mikrotikService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting suspend expired subscriptions job");

        try
        {
            var expiredSubscriptions = await _unitOfWork.Subscriptions.GetExpiredSubscriptionsAsync();

            _logger.LogInformation("Found {Count} expired subscriptions to process", 
                expiredSubscriptions.Count());

            foreach (var subscription in expiredSubscriptions)
            {
                try
                {
                    subscription.MarkAsExpired();
                    _unitOfWork.Subscriptions.Update(subscription);

                    var pppoeAccount = await _unitOfWork.PppoeAccounts
                        .GetBySubscriptionIdAsync(subscription.Id);

                    if (pppoeAccount != null && pppoeAccount.IsEnabled)
                    {
                        // await _mikrotikService.DisablePppoeUserAsync(pppoeAccount.MikroTikDeviceId, pppoeAccount.Username);
                        // Skip for now as DisablePppoeUserAsync not implemented
                        pppoeAccount.Disable();
                        _unitOfWork.PppoeAccounts.Update(pppoeAccount);
                    }

                    _logger.LogInformation("Suspended expired subscription {SubscriptionId}", subscription.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error suspending subscription {SubscriptionId}", subscription.Id);
                }
            }

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Suspend expired subscriptions job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during suspend expired subscriptions job");
            throw;
        }
    }
}
