using FluentValidation;
using CustomerService.Core.DTOs;

namespace CustomerService.Core.Validators;

public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerRequest>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Customer name is required")
            .MaximumLength(500)
            .WithMessage("Customer name cannot exceed 500 characters");

        RuleFor(x => x.ContactEmail)
            .NotEmpty()
            .WithMessage("Contact email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.BillingAddress)
            .NotEmpty()
            .WithMessage("Billing address is required")
            .MaximumLength(1000)
            .WithMessage("Billing address cannot exceed 1000 characters");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Credit limit must be non-negative");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50)
            .WithMessage("Tax number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxNumber));

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(50)
            .WithMessage("Registration number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.RegistrationNumber));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20)
            .WithMessage("Contact phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));
    }
}