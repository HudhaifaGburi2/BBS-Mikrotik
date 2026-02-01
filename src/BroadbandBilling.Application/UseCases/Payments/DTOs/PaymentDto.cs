using BroadbandBilling.Application.Common.DTOs;

namespace BroadbandBilling.Application.UseCases.Payments.DTOs;

public class PaymentDto : BaseDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid SubscriberId { get; set; }
    public string SubscriberName { get; set; } = string.Empty;
    public string PaymentReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
}

public class CreatePaymentDto
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
}

public class ProcessPaymentDto
{
    public Guid PaymentId { get; set; }
    public string? TransactionId { get; set; }
}
