namespace BroadbandBilling.Domain.Exceptions;

public class SubscriberNotFoundException : DomainException
{
    public SubscriberNotFoundException(Guid subscriberId) 
        : base($"Subscriber with ID '{subscriberId}' was not found.")
    {
    }

    public SubscriberNotFoundException(string identifier) 
        : base($"Subscriber '{identifier}' was not found.")
    {
    }
}
