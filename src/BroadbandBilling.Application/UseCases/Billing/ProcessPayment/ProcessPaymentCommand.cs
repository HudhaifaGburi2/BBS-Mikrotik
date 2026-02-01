using BroadbandBilling.Application.UseCases.Payments.DTOs;

namespace BroadbandBilling.Application.UseCases.Billing.ProcessPayment;

public class ProcessPaymentCommand
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
}

public class ProcessPaymentResult
{
    public PaymentDto Payment { get; set; } = null!;
    public bool InvoiceFullyPaid { get; set; }
}
