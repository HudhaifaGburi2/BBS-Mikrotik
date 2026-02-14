using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.UseCases.Billing.ProcessPayment;
using BroadbandBilling.Application.UseCases.Payments.DTOs;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<IEnumerable<PaymentDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<PaymentDto>>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<PaymentDto>>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentDto>> ProcessAsync(ProcessPaymentCommand command, CancellationToken cancellationToken = default);
}
