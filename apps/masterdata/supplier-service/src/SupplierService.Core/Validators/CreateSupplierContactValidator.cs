using FluentValidation;
using SupplierService.Core.DTOs;

namespace SupplierService.Core.Validators;

public class CreateSupplierContactValidator : AbstractValidator<CreateSupplierContactRequest>
{
    public CreateSupplierContactValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name is required")
            .MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name is required")
            .MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Mobile));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("Job Title cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.JobTitle));

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Department));
    }
}