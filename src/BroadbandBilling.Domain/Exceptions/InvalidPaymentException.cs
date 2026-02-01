namespace BroadbandBilling.Domain.Exceptions;

public class InvalidPaymentException : DomainException
{
    public InvalidPaymentException(string message) : base(message)
    {
    }

    public InvalidPaymentException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
