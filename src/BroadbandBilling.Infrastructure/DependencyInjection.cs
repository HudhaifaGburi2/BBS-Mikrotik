using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Infrastructure.Data;
using BroadbandBilling.Infrastructure.Repositories;
using BroadbandBilling.Infrastructure.Services;
using BroadbandBilling.Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.SqlServer;

namespace BroadbandBilling.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        services.AddScoped<ISubscriberRepository, SubscriberRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPppoeAccountRepository, PppoeAccountRepository>();
        services.AddScoped<IMikroTikDeviceRepository, MikroTikDeviceRepository>();
        services.AddScoped<IUsageLogRepository, UsageLogRepository>();

        services.AddScoped<IMikroTikService, MikroTikService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<BroadbandBilling.Application.Interfaces.IPasswordHasher, PasswordHasher>();
        services.AddScoped<BroadbandBilling.Application.Interfaces.IJwtTokenService, JwtTokenService>();
        services.AddScoped<BillingCycleJob>();
        services.AddScoped<SuspendExpiredSubscriptionsJob>();
        services.AddScoped<UsageSyncJob>();

        /*var hangfireConnectionString = configuration.GetConnectionString("HangfireConnection") ?? connectionString;

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        services.AddHangfireServer();*/

        return services;
    }
}
