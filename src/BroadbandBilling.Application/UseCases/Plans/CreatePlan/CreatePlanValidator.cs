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

        RuleFor(x => x)
            .Must(x => x.BillingCycleDays > 0 || (x.BillingCycleHours.HasValue && x.BillingCycleHours.Value > 0))
            .WithMessage("يجب تحديد مدة الاشتراك بالأيام أو الساعات");

        RuleFor(x => x.BillingCycleDays)
            .LessThanOrEqualTo(365).WithMessage("Billing cycle cannot exceed 365 days")
            .When(x => x.BillingCycleDays > 0);

        RuleFor(x => x.BillingCycleHours)
            .LessThanOrEqualTo(8760).WithMessage("Billing cycle hours cannot exceed 8760 (1 year)")
            .When(x => x.BillingCycleHours.HasValue && x.BillingCycleHours.Value > 0);

        RuleFor(x => x.MikroTikProfileName)
            .NotEmpty().WithMessage("MikroTik profile name is required")
            .MaximumLength(100).WithMessage("Profile name cannot exceed 100 characters");
    }
}
