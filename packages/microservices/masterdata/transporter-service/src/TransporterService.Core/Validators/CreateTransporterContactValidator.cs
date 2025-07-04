using FluentValidation;
using TransporterService.Core.DTOs;

namespace TransporterService.Core.Validators;

public class CreateTransporterContactValidator : AbstractValidator<CreateTransporterContactRequest>
{
    public CreateTransporterContactValidator()
    {
        RuleFor(x => x.TransporterId)
            .NotEmpty().WithMessage("Transporter ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email address is required")
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters");

        RuleFor(x => x.ContactType)
            .IsInEnum().WithMessage("Valid contact type is required");

        RuleFor(x => x.AlternatePhone)
            .MaximumLength(20).WithMessage("Alternate phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.AlternatePhone));

        RuleFor(x => x.Position)
            .MaximumLength(100).WithMessage("Position cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Position));

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Department));
    }
}