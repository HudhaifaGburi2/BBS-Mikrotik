using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetByStatusAsync(SubscriptionStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(int daysThreshold, CancellationToken cancellationToken = default);
    Task<Subscription?> GetActiveSubscriptionBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default);
}
