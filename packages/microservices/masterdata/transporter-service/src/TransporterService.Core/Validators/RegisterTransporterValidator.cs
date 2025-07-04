using FluentValidation;
using TransporterService.Core.DTOs;

namespace TransporterService.Core.Validators;

public class RegisterTransporterValidator : AbstractValidator<RegisterTransporterRequest>
{
    public RegisterTransporterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Transporter name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.RegistrationNumber)
            .NotEmpty().WithMessage("Registration number is required")
            .MaximumLength(50).WithMessage("Registration number cannot exceed 50 characters");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required")
            .EmailAddress().WithMessage("Valid email address is required")
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters");

        RuleFor(x => x.ContactPhone)
            .NotEmpty().WithMessage("Contact phone is required")
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.FleetSize)
            .GreaterThanOrEqualTo(0).WithMessage("Fleet size must be zero or greater");

        RuleFor(x => x.TransporterType)
            .IsInEnum().WithMessage("Valid transporter type is required");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage("Tax number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxNumber));

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Website cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.LicenseExpiryDate)
            .GreaterThan(DateTime.Today).WithMessage("License expiry date must be in the future")
            .When(x => x.LicenseExpiryDate.HasValue);
    }
}