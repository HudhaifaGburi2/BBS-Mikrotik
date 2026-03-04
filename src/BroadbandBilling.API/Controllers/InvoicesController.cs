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
        // Check if user is a subscriber - if so, return only their invoices
        var subscriberIdClaim = User.FindFirst("SubscriberId")?.Value;
        if (!string.IsNullOrEmpty(subscriberIdClaim) && Guid.TryParse(subscriberIdClaim, out var subscriberId))
        {
            var myResult = await _invoiceService.GetBySubscriberIdAsync(subscriberId, cancellationToken);
            return Ok(myResult.Data);
        }
        
        var result = await _invoiceService.GetAllAsync(cancellationToken);
        return Ok(result.Data);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyInvoices(CancellationToken cancellationToken)
    {
        var subscriberIdClaim = User.FindFirst("SubscriberId")?.Value;
        if (string.IsNullOrEmpty(subscriberIdClaim) || !Guid.TryParse(subscriberIdClaim, out var subscriberId))
        {
            return Unauthorized(new { error = "لا يوجد معرف مشترك في الجلسة" });
        }

        var result = await _invoiceService.GetBySubscriberIdAsync(subscriberId, cancellationToken);
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
