using FluentValidation;
using SupplierService.Core.DTOs;

namespace SupplierService.Core.Validators;

public class CreateSupplierContractValidator : AbstractValidator<CreateSupplierContractRequest>
{
    public CreateSupplierContractValidator()
    {
        RuleFor(x => x.ContractNumber)
            .NotEmpty().WithMessage("Contract Number is required")
            .MaximumLength(50).WithMessage("Contract Number cannot exceed 50 characters");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start Date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End Date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End Date must be after Start Date");

        RuleFor(x => x.ContractValue)
            .GreaterThanOrEqualTo(0).WithMessage("Contract Value must be zero or greater")
            .When(x => x.ContractValue.HasValue);

        RuleFor(x => x.Currency)
            .MaximumLength(10).WithMessage("Currency cannot exceed 10 characters")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.PaymentDays)
            .GreaterThan(0).WithMessage("Payment Days must be greater than zero")
            .When(x => x.PaymentDays.HasValue);

        RuleFor(x => x.RenewalPeriodMonths)
            .GreaterThan(0).WithMessage("Renewal Period must be greater than zero months")
            .When(x => x.RenewalPeriodMonths.HasValue);

        RuleFor(x => x.RenewalDate)
            .GreaterThan(x => x.EndDate).WithMessage("Renewal Date must be after End Date")
            .When(x => x.RenewalDate.HasValue);
    }
}