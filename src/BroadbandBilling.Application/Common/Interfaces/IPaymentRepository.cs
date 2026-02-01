using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByMethodAsync(PaymentMethod method, CancellationToken cancellationToken = default);
    Task<string> GeneratePaymentReferenceAsync(CancellationToken cancellationToken = default);
}
