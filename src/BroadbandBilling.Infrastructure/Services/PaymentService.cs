using AutoMapper;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Billing.ProcessPayment;
using BroadbandBilling.Application.UseCases.Payments.DTOs;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ProcessPaymentHandler _processPaymentHandler;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ProcessPaymentHandler processPaymentHandler,
        ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _processPaymentHandler = processPaymentHandler;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetAllAsync(cancellationToken);
        var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);
        return ApiResponse<IEnumerable<PaymentDto>>.SuccessResponse(paymentDtos, "Payments retrieved successfully");
    }

    public async Task<ApiResponse<PaymentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id, cancellationToken);
        if (payment == null)
        {
            return ApiResponse<PaymentDto>.FailureResponse("Payment not found", $"Payment with ID {id} not found");
        }

        var paymentDto = _mapper.Map<PaymentDto>(payment);
        return ApiResponse<PaymentDto>.SuccessResponse(paymentDto, "Payment retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetByInvoiceIdAsync(invoiceId, cancellationToken);
        var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);
        return ApiResponse<IEnumerable<PaymentDto>>.SuccessResponse(paymentDtos, "Payments retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetBySubscriberIdAsync(subscriberId, cancellationToken);
        var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);
        return ApiResponse<IEnumerable<PaymentDto>>.SuccessResponse(paymentDtos, "Payments retrieved successfully");
    }

    public async Task<ApiResponse<PaymentDto>> ProcessAsync(ProcessPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _processPaymentHandler.HandleAsync(command, cancellationToken);
        return ApiResponse<PaymentDto>.SuccessResponse(result.Payment, "Payment processed successfully");
    }
}
