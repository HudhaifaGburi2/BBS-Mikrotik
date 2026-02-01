using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Plans.CreatePlan;
using BroadbandBilling.Application.UseCases.Plans.DTOs;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlansController : ControllerBase
{
    private readonly CreatePlanHandler _createPlanHandler;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PlansController(
        CreatePlanHandler createPlanHandler,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _createPlanHandler = createPlanHandler;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlanDto>>>> GetAll([FromQuery] bool activeOnly = true)
    {
        var plans = activeOnly
            ? await _unitOfWork.Plans.GetActivePlansAsync()
            : await _unitOfWork.Plans.GetAllAsync();

        var planDtos = _mapper.Map<IEnumerable<PlanDto>>(plans);
        
        return Ok(ApiResponse<IEnumerable<PlanDto>>
            .SuccessResponse(planDtos, "Plans retrieved successfully"));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PlanDto>>> GetById(Guid id)
    {
        var plan = await _unitOfWork.Plans.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound(ApiResponse<PlanDto>
                .FailureResponse("Plan not found", $"Plan with ID {id} not found"));
        }

        var planDto = _mapper.Map<PlanDto>(plan);
        
        return Ok(ApiResponse<PlanDto>.SuccessResponse(planDto, "Plan retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PlanDto>>> Create([FromBody] CreatePlanCommand command)
    {
        var validator = new CreatePlanValidator();
        var validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PlanDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _createPlanHandler.HandleAsync(command);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Plan.Id },
            ApiResponse<PlanDto>.SuccessResponse(result.Plan, "Plan created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PlanDto>>> Update(Guid id, [FromBody] UpdatePlanDto dto)
    {
        var plan = await _unitOfWork.Plans.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound(ApiResponse<PlanDto>
                .FailureResponse("Plan not found", $"Plan with ID {id} not found"));
        }

        _unitOfWork.Plans.Update(plan);
        await _unitOfWork.CommitAsync();

        var planDto = _mapper.Map<PlanDto>(plan);
        
        return Ok(ApiResponse<PlanDto>.SuccessResponse(planDto, "Plan updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var plan = await _unitOfWork.Plans.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound(ApiResponse<bool>
                .FailureResponse("Plan not found", $"Plan with ID {id} not found"));
        }

        plan.Deactivate();
        _unitOfWork.Plans.Update(plan);
        await _unitOfWork.CommitAsync();
        
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Plan deactivated successfully"));
    }
}
