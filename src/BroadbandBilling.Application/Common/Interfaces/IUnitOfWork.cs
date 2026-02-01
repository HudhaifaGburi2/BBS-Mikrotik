namespace BroadbandBilling.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ISubscriberRepository Subscribers { get; }
    ISubscriptionRepository Subscriptions { get; }
    IPlanRepository Plans { get; }
    IInvoiceRepository Invoices { get; }
    IPaymentRepository Payments { get; }
    IPppoeAccountRepository PppoeAccounts { get; }
    IMikroTikDeviceRepository MikroTikDevices { get; }
    IUsageLogRepository UsageLogs { get; }

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
