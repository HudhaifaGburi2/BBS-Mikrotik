using BroadbandBilling.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BroadbandBilling.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Admin> Admins { get; }
    DbSet<Subscriber> Subscribers { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Plan> Plans { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PppoeAccount> PppoeAccounts { get; }
    DbSet<MikroTikDevice> MikroTikDevices { get; }
    DbSet<UsageLog> UsageLogs { get; }
    DbSet<LoginHistory> LoginHistory { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
