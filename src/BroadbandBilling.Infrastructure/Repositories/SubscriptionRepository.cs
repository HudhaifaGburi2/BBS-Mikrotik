using Microsoft.EntityFrameworkCore;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using BroadbandBilling.Infrastructure.Data;

namespace BroadbandBilling.Infrastructure.Repositories;

public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Subscription?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Subscriber)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Plan)
            .Where(s => s.SubscriberId == subscriberId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetByStatusAsync(SubscriptionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Subscriber)
            .Include(s => s.Plan)
            .Where(s => s.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(s => s.Subscriber)
            .Include(s => s.Plan)
            .Where(s => (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Suspended) 
                     && EF.Property<DateTime>(s.BillingPeriod, "EndDate") < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(int daysThreshold, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var thresholdDate = now.AddDays(daysThreshold);
        
        return await _dbSet
            .Include(s => s.Subscriber)
            .Include(s => s.Plan)
            .Where(s => s.Status == SubscriptionStatus.Active 
                     && EF.Property<DateTime>(s.BillingPeriod, "EndDate") >= now
                     && EF.Property<DateTime>(s.BillingPeriod, "EndDate") <= thresholdDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetActiveSubscriptionBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.SubscriberId == subscriberId 
                                   && s.Status == SubscriptionStatus.Active, 
                                   cancellationToken);
    }
}
