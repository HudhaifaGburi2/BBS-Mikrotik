using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface IUsageLogRepository : IRepository<UsageLog>
{
    Task<IEnumerable<UsageLog>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageLog>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageLog>> GetByPppoeAccountIdAsync(Guid pppoeAccountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageLog>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageLog>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<double> GetTotalDataUsageAsync(Guid subscriberId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
