namespace BroadbandBilling.Application.Common.Interfaces;

public interface IGeideaPaymentService
{
    Task<GeideaSessionResult> CreatePaymentSessionAsync(CreateGeideaSessionRequest request, CancellationToken cancellationToken = default);
    Task<GeideaPaymentResult> VerifyPaymentAsync(string orderId, CancellationToken cancellationToken = default);
    bool ValidateWebhookSignature(string payload, string signature);
}

public record CreateGeideaSessionRequest(
    Guid SubscriptionId,
    decimal Amount,
    string Currency,
    string CustomerEmail,
    string? CustomerName = null,
    string? CustomerPhone = null,
    string? Description = null
);

public record GeideaSessionResult(
    bool Success,
    string? SessionId,
    string? RedirectUrl,
    string? ErrorMessage,
    string? RawResponse
);

public record GeideaPaymentResult(
    bool Success,
    string? OrderId,
    string? TransactionId,
    string? Status,
    decimal? Amount,
    string? Currency,
    string? CardBrand,
    string? CardLast4,
    string? ErrorMessage,
    string? RawResponse
);

public record GeideaWebhookPayload(
    string OrderId,
    string? TransactionId,
    string Status,
    decimal Amount,
    string Currency,
    string? MerchantReferenceId,
    string? CardBrand,
    string? CardLast4,
    string? ResponseCode,
    string? ResponseMessage
);
