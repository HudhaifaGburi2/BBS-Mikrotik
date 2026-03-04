using BroadbandBilling.Domain.Interfaces;
using BroadbandBilling.Domain.ValueObjects;

namespace BroadbandBilling.Domain.Entities;

public class Plan : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public int SpeedMbps { get; private set; }
    public int DataLimitGB { get; private set; }
    public int BillingCycleDays { get; private set; }
    public int? BillingCycleHours { get; private set; }
    public bool IsActive { get; private set; }
    public string MikroTikProfileName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Plan()
    {
        Name = null!;
        Description = null!;
        Price = null!;
        MikroTikProfileName = null!;
    }

    private Plan(string name, string description, Money price, int speedMbps, 
        int dataLimitGB, int billingCycleDays, int? billingCycleHours, string mikroTikProfileName)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        SpeedMbps = speedMbps > 0 ? speedMbps : throw new ArgumentException("Speed must be positive", nameof(speedMbps));
        DataLimitGB = dataLimitGB >= 0 ? dataLimitGB : throw new ArgumentException("Data limit cannot be negative", nameof(dataLimitGB));
        BillingCycleDays = billingCycleDays >= 0 ? billingCycleDays : throw new ArgumentException("Billing cycle days cannot be negative", nameof(billingCycleDays));
        BillingCycleHours = billingCycleHours;
        
        // Ensure at least 1 hour of billing cycle
        var totalHours = (billingCycleDays * 24) + (billingCycleHours ?? 0);
        if (totalHours <= 0)
            throw new ArgumentException("Billing cycle must be at least 1 hour", nameof(billingCycleDays));
        MikroTikProfileName = mikroTikProfileName ?? throw new ArgumentNullException(nameof(mikroTikProfileName));
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Plan Create(string name, string description, decimal price, 
        int speedMbps, int dataLimitGB, int billingCycleDays, 
        string mikroTikProfileName, string currency = "USD", int? billingCycleHours = null)
    {
        return new Plan(name, description, Money.Create(price, currency), 
            speedMbps, dataLimitGB, billingCycleDays, billingCycleHours, mikroTikProfileName);
    }

    public void UpdateDetails(string name, string description, decimal price, 
        int speedMbps, int dataLimitGB, string currency = "USD", int? billingCycleHours = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Price = Money.Create(price, currency);
        SpeedMbps = speedMbps > 0 ? speedMbps : throw new ArgumentException("Speed must be positive");
        DataLimitGB = dataLimitGB >= 0 ? dataLimitGB : throw new ArgumentException("Data limit cannot be negative");
        BillingCycleHours = billingCycleHours;
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetTotalBillingHours()
    {
        return (BillingCycleDays * 24) + (BillingCycleHours ?? 0);
    }

    public bool HasDataLimit() => DataLimitGB > 0;

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsUnlimited() => DataLimitGB == 0;

    public Money CalculateProRatedPrice(int daysUsed)
    {
        if (daysUsed <= 0) return Money.Zero(Price.Currency);
        if (daysUsed >= BillingCycleDays) return Price;

        var pricePerDay = Price.Amount / BillingCycleDays;
        return Money.Create(pricePerDay * daysUsed, Price.Currency);
    }
}
