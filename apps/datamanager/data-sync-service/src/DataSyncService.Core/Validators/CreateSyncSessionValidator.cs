using FluentValidation;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Validators;

public class CreateSyncSessionValidator : AbstractValidator<CreateSyncSessionRequest>
{
    public CreateSyncSessionValidator()
    {
        RuleFor(x => x.SourceSiteId)
            .NotEmpty()
            .WithMessage("Source site ID is required")
            .MaximumLength(50)
            .WithMessage("Source site ID cannot exceed 50 characters");

        RuleFor(x => x.TargetSiteId)
            .NotEmpty()
            .WithMessage("Target site ID is required")
            .MaximumLength(50)
            .WithMessage("Target site ID cannot exceed 50 characters");

        RuleFor(x => x.SourceSiteId)
            .NotEqual(x => x.TargetSiteId)
            .WithMessage("Source and target sites cannot be the same");

        RuleFor(x => x.Direction)
            .IsInEnum()
            .WithMessage("Invalid sync direction specified");

        RuleFor(x => x.Mode)
            .IsInEnum()
            .WithMessage("Invalid sync mode specified");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Invalid sync priority specified");

        RuleFor(x => x.MetadataJson)
            .Must(BeValidJsonOrEmpty)
            .WithMessage("Metadata must be valid JSON or empty");
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