using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BroadbandBilling.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BroadbandBilling.Infrastructure.Services;

public class GeideaSettings
{
    public string BaseUrl { get; set; } = "https://api.merchant.geidea.net";
    public string MerchantPublicKey { get; set; } = string.Empty;
    public string ApiPassword { get; set; } = string.Empty;
    public string Currency { get; set; } = "SAR";
    public string CallbackUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string WebhookSecretKey { get; set; } = string.Empty;
}

public class GeideaPaymentService : IGeideaPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly GeideaSettings _settings;
    private readonly ILogger<GeideaPaymentService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public GeideaPaymentService(
        HttpClient httpClient,
        IOptions<GeideaSettings> settings,
        ILogger<GeideaPaymentService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_settings.MerchantPublicKey}:{_settings.ApiPassword}"));
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", credentials);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<GeideaSessionResult> CreatePaymentSessionAsync(
        CreateGeideaSessionRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionRequest = new
            {
                amount = request.Amount,
                currency = request.Currency,
                merchantReferenceId = request.SubscriptionId.ToString(),
                callbackUrl = _settings.CallbackUrl,
                returnUrl = _settings.ReturnUrl,
                customer = new
                {
                    email = request.CustomerEmail,
                    name = request.CustomerName,
                    phoneNumber = request.CustomerPhone
                },
                appearance = new
                {
                    merchant = new
                    {
                        name = "DOSHI Internet Services"
                    },
                    showEmail = true,
                    showPhone = true
                }
            };

            var jsonContent = JsonSerializer.Serialize(sessionRequest, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation("Creating Geidea payment session for subscription {SubscriptionId}, amount: {Amount} {Currency}",
                request.SubscriptionId, request.Amount, request.Currency);

            var response = await _httpClient.PostAsync(
                "/payment-intent/api/v2/direct/session", 
                content, 
                cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Geidea session creation failed: {StatusCode} - {Response}",
                    response.StatusCode, responseContent);

                return new GeideaSessionResult(
                    Success: false,
                    SessionId: null,
                    RedirectUrl: null,
                    ErrorMessage: $"Payment gateway error: {response.StatusCode}",
                    RawResponse: responseContent
                );
            }

            var sessionResponse = JsonSerializer.Deserialize<GeideaSessionResponse>(responseContent, _jsonOptions);

            if (sessionResponse?.Session == null)
            {
                _logger.LogError("Invalid Geidea session response: {Response}", responseContent);
                return new GeideaSessionResult(
                    Success: false,
                    SessionId: null,
                    RedirectUrl: null,
                    ErrorMessage: "Invalid response from payment gateway",
                    RawResponse: responseContent
                );
            }

            _logger.LogInformation("Geidea session created successfully: {SessionId}", sessionResponse.Session.Id);

            return new GeideaSessionResult(
                Success: true,
                SessionId: sessionResponse.Session.Id,
                RedirectUrl: sessionResponse.Session.PaymentUrl,
                ErrorMessage: null,
                RawResponse: responseContent
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Geidea payment session for subscription {SubscriptionId}",
                request.SubscriptionId);

            return new GeideaSessionResult(
                Success: false,
                SessionId: null,
                RedirectUrl: null,
                ErrorMessage: ex.Message,
                RawResponse: null
            );
        }
    }

    public async Task<GeideaPaymentResult> VerifyPaymentAsync(
        string orderId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Verifying Geidea payment for order {OrderId}", orderId);

            var response = await _httpClient.GetAsync(
                $"/pgw/api/v1/direct/order/{orderId}",
                cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Geidea payment verification failed: {StatusCode} - {Response}",
                    response.StatusCode, responseContent);

                return new GeideaPaymentResult(
                    Success: false,
                    OrderId: orderId,
                    TransactionId: null,
                    Status: "Error",
                    Amount: null,
                    Currency: null,
                    CardBrand: null,
                    CardLast4: null,
                    ErrorMessage: $"Verification failed: {response.StatusCode}",
                    RawResponse: responseContent
                );
            }

            var orderResponse = JsonSerializer.Deserialize<GeideaOrderResponse>(responseContent, _jsonOptions);

            if (orderResponse?.Order == null)
            {
                return new GeideaPaymentResult(
                    Success: false,
                    OrderId: orderId,
                    TransactionId: null,
                    Status: "Error",
                    Amount: null,
                    Currency: null,
                    CardBrand: null,
                    CardLast4: null,
                    ErrorMessage: "Invalid order response",
                    RawResponse: responseContent
                );
            }

            var order = orderResponse.Order;
            var isSuccess = order.Status?.Equals("Success", StringComparison.OrdinalIgnoreCase) == true ||
                           order.DetailedStatus?.Equals("Paid", StringComparison.OrdinalIgnoreCase) == true;

            _logger.LogInformation("Geidea payment verification result for order {OrderId}: {Status}",
                orderId, order.Status);

            return new GeideaPaymentResult(
                Success: isSuccess,
                OrderId: order.OrderId,
                TransactionId: order.Transactions?.FirstOrDefault()?.TransactionId,
                Status: order.Status,
                Amount: order.Amount,
                Currency: order.Currency,
                CardBrand: order.PaymentMethod?.Brand,
                CardLast4: order.PaymentMethod?.MaskedCardNumber?[^4..],
                ErrorMessage: isSuccess ? null : order.DetailedStatus,
                RawResponse: responseContent
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Geidea payment for order {OrderId}", orderId);

            return new GeideaPaymentResult(
                Success: false,
                OrderId: orderId,
                TransactionId: null,
                Status: "Error",
                Amount: null,
                Currency: null,
                CardBrand: null,
                CardLast4: null,
                ErrorMessage: ex.Message,
                RawResponse: null
            );
        }
    }

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_settings.WebhookSecretKey))
        {
            _logger.LogWarning("Webhook secret key not configured, skipping signature validation");
            return true;
        }

        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.WebhookSecretKey));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToBase64String(computedHash);

            var isValid = string.Equals(signature, computedSignature, StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                _logger.LogWarning("Webhook signature validation failed");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating webhook signature");
            return false;
        }
    }

    private class GeideaSessionResponse
    {
        public GeideaSession? Session { get; set; }
    }

    private class GeideaSession
    {
        public string? Id { get; set; }
        public string? PaymentUrl { get; set; }
    }

    private class GeideaOrderResponse
    {
        public GeideaOrder? Order { get; set; }
    }

    private class GeideaOrder
    {
        public string? OrderId { get; set; }
        public string? MerchantReferenceId { get; set; }
        public string? Status { get; set; }
        public string? DetailedStatus { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public GeideaPaymentMethod? PaymentMethod { get; set; }
        public List<GeideaTransaction>? Transactions { get; set; }
    }

    private class GeideaPaymentMethod
    {
        public string? Brand { get; set; }
        public string? MaskedCardNumber { get; set; }
    }

    private class GeideaTransaction
    {
        public string? TransactionId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
    }
}
