using FluentValidation;
using WeightDataService.Core.DTOs;

namespace WeightDataService.Core.Validators;

public class CreateWeightMeasurementValidator : AbstractValidator<CreateWeightMeasurementDto>
{
    public CreateWeightMeasurementValidator()
    {
        RuleFor(x => x.WeighbridgeId)
            .NotEmpty().WithMessage("Weighbridge ID is required")
            .MaximumLength(50).WithMessage("Weighbridge ID cannot exceed 50 characters");

        RuleFor(x => x.VehicleRegistration)
            .NotEmpty().WithMessage("Vehicle registration is required")
            .MaximumLength(20).WithMessage("Vehicle registration cannot exceed 20 characters");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .LessThan(1000000).WithMessage("Weight cannot exceed 1,000,000 kg");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid measurement type");

        RuleFor(x => x.TareWeight)
            .GreaterThanOrEqualTo(0).When(x => x.TareWeight.HasValue)
            .WithMessage("Tare weight must be greater than or equal to 0");

        RuleFor(x => x.TicketReference)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.TicketReference))
            .WithMessage("Ticket reference cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters");

        RuleFor(x => x.ProductType)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.ProductType))
            .WithMessage("Product type cannot exceed 100 characters");

        RuleFor(x => x.CustomerReference)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.CustomerReference))
            .WithMessage("Customer reference cannot exceed 100 characters");
    }
}