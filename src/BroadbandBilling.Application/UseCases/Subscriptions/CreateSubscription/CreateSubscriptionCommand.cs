using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;

namespace BroadbandBilling.Application.UseCases.Subscriptions.CreateSubscription;

public class CreateSubscriptionCommand
{
    public Guid SubscriberId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }
}

public class CreateSubscriptionResult
{
    public SubscriptionDto Subscription { get; set; } = null!;
}
