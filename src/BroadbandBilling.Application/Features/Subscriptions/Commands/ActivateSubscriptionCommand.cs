using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Subscriptions.Commands;

/// <summary>
/// Command to activate a subscription after payment is confirmed.
/// This enables the PPP secret on MikroTik and resets usage counters.
/// </summary>
public record ActivateSubscriptionCommand(
    Guid SubscriptionId,
    string? PaymentReference = null
) : IRequest<ActivateSubscriptionResult>;

public record ActivateSubscriptionResult(
    bool Success,
    string Message,
    Guid? SubscriptionId = null,
    string? Error = null
);

public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, ActivateSubscriptionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<ActivateSubscriptionCommandHandler> _logger;

    public ActivateSubscriptionCommandHandler(
        IApplicationDbContext dbContext,
        IMikroTikService mikroTikService,
        ILogger<ActivateSubscriptionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    public async Task<ActivateSubscriptionResult> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Get subscription with related data
        var subscription = await _dbContext.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Subscriber)
                .ThenInclude(sub => sub.PppoeAccounts)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            return new ActivateSubscriptionResult(false, "الاشتراك غير موجود", null, "Subscription not found");
        }

        if (subscription.Status == SubscriptionStatus.Active)
        {
            return new ActivateSubscriptionResult(true, "الاشتراك مفعل بالفعل", subscription.Id);
        }

        if (subscription.Status != SubscriptionStatus.PendingActivation)
        {
            return new ActivateSubscriptionResult(false, 
                $"لا يمكن تفعيل اشتراك بحالة {subscription.Status}", 
                null, 
                $"Cannot activate subscription with status {subscription.Status}");
        }

        try
        {
            // Get PPPoE account for this subscription
            var pppoeAccount = subscription.Subscriber.PppoeAccounts
                .FirstOrDefault(p => p.SubscriptionId == subscription.Id);

            if (pppoeAccount == null)
            {
                return new ActivateSubscriptionResult(false, 
                    "لا يوجد حساب PPPoE مرتبط بهذا الاشتراك", 
                    null, 
                    "No PPPoE account found for this subscription");
            }

            // 1. Activate subscription in database
            subscription.Activate();
            
            // Mark as paid if payment reference provided
            if (!string.IsNullOrEmpty(request.PaymentReference))
            {
                subscription.MarkAsPaid(request.PaymentReference);
            }
            
            // Reset data usage counters
            subscription.ResetDataUsage();

            // 2. Enable PPPoE account in database
            pppoeAccount.Enable();

            // 3. Enable PPP secret on MikroTik and reset counters
            var activateResult = await _mikroTikService.ActivateUserAsync(
                new DeletePppUserRequest { PppUsername = pppoeAccount.Username }, 
                cancellationToken);

            if (activateResult.Success)
            {
                // Reset MikroTik counters
                await _mikroTikService.ResetUserQuotaAsync(
                    new ResetUserQuotaRequest { PppUsername = pppoeAccount.Username },
                    cancellationToken);

                // Update comment to remove "Pending Activation"
                var plan = subscription.Plan;
                if (plan != null)
                {
                    try
                    {
                        // Update the comment on MikroTik
                        var updateRequest = new UpdatePppUserRequest
                        {
                            PppUsername = pppoeAccount.Username,
                            Comment = $"Plan: {plan.Name}"
                        };
                        await _mikroTikService.UpdatePppUserAsync(updateRequest, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update MikroTik comment for user {Username}", pppoeAccount.Username);
                    }
                }

                subscription.SetMikroTikSynced(true);
                pppoeAccount.MarkAsSynced();
                
                _logger.LogInformation(
                    "Successfully activated subscription {SubscriptionId} for user {Username}", 
                    subscription.Id, pppoeAccount.Username);
            }
            else
            {
                subscription.SetMikroTikSynced(false, activateResult.Error);
                _logger.LogWarning(
                    "MikroTik activation failed for subscription {SubscriptionId}: {Error}", 
                    subscription.Id, activateResult.Error);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ActivateSubscriptionResult(
                true, 
                "تم تفعيل الاشتراك بنجاح", 
                subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating subscription {SubscriptionId}", request.SubscriptionId);
            return new ActivateSubscriptionResult(
                false, 
                "حدث خطأ أثناء تفعيل الاشتراك", 
                null, 
                ex.Message);
        }
    }
}
