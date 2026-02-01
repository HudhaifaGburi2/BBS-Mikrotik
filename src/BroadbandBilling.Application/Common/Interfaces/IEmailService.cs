namespace BroadbandBilling.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string to, string subscriberName, string username, string password, CancellationToken cancellationToken = default);
    Task SendInvoiceEmailAsync(string to, string subscriberName, string invoiceNumber, decimal amount, DateTime dueDate, CancellationToken cancellationToken = default);
    Task SendPaymentConfirmationEmailAsync(string to, string subscriberName, string paymentReference, decimal amount, CancellationToken cancellationToken = default);
    Task SendSubscriptionExpiryReminderAsync(string to, string subscriberName, DateTime expiryDate, int daysRemaining, CancellationToken cancellationToken = default);
    Task SendSubscriptionSuspendedEmailAsync(string to, string subscriberName, string reason, CancellationToken cancellationToken = default);
}
