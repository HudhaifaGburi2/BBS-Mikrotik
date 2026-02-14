using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlanDto>>>> GetAll(
        [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await _planService.GetAllAsync(activeOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PlanDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _planService.GetByIdAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PlanDto>>> Create(
        [FromBody] CreatePlanCommand command, CancellationToken cancellationToken)
    {
        var result = await _planService.CreateAsync(command, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PlanDto>>> Update(
        Guid id, [FromBody] UpdatePlanDto dto, CancellationToken cancellationToken)
    {
        var result = await _planService.UpdateAsync(id, dto, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _planService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
