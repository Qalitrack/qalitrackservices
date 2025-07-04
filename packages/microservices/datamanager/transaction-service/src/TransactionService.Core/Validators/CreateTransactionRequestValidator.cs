using FluentValidation;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;

namespace TransactionService.Core.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.TransactionType)
            .IsInEnum()
            .WithMessage("Transaction type must be a valid enum value");

        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("Vehicle ID is required")
            .MaximumLength(50)
            .WithMessage("Vehicle ID must not exceed 50 characters");

        RuleFor(x => x.DriverId)
            .NotEmpty()
            .WithMessage("Driver ID is required")
            .MaximumLength(50)
            .WithMessage("Driver ID must not exceed 50 characters");

        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .WithMessage("Supplier ID is required")
            .MaximumLength(50)
            .WithMessage("Supplier ID must not exceed 50 characters");

        RuleFor(x => x.CustomerId)
            .MaximumLength(50)
            .WithMessage("Customer ID must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.CustomerId));

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required")
            .MaximumLength(50)
            .WithMessage("Product ID must not exceed 50 characters");

        RuleFor(x => x.RouteId)
            .NotEmpty()
            .WithMessage("Route ID is required")
            .MaximumLength(50)
            .WithMessage("Route ID must not exceed 50 characters");

        RuleFor(x => x.WeighbridgeId)
            .NotEmpty()
            .WithMessage("Weighbridge ID is required")
            .MaximumLength(50)
            .WithMessage("Weighbridge ID must not exceed 50 characters");

        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization ID is required")
            .MaximumLength(50)
            .WithMessage("Organization ID must not exceed 50 characters");

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
            .Must(HaveCustomerForOutbound)
            .WithMessage("Customer ID is required for outgoing transactions")
            .OverridePropertyName(nameof(CreateTransactionRequest.CustomerId));

        RuleFor(x => x.Metadata)
            .Must(HaveValidMetadata)
            .WithMessage("Metadata contains invalid values")
            .When(x => x.Metadata != null);
    }

    private bool HaveCustomerForOutbound(CreateTransactionRequest request)
    {
        if (request.TransactionType == TransactionType.Outgoing)
        {
            return !string.IsNullOrEmpty(request.CustomerId);
        }
        return true;
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