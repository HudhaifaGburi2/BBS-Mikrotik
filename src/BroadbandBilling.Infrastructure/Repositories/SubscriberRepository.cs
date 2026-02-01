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
}
