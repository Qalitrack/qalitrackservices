using FluentValidation;
using SupplierService.Core.DTOs;

namespace SupplierService.Core.Validators;

public class RegisterSupplierValidator : AbstractValidator<RegisterSupplierRequest>
{
    public RegisterSupplierValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier Name is required")
            .MaximumLength(200).WithMessage("Supplier Name cannot exceed 200 characters");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact Email is required")
            .EmailAddress().WithMessage("Contact Email must be a valid email address")
            .MaximumLength(100).WithMessage("Contact Email cannot exceed 100 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage("Tax Number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxNumber));

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(50).WithMessage("Registration Number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.RegistrationNumber));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage("Contact Phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Website cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Industry)
            .MaximumLength(100).WithMessage("Industry cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Industry));

        RuleFor(x => x.EmployeeCount)
            .GreaterThanOrEqualTo(0).WithMessage("Employee Count must be zero or greater")
            .When(x => x.EmployeeCount.HasValue);

        RuleFor(x => x.EstablishedDate)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Established Date cannot be in the future")
            .When(x => x.EstablishedDate.HasValue);
    }
}