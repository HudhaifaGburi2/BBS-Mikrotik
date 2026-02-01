using BroadbandBilling.Application.UseCases.Invoices.DTOs;

namespace BroadbandBilling.Application.UseCases.Billing.GenerateInvoice;

public class GenerateInvoiceCommand
{
    public Guid SubscriberId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public DateTime IssueDate { get; set; }
    public int DueDays { get; set; } = 7;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public string? Notes { get; set; }
}

public class GenerateInvoiceResult
{
    public InvoiceDto Invoice { get; set; } = null!;
}
