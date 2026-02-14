using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await _planService.GetAllAsync(activeOnly, cancellationToken);
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _planService.GetByIdAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePlanCommand command, CancellationToken cancellationToken)
    {
        var result = await _planService.CreateAsync(command, cancellationToken);
        if (!result.Success)
            return BadRequest(new { error = result.Message, errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdatePlanDto dto, CancellationToken cancellationToken)
    {
        var result = await _planService.UpdateAsync(id, dto, cancellationToken);
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _planService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return Ok(new { message = "تم إلغاء تفعيل الباقة بنجاح" });
    }
}
