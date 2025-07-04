using FluentValidation;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;

namespace TransactionService.Core.Validators;

public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid enum value")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.GrossWeight)
            .GreaterThan(0)
            .WithMessage("Gross weight must be greater than 0")
            .LessThanOrEqualTo(100000) // 100 tons max
            .WithMessage("Gross weight cannot exceed 100,000 kg")
            .When(x => x.GrossWeight.HasValue);

        RuleFor(x => x.TareWeight)
            .GreaterThan(0)
            .WithMessage("Tare weight must be greater than 0")
            .LessThanOrEqualTo(50000) // 50 tons max for vehicle tare weight
            .WithMessage("Tare weight cannot exceed 50,000 kg")
            .When(x => x.TareWeight.HasValue);

        RuleFor(x => x.NetWeight)
            .GreaterThan(0)
            .WithMessage("Net weight must be greater than 0")
            .LessThanOrEqualTo(80000) // 80 tons max net weight
            .WithMessage("Net weight cannot exceed 80,000 kg")
            .When(x => x.NetWeight.HasValue);

        RuleFor(x => x.EntryWeighingTime)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Entry weighing time cannot be in the future")
            .When(x => x.EntryWeighingTime.HasValue);

        RuleFor(x => x.ExitWeighingTime)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Exit weighing time cannot be in the future")
            .When(x => x.ExitWeighingTime.HasValue);

        RuleFor(x => x.DeliveryNoteNumber)
            .MaximumLength(100)
            .WithMessage("Delivery note number must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.DeliveryNoteNumber));

        RuleFor(x => x.PermitNumber)
            .MaximumLength(100)
            .WithMessage("Permit number must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.PermitNumber));

        RuleFor(x => x.Remarks)
            .MaximumLength(1000)
            .WithMessage("Remarks must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Remarks));

        // Business rule validations
        RuleFor(x => x)
            .Must(HaveValidWeightRelationship)
            .WithMessage("Net weight must be less than or equal to gross weight minus tare weight")
            .When(x => x.GrossWeight.HasValue && x.TareWeight.HasValue && x.NetWeight.HasValue);

        RuleFor(x => x)
            .Must(HaveValidWeighingTimeSequence)
            .WithMessage("Exit weighing time must be after entry weighing time")
            .When(x => x.EntryWeighingTime.HasValue && x.ExitWeighingTime.HasValue);

        RuleFor(x => x)
            .Must(HaveReasonableWeighingTimeGap)
            .WithMessage("Time gap between entry and exit weighing should be reasonable (minimum 1 minute, maximum 24 hours)")
            .When(x => x.EntryWeighingTime.HasValue && x.ExitWeighingTime.HasValue);

        RuleFor(x => x.Metadata)
            .Must(HaveValidMetadata)
            .WithMessage("Metadata contains invalid values")
            .When(x => x.Metadata != null);

        // Status transition validation
        RuleFor(x => x)
            .Must(HaveValidStatusForWeights)
            .WithMessage("Both entry and exit weights are required when marking transaction as completed")
            .When(x => x.Status == TransactionStatus.Completed);
    }

    private bool HaveValidWeightRelationship(UpdateTransactionRequest request)
    {
        if (!request.GrossWeight.HasValue || !request.TareWeight.HasValue || !request.NetWeight.HasValue)
            return true; // Skip validation if not all weights are provided

        var calculatedNetWeight = request.GrossWeight.Value - request.TareWeight.Value;
        var tolerance = 0.1m; // Allow 0.1kg tolerance for rounding

        return Math.Abs(request.NetWeight.Value - calculatedNetWeight) <= tolerance;
    }

    private bool HaveValidWeighingTimeSequence(UpdateTransactionRequest request)
    {
        if (!request.EntryWeighingTime.HasValue || !request.ExitWeighingTime.HasValue)
            return true;

        return request.ExitWeighingTime.Value > request.EntryWeighingTime.Value;
    }

    private bool HaveReasonableWeighingTimeGap(UpdateTransactionRequest request)
    {
        if (!request.EntryWeighingTime.HasValue || !request.ExitWeighingTime.HasValue)
            return true;

        var timeGap = request.ExitWeighingTime.Value - request.EntryWeighingTime.Value;
        
        // Minimum 1 minute, maximum 24 hours
        return timeGap >= TimeSpan.FromMinutes(1) && timeGap <= TimeSpan.FromHours(24);
    }

    private bool HaveValidStatusForWeights(UpdateTransactionRequest request)
    {
        if (request.Status != TransactionStatus.Completed)
            return true;

        // For completed status, we need both weighing times
        return request.EntryWeighingTime.HasValue && request.ExitWeighingTime.HasValue;
    }

    private bool HaveValidMetadata(Dictionary<string, object>? metadata)
    {
        if (metadata == null)
            return true;

        // Check for reasonable metadata size and key constraints
        if (metadata.Count > 50)
            return false;

        foreach (var kvp in metadata)
        {
            // Key validation
            if (string.IsNullOrWhiteSpace(kvp.Key) || kvp.Key.Length > 100)
                return false;

            // Value validation (basic type checking)
            if (kvp.Value != null)
            {
                var valueType = kvp.Value.GetType();
                if (!IsAllowedMetadataType(valueType))
                    return false;

                // Check string value length
                if (kvp.Value is string stringValue && stringValue.Length > 1000)
                    return false;
            }
        }

        return true;
    }

    private bool IsAllowedMetadataType(Type type)
    {
        var allowedTypes = new[]
        {
            typeof(string),
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(bool),
            typeof(DateTime),
            typeof(Guid)
        };

        return allowedTypes.Contains(type) || 
               allowedTypes.Contains(Nullable.GetUnderlyingType(type));
    }
}