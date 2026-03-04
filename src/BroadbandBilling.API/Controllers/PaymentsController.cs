using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Payments.Commands;
using BroadbandBilling.Application.UseCases.Billing.ProcessPayment;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IMediator _mediator;
    private readonly IGeideaPaymentService _geideaService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        IMediator mediator,
        IGeideaPaymentService geideaService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _mediator = mediator;
        _geideaService = geideaService;
        _logger = logger;
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

    [HttpPost("create-session")]
    public async Task<IActionResult> CreatePaymentSession(
        [FromBody] CreatePaymentSessionRequest request, 
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var subscriberIdClaim = User.FindFirst("SubscriberId")?.Value;
        Guid subscriberId;
        
        if (!string.IsNullOrEmpty(subscriberIdClaim) && Guid.TryParse(subscriberIdClaim, out var parsedSubId))
        {
            subscriberId = parsedSubId;
        }
        else
        {
            subscriberId = request.SubscriberId ?? Guid.Empty;
        }

        if (subscriberId == Guid.Empty)
        {
            return BadRequest(new { error = "معرف المشترك مطلوب" });
        }

        var command = new CreatePaymentSessionCommand(
            request.PlanId,
            subscriberId,
            request.PaymentMethod
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new
        {
            success = true,
            subscriptionId = result.SubscriptionId,
            transactionId = result.TransactionId,
            redirectUrl = result.RedirectUrl
        });
    }

    [HttpPost("geidea/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> GeideaWebhook(CancellationToken cancellationToken)
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync(cancellationToken);

            _logger.LogInformation("Received Geidea webhook: {Payload}", payload);

            var signature = Request.Headers["X-Signature"].FirstOrDefault() ?? 
                           Request.Headers["Signature"].FirstOrDefault();

            if (!string.IsNullOrEmpty(signature) && !_geideaService.ValidateWebhookSignature(payload, signature))
            {
                _logger.LogWarning("Invalid Geidea webhook signature");
                return Unauthorized(new { error = "Invalid signature" });
            }

            var webhookData = JsonSerializer.Deserialize<GeideaWebhookData>(payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (webhookData?.Order == null)
            {
                _logger.LogWarning("Invalid Geidea webhook payload structure");
                return BadRequest(new { error = "Invalid payload" });
            }

            var command = new HandleGeideaWebhookCommand(
                OrderId: webhookData.Order.OrderId ?? "",
                TransactionId: webhookData.Order.Transactions?.FirstOrDefault()?.TransactionId,
                Status: webhookData.Order.Status ?? "",
                Amount: webhookData.Order.Amount ?? 0,
                Currency: webhookData.Order.Currency ?? "SAR",
                MerchantReferenceId: webhookData.Order.MerchantReferenceId,
                CardBrand: webhookData.Order.PaymentMethod?.Brand,
                CardLast4: webhookData.Order.PaymentMethod?.MaskedCardNumber?[^4..],
                ResponseCode: webhookData.Order.DetailedStatus,
                ResponseMessage: webhookData.Order.DetailedStatus,
                RawPayload: payload
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Geidea webhook processing failed: {Message}", result.Message);
                return BadRequest(new { error = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Geidea webhook");
            return StatusCode(500, new { error = "Internal error processing webhook" });
        }
    }

    [HttpGet("verify/{orderId}")]
    public async Task<IActionResult> VerifyPayment(string orderId, CancellationToken cancellationToken)
    {
        var result = await _geideaService.VerifyPaymentAsync(orderId, cancellationToken);

        return Ok(new
        {
            success = result.Success,
            orderId = result.OrderId,
            transactionId = result.TransactionId,
            status = result.Status,
            amount = result.Amount,
            currency = result.Currency,
            cardBrand = result.CardBrand,
            cardLast4 = result.CardLast4,
            error = result.ErrorMessage
        });
    }
}

public record CreatePaymentSessionRequest(
    Guid PlanId,
    Guid? SubscriberId,
    string PaymentMethod
);

public class GeideaWebhookData
{
    public GeideaWebhookOrder? Order { get; set; }
}

public class GeideaWebhookOrder
{
    public string? OrderId { get; set; }
    public string? MerchantReferenceId { get; set; }
    public string? Status { get; set; }
    public string? DetailedStatus { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public GeideaWebhookPaymentMethod? PaymentMethod { get; set; }
    public List<GeideaWebhookTransaction>? Transactions { get; set; }
}

public class GeideaWebhookPaymentMethod
{
    public string? Brand { get; set; }
    public string? MaskedCardNumber { get; set; }
}

public class GeideaWebhookTransaction
{
    public string? TransactionId { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
}
