using BroadbandBilling.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending SMS to {PhoneNumber}: {Message}", phoneNumber, message);
        await Task.CompletedTask;
    }

    public async Task SendWelcomeSmsAsync(string phoneNumber, string subscriberName, string username, CancellationToken cancellationToken = default)
    {
        var message = $"Welcome {subscriberName}! Your username is {username}. Thank you for choosing our service.";
        await SendAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendInvoiceReminderAsync(string phoneNumber, string subscriberName, decimal amount, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        var message = $"Dear {subscriberName}, your invoice of {amount:C} is due on {dueDate:yyyy-MM-dd}.";
        await SendAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendPaymentConfirmationAsync(string phoneNumber, string subscriberName, decimal amount, CancellationToken cancellationToken = default)
    {
        var message = $"Payment of {amount:C} received. Thank you {subscriberName}!";
        await SendAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendSubscriptionExpiryReminderAsync(string phoneNumber, string subscriberName, int daysRemaining, CancellationToken cancellationToken = default)
    {
        var message = $"{subscriberName}, your subscription expires in {daysRemaining} days. Please renew to continue service.";
        await SendAsync(phoneNumber, message, cancellationToken);
    }
}
