using BroadbandBilling.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
        await Task.CompletedTask;
    }

    public async Task SendWelcomeEmailAsync(string to, string subscriberName, string username, string password, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Broadband Billing System";
        var body = $"Dear {subscriberName},\n\nYour account has been created.\nUsername: {username}\nPassword: {password}";
        await SendAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendInvoiceEmailAsync(string to, string subscriberName, string invoiceNumber, decimal amount, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        var subject = $"Invoice {invoiceNumber} - Due {dueDate:yyyy-MM-dd}";
        var body = $"Dear {subscriberName},\n\nYour invoice {invoiceNumber} for {amount:C} is due on {dueDate:yyyy-MM-dd}.";
        await SendAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendPaymentConfirmationEmailAsync(string to, string subscriberName, string paymentReference, decimal amount, CancellationToken cancellationToken = default)
    {
        var subject = "Payment Confirmation";
        var body = $"Dear {subscriberName},\n\nYour payment of {amount:C} (Ref: {paymentReference}) has been received.";
        await SendAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendSubscriptionExpiryReminderAsync(string to, string subscriberName, DateTime expiryDate, int daysRemaining, CancellationToken cancellationToken = default)
    {
        var subject = "Subscription Expiry Reminder";
        var body = $"Dear {subscriberName},\n\nYour subscription will expire in {daysRemaining} days on {expiryDate:yyyy-MM-dd}.";
        await SendAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendSubscriptionSuspendedEmailAsync(string to, string subscriberName, string reason, CancellationToken cancellationToken = default)
    {
        var subject = "Subscription Suspended";
        var body = $"Dear {subscriberName},\n\nYour subscription has been suspended. Reason: {reason}";
        await SendAsync(to, subject, body, true, cancellationToken);
    }
}
