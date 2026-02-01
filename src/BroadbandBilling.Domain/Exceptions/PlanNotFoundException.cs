namespace BroadbandBilling.Domain.Exceptions;

public class PlanNotFoundException : DomainException
{
    public PlanNotFoundException(Guid planId) 
        : base($"Plan with ID '{planId}' was not found.")
    {
    }

    public PlanNotFoundException(string planName) 
        : base($"Plan '{planName}' was not found.")
    {
    }
}
