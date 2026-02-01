using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface IPlanRepository : IRepository<Plan>
{
    Task<IEnumerable<Plan>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    Task<Plan?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> PlanNameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
