using Microsoft.EntityFrameworkCore;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Infrastructure.Data.Seeders;

public static class DataSeeder
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        SeedPlans(modelBuilder);
        SeedMikroTikDevices(modelBuilder);
    }

    private static void SeedPlans(ModelBuilder modelBuilder)
    {
        var basicPlan = Plan.Create(
            "Basic 10M",
            "Basic internet plan with 10 Mbps speed",
            15.00m,
            10,
            0,
            30,
            "BASIC_10M",
            "USD"
        );

        var standardPlan = Plan.Create(
            "Standard 20M",
            "Standard internet plan with 20 Mbps speed",
            25.00m,
            20,
            0,
            30,
            "STANDARD_20M",
            "USD"
        );

        var premiumPlan = Plan.Create(
            "Premium 50M",
            "Premium internet plan with 50 Mbps speed",
            50.00m,
            50,
            0,
            30,
            "PREMIUM_50M",
            "USD"
        );

        var businessPlan = Plan.Create(
            "Business 100M",
            "Business internet plan with 100 Mbps speed and unlimited data",
            100.00m,
            100,
            0,
            30,
            "BUSINESS_100M",
            "USD"
        );

        modelBuilder.Entity<Plan>().HasData(
            new
            {
                basicPlan.Id,
                basicPlan.Name,
                basicPlan.Description,
                basicPlan.SpeedMbps,
                basicPlan.DataLimitGB,
                basicPlan.BillingCycleDays,
                basicPlan.MikroTikProfileName,
                basicPlan.IsActive,
                basicPlan.CreatedAt,
                basicPlan.UpdatedAt
            },
            new
            {
                standardPlan.Id,
                standardPlan.Name,
                standardPlan.Description,
                standardPlan.SpeedMbps,
                standardPlan.DataLimitGB,
                standardPlan.BillingCycleDays,
                standardPlan.MikroTikProfileName,
                standardPlan.IsActive,
                standardPlan.CreatedAt,
                standardPlan.UpdatedAt
            },
            new
            {
                premiumPlan.Id,
                premiumPlan.Name,
                premiumPlan.Description,
                premiumPlan.SpeedMbps,
                premiumPlan.DataLimitGB,
                premiumPlan.BillingCycleDays,
                premiumPlan.MikroTikProfileName,
                premiumPlan.IsActive,
                premiumPlan.CreatedAt,
                premiumPlan.UpdatedAt
            },
            new
            {
                businessPlan.Id,
                businessPlan.Name,
                businessPlan.Description,
                businessPlan.SpeedMbps,
                businessPlan.DataLimitGB,
                businessPlan.BillingCycleDays,
                businessPlan.MikroTikProfileName,
                businessPlan.IsActive,
                businessPlan.CreatedAt,
                businessPlan.UpdatedAt
            }
        );

        modelBuilder.Entity<Plan>().OwnsOne(p => p.Price).HasData(
            new { PlanId = basicPlan.Id, Amount = 15.00m, Currency = "USD" },
            new { PlanId = standardPlan.Id, Amount = 25.00m, Currency = "USD" },
            new { PlanId = premiumPlan.Id, Amount = 50.00m, Currency = "USD" },
            new { PlanId = businessPlan.Id, Amount = 100.00m, Currency = "USD" }
        );
    }

    private static void SeedMikroTikDevices(ModelBuilder modelBuilder)
    {
        var mainRouter = MikroTikDevice.Create(
            "Main Router",
            "192.168.88.1",
            8728,
            "admin",
            "password",
            "Headquarters"
        );

        modelBuilder.Entity<MikroTikDevice>().HasData(
            new
            {
                mainRouter.Id,
                mainRouter.Name,
                mainRouter.Port,
                mainRouter.Username,
                mainRouter.Password,
                mainRouter.IsActive,
                mainRouter.Location,
                mainRouter.LastConnectedAt,
                mainRouter.CreatedAt,
                mainRouter.UpdatedAt
            }
        );

        modelBuilder.Entity<MikroTikDevice>().OwnsOne(m => m.IpAddress).HasData(
            new { MikroTikDeviceId = mainRouter.Id, Value = "192.168.88.1" }
        );
    }
}
