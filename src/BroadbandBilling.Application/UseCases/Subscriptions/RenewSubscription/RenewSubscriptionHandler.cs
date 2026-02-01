using AutoMapper;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;
using BroadbandBilling.Domain.Exceptions;

namespace BroadbandBilling.Application.UseCases.Subscriptions.RenewSubscription;

public class RenewSubscriptionHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RenewSubscriptionHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RenewSubscriptionResult> HandleAsync(RenewSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions
            .GetWithDetailsAsync(command.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new InvalidSubscriptionException($"Subscription with ID '{command.SubscriptionId}' not found");
        }

        var plan = await _unitOfWork.Plans.GetByIdAsync(subscription.PlanId, cancellationToken);
        if (plan == null)
        {
            throw new PlanNotFoundException(subscription.PlanId);
        }

        subscription.Renew(plan.BillingCycleDays);

        _unitOfWork.Subscriptions.Update(subscription);
        await _unitOfWork.CommitAsync(cancellationToken);

        var subscriptionDto = _mapper.Map<SubscriptionDto>(subscription);

        return new RenewSubscriptionResult { Subscription = subscriptionDto };
    }
}
