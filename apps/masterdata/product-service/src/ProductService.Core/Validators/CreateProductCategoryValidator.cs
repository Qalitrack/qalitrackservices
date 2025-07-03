using FluentValidation;
using ProductService.Core.DTOs;

namespace ProductService.Core.Validators;

public class CreateProductCategoryValidator : AbstractValidator<CreateProductCategoryRequest>
{
    public CreateProductCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(50).WithMessage("Category name cannot exceed 50 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Category code is required")
            .MaximumLength(20).WithMessage("Category code cannot exceed 20 characters");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be 0 or greater");
    }
}