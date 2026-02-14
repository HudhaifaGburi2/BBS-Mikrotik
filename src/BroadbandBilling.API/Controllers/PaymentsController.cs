using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Billing.ProcessPayment;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetAllAsync(cancellationToken);
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByIdAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpGet("invoice/{invoiceId}")]
    public async Task<IActionResult> GetByInvoice(
        Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByInvoiceIdAsync(invoiceId, cancellationToken);
        return Ok(result.Data);
    }

    [HttpGet("subscriber/{subscriberId}")]
    public async Task<IActionResult> GetBySubscriber(
        Guid subscriberId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetBySubscriberIdAsync(subscriberId, cancellationToken);
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var result = await _paymentService.ProcessAsync(command, cancellationToken);
        if (!result.Success)
            return BadRequest(new { error = result.Message, errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }
}
