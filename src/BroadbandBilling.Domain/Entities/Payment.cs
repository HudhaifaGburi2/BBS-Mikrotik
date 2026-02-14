using BroadbandBilling.Domain.Enums;
using BroadbandBilling.Domain.Exceptions;
using BroadbandBilling.Domain.Interfaces;
using BroadbandBilling.Domain.ValueObjects;

namespace BroadbandBilling.Domain.Entities;

public class Payment : IEntity
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid SubscriberId { get; private set; }
    public string PaymentReference { get; private set; }
    public Money Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public string? TransactionId { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Invoice Invoice { get; private set; }
    public Subscriber Subscriber { get; private set; }

    private Payment()
    {
        PaymentReference = null!;
        Amount = null!;
        Invoice = null!;
        Subscriber = null!;
    }

    private Payment(Guid invoiceId, Guid subscriberId, string paymentReference,
        Money amount, PaymentMethod method, DateTime paymentDate, 
        string? transactionId, string? notes)
    {
        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        SubscriberId = subscriberId;
        Invoice = null!;
        Subscriber = null!;
        PaymentReference = paymentReference ?? throw new ArgumentNullException(nameof(paymentReference));
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        Method = method;
        Status = PaymentStatus.Pending;
        PaymentDate = paymentDate;
        TransactionId = transactionId;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }

    public static Payment Create(Guid invoiceId, Guid subscriberId, 
        string paymentReference, decimal amount, PaymentMethod method,
        DateTime? paymentDate = null, string? transactionId = null, 
        string? notes = null, string currency = "USD")
    {
        if (invoiceId == Guid.Empty)
            throw new ArgumentException("Invoice ID is required", nameof(invoiceId));

        if (subscriberId == Guid.Empty)
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));

        if (string.IsNullOrWhiteSpace(paymentReference))
            throw new ArgumentException("Payment reference is required", nameof(paymentReference));

        if (amount <= 0)
            throw new InvalidPaymentException("Payment amount must be positive");

        return new Payment(
            invoiceId,
            subscriberId,
            paymentReference,
            Money.Create(amount, currency),
            method,
            paymentDate ?? DateTime.UtcNow,
            transactionId,
            notes
        );
    }

    public void MarkAsCompleted(string? transactionId = null)
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidPaymentException("Payment is already completed");

        if (Status == PaymentStatus.Cancelled)
            throw new InvalidPaymentException("Cannot complete cancelled payment");

        Status = PaymentStatus.Completed;
        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            TransactionId = transactionId;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidPaymentException("Cannot fail completed payment");

        Status = PaymentStatus.Failed;
        Notes = $"{Notes ?? ""} | Failed: {reason}";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidPaymentException("Cannot cancel completed payment");

        if (Status == PaymentStatus.Refunded)
            throw new InvalidPaymentException("Cannot cancel refunded payment");

        Status = PaymentStatus.Cancelled;
        Notes = $"{Notes ?? ""} | Cancelled: {reason}";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund(string reason)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidPaymentException("Only completed payments can be refunded");

        Status = PaymentStatus.Refunded;
        Notes = $"{Notes ?? ""} | Refunded: {reason}";
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsSuccessful() => Status == PaymentStatus.Completed;

    public bool CanBeRefunded() => Status == PaymentStatus.Completed;
}
