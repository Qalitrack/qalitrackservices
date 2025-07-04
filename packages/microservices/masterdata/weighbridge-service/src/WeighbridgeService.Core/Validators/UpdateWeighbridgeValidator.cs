using FluentValidation;
using WeighbridgeService.Core.DTOs;

namespace WeighbridgeService.Core.Validators;

public class UpdateWeighbridgeValidator : AbstractValidator<UpdateWeighbridgeRequest>
{
    public UpdateWeighbridgeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

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

        RuleFor(x => x)
            .Must(x => x.MinCapacity < x.MaxCapacity)
            .WithMessage("Min capacity must be less than max capacity");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }
}