using BroadbandBilling.Application.UseCases.Subscribers.DTOs;

namespace BroadbandBilling.Application.UseCases.Subscribers.GetSubscriber;

public class GetSubscriberQuery
{
    public Guid SubscriberId { get; set; }
}

public class GetSubscriberResult
{
    public SubscriberDto Subscriber { get; set; } = null!;
}
