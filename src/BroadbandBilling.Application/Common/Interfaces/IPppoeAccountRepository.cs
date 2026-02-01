using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface IPppoeAccountRepository : IRepository<PppoeAccount>
{
    Task<PppoeAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<PppoeAccount?> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default);
    Task<PppoeAccount?> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PppoeAccount>> GetEnabledAccountsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PppoeAccount>> GetOnlineAccountsAsync(CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
}
