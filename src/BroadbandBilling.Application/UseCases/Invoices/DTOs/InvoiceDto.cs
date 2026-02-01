using BroadbandBilling.Application.Common.DTOs;

namespace BroadbandBilling.Application.UseCases.Invoices.DTOs;

public class InvoiceDto : BaseDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid SubscriberId { get; set; }
    public string SubscriberName { get; set; } = string.Empty;
    public Guid? SubscriptionId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int DaysOverdue { get; set; }
}

public class CreateInvoiceDto
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
