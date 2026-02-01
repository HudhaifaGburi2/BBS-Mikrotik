using BroadbandBilling.Application.UseCases.Subscribers.DTOs;

namespace BroadbandBilling.Application.UseCases.Subscribers.CreateSubscriber;

public class CreateSubscriberCommand
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? NationalId { get; set; }
}

public class CreateSubscriberResult
{
    public SubscriberDto Subscriber { get; set; } = null!;
}
