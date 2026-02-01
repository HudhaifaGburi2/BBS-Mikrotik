namespace BroadbandBilling.Domain.Exceptions;

public class InvalidSubscriptionException : DomainException
{
    public InvalidSubscriptionException(string message) : base(message)
    {
    }

    public InvalidSubscriptionException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
