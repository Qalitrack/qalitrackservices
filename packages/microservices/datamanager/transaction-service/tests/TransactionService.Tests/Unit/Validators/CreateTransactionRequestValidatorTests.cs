using FluentAssertions;
using FluentValidation.TestHelper;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Validators;
using Xunit;

namespace TransactionService.Tests.Unit.Validators;

public class CreateTransactionRequestValidatorTests
{
    private readonly CreateTransactionRequestValidator _validator;

    public CreateTransactionRequestValidatorTests()
    {
        _validator = new CreateTransactionRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Incoming,
            VehicleId = "VEH001",
            DriverId = "DRV001",
            SupplierId = "SUP001",
            ProductId = "PRD001",
            RouteId = "RTE001",
            WeighbridgeId = "WB001",
            OrganizationId = "ORG001",
            DeliveryNoteNumber = "DN001",
            PermitNumber = "PM001",
            Remarks = "Test remarks"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyVehicleId_ShouldHaveValidationError(string vehicleId)
    {
        // Arrange
        var request = CreateValidRequest();
        request.VehicleId = vehicleId;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId)
            .WithErrorMessage("Vehicle ID is required");
    }

    [Fact]
    public void Validate_VehicleIdTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.VehicleId = new string('A', 51); // 51 characters

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId)
            .WithErrorMessage("Vehicle ID must not exceed 50 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyDriverId_ShouldHaveValidationError(string driverId)
    {
        // Arrange
        var request = CreateValidRequest();
        request.DriverId = driverId;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DriverId)
            .WithErrorMessage("Driver ID is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyOrganizationId_ShouldHaveValidationError(string organizationId)
    {
        // Arrange
        var request = CreateValidRequest();
        request.OrganizationId = organizationId;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrganizationId)
            .WithErrorMessage("Organization ID is required");
    }

    [Fact]
    public void Validate_OutgoingTransactionWithoutCustomer_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TransactionType = TransactionType.Outgoing;
        request.CustomerId = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("Customer ID is required for outgoing transactions");
    }

    [Fact]
    public void Validate_OutgoingTransactionWithCustomer_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TransactionType = TransactionType.Outgoing;
        request.CustomerId = "CUST001";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_IncomingTransactionWithoutCustomer_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TransactionType = TransactionType.Incoming;
        request.CustomerId = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_DeliveryNoteNumberTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DeliveryNoteNumber = new string('D', 101); // 101 characters

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeliveryNoteNumber)
            .WithErrorMessage("Delivery note number must not exceed 100 characters");
    }

    [Fact]
    public void Validate_PermitNumberTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PermitNumber = new string('P', 101); // 101 characters

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PermitNumber)
            .WithErrorMessage("Permit number must not exceed 100 characters");
    }

    [Fact]
    public void Validate_RemarksTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Remarks = new string('R', 1001); // 1001 characters

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Remarks)
            .WithErrorMessage("Remarks must not exceed 1000 characters");
    }

    [Fact]
    public void Validate_MetadataTooLarge_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        var largeMetadata = new Dictionary<string, object>();
        
        // Create metadata that would serialize to more than 10,000 characters
        for (int i = 0; i < 100; i++)
        {
            largeMetadata[$"key{i}"] = new string('A', 200);
        }
        request.Metadata = largeMetadata;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata)
            .WithErrorMessage("Metadata size exceeds the maximum allowed limit");
    }

    [Fact]
    public void Validate_MetadataWithInvalidValue_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Metadata = new Dictionary<string, object>
        {
            ["validKey"] = "validValue",
            ["invalidKey"] = new object() // Object that can't be serialized properly
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata)
            .WithErrorMessage("Metadata contains invalid values. Only strings, numbers, booleans, and nested objects are allowed.");
    }

    [Fact]
    public void Validate_ValidMetadata_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Metadata = new Dictionary<string, object>
        {
            ["stringValue"] = "test",
            ["numberValue"] = 42,
            ["booleanValue"] = true,
            ["nestedObject"] = new Dictionary<string, object>
            {
                ["nestedString"] = "nested test"
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata);
    }

    private static CreateTransactionRequest CreateValidRequest()
    {
        return new CreateTransactionRequest
        {
            TransactionType = TransactionType.Incoming,
            VehicleId = "VEH001",
            DriverId = "DRV001",
            SupplierId = "SUP001",
            ProductId = "PRD001",
            RouteId = "RTE001",
            WeighbridgeId = "WB001",
            OrganizationId = "ORG001"
        };
    }
}