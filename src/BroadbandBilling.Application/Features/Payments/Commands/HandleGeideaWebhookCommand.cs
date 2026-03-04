using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Payments.Commands;

public record HandleGeideaWebhookCommand(
    string OrderId,
    string? TransactionId,
    string Status,
    decimal Amount,
    string Currency,
    string? MerchantReferenceId,
    string? CardBrand,
    string? CardLast4,
    string? ResponseCode,
    string? ResponseMessage,
    string RawPayload
) : IRequest<WebhookResult>;

public record WebhookResult(
    bool Success,
    string? Message
);

public class HandleGeideaWebhookCommandHandler : IRequestHandler<HandleGeideaWebhookCommand, WebhookResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeideaPaymentService _geideaService;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<HandleGeideaWebhookCommandHandler> _logger;

    public HandleGeideaWebhookCommandHandler(
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IGeideaPaymentService geideaService,
        IMikroTikService mikroTikService,
        ILogger<HandleGeideaWebhookCommandHandler> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _geideaService = geideaService;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task<WebhookResult> Handle(HandleGeideaWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing Geidea webhook for order {OrderId}, status: {Status}",
                request.OrderId, request.Status);

            if (string.IsNullOrEmpty(request.MerchantReferenceId) || 
                !Guid.TryParse(request.MerchantReferenceId, out var subscriptionId))
            {
                _logger.LogWarning("Invalid merchant reference ID in webhook: {MerchantReferenceId}",
                    request.MerchantReferenceId);
                return new WebhookResult(false, "Invalid merchant reference ID");
            }

            var subscription = await _dbContext.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.Subscriber)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription not found for webhook: {SubscriptionId}", subscriptionId);
                return new WebhookResult(false, "Subscription not found");
            }

            if (subscription.IsPaid)
            {
                _logger.LogInformation("Subscription {SubscriptionId} already paid, ignoring duplicate webhook",
                    subscriptionId);
                return new WebhookResult(true, "Already processed");
            }

            var transaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.SubscriptionId == subscriptionId && 
                                         t.Status != PaymentTransactionStatus.Successful,
                    cancellationToken);

            var verificationResult = await _geideaService.VerifyPaymentAsync(request.OrderId, cancellationToken);

            if (!verificationResult.Success)
            {
                _logger.LogWarning("Payment verification failed for order {OrderId}: {Error}",
                    request.OrderId, verificationResult.ErrorMessage);

                if (transaction != null)
                {
                    transaction.MarkAsFailed(
                        verificationResult.ErrorMessage ?? "Payment verification failed",
                        verificationResult.RawResponse
                    );
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                return new WebhookResult(false, "Payment verification failed");
            }

            if (transaction != null)
            {
                transaction.MarkAsSuccessful(
                    verificationResult.TransactionId ?? request.TransactionId ?? "",
                    verificationResult.OrderId,
                    verificationResult.CardBrand ?? request.CardBrand,
                    verificationResult.CardLast4 ?? request.CardLast4,
                    request.RawPayload
                );
            }

            subscription.MarkAsPaid(request.OrderId);
            subscription.Activate();

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Payment successful for subscription {SubscriptionId}, activating MikroTik profile",
                subscriptionId);

            await ActivateMikroTikProfileAsync(subscription, cancellationToken);

            return new WebhookResult(true, "Payment processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Geidea webhook for order {OrderId}", request.OrderId);
            return new WebhookResult(false, "Internal error processing webhook");
        }
    }

    private async Task ActivateMikroTikProfileAsync(
        BroadbandBilling.Domain.Entities.Subscription subscription,
        CancellationToken cancellationToken)
    {
        try
        {
            var pppoeAccount = await _dbContext.PppoeAccounts
                .FirstOrDefaultAsync(p => p.SubscriberId == subscription.SubscriberId, cancellationToken);

            if (pppoeAccount == null)
            {
                _logger.LogWarning("No PPPoE account found for subscriber {SubscriberId}, skipping MikroTik activation",
                    subscription.SubscriberId);
                subscription.SetMikroTikSynced(false, "No PPPoE account found");
                await _unitOfWork.CommitAsync(cancellationToken);
                return;
            }

            var profileName = subscription.Plan?.MikroTikProfileName;
            if (string.IsNullOrEmpty(profileName))
            {
                _logger.LogWarning("No MikroTik profile name configured for plan {PlanId}",
                    subscription.PlanId);
                subscription.SetMikroTikSynced(false, "No MikroTik profile configured");
                await _unitOfWork.CommitAsync(cancellationToken);
                return;
            }

            var updateResult = await _mikroTikService.UpdateUserProfileAsync(
                new UpdateProfileRequest
                {
                    PppUsername = pppoeAccount.Username,
                    NewProfile = profileName
                },
                cancellationToken
            );

            if (updateResult.Success)
            {
                subscription.SetMikroTikSynced(true);
                _logger.LogInformation("MikroTik profile updated for user {Username} to {Profile}",
                    pppoeAccount.Username, profileName);
            }
            else
            {
                subscription.SetMikroTikSynced(false, updateResult.Message);
                _logger.LogWarning("Failed to update MikroTik profile for user {Username}: {Error}",
                    pppoeAccount.Username, updateResult.Message);
            }

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating MikroTik profile for subscription {SubscriptionId}",
                subscription.Id);
            subscription.SetMikroTikSynced(false, ex.Message);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
    }
}
