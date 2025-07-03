using FluentValidation;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Validators;

public class ResolveConflictValidator : AbstractValidator<ResolveConflictRequest>
{
    public ResolveConflictValidator()
    {
        RuleFor(x => x.ResolutionStrategy)
            .IsInEnum()
            .WithMessage("Invalid resolution strategy specified");

        RuleFor(x => x.ResolvedBy)
            .NotEmpty()
            .WithMessage("Resolved by is required")
            .MaximumLength(100)
            .WithMessage("Resolved by cannot exceed 100 characters");

        RuleFor(x => x.ResolutionReason)
            .MaximumLength(500)
            .WithMessage("Resolution reason cannot exceed 500 characters");

        RuleFor(x => x.ResolvedDataJson)
            .Must(BeValidJsonOrEmpty)
            .WithMessage("Resolved data must be valid JSON or empty")
            .NotEmpty()
            .When(x => x.ResolutionStrategy == ConflictResolutionStrategy.Manual || 
                      x.ResolutionStrategy == ConflictResolutionStrategy.UserDefined)
            .WithMessage("Resolved data JSON is required for manual or user-defined resolution strategies");
    }

    private bool BeValidJsonOrEmpty(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}