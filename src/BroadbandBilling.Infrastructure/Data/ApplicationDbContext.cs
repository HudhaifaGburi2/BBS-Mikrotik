using Microsoft.EntityFrameworkCore;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Infrastructure.Data.Seeders;
using System.Reflection;

namespace BroadbandBilling.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PppoeAccount> PppoeAccounts => Set<PppoeAccount>();
    public DbSet<MikroTikDevice> MikroTikDevices => Set<MikroTikDevice>();
    public DbSet<UsageLog> UsageLogs => Set<UsageLog>();
    public DbSet<LoginHistory> LoginHistory => Set<LoginHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        DataSeeder.SeedData(modelBuilder);
    }
}
