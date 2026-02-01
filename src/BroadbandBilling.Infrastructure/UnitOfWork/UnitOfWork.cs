using Microsoft.EntityFrameworkCore.Storage;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Infrastructure.Data;
using BroadbandBilling.Infrastructure.Repositories;

namespace BroadbandBilling.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Subscribers = new SubscriberRepository(_context);
        Subscriptions = new SubscriptionRepository(_context);
        Plans = new PlanRepository(_context);
        Invoices = new InvoiceRepository(_context);
        Payments = new PaymentRepository(_context);
        PppoeAccounts = new PppoeAccountRepository(_context);
        MikroTikDevices = new MikroTikDeviceRepository(_context);
        UsageLogs = new UsageLogRepository(_context);
    }

    public ISubscriberRepository Subscribers { get; }
    public ISubscriptionRepository Subscriptions { get; }
    public IPlanRepository Plans { get; }
    public IInvoiceRepository Invoices { get; }
    public IPaymentRepository Payments { get; }
    public IPppoeAccountRepository PppoeAccounts { get; }
    public IMikroTikDeviceRepository MikroTikDevices { get; }
    public IUsageLogRepository UsageLogs { get; }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
