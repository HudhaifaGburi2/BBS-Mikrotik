using FluentValidation;

namespace BroadbandBilling.Application.UseCases.Subscriptions.CreateSubscription;

public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.SubscriberId)
            .NotEmpty().WithMessage("Subscriber ID is required");

        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("Plan ID is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date cannot be in the past");
    }
}
