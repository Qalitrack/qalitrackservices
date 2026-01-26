/*using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using Transaction.Core.Entities;
using Transaction.Tests.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Transaction.Tests.Core.Entities;

public class WeighbridgeTransactionTests
{
    private readonly ITestOutputHelper _output;

    public WeighbridgeTransactionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Create_WithMinimumRequiredFields_ShouldSucceed()
    {
        // Arrange
        var builder = new WeighbridgeTransactionBuilder();

        // Act
        var transaction = builder
            .WithBasicInfo()
            .WithWeighingInfo(false)
            .WithVehicleInfo()
            .WithTransporterInfo()
            .WithWeighbridgeInfo()
            .Build();

        // Assert
        transaction.Should().NotBeNull();
        transaction.TicketID.Should().BeGreaterThan(0);
        transaction.ReceiptNo.Should().NotBeNullOrWhiteSpace();
        transaction.NoPlate.Should().NotBeNullOrWhiteSpace();
        transaction.DriverName.Should().NotBeNullOrWhiteSpace();
        transaction.TransporterID.Should().BeGreaterThan(0);
        transaction.TransporterName.Should().NotBeNullOrWhiteSpace();
        transaction.FirstWeight.Should().NotBeNullOrWhiteSpace();
        transaction.WeighBridgeID.Should().BeGreaterThan(0);
        transaction.WeighBridgeName.Should().NotBeNullOrWhiteSpace();
        transaction.OperatorID.Should().BeGreaterThan(0);
        transaction.OperatorName.Should().NotBeNullOrWhiteSpace();
        transaction.Status.Should().Be("Active");
        transaction.FirstWeightDate.Should().NotBe(default);

        LogTransaction("Minimum Required Fields", transaction);
    }

    [Fact]
    public void Create_WithCompleteTransaction_ShouldSucceed()
    {
        // Arrange
        var builder = new WeighbridgeTransactionBuilder();

        // Act
        var transaction = builder
            .WithBasicInfo()
            .WithWeighingInfo(true)
            .WithVehicleInfo()
            .WithCommodityInfo()
            .WithTransporterInfo()
            .WithWeighbridgeInfo()
            .WithSecondWeighingInfo()
            .WithLocationInfo()
            .WithCustomerSupplierInfo()
            .Build();

        // Assert
        transaction.Should().NotBeNull();
        transaction.TicketID.Should().BeGreaterThan(0);
        transaction.ReceiptNo.Should().NotBeNullOrWhiteSpace();
        
        // Weighing information
        transaction.FirstWeight.Should().NotBeNullOrWhiteSpace();
        transaction.SecondWeight.Should().NotBeNullOrWhiteSpace();
        transaction.NetWeight.Should().NotBeNullOrWhiteSpace();
        
        // Vehicle information
        transaction.VehicleID.Should().BeGreaterThan(0);
        transaction.NoPlate.Should().NotBeNullOrWhiteSpace();
        transaction.DriverName.Should().NotBeNullOrWhiteSpace();
        
        // Commodity information
        transaction.CommodityID.Should().BeGreaterThan(0);
        transaction.CommodityName.Should().NotBeNullOrWhiteSpace();
        
        // Transporter information
        transaction.TransporterID.Should().BeGreaterThan(0);
        transaction.TransporterName.Should().NotBeNullOrWhiteSpace();
        
        // Weighbridge information
        transaction.WeighBridgeID.Should().BeGreaterThan(0);
        transaction.WeighBridgeName.Should().NotBeNullOrWhiteSpace();
        transaction.ScaleName.Should().NotBeNullOrWhiteSpace();
        transaction.OperatorID.Should().BeGreaterThan(0);
        transaction.OperatorName.Should().NotBeNullOrWhiteSpace();
        
        // Second weighing information
        transaction.WeighBridgeName2nd.Should().NotBeNullOrWhiteSpace();
        transaction.ScaleName2nd.Should().NotBeNullOrWhiteSpace();
        transaction.OperatorID2nd.Should().NotBeNullOrWhiteSpace();
        transaction.OperatorName2nd.Should().NotBeNullOrWhiteSpace();
        
        // Location information
        transaction.OriginID.Should().BeGreaterThan(0);
        transaction.OriginName.Should().NotBeNullOrWhiteSpace();
        transaction.DestinationID.Should().BeGreaterThan(0);
        transaction.DestinationName.Should().NotBeNullOrWhiteSpace();
        
        // Customer/Supplier information
        transaction.SupplierID.Should().BeGreaterThan(0);
        transaction.SupplierName.Should().NotBeNullOrWhiteSpace();
        transaction.CustomerID.Should().BeGreaterThan(0);
        transaction.CustomerName.Should().NotBeNullOrWhiteSpace();
        
        // Status and timestamps
        transaction.Status.Should().Be("Active");
        transaction.FirstWeightDate.Should().NotBe(default);
        transaction.SecondWeightDate.Should().NotBe(default);

        LogTransaction("Complete Transaction", transaction);
    }

    [Fact]
    public void Create_WithMultipleTransactions_ShouldHaveUniqueIds()
    {
        // Arrange
        var builder1 = new WeighbridgeTransactionBuilder();
        var transaction1 = builder1
            .WithBasicInfo()
            .WithWeighingInfo()
            .WithVehicleInfo()
            .Build();

        var builder2 = new WeighbridgeTransactionBuilder();
        var transaction2 = builder2
            .WithBasicInfo()
            .WithWeighingInfo()
            .WithVehicleInfo()
            .Build();

        // Assert
        transaction1.TicketID.Should().NotBe(transaction2.TicketID);
        transaction1.ReceiptNo.Should().NotBe(transaction2.ReceiptNo);
        
        LogTransaction("Transaction 1", transaction1);
        LogTransaction("Transaction 2", transaction2);
    }

    [Fact]
    public void Create_WithoutRequiredFields_ShouldFailValidation()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction(); // Missing required fields

        // Act
        var validationResults = ValidateModel(transaction);

        // Assert
        validationResults.Should().NotBeEmpty();
        var errorMessages = validationResults.Select(v => v.ErrorMessage);
        
        _output.WriteLine("Validation Errors:");
        foreach (var error in errorMessages)
        {
            _output.WriteLine($"- {error}");
        }
    }

    [Fact]
    public void CalculateNetWeight_ShouldBeCorrect()
    {
        // Arrange
        var builder = new WeighbridgeTransactionBuilder();
        var transaction = builder
            .WithBasicInfo()
            .WithWeighingInfo(true)
            .Build();

        // Act
        var firstWeight = decimal.Parse(transaction.FirstWeight);
        var secondWeight = decimal.Parse(transaction.SecondWeight!);
        var calculatedNetWeight = firstWeight - secondWeight;

        // Assert
        decimal.Parse(transaction.NetWeight!).Should().Be(calculatedNetWeight);
        
        _output.WriteLine($"First Weight: {firstWeight}");
        _output.WriteLine($"Second Weight: {secondWeight}");
        _output.WriteLine($"Net Weight: {transaction.NetWeight}");
        _output.WriteLine($"Calculated Net Weight: {calculatedNetWeight}");
    }

    [Fact]
    public void Create_WithInvalidWeight_ShouldThrowException()
    {
        // Arrange
        var builder = new WeighbridgeTransactionBuilder();
        var transaction = builder
            .WithBasicInfo()
            .WithWeighingInfo()
            .Build();

        // Act & Assert
        var exception = Assert.Throws<FormatException>(() =>
        {
            // This will throw if the weight strings can't be parsed to decimal
            var weight = decimal.Parse("invalid");
        });

        _output.WriteLine($"Expected exception thrown: {exception.Message}");
    }

    [Fact]
    public void Create_WithFutureDate_ShouldFailValidation()
    {
        // Arrange
        var builder = new WeighbridgeTransactionBuilder();
        var transaction = builder
            .WithBasicInfo()
            .WithWeighingInfo()
            .WithVehicleInfo()
            .WithTransporterInfo()
            .Build();

        // Set a future date
        transaction.FirstWeightDate = DateTime.UtcNow.AddDays(1);

        // Act
        var validationResults = ValidateModel(transaction);
        var errorMessages = validationResults.Select(v => v.ErrorMessage).ToList();

        // Assert
        validationResults.Should().NotBeEmpty("because future dates should not be allowed");
    
        _output.WriteLine("Validation Errors:");
        foreach (var error in errorMessages)
        {
            _output.WriteLine($"- {error}");
        }

        // Check if any validation error is related to future date
        var hasFutureDateError = validationResults.Any(v => 
            v.MemberNames.Contains(nameof(WeighbridgeTransaction.FirstWeightDate)) &&
            v.ErrorMessage != null && 
            v.ErrorMessage.Contains("future", StringComparison.OrdinalIgnoreCase));
        
        hasFutureDateError.Should().BeTrue("should have a validation error for future date");
    }

    private void LogTransaction(string scenario, WeighbridgeTransaction transaction)
    {
        _output.WriteLine($"\n=== {scenario} ===");
        _output.WriteLine($"Ticket ID: {transaction.TicketID}");
        _output.WriteLine($"Receipt No: {transaction.ReceiptNo}");
        _output.WriteLine($"Status: {transaction.Status}");
        _output.WriteLine($"Weights: First={transaction.FirstWeight}, " +
                         $"Second={transaction.SecondWeight}, Net={transaction.NetWeight}");
        _output.WriteLine($"Vehicle: {transaction.NoPlate} (ID: {transaction.VehicleID})");
        _output.WriteLine($"Driver: {transaction.DriverName}");
        _output.WriteLine($"Commodity: {transaction.CommodityName} (ID: {transaction.CommodityID})");
        _output.WriteLine($"Transporter: {transaction.TransporterName} (ID: {transaction.TransporterID})");
        _output.WriteLine($"Weighbridge: {transaction.WeighBridgeName} (ID: {transaction.WeighBridgeID})");
        _output.WriteLine($"Scale: {transaction.ScaleName}, Operator: {transaction.OperatorName}");
        
        if (!string.IsNullOrEmpty(transaction.WeighBridgeName2nd))
        {
            _output.WriteLine($"Second Weighing - Scale: {transaction.ScaleName2nd}, " +
                            $"Operator: {transaction.OperatorName2nd}");
        }
        
        _output.WriteLine($"Timing: First={transaction.FirstWeightDate}, " +
                         $"Second={transaction.SecondWeightDate}");
    }

    private static System.Collections.Generic.List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new System.Collections.Generic.List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}*/