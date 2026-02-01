namespace BroadbandBilling.Domain.Exceptions;

public class InvalidInvoiceException : DomainException
{
    public InvalidInvoiceException(string message) : base(message)
    {
    }

    public InvalidInvoiceException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
