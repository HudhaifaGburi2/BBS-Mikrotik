using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.UseCases.Billing.GenerateInvoice;
using BroadbandBilling.Application.UseCases.Invoices.DTOs;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface IInvoiceService
{
    Task<ApiResponse<IEnumerable<InvoiceDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<InvoiceDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<InvoiceDto>>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<InvoiceDto>>> GetOverdueAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<InvoiceDto>> GenerateAsync(GenerateInvoiceCommand command, CancellationToken cancellationToken = default);
}
