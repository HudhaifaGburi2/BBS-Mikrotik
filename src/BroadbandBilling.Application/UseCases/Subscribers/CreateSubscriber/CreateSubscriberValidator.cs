using FluentValidation;

namespace BroadbandBilling.Application.UseCases.Subscribers.CreateSubscriber;

public class CreateSubscriberValidator : AbstractValidator<CreateSubscriberCommand>
{
    public CreateSubscriberValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.NationalId)
            .MaximumLength(50).WithMessage("National ID cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.NationalId));
    }
}
