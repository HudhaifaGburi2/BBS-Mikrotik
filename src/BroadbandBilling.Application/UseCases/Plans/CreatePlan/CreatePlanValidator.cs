using FluentValidation;

namespace BroadbandBilling.Application.UseCases.Plans.CreatePlan;

public class CreatePlanValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required")
            .MaximumLength(100).WithMessage("Plan name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.SpeedMbps)
            .GreaterThan(0).WithMessage("Speed must be greater than 0");

        RuleFor(x => x.DataLimitGB)
            .GreaterThanOrEqualTo(0).WithMessage("Data limit cannot be negative");

        RuleFor(x => x.BillingCycleDays)
            .GreaterThan(0).WithMessage("Billing cycle must be at least 1 day")
            .LessThanOrEqualTo(365).WithMessage("Billing cycle cannot exceed 365 days");

        RuleFor(x => x.MikroTikProfileName)
            .NotEmpty().WithMessage("MikroTik profile name is required")
            .MaximumLength(100).WithMessage("Profile name cannot exceed 100 characters");
    }
}
