using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Billing.ProcessPayment;
using BroadbandBilling.Application.UseCases.Payments.DTOs;

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
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByIdAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("invoice/{invoiceId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetByInvoice(
        Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByInvoiceIdAsync(invoiceId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("subscriber/{subscriberId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetBySubscriber(
        Guid subscriberId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetBySubscriberIdAsync(subscriberId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Create(
        [FromBody] ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var result = await _paymentService.ProcessAsync(command, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
}
