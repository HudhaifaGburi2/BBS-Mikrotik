using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetWithPaymentsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetPendingInvoicesAsync(CancellationToken cancellationToken = default);
    Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default);
}
