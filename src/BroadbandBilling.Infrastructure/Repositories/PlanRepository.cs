using Microsoft.EntityFrameworkCore;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Infrastructure.Data;

namespace BroadbandBilling.Infrastructure.Repositories;

public class PlanRepository : GenericRepository<Plan>, IPlanRepository
{
    public PlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Plan>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => EF.Property<decimal>(p.Price, "Amount"))
            .ToListAsync(cancellationToken);
    }

    public async Task<Plan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<bool> PlanNameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (excludeId.HasValue)
        {
            return await _dbSet.AnyAsync(p => p.Name == name && p.Id != excludeId.Value, cancellationToken);
        }
        
        return await _dbSet.AnyAsync(p => p.Name == name, cancellationToken);
    }
}
