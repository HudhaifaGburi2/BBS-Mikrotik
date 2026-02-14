using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Billing.GenerateInvoice;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetAllAsync(cancellationToken);
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetByIdAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpGet("subscriber/{subscriberId}")]
    public async Task<IActionResult> GetBySubscriber(
        Guid subscriberId, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetBySubscriberIdAsync(subscriberId, cancellationToken);
        return Ok(result.Data);
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue(CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetOverdueAsync(cancellationToken);
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] GenerateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GenerateAsync(command, cancellationToken);
        if (!result.Success)
            return BadRequest(new { error = result.Message, errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }
}
