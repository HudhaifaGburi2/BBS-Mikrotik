namespace BroadbandBilling.Application.Common.Interfaces;

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task SendWelcomeSmsAsync(string phoneNumber, string subscriberName, string username, CancellationToken cancellationToken = default);
    Task SendInvoiceReminderAsync(string phoneNumber, string subscriberName, decimal amount, DateTime dueDate, CancellationToken cancellationToken = default);
    Task SendPaymentConfirmationAsync(string phoneNumber, string subscriberName, decimal amount, CancellationToken cancellationToken = default);
    Task SendSubscriptionExpiryReminderAsync(string phoneNumber, string subscriberName, int daysRemaining, CancellationToken cancellationToken = default);
}
