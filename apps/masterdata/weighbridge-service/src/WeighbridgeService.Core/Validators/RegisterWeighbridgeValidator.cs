using FluentValidation;
using WeighbridgeService.Core.DTOs;

namespace WeighbridgeService.Core.Validators;

public class RegisterWeighbridgeValidator : AbstractValidator<RegisterWeighbridgeRequest>
{
    public RegisterWeighbridgeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MinimumLength(2).WithMessage("Code must be at least 2 characters")
            .MaximumLength(20).WithMessage("Code cannot exceed 20 characters")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("Max capacity must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Max capacity cannot exceed 1,000,000 kg");

        RuleFor(x => x.MinCapacity)
            .GreaterThanOrEqualTo(0).WithMessage("Min capacity cannot be negative");

        RuleFor(x => x.Accuracy)
            .GreaterThan(0).WithMessage("Accuracy must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Accuracy cannot exceed 100 kg");

        RuleFor(x => x.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required")
            .MaximumLength(100).WithMessage("Manufacturer cannot exceed 100 characters");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(100).WithMessage("Model cannot exceed 100 characters");

        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial number is required")
            .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters");

        RuleFor(x => x.InstallationDate)
            .NotEmpty().WithMessage("Installation date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Installation date cannot be in the future");

        RuleFor(x => x.CalibrationDate)
            .NotEmpty().WithMessage("Calibration date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Calibration date cannot be in the future");

        RuleFor(x => x.NextCalibrationDate)
            .NotEmpty().WithMessage("Next calibration date is required")
            .GreaterThan(DateTime.Now).WithMessage("Next calibration date must be in the future");

        RuleFor(x => x)
            .Must(x => x.MinCapacity < x.MaxCapacity)
            .WithMessage("Min capacity must be less than max capacity");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }
}