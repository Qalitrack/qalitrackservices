using FluentValidation;
using WeightDataService.Core.DTOs;

namespace WeightDataService.Core.Validators;

public class CreateWeightCorrectionValidator : AbstractValidator<CreateWeightCorrectionDto>
{
    public CreateWeightCorrectionValidator()
    {
        RuleFor(x => x.WeightMeasurementId)
            .NotEmpty().WithMessage("Weight measurement ID is required");

        RuleFor(x => x.CorrectedWeight)
            .GreaterThan(0).WithMessage("Corrected weight must be greater than 0")
            .LessThan(1000000).WithMessage("Corrected weight cannot exceed 1,000,000 kg");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Correction reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}