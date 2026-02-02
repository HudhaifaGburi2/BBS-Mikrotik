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
        var basicPlanId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var standardPlanId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var premiumPlanId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        var businessPlanId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Plan>().HasData(
            new
            {
                Id = basicPlanId,
                Name = "الباقة الأساسية 10 ميجا",
                Description = "باقة إنترنت أساسية بسرعة 10 ميجابت في الثانية",
                SpeedMbps = 10,
                DataLimitGB = 0,
                BillingCycleDays = 30,
                MikroTikProfileName = "BASIC_10M",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new
            {
                Id = standardPlanId,
                Name = "الباقة القياسية 20 ميجا",
                Description = "باقة إنترنت قياسية بسرعة 20 ميجابت في الثانية",
                SpeedMbps = 20,
                DataLimitGB = 0,
                BillingCycleDays = 30,
                MikroTikProfileName = "STANDARD_20M",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new
            {
                Id = premiumPlanId,
                Name = "الباقة المميزة 50 ميجا",
                Description = "باقة إنترنت مميزة بسرعة 50 ميجابت في الثانية",
                SpeedMbps = 50,
                DataLimitGB = 0,
                BillingCycleDays = 30,
                MikroTikProfileName = "PREMIUM_50M",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new
            {
                Id = businessPlanId,
                Name = "باقة الأعمال 100 ميجا",
                Description = "باقة إنترنت للأعمال بسرعة 100 ميجابت في الثانية ببيانات غير محدودة",
                SpeedMbps = 100,
                DataLimitGB = 0,
                BillingCycleDays = 30,
                MikroTikProfileName = "BUSINESS_100M",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );

        modelBuilder.Entity<Plan>().OwnsOne(p => p.Price).HasData(
            new { PlanId = basicPlanId, Amount = 56.25m, Currency = "SAR" },
            new { PlanId = standardPlanId, Amount = 93.75m, Currency = "SAR" },
            new { PlanId = premiumPlanId, Amount = 187.50m, Currency = "SAR" },
            new { PlanId = businessPlanId, Amount = 375.00m, Currency = "SAR" }
        );
    }

    private static void SeedMikroTikDevices(ModelBuilder modelBuilder)
    {
        var mainRouterId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<MikroTikDevice>().HasData(
            new
            {
                Id = mainRouterId,
                Name = "الموجه الرئيسي",
                Port = 8728,
                Username = "admin",
                Password = "password",
                IsActive = true,
                Location = "المقر الرئيسي",
                Description = (string?)null,
                IsOnline = false,
                LastPingDate = (DateTime?)null,
                LastConnectedAt = (DateTime?)null,
                MaxUsers = 1000,
                CurrentActiveUsers = 0,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );

        modelBuilder.Entity<MikroTikDevice>().OwnsOne(m => m.IpAddress).HasData(
            new { MikroTikDeviceId = mainRouterId, Value = "192.168.88.1" }
        );
    }
}
