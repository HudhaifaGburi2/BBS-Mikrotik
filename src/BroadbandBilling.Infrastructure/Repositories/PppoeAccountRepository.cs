using Microsoft.EntityFrameworkCore;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Infrastructure.Data;

namespace BroadbandBilling.Infrastructure.Repositories;

public class PppoeAccountRepository : GenericRepository<PppoeAccount>, IPppoeAccountRepository
{
    public PppoeAccountRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PppoeAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Subscriber)
            .Include(p => p.Subscription)
            .Include(p => p.MikroTikDevice)
            .FirstOrDefaultAsync(p => p.Username == username, cancellationToken);
    }

    public async Task<PppoeAccount?> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Subscription)
            .Include(p => p.MikroTikDevice)
            .FirstOrDefaultAsync(p => p.SubscriberId == subscriberId, cancellationToken);
    }

    public async Task<PppoeAccount?> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Subscriber)
            .Include(p => p.MikroTikDevice)
            .FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId, cancellationToken);
    }

    public async Task<IEnumerable<PppoeAccount>> GetEnabledAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Subscriber)
            .Include(p => p.MikroTikDevice)
            .Where(p => p.IsEnabled)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PppoeAccount>> GetOnlineAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Subscriber)
            .Include(p => p.MikroTikDevice)
            .Where(p => p.IsEnabled 
                     && p.LastConnectedAt != null
                     && (p.LastDisconnectedAt == null || p.LastConnectedAt > p.LastDisconnectedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(p => p.Username == username, cancellationToken);
    }
}
