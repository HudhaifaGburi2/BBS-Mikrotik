using AutoMapper;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Exceptions;

namespace BroadbandBilling.Application.UseCases.Subscriptions.CreateSubscription;

public class CreateSubscriptionHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSubscriptionHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CreateSubscriptionResult> HandleAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(command.SubscriberId, cancellationToken);
        if (subscriber == null)
        {
            throw new SubscriberNotFoundException(command.SubscriberId);
        }

        var plan = await _unitOfWork.Plans.GetByIdAsync(command.PlanId, cancellationToken);
        if (plan == null)
        {
            throw new PlanNotFoundException(command.PlanId);
        }

        var activeSubscription = await _unitOfWork.Subscriptions
            .GetActiveSubscriptionBySubscriberIdAsync(command.SubscriberId, cancellationToken);
        
        if (activeSubscription != null)
        {
            throw new InvalidSubscriptionException("Subscriber already has an active subscription");
        }

        var subscription = Subscription.Create(
            command.SubscriberId,
            command.PlanId,
            command.StartDate,
            plan.BillingCycleDays,
            plan.Price.Amount,
            plan.BillingCycleHours
        );

        await _unitOfWork.Subscriptions.AddAsync(subscription, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var subscriptionWithDetails = await _unitOfWork.Subscriptions
            .GetWithDetailsAsync(subscription.Id, cancellationToken);

        var subscriptionDto = _mapper.Map<SubscriptionDto>(subscriptionWithDetails);

        return new CreateSubscriptionResult { Subscription = subscriptionDto };
    }
}
