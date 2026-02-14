using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Billing.GenerateInvoice;
using BroadbandBilling.Application.UseCases.Invoices.DTOs;

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
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetByIdAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("subscriber/{subscriberId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceDto>>>> GetBySubscriber(
        Guid subscriberId, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetBySubscriberIdAsync(subscriberId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceDto>>>> GetOverdue(CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetOverdueAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Create(
        [FromBody] GenerateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GenerateAsync(command, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
}
