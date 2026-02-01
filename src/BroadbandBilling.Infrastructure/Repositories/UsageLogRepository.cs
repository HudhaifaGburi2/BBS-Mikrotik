using Microsoft.EntityFrameworkCore;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Infrastructure.Data;

namespace BroadbandBilling.Infrastructure.Repositories;

public class UsageLogRepository : GenericRepository<UsageLog>, IUsageLogRepository
{
    public UsageLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UsageLog>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.SubscriberId == subscriberId)
            .OrderByDescending(u => u.SessionStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsageLog>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.SubscriptionId == subscriptionId)
            .OrderByDescending(u => u.SessionStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsageLog>> GetByPppoeAccountIdAsync(Guid pppoeAccountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.PppoeAccountId == pppoeAccountId)
            .OrderByDescending(u => u.SessionStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsageLog>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Subscriber)
            .Include(u => u.PppoeAccount)
            .Where(u => u.SessionEnd == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsageLog>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Subscriber)
            .Where(u => u.SessionStart >= startDate && u.SessionStart <= endDate)
            .OrderByDescending(u => u.SessionStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<double> GetTotalDataUsageAsync(Guid subscriberId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var totalBytes = await _dbSet
            .Where(u => u.SubscriberId == subscriberId 
                     && u.SessionStart >= startDate 
                     && u.SessionStart <= endDate)
            .SumAsync(u => u.TotalBytes, cancellationToken);

        return totalBytes / (1024.0 * 1024.0 * 1024.0);
    }
}
