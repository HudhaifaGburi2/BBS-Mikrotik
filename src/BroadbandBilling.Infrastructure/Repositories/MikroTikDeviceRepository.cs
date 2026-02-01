using Microsoft.EntityFrameworkCore;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Infrastructure.Data;

namespace BroadbandBilling.Infrastructure.Repositories;

public class MikroTikDeviceRepository : GenericRepository<MikroTikDevice>, IMikroTikDeviceRepository
{
    public MikroTikDeviceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MikroTikDevice>> GetActiveDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<MikroTikDevice?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Name == name, cancellationToken);
    }
}
