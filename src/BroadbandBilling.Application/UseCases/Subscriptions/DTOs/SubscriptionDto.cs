using BroadbandBilling.Application.Common.DTOs;

namespace BroadbandBilling.Application.UseCases.Subscriptions.DTOs;

public class SubscriptionDto : BaseDto
{
    public Guid SubscriberId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ActivatedAt { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public int RemainingDays { get; set; }
    public long DataUsedBytes { get; set; }
    public bool DataLimitExceeded { get; set; }
    public DateTime? DataLimitExceededAt { get; set; }
    public int DataLimitGB { get; set; }
}

public class CreateSubscriptionDto
{
    public Guid SubscriberId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }
}

public class RenewSubscriptionDto
{
    public Guid SubscriptionId { get; set; }
}

public class SuspendSubscriptionDto
{
    public Guid SubscriptionId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CancelSubscriptionDto
{
    public Guid SubscriptionId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
