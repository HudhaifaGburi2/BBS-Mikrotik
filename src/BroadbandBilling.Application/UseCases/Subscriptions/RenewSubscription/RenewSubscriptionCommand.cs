using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;

namespace BroadbandBilling.Application.UseCases.Subscriptions.RenewSubscription;

public class RenewSubscriptionCommand
{
    public Guid SubscriptionId { get; set; }
}

public class RenewSubscriptionResult
{
    public SubscriptionDto Subscription { get; set; } = null!;
}
