using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.UseCases.Plans.CreatePlan;
using BroadbandBilling.Application.UseCases.Plans.DTOs;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface IPlanService
{
    Task<ApiResponse<IEnumerable<PlanDto>>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<ApiResponse<PlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PlanDto>> CreateAsync(CreatePlanCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<PlanDto>> UpdateAsync(Guid id, UpdatePlanDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
