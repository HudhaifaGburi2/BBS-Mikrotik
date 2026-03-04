using AutoMapper;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Plans.CreatePlan;
using BroadbandBilling.Application.UseCases.Plans.DTOs;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.Services;

public class PlanService : IPlanService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CreatePlanHandler _createPlanHandler;
    private readonly ILogger<PlanService> _logger;

    public PlanService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CreatePlanHandler createPlanHandler,
        ILogger<PlanService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createPlanHandler = createPlanHandler;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<PlanDto>>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var plans = activeOnly
            ? await _unitOfWork.Plans.GetActivePlansAsync(cancellationToken)
            : await _unitOfWork.Plans.GetAllAsync(cancellationToken);

        var planDtos = _mapper.Map<IEnumerable<PlanDto>>(plans);
        return ApiResponse<IEnumerable<PlanDto>>.SuccessResponse(planDtos, "Plans retrieved successfully");
    }

    public async Task<ApiResponse<PlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.Plans.GetByIdAsync(id, cancellationToken);
        if (plan == null)
        {
            return ApiResponse<PlanDto>.FailureResponse("Plan not found", $"Plan with ID {id} not found");
        }

        var planDto = _mapper.Map<PlanDto>(plan);
        return ApiResponse<PlanDto>.SuccessResponse(planDto, "Plan retrieved successfully");
    }

    public async Task<ApiResponse<PlanDto>> CreateAsync(CreatePlanCommand command, CancellationToken cancellationToken = default)
    {
        var validator = new CreatePlanValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResponse<PlanDto>.FailureResponse("Validation failed", errors);
        }

        var result = await _createPlanHandler.HandleAsync(command, cancellationToken);
        return ApiResponse<PlanDto>.SuccessResponse(result.Plan, "Plan created successfully");
    }

    public async Task<ApiResponse<PlanDto>> UpdateAsync(Guid id, UpdatePlanDto dto, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.Plans.GetByIdAsync(id, cancellationToken);
        if (plan == null)
        {
            return ApiResponse<PlanDto>.FailureResponse("Plan not found", $"Plan with ID {id} not found");
        }

        plan.UpdateDetails(dto.Name, dto.Description, dto.Price, dto.SpeedMbps, dto.DataLimitGB, dto.Currency, dto.BillingCycleHours);
        _unitOfWork.Plans.Update(plan);
        await _unitOfWork.CommitAsync(cancellationToken);

        var planDto = _mapper.Map<PlanDto>(plan);
        return ApiResponse<PlanDto>.SuccessResponse(planDto, "Plan updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.Plans.GetByIdAsync(id, cancellationToken);
        if (plan == null)
        {
            return ApiResponse<bool>.FailureResponse("Plan not found", $"Plan with ID {id} not found");
        }

        plan.Deactivate();
        _unitOfWork.Plans.Update(plan);
        await _unitOfWork.CommitAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Plan deactivated successfully");
    }
}
