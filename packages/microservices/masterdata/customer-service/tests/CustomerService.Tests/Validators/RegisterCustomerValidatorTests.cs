using FluentValidation.TestHelper;
using CustomerService.Core.Validators;

namespace CustomerService.Tests.Validators;

[Trait("Category", "Unit")]
public class RegisterCustomerValidatorTests
{
    private readonly RegisterCustomerValidator _validator;

    public RegisterCustomerValidatorTests()
    {
        _validator = new RegisterCustomerValidator();
    }

    #region Name Validation Tests

    [Fact]
    public void Validate_ShouldHaveError_WhenNameIsEmpty()
    {
        // Arrange
        var request = new RegisterCustomerRequest { Name = string.Empty };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Customer name is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNameIsNull()
    {
        // Arrange
        var request = new RegisterCustomerRequest { Name = null! };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Customer name is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('A', 501); // Exceeds 500 character limit
        var request = new RegisterCustomerRequest { Name = longName };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Customer name cannot exceed 500 characters");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenNameIsValid()
    {
        // Arrange
        var request = new RegisterCustomerRequest { Name = "Valid Customer Name" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenNameIsAtMaxLength()
    {
        // Arrange
        var maxLengthName = new string('A', 500); // Exactly 500 characters
        var request = new RegisterCustomerRequest { Name = maxLengthName };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Contact Email Validation Tests

    [Fact]
    public void Validate_ShouldHaveError_WhenContactEmailIsEmpty()
    {
        // Arrange
        var request = new RegisterCustomerRequest { ContactEmail = string.Empty };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail)
              .WithErrorMessage("Contact email is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenContactEmailIsNull()
    {
        // Arrange
        var request = new RegisterCustomerRequest { ContactEmail = null! };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail)
              .WithErrorMessage("Contact email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid.email")]
    public void Validate_ShouldHaveError_WhenContactEmailIsInvalid(string invalidEmail)
    {
        // Arrange
        var request = new RegisterCustomerRequest { ContactEmail = invalidEmail };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail)
              .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenContactEmailExceedsMaxLength()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@test.com"; // Exceeds 255 character limit
        var request = new RegisterCustomerRequest { ContactEmail = longEmail };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail)
              .WithErrorMessage("Email cannot exceed 255 characters");
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("test.email+tag@example.org")]
    [InlineData("user123@test-domain.com")]
    public void Validate_ShouldNotHaveError_WhenContactEmailIsValid(string validEmail)
    {
        // Arrange
        var request = new RegisterCustomerRequest { ContactEmail = validEmail };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ContactEmail);
    }

    #endregion

    #region Billing Address Validation Tests

    [Fact]
    public void Validate_ShouldHaveError_WhenBillingAddressIsEmpty()
    {
        // Arrange
        var request = new RegisterCustomerRequest { BillingAddress = string.Empty };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BillingAddress)
              .WithErrorMessage("Billing address is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenBillingAddressIsNull()
    {
        // Arrange
        var request = new RegisterCustomerRequest { BillingAddress = null! };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BillingAddress)
              .WithErrorMessage("Billing address is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenBillingAddressExceedsMaxLength()
    {
        // Arrange
        var longAddress = new string('A', 1001); // Exceeds 1000 character limit
        var request = new RegisterCustomerRequest { BillingAddress = longAddress };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BillingAddress)
              .WithErrorMessage("Billing address cannot exceed 1000 characters");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenBillingAddressIsValid()
    {
        // Arrange
        var request = new RegisterCustomerRequest 
        { 
            BillingAddress = "123 Main Street, Anytown, ST 12345" 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.BillingAddress);
    }

    #endregion

    #region Credit Limit Validation Tests

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void Validate_ShouldHaveError_WhenCreditLimitIsNegative(decimal negativeAmount)
    {
        // Arrange
        var request = new RegisterCustomerRequest { CreditLimit = negativeAmount };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CreditLimit)
              .WithErrorMessage("Credit limit must be non-negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    [InlineData(50000.50)]
    [InlineData(999999.99)]
    public void Validate_ShouldNotHaveError_WhenCreditLimitIsValid(decimal validAmount)
    {
        // Arrange
        var request = new RegisterCustomerRequest { CreditLimit = validAmount };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.CreditLimit);
    }

    #endregion

    #region Tax Number Validation Tests

    [Fact]
    public void Validate_ShouldNotHaveError_WhenTaxNumberIsNull()
    {
        // Arrange
        var request = new RegisterCustomerRequest { TaxNumber = null };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.TaxNumber);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenTaxNumberIsEmpty()
    {
        // Arrange
        var request = new RegisterCustomerRequest { TaxNumber = string.Empty };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.TaxNumber);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTaxNumberExceedsMaxLength()
    {
        // Arrange
        var longTaxNumber = new string('1', 51); // Exceeds 50 character limit
        var request = new RegisterCustomerRequest { TaxNumber = longTaxNumber };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TaxNumber)
              .WithErrorMessage("Tax number cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("TAX-123-456")]
    [InlineData("12-3456789")]
    public void Validate_ShouldNotHaveError_WhenTaxNumberIsValid(string validTaxNumber)
    {
        // Arrange
        var request = new RegisterCustomerRequest { TaxNumber = validTaxNumber };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.TaxNumber);
    }

    #endregion

    #region Registration Number Validation Tests

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRegistrationNumberIsNull()
    {
        // Arrange
        var request = new RegisterCustomerRequest { RegistrationNumber = null };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.RegistrationNumber);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenRegistrationNumberIsEmpty()
    {
        // Arrange
        var request = new RegisterCustomerRequest { RegistrationNumber = string.Empty };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.RegistrationNumber);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRegistrationNumberExceedsMaxLength()
    {
        // Arrange
        var longRegNumber = new string('A', 51); // Exceeds 50 character limit
        var request = new RegisterCustomerRequest { RegistrationNumber = longRegNumber };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RegistrationNumber)
              .WithErrorMessage("Registration number cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("REG123456")]
    [InlineData("ABC-123-DEF")]
    [InlineData("12345")]
    public void Validate_ShouldNotHaveError_WhenRegistrationNumberIsValid(string validRegNumber)
    {
        // Arrange
        var request = new RegisterCustomerRequest { RegistrationNumber = validRegNumber };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.RegistrationNumber);
    }

    #endregion

    #region Contact Phone Validation Tests

    [Fact]
    public void Validate_ShouldNotHaveError_WhenContactPhoneIsNull()
    {
        // Arrange
        var request = new RegisterCustomerRequest { ContactPhone = null };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ContactPhone);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenContactPhoneIsEmpty()
    {
        // Arrange
        var request = new RegisterCustomerRequest { ContactPhone = string.Empty };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ContactPhone);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenContactPhoneExceedsMaxLength()
    {
        // Arrange
        var longPhone = new string('1', 21); // Exceeds 20 character limit
        var request = new RegisterCustomerRequest { ContactPhone = longPhone };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ContactPhone)
              .WithErrorMessage("Contact phone cannot exceed 20 characters");
    }

    [Theory]
    [InlineData("+1-555-123-4567")]
    [InlineData("555-123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("15551234567")]
    [InlineData("+44 20 7946 0958")]
    public void Validate_ShouldNotHaveError_WhenContactPhoneIsValid(string validPhone)
    {
        // Arrange
        var request = new RegisterCustomerRequest { ContactPhone = validPhone };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ContactPhone);
    }

    #endregion

    #region Complete Valid Request Tests

    [Fact]
    public void Validate_ShouldPassValidation_WhenAllFieldsAreValid()
    {
        // Arrange
        var validRequest = new RegisterCustomerRequest
        {
            Name = "Valid Customer Corp",
            ContactEmail = "contact@validcustomer.com",
            BillingAddress = "123 Valid Street, Valid City, VC 12345",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000,
            TaxNumber = "TAX123456789",
            RegistrationNumber = "REG987654321",
            ContactPhone = "+1-555-123-4567",
            Notes = "Valid customer registration"
        };

        // Act & Assert
        var result = _validator.TestValidate(validRequest);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldPassValidation_WhenOnlyRequiredFieldsProvided()
    {
        // Arrange
        var minimalRequest = new RegisterCustomerRequest
        {
            Name = "Minimal Customer",
            ContactEmail = "minimal@customer.com",
            BillingAddress = "123 Minimal St",
            CustomerType = CustomerType.Individual,
            CreditLimit = 0
            // Optional fields (TaxNumber, RegistrationNumber, ContactPhone, Notes) not provided
        };

        // Act & Assert
        var result = _validator.TestValidate(minimalRequest);
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Validate_ShouldHandleWhitespaceInRequiredFields()
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = "   ", // Only whitespace
            ContactEmail = "  test@example.com  ", // Valid email with surrounding whitespace
            BillingAddress = "   123 Main St   " // Valid address with surrounding whitespace
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        
        // Name with only whitespace should fail
        result.ShouldHaveValidationErrorFor(x => x.Name);
        
        // Email and address should be valid (trimming is typically handled by the application)
        result.ShouldNotHaveValidationErrorFor(x => x.ContactEmail);
        result.ShouldNotHaveValidationErrorFor(x => x.BillingAddress);
    }

    [Fact]
    public void Validate_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = "Üñíçødé Çømpåñy",
            ContactEmail = "unicode@tëst.com",
            BillingAddress = "123 Üñíçødé Street, Tøkyø",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 5000
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
        result.ShouldNotHaveValidationErrorFor(x => x.BillingAddress);
        // Email validation depends on the email validator implementation
    }

    #endregion
}