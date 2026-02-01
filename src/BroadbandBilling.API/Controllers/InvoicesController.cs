using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
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
    private readonly GenerateInvoiceHandler _generateInvoiceHandler;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InvoicesController(
        GenerateInvoiceHandler generateInvoiceHandler,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _generateInvoiceHandler = generateInvoiceHandler;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceDto>>>> GetAll()
    {
        var invoices = await _unitOfWork.Invoices.GetAllAsync();
        var invoiceDtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        
        return Ok(ApiResponse<IEnumerable<InvoiceDto>>
            .SuccessResponse(invoiceDtos, "Invoices retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetById(Guid id)
    {
        var invoice = await _unitOfWork.Invoices.GetWithPaymentsAsync(id);
        if (invoice == null)
        {
            return NotFound(ApiResponse<InvoiceDto>
                .FailureResponse("Invoice not found", $"Invoice with ID {id} not found"));
        }

        var invoiceDto = _mapper.Map<InvoiceDto>(invoice);
        
        return Ok(ApiResponse<InvoiceDto>.SuccessResponse(invoiceDto, "Invoice retrieved successfully"));
    }

    [HttpGet("subscriber/{subscriberId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceDto>>>> GetBySubscriber(Guid subscriberId)
    {
        var invoices = await _unitOfWork.Invoices.GetBySubscriberIdAsync(subscriberId);
        var invoiceDtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        
        return Ok(ApiResponse<IEnumerable<InvoiceDto>>
            .SuccessResponse(invoiceDtos, "Invoices retrieved successfully"));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceDto>>>> GetOverdue()
    {
        var invoices = await _unitOfWork.Invoices.GetOverdueInvoicesAsync();
        var invoiceDtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        
        return Ok(ApiResponse<IEnumerable<InvoiceDto>>
            .SuccessResponse(invoiceDtos, "Overdue invoices retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Create([FromBody] GenerateInvoiceCommand command)
    {
        var result = await _generateInvoiceHandler.HandleAsync(command);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Invoice.Id },
            ApiResponse<InvoiceDto>.SuccessResponse(result.Invoice, "Invoice created successfully"));
    }
}
