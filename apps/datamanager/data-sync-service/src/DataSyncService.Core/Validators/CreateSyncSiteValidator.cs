using FluentValidation;
using DataSyncService.Core.DTOs;

namespace DataSyncService.Core.Validators;

public class CreateSyncSiteValidator : AbstractValidator<CreateSyncSiteRequest>
{
    public CreateSyncSiteValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required")
            .MaximumLength(50)
            .WithMessage("Site ID cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Site ID can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Site name is required")
            .MaximumLength(100)
            .WithMessage("Site name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required")
            .MaximumLength(1000)
            .WithMessage("Connection string cannot exceed 1000 characters");

        RuleFor(x => x.ApiEndpoint)
            .NotEmpty()
            .WithMessage("API endpoint is required")
            .Must(BeValidUri)
            .WithMessage("API endpoint must be a valid URI");

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage("Location is required")
            .MaximumLength(100)
            .WithMessage("Location cannot exceed 100 characters");

        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .WithMessage("Time zone is required")
            .Must(BeValidTimeZone)
            .WithMessage("Time zone must be a valid timezone identifier");

        RuleFor(x => x.Priority)
            .GreaterThan(0)
            .WithMessage("Priority must be greater than 0")
            .LessThanOrEqualTo(10)
            .WithMessage("Priority cannot exceed 10");

        RuleFor(x => x.ConfigurationJson)
            .Must(BeValidJsonOrEmpty)
            .WithMessage("Configuration must be valid JSON or empty");
    }

    private bool BeValidUri(string uri)
    {
        return Uri.TryCreate(uri, UriKind.Absolute, out _);
    }

    private bool BeValidTimeZone(string timeZone)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return true;
        }
        catch
        {
            return false;
        }
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