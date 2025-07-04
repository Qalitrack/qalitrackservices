using FluentValidation;
using ProductService.Core.DTOs;

namespace ProductService.Core.Validators;

public class RegisterProductValidator : AbstractValidator<RegisterProductRequest>
{
    public RegisterProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Product code is required")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.UnitOfMeasure)
            .NotEmpty().WithMessage("Unit of measure is required")
            .MaximumLength(20).WithMessage("Unit of measure cannot exceed 20 characters");

        RuleFor(x => x.Weight)
            .GreaterThan(0).When(x => x.Weight.HasValue)
            .WithMessage("Weight must be greater than 0");

        RuleFor(x => x.Density)
            .GreaterThan(0).When(x => x.Density.HasValue)
            .WithMessage("Density must be greater than 0");

        RuleFor(x => x.HazmatClass)
            .NotEmpty().When(x => x.IsHazardous)
            .WithMessage("Hazmat class is required for hazardous products");
    }
}