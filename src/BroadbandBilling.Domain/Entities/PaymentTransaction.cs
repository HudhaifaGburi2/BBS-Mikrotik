using BroadbandBilling.Domain.Enums;
using BroadbandBilling.Domain.Interfaces;

namespace BroadbandBilling.Domain.Entities;

public class PaymentTransaction : IEntity
{
    public Guid Id { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid SubscriberId { get; private set; }
    public string Gateway { get; private set; }
    public string? SessionId { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayOrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentTransactionStatus Status { get; private set; }
    public string? CardBrand { get; private set; }
    public string? CardLast4 { get; private set; }
    public string? RawRequest { get; private set; }
    public string? RawResponse { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public Subscription Subscription { get; private set; }
    public Subscriber Subscriber { get; private set; }

    private PaymentTransaction()
    {
        Gateway = null!;
        Currency = null!;
        Subscription = null!;
        Subscriber = null!;
    }

    private PaymentTransaction(
        Guid subscriptionId,
        Guid subscriberId,
        string gateway,
        decimal amount,
        string currency)
    {
        Id = Guid.NewGuid();
        SubscriptionId = subscriptionId;
        SubscriberId = subscriberId;
        Gateway = gateway;
        Amount = amount;
        Currency = currency;
        Status = PaymentTransactionStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Subscription = null!;
        Subscriber = null!;
    }

    public static PaymentTransaction Create(
        Guid subscriptionId,
        Guid subscriberId,
        string gateway,
        decimal amount,
        string currency = "SAR")
    {
        if (subscriptionId == Guid.Empty)
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        if (subscriberId == Guid.Empty)
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));

        if (string.IsNullOrWhiteSpace(gateway))
            throw new ArgumentException("Gateway is required", nameof(gateway));

        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        return new PaymentTransaction(subscriptionId, subscriberId, gateway, amount, currency);
    }

    public void SetSessionId(string sessionId)
    {
        SessionId = sessionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRawRequest(string rawRequest)
    {
        RawRequest = rawRequest;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        Status = PaymentTransactionStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSuccessful(
        string gatewayTransactionId,
        string? gatewayOrderId = null,
        string? cardBrand = null,
        string? cardLast4 = null,
        string? rawResponse = null)
    {
        Status = PaymentTransactionStatus.Successful;
        GatewayTransactionId = gatewayTransactionId;
        GatewayOrderId = gatewayOrderId;
        CardBrand = cardBrand;
        CardLast4 = cardLast4;
        RawResponse = rawResponse;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason, string? rawResponse = null)
    {
        Status = PaymentTransactionStatus.Failed;
        FailureReason = reason;
        RawResponse = rawResponse;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCancelled(string? reason = null)
    {
        Status = PaymentTransactionStatus.Cancelled;
        FailureReason = reason ?? "Cancelled by user";
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsSuccessful() => Status == PaymentTransactionStatus.Successful;
    public bool IsPending() => Status == PaymentTransactionStatus.Pending || Status == PaymentTransactionStatus.Processing;
}
