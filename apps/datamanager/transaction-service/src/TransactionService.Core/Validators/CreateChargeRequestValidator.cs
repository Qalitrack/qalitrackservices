using FluentValidation;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;

namespace TransactionService.Core.Validators;

public class CreateChargeRequestValidator : AbstractValidator<CreateChargeRequest>
{
    public CreateChargeRequestValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("Transaction ID is required")
            .MaximumLength(50)
            .WithMessage("Transaction ID must not exceed 50 characters");

        RuleFor(x => x.ChargeType)
            .IsInEnum()
            .WithMessage("Charge type must be a valid enum value");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Amount cannot exceed 1,000,000");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100)
            .WithMessage("Tax rate must be between 0 and 100 percent")
            .When(x => x.TaxRate.HasValue);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-character ISO currency code")
            .Must(BeValidCurrency)
            .WithMessage("Currency must be a valid ISO currency code");

        // Business rule validations
        RuleFor(x => x)
            .Must(HaveValidDiscountAmount)
            .WithMessage("Discount amount must be negative")
            .When(x => x.ChargeType == ChargeType.Discount);

        RuleFor(x => x)
            .Must(HaveValidTaxConfiguration)
            .WithMessage("Tax charges should not have additional tax rates applied")
            .When(x => x.ChargeType == ChargeType.Tax);

        RuleFor(x => x)
            .Must(HaveReasonableAmount)
            .WithMessage("Amount seems unreasonable for the charge type")
            .When(x => ShouldValidateAmountForChargeType(x.ChargeType));

        RuleFor(x => x.Description)
            .Must(HaveAppropriateDescription)
            .WithMessage("Description should be appropriate for the charge type")
            .WithName("Description");
    }

    private bool BeValidCurrency(string currency)
    {
        if (string.IsNullOrEmpty(currency))
            return false;

        // Common ISO currency codes
        var validCurrencies = new[]
        {
            "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD",
            "MXN", "SGD", "HKD", "NOK", "TRY", "ZAR", "BRL", "RUB", "INR", "KRW"
        };

        return validCurrencies.Contains(currency.ToUpperInvariant());
    }

    private bool HaveValidDiscountAmount(CreateChargeRequest request)
    {
        if (request.ChargeType != ChargeType.Discount)
            return true;

        return request.Amount < 0;
    }

    private bool HaveValidTaxConfiguration(CreateChargeRequest request)
    {
        if (request.ChargeType != ChargeType.Tax)
            return true;

        // Tax charges should not have additional tax rates
        return !request.TaxRate.HasValue || request.TaxRate.Value == 0;
    }

    private bool ShouldValidateAmountForChargeType(ChargeType chargeType)
    {
        // Only validate amounts for certain charge types that have reasonable ranges
        return chargeType == ChargeType.BaseCharge || 
               chargeType == ChargeType.ProcessingFee || 
               chargeType == ChargeType.DocumentationFee;
    }

    private bool HaveReasonableAmount(CreateChargeRequest request)
    {
        return request.ChargeType switch
        {
            ChargeType.BaseCharge => request.Amount >= 5 && request.Amount <= 1000,
            ChargeType.ProcessingFee => request.Amount >= 1 && request.Amount <= 500,
            ChargeType.DocumentationFee => request.Amount >= 1 && request.Amount <= 100,
            ChargeType.StorageFee => request.Amount >= 1 && request.Amount <= 10000,
            ChargeType.OvertimeFee => request.Amount >= 5 && request.Amount <= 1000,
            ChargeType.WeightPenalty => request.Amount >= 1 && request.Amount <= 50000,
            ChargeType.Fine => request.Amount >= 10 && request.Amount <= 100000,
            _ => true // No specific validation for other types
        };
    }

    private bool HaveAppropriateDescription(CreateChargeRequest request, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return false;

        // Basic validation that description contains some meaningful content
        if (description.Trim().Length < 5)
            return false;

        // Check for common inappropriate content
        var inappropriateTerms = new[] { "test", "dummy", "xxx", "temp" };
        var lowerDescription = description.ToLowerInvariant();
        
        // Don't allow descriptions that are just inappropriate terms
        if (inappropriateTerms.Any(term => lowerDescription == term))
            return false;

        // Validate that description somewhat matches charge type
        return request.ChargeType switch
        {
            ChargeType.BaseCharge => !lowerDescription.Contains("penalty") && !lowerDescription.Contains("fine"),
            ChargeType.WeightPenalty => lowerDescription.Contains("weight") || lowerDescription.Contains("penalty") || lowerDescription.Contains("overweight"),
            ChargeType.StorageFee => lowerDescription.Contains("storage") || lowerDescription.Contains("parking") || lowerDescription.Contains("hold"),
            ChargeType.OvertimeFee => lowerDescription.Contains("overtime") || lowerDescription.Contains("after hours") || lowerDescription.Contains("weekend"),
            ChargeType.DocumentationFee => lowerDescription.Contains("document") || lowerDescription.Contains("paperwork") || lowerDescription.Contains("record"),
            ChargeType.Tax => lowerDescription.Contains("tax") || lowerDescription.Contains("vat") || lowerDescription.Contains("duty"),
            ChargeType.Discount => lowerDescription.Contains("discount") || lowerDescription.Contains("rebate") || lowerDescription.Contains("credit"),
            ChargeType.Fine => lowerDescription.Contains("fine") || lowerDescription.Contains("penalty") || lowerDescription.Contains("violation"),
            _ => true // No specific validation for other types
        };
    }
}