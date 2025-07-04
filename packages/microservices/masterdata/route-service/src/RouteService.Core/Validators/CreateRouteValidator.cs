using FluentValidation;
using RouteService.Core.DTOs;

namespace RouteService.Core.Validators;

public class CreateRouteValidator : AbstractValidator<CreateRouteRequest>
{
    public CreateRouteValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Route name is required")
            .MaximumLength(100).WithMessage("Route name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Route code is required")
            .MaximumLength(20).WithMessage("Route code cannot exceed 20 characters")
            .Matches("^[A-Z0-9_-]+$").WithMessage("Route code can only contain uppercase letters, numbers, underscores, and hyphens");

        RuleFor(x => x.Origin)
            .NotEmpty().WithMessage("Origin is required")
            .MaximumLength(200).WithMessage("Origin cannot exceed 200 characters");

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required")
            .MaximumLength(200).WithMessage("Destination cannot exceed 200 characters");

        RuleFor(x => x.Distance)
            .GreaterThan(0).WithMessage("Distance must be greater than 0");

        RuleFor(x => x.EstimatedDuration)
            .Must(duration => duration.TotalMinutes > 0)
            .WithMessage("Estimated duration must be greater than 0 minutes");

        RuleFor(x => x.MaxVehicleWeight)
            .GreaterThan(0).When(x => x.MaxVehicleWeight.HasValue)
            .WithMessage("Maximum vehicle weight must be greater than 0");

        RuleFor(x => x.MaxVehicleHeight)
            .GreaterThan(0).When(x => x.MaxVehicleHeight.HasValue)
            .WithMessage("Maximum vehicle height must be greater than 0");

        RuleFor(x => x.MaxVehicleWidth)
            .GreaterThan(0).When(x => x.MaxVehicleWidth.HasValue)
            .WithMessage("Maximum vehicle width must be greater than 0");

        RuleFor(x => x.MaxVehicleLength)
            .GreaterThan(0).When(x => x.MaxVehicleLength.HasValue)
            .WithMessage("Maximum vehicle length must be greater than 0");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.EffectiveDate)
            .LessThan(x => x.ExpirationDate).When(x => x.EffectiveDate.HasValue && x.ExpirationDate.HasValue)
            .WithMessage("Effective date must be before expiration date");
    }
}