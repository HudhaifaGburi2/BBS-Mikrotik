using BroadbandBilling.Domain.Enums;
using BroadbandBilling.Domain.Exceptions;
using BroadbandBilling.Domain.Interfaces;
using BroadbandBilling.Domain.ValueObjects;

namespace BroadbandBilling.Domain.Entities;

public class Subscription : IEntity
{
    public Guid Id { get; private set; }
    public Guid SubscriberId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateRange BillingPeriod { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public string? GatewayPaymentId { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public bool MikroTikSynced { get; private set; }
    public DateTime? MikroTikSyncedAt { get; private set; }
    public string? MikroTikSyncError { get; private set; }
    public long DataUsedBytes { get; private set; }
    public bool DataLimitExceeded { get; private set; }
    public DateTime? DataLimitExceededAt { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public DateTime? SuspendedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Subscriber Subscriber { get; private set; }
    public Plan Plan { get; private set; }
    public ICollection<PaymentTransaction> PaymentTransactions { get; private set; }

    private Subscription()
    {
        BillingPeriod = null!;
        Subscriber = null!;
        Plan = null!;
        PaymentTransactions = new List<PaymentTransaction>();
    }

    private Subscription(Guid subscriberId, Guid planId, DateTime startDate, int billingCycleDays, int? billingCycleHours, decimal amount)
    {
        Id = Guid.NewGuid();
        SubscriberId = subscriberId;
        PlanId = planId;
        Subscriber = null!;
        Plan = null!;
        PaymentTransactions = new List<PaymentTransaction>();
        
        // Calculate end date based on days and hours
        var totalHours = (billingCycleDays * 24) + (billingCycleHours ?? 0);
        var endDate = startDate.AddHours(totalHours).AddSeconds(-1);
        BillingPeriod = DateRange.Create(startDate, endDate);
        
        Status = SubscriptionStatus.PendingActivation;
        Amount = amount;
        IsPaid = false;
        MikroTikSynced = false;
        DataUsedBytes = 0;
        DataLimitExceeded = false;
        CreatedAt = DateTime.UtcNow;
    }

    public static Subscription Create(Guid subscriberId, Guid planId, 
        DateTime startDate, int billingCycleDays, decimal amount = 0, int? billingCycleHours = null)
    {
        if (subscriberId == Guid.Empty)
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));

        if (planId == Guid.Empty)
            throw new ArgumentException("Plan ID is required", nameof(planId));

        // Validate total billing cycle is at least 1 hour
        var totalHours = (billingCycleDays * 24) + (billingCycleHours ?? 0);
        if (totalHours <= 0)
            throw new ArgumentException("Billing cycle must be at least 1 hour", nameof(billingCycleDays));

        return new Subscription(subscriberId, planId, startDate, billingCycleDays, billingCycleHours, amount);
    }

    public void SetGatewayPaymentId(string gatewayPaymentId)
    {
        GatewayPaymentId = gatewayPaymentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(string? gatewayPaymentId = null)
    {
        IsPaid = true;
        PaidAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(gatewayPaymentId))
        {
            GatewayPaymentId = gatewayPaymentId;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMikroTikSynced(bool synced, string? error = null)
    {
        MikroTikSynced = synced;
        MikroTikSyncedAt = synced ? DateTime.UtcNow : null;
        MikroTikSyncError = error;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == SubscriptionStatus.Active)
            throw new InvalidSubscriptionException("Subscription is already active");

        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidSubscriptionException("Cannot activate cancelled subscription");

        Status = SubscriptionStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        SuspendedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend(string reason = "")
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidSubscriptionException("Only active subscriptions can be suspended");

        Status = SubscriptionStatus.Suspended;
        SuspendedAt = DateTime.UtcNow;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidSubscriptionException("Subscription is already cancelled");

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason ?? "No reason provided";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Renew(int billingCycleDays)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidSubscriptionException("Cannot renew cancelled subscription");

        var newStartDate = BillingPeriod.EndDate.AddDays(1);
        var newEndDate = newStartDate.AddDays(billingCycleDays - 1);
        BillingPeriod = DateRange.Create(newStartDate, newEndDate);
        
        if (Status == SubscriptionStatus.Expired)
        {
            Status = SubscriptionStatus.PendingActivation;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        if (Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Suspended)
        {
            Status = SubscriptionStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool IsExpired()
    {
        return Status == SubscriptionStatus.Expired || 
               BillingPeriod.IsExpired(DateTime.UtcNow);
    }

    public bool IsActive()
    {
        return Status == SubscriptionStatus.Active && 
               !BillingPeriod.IsExpired(DateTime.UtcNow);
    }

    public int GetRemainingDays()
    {
        if (IsExpired()) return 0;
        var remaining = (BillingPeriod.EndDate - DateTime.UtcNow.Date).Days;
        return Math.Max(0, remaining);
    }

    public bool ShouldBeExpired()
    {
        return BillingPeriod.IsExpired(DateTime.UtcNow) && 
               Status != SubscriptionStatus.Cancelled;
    }

    public void UpdateDataUsage(long bytesUsed)
    {
        DataUsedBytes = bytesUsed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDataLimitExceeded()
    {
        DataLimitExceeded = true;
        DataLimitExceededAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public double GetDataUsedGB()
    {
        return DataUsedBytes / (1024.0 * 1024.0 * 1024.0);
    }

    public bool HasExceededDataLimit(int dataLimitGB)
    {
        if (dataLimitGB <= 0) return false;
        return GetDataUsedGB() >= dataLimitGB;
    }
}
