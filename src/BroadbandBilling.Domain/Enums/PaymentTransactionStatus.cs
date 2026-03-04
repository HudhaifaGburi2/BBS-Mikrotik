namespace BroadbandBilling.Domain.Enums;

public enum PaymentTransactionStatus
{
    Pending = 1,
    Processing = 2,
    Successful = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6
}
