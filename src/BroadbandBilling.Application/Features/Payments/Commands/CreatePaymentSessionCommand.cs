using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Application.Features.Payments.Commands;

public record CreatePaymentSessionCommand(
    Guid PlanId,
    Guid SubscriberId,
    string PaymentMethod
) : IRequest<PaymentSessionResult>;

public record PaymentSessionResult(
    bool Success,
    Guid? SubscriptionId,
    Guid? TransactionId,
    string? RedirectUrl,
    string? ErrorMessage
);

public class CreatePaymentSessionCommandHandler : IRequestHandler<CreatePaymentSessionCommand, PaymentSessionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeideaPaymentService _geideaService;
    private readonly ILogger<CreatePaymentSessionCommandHandler> _logger;

    public CreatePaymentSessionCommandHandler(
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IGeideaPaymentService geideaService,
        ILogger<CreatePaymentSessionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _geideaService = geideaService;
        _logger = logger;
    }

    public async Task<PaymentSessionResult> Handle(CreatePaymentSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _unitOfWork.Plans.GetByIdAsync(request.PlanId, cancellationToken);
            if (plan == null)
            {
                return new PaymentSessionResult(false, null, null, null, "الباقة غير موجودة");
            }

            if (!plan.IsActive)
            {
                return new PaymentSessionResult(false, null, null, null, "الباقة غير متاحة حالياً");
            }

            var subscriber = await _dbContext.Subscribers
                .FirstOrDefaultAsync(s => s.Id == request.SubscriberId, cancellationToken);
            
            if (subscriber == null)
            {
                return new PaymentSessionResult(false, null, null, null, "المشترك غير موجود");
            }

            var existingActiveSubscription = await _dbContext.Subscriptions
                .AnyAsync(s => s.SubscriberId == request.SubscriberId && 
                              s.Status == SubscriptionStatus.Active, cancellationToken);

            if (existingActiveSubscription)
            {
                return new PaymentSessionResult(false, null, null, null, "لديك اشتراك نشط بالفعل");
            }

            var subscription = Subscription.Create(
                request.SubscriberId,
                request.PlanId,
                DateTime.UtcNow,
                plan.BillingCycleDays,
                plan.Price.Amount,
                plan.BillingCycleHours
            );

            await _unitOfWork.Subscriptions.AddAsync(subscription, cancellationToken);

            var transaction = PaymentTransaction.Create(
                subscription.Id,
                request.SubscriberId,
                "Geidea",
                plan.Price.Amount,
                plan.Price.Currency
            );

            _dbContext.PaymentTransactions.Add(transaction);

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Created pending subscription {SubscriptionId} for subscriber {SubscriberId}",
                subscription.Id, request.SubscriberId);

            if (request.PaymentMethod == "BankTransfer")
            {
                return new PaymentSessionResult(
                    Success: true,
                    SubscriptionId: subscription.Id,
                    TransactionId: transaction.Id,
                    RedirectUrl: null,
                    ErrorMessage: null
                );
            }

            var sessionResult = await _geideaService.CreatePaymentSessionAsync(
                new CreateGeideaSessionRequest(
                    SubscriptionId: subscription.Id,
                    Amount: plan.Price.Amount,
                    Currency: plan.Price.Currency,
                    CustomerEmail: subscriber.Email,
                    CustomerName: subscriber.FullName,
                    CustomerPhone: subscriber.PhoneNumber,
                    Description: $"اشتراك {plan.Name}"
                ),
                cancellationToken
            );

            if (!sessionResult.Success)
            {
                transaction.MarkAsFailed(sessionResult.ErrorMessage ?? "Failed to create payment session", sessionResult.RawResponse);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogError("Failed to create Geidea session for subscription {SubscriptionId}: {Error}",
                    subscription.Id, sessionResult.ErrorMessage);

                return new PaymentSessionResult(
                    Success: false,
                    SubscriptionId: subscription.Id,
                    TransactionId: transaction.Id,
                    RedirectUrl: null,
                    ErrorMessage: "فشل في إنشاء جلسة الدفع. الرجاء المحاولة مرة أخرى."
                );
            }

            transaction.SetSessionId(sessionResult.SessionId!);
            transaction.SetRawRequest(sessionResult.RawResponse ?? "");
            transaction.MarkAsProcessing();
            subscription.SetGatewayPaymentId(sessionResult.SessionId!);

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Created Geidea payment session {SessionId} for subscription {SubscriptionId}",
                sessionResult.SessionId, subscription.Id);

            return new PaymentSessionResult(
                Success: true,
                SubscriptionId: subscription.Id,
                TransactionId: transaction.Id,
                RedirectUrl: sessionResult.RedirectUrl,
                ErrorMessage: null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment session for plan {PlanId}, subscriber {SubscriberId}",
                request.PlanId, request.SubscriberId);

            return new PaymentSessionResult(
                Success: false,
                SubscriptionId: null,
                TransactionId: null,
                RedirectUrl: null,
                ErrorMessage: "حدث خطأ أثناء إنشاء جلسة الدفع"
            );
        }
    }
}
