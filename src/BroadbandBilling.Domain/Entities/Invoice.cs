using BroadbandBilling.Domain.Enums;
using BroadbandBilling.Domain.Exceptions;
using BroadbandBilling.Domain.Interfaces;
using BroadbandBilling.Domain.ValueObjects;

namespace BroadbandBilling.Domain.Entities;

public class Invoice : IEntity
{
    public Guid Id { get; private set; }
    public string InvoiceNumber { get; private set; }
    public Guid SubscriberId { get; private set; }
    public Guid? SubscriptionId { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public Money Subtotal { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money TotalAmount { get; private set; }
    public Money PaidAmount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Subscriber Subscriber { get; private set; }
    public Subscription? Subscription { get; private set; }

    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private Invoice() { }

    private Invoice(string invoiceNumber, Guid subscriberId, Guid? subscriptionId,
        DateTime issueDate, DateTime dueDate, Money subtotal, Money taxAmount,
        Money discountAmount, string? notes, string currency)
    {
        Id = Guid.NewGuid();
        InvoiceNumber = invoiceNumber ?? throw new ArgumentNullException(nameof(invoiceNumber));
        SubscriberId = subscriberId;
        SubscriptionId = subscriptionId;
        IssueDate = issueDate;
        DueDate = dueDate;
        Subtotal = subtotal ?? throw new ArgumentNullException(nameof(subtotal));
        TaxAmount = taxAmount ?? Money.Zero(currency);
        DiscountAmount = discountAmount ?? Money.Zero(currency);
        PaidAmount = Money.Zero(currency);
        Status = InvoiceStatus.Pending;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;

        CalculateTotal();
    }

    public static Invoice Create(string invoiceNumber, Guid subscriberId, 
        Guid? subscriptionId, DateTime issueDate, int dueDays, 
        decimal subtotal, decimal taxAmount = 0, decimal discountAmount = 0,
        string? notes = null, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required", nameof(invoiceNumber));

        if (subscriberId == Guid.Empty)
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));

        if (dueDays < 0)
            throw new ArgumentException("Due days cannot be negative", nameof(dueDays));

        var dueDate = issueDate.AddDays(dueDays);

        return new Invoice(
            invoiceNumber, 
            subscriberId, 
            subscriptionId,
            issueDate, 
            dueDate,
            Money.Create(subtotal, currency),
            Money.Create(taxAmount, currency),
            Money.Create(discountAmount, currency),
            notes,
            currency
        );
    }

    private void CalculateTotal()
    {
        TotalAmount = Subtotal + TaxAmount - DiscountAmount;
        
        if (TotalAmount.Amount < 0)
            throw new InvalidInvoiceException("Total amount cannot be negative");
    }

    public void AddPayment(Money amount)
    {
        if (Status == InvoiceStatus.Cancelled)
            throw new InvalidInvoiceException("Cannot add payment to cancelled invoice");

        if (Status == InvoiceStatus.Refunded)
            throw new InvalidInvoiceException("Cannot add payment to refunded invoice");

        PaidAmount = PaidAmount + amount;
        UpdateStatus();
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateStatus()
    {
        if (PaidAmount.Amount >= TotalAmount.Amount)
        {
            Status = InvoiceStatus.Paid;
        }
        else if (PaidAmount.Amount > 0 && PaidAmount.Amount < TotalAmount.Amount)
        {
            Status = InvoiceStatus.PartiallyPaid;
        }
        else if (DateTime.UtcNow > DueDate && Status == InvoiceStatus.Pending)
        {
            Status = InvoiceStatus.Overdue;
        }
    }

    public void MarkAsOverdue()
    {
        if (Status == InvoiceStatus.Pending || Status == InvoiceStatus.PartiallyPaid)
        {
            if (DateTime.UtcNow > DueDate)
            {
                Status = InvoiceStatus.Overdue;
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    public void Cancel(string reason)
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidInvoiceException("Cannot cancel paid invoice");

        if (PaidAmount.Amount > 0)
            throw new InvalidInvoiceException("Cannot cancel invoice with payments. Refund first.");

        Status = InvoiceStatus.Cancelled;
        Notes = $"{Notes ?? ""} | Cancelled: {reason}";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund()
    {
        if (Status != InvoiceStatus.Paid)
            throw new InvalidInvoiceException("Only paid invoices can be refunded");

        Status = InvoiceStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public Money GetRemainingAmount()
    {
        var remaining = TotalAmount - PaidAmount;
        return remaining.Amount > 0 ? remaining : Money.Zero(TotalAmount.Currency);
    }

    public bool IsFullyPaid() => Status == InvoiceStatus.Paid;

    public bool IsOverdue() => Status == InvoiceStatus.Overdue || 
        (DateTime.UtcNow > DueDate && Status != InvoiceStatus.Paid && Status != InvoiceStatus.Cancelled);

    public int GetDaysOverdue()
    {
        if (!IsOverdue()) return 0;
        return (DateTime.UtcNow.Date - DueDate.Date).Days;
    }
}
