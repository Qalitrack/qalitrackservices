using FluentValidation;
using CustomerService.Core.DTOs;

namespace CustomerService.Core.Validators;

public class CreateCustomerContactValidator : AbstractValidator<CreateCustomerContactRequest>
{
    public CreateCustomerContactValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(100)
            .WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Mobile)
            .MaximumLength(20)
            .WithMessage("Mobile cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Mobile));

        RuleFor(x => x.Position)
            .MaximumLength(100)
            .WithMessage("Position cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Position));

        RuleFor(x => x.Department)
            .MaximumLength(100)
            .WithMessage("Department cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Department));
    }
}