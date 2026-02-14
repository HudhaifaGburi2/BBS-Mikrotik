using AutoMapper;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Billing.GenerateInvoice;
using BroadbandBilling.Application.UseCases.Invoices.DTOs;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly GenerateInvoiceHandler _generateInvoiceHandler;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        GenerateInvoiceHandler generateInvoiceHandler,
        ILogger<InvoiceService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _generateInvoiceHandler = generateInvoiceHandler;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<InvoiceDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _unitOfWork.Invoices.GetAllAsync(cancellationToken);
        var invoiceDtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        return ApiResponse<IEnumerable<InvoiceDto>>.SuccessResponse(invoiceDtos, "Invoices retrieved successfully");
    }

    public async Task<ApiResponse<InvoiceDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetWithPaymentsAsync(id, cancellationToken);
        if (invoice == null)
        {
            return ApiResponse<InvoiceDto>.FailureResponse("Invoice not found", $"Invoice with ID {id} not found");
        }

        var invoiceDto = _mapper.Map<InvoiceDto>(invoice);
        return ApiResponse<InvoiceDto>.SuccessResponse(invoiceDto, "Invoice retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<InvoiceDto>>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default)
    {
        var invoices = await _unitOfWork.Invoices.GetBySubscriberIdAsync(subscriberId, cancellationToken);
        var invoiceDtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        return ApiResponse<IEnumerable<InvoiceDto>>.SuccessResponse(invoiceDtos, "Invoices retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<InvoiceDto>>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _unitOfWork.Invoices.GetOverdueInvoicesAsync(cancellationToken);
        var invoiceDtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        return ApiResponse<IEnumerable<InvoiceDto>>.SuccessResponse(invoiceDtos, "Overdue invoices retrieved successfully");
    }

    public async Task<ApiResponse<InvoiceDto>> GenerateAsync(GenerateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _generateInvoiceHandler.HandleAsync(command, cancellationToken);
        return ApiResponse<InvoiceDto>.SuccessResponse(result.Invoice, "Invoice created successfully");
    }
}
