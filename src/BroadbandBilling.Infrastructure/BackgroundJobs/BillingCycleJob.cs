using BroadbandBilling.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.BackgroundJobs;

public class BillingCycleJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BillingCycleJob> _logger;

    public BillingCycleJob(IUnitOfWork unitOfWork, ILogger<BillingCycleJob> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting billing cycle job");

        try
        {
            var activeSubscriptions = await _unitOfWork.Subscriptions
                .GetByStatusAsync(Domain.Enums.SubscriptionStatus.Active);

            _logger.LogInformation("Processing {Count} active subscriptions for billing", 
                activeSubscriptions.Count());

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Billing cycle job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during billing cycle job");
            throw;
        }
    }
}
