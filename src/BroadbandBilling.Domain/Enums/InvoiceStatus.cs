namespace BroadbandBilling.Domain.Enums;

public enum InvoiceStatus
{
    Pending = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Overdue = 4,
    Cancelled = 5,
    Refunded = 6
}
