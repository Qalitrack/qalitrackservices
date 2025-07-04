using FluentValidation;
using WeighbridgeService.Core.DTOs;

namespace WeighbridgeService.Core.Validators;

public class ScheduleCalibrationValidator : AbstractValidator<ScheduleCalibrationRequest>
{
    public ScheduleCalibrationValidator()
    {
        RuleFor(x => x.ScheduledDate)
            .NotEmpty().WithMessage("Scheduled date is required")
            .GreaterThan(DateTime.Now).WithMessage("Scheduled date must be in the future");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid calibration type");

        RuleFor(x => x.CalibrationCompany)
            .MaximumLength(200).WithMessage("Calibration company cannot exceed 200 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }
}