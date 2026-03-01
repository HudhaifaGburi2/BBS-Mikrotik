using Microsoft.EntityFrameworkCore;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Infrastructure.Data;

namespace BroadbandBilling.Infrastructure.Repositories;

public class SubscriberRepository : GenericRepository<Subscriber>, ISubscriberRepository
{
    public SubscriberRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Subscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Email == email, cancellationToken);
    }

    public async Task<Subscriber?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<Subscriber?> GetWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Subscriptions)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(s => s.Email == email, cancellationToken);
    }

    public async Task<PagedResult<Subscriber>> GetPagedAsync(int page, int pageSize, string? search = null, bool? isActive = null, bool? hasActiveSubscription = null, string? sortBy = null, bool sortDescending = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(s => s.Subscriptions)
            .Include(s => s.PppoeAccounts)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => 
                s.FullName.Contains(search) ||
                s.Email.Contains(search) ||
                s.PhoneNumber.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (hasActiveSubscription.HasValue)
        {
            // Use inline expression instead of method call for EF Core translation
            var now = DateTime.UtcNow;
            if (hasActiveSubscription.Value)
            {
                query = query.Where(s => s.Subscriptions.Any(sub => 
                    sub.Status == Domain.Enums.SubscriptionStatus.Active && 
                    sub.BillingPeriod.EndDate >= now));
            }
            else
            {
                query = query.Where(s => !s.Subscriptions.Any(sub => 
                    sub.Status == Domain.Enums.SubscriptionStatus.Active && 
                    sub.BillingPeriod.EndDate >= now));
            }
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLowerInvariant())
            {
                case "fullname":
                    query = sortDescending ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName);
                    break;
                case "email":
                    query = sortDescending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email);
                    break;
                case "createdat":
                    query = sortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt);
                    break;
                default:
                    query = sortDescending ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(s => s.FullName);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Subscriber>(
            items,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling((double)totalCount / pageSize)
        );
    }

    public void Delete(Subscriber subscriber)
    {
        _dbSet.Remove(subscriber);
    }
}
