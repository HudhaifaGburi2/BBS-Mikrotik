using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
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
    private readonly ProcessPaymentHandler _processPaymentHandler;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentsController(
        ProcessPaymentHandler processPaymentHandler,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _processPaymentHandler = processPaymentHandler;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetAll()
    {
        var payments = await _unitOfWork.Payments.GetAllAsync();
        var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);
        
        return Ok(ApiResponse<IEnumerable<PaymentDto>>
            .SuccessResponse(paymentDtos, "Payments retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(Guid id)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id);
        if (payment == null)
        {
            return NotFound(ApiResponse<PaymentDto>
                .FailureResponse("Payment not found", $"Payment with ID {id} not found"));
        }

        var paymentDto = _mapper.Map<PaymentDto>(payment);
        
        return Ok(ApiResponse<PaymentDto>.SuccessResponse(paymentDto, "Payment retrieved successfully"));
    }

    [HttpGet("invoice/{invoiceId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetByInvoice(Guid invoiceId)
    {
        var payments = await _unitOfWork.Payments.GetByInvoiceIdAsync(invoiceId);
        var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);
        
        return Ok(ApiResponse<IEnumerable<PaymentDto>>
            .SuccessResponse(paymentDtos, "Payments retrieved successfully"));
    }

    [HttpGet("subscriber/{subscriberId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetBySubscriber(Guid subscriberId)
    {
        var payments = await _unitOfWork.Payments.GetBySubscriberIdAsync(subscriberId);
        var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);
        
        return Ok(ApiResponse<IEnumerable<PaymentDto>>
            .SuccessResponse(paymentDtos, "Payments retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Create([FromBody] ProcessPaymentCommand command)
    {
        var result = await _processPaymentHandler.HandleAsync(command);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Payment.Id },
            ApiResponse<PaymentDto>.SuccessResponse(result.Payment, "Payment processed successfully"));
    }
}
