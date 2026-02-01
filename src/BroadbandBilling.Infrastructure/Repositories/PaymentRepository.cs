using Microsoft.EntityFrameworkCore;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using BroadbandBilling.Infrastructure.Data;

namespace BroadbandBilling.Infrastructure.Repositories;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Invoice)
            .Include(p => p.Subscriber)
            .FirstOrDefaultAsync(p => p.PaymentReference == reference, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Invoice)
            .Where(p => p.SubscriberId == subscriberId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Invoice)
            .Include(p => p.Subscriber)
            .Where(p => p.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByMethodAsync(PaymentMethod method, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Invoice)
            .Include(p => p.Subscriber)
            .Where(p => p.Method == method)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GeneratePaymentReferenceAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var day = DateTime.UtcNow.Day;
        
        var prefix = $"PAY-{year}{month:D2}{day:D2}";
        
        var lastPayment = await _dbSet
            .Where(p => p.PaymentReference.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentReference)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastPayment == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumberStr = lastPayment.PaymentReference.Substring(lastPayment.PaymentReference.LastIndexOf('-') + 1);
        if (int.TryParse(lastNumberStr, out var lastNumber))
        {
            return $"{prefix}-{(lastNumber + 1):D4}";
        }

        return $"{prefix}-0001";
    }
}
