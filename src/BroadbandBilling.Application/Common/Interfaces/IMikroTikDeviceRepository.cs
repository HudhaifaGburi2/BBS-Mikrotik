using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface IMikroTikDeviceRepository : IRepository<MikroTikDevice>
{
    Task<IEnumerable<MikroTikDevice>> GetActiveDevicesAsync(CancellationToken cancellationToken = default);
    Task<MikroTikDevice?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
