using FluentValidation;
using RouteService.Core.DTOs;

namespace RouteService.Core.Validators;

public class CreateRouteWaypointValidator : AbstractValidator<CreateRouteWaypointRequest>
{
    public CreateRouteWaypointValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Waypoint name is required")
            .MaximumLength(200).WithMessage("Waypoint name cannot exceed 200 characters");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees");

        RuleFor(x => x.Sequence)
            .GreaterThan(0).WithMessage("Sequence must be greater than 0");

        RuleFor(x => x.DistanceFromPrevious)
            .GreaterThanOrEqualTo(0).When(x => x.DistanceFromPrevious.HasValue)
            .WithMessage("Distance from previous waypoint must be greater than or equal to 0");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Address)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Address))
            .WithMessage("Address cannot exceed 100 characters");

        RuleFor(x => x.City)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.City))
            .WithMessage("City cannot exceed 50 characters");

        RuleFor(x => x.State)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.State))
            .WithMessage("State cannot exceed 50 characters");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PostalCode))
            .WithMessage("Postal code cannot exceed 20 characters");

        RuleFor(x => x.Country)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Country))
            .WithMessage("Country cannot exceed 50 characters");
    }
}