using AutoMapper;
using FluentAssertions;
using Moq;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;
using TransactionService.Core.Services;
using Xunit;

namespace TransactionService.Tests.Unit.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IWorkflowService> _workflowServiceMock;
    private readonly Mock<IChargeService> _chargeServiceMock;
    private readonly Mock<IMasterDataIntegrationService> _masterDataServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Core.Services.TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _workflowServiceMock = new Mock<IWorkflowService>();
        _chargeServiceMock = new Mock<IChargeService>();
        _masterDataServiceMock = new Mock<IMasterDataIntegrationService>();
        _mapperMock = new Mock<IMapper>();

        _transactionService = new Core.Services.TransactionService(
            _transactionRepositoryMock.Object,
            _workflowServiceMock.Object,
            _chargeServiceMock.Object,
            _masterDataServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task CreateTransactionAsync_ValidRequest_ReturnsTransactionDto()
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
            OrganizationId = "ORG001"
        };

        var transaction = new WeighingTransaction
        {
            Id = "TXN001",
            TransactionNumber = "TXN20241201001"
        };

        var expectedDto = new TransactionDto
        {
            Id = "TXN001",
            TransactionNumber = "TXN20241201001"
        };

        // Setup mocks
        SetupMasterDataValidationMocks(true);
        _transactionRepositoryMock.Setup(x => x.GenerateTransactionNumberAsync())
            .ReturnsAsync("TXN20241201001");
        _mapperMock.Setup(x => x.Map<WeighingTransaction>(request))
            .Returns(transaction);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<WeighingTransaction>()))
            .ReturnsAsync(transaction);
        _workflowServiceMock.Setup(x => x.InitializeWorkflowAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _chargeServiceMock.Setup(x => x.CalculateChargesAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mapperMock.Setup(x => x.Map<TransactionDto>(transaction))
            .Returns(expectedDto);

        // Act
        var result = await _transactionService.CreateTransactionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("TXN001");
        result.TransactionNumber.Should().Be("TXN20241201001");

        _workflowServiceMock.Verify(x => x.InitializeWorkflowAsync("TXN001"), Times.Once);
        _chargeServiceMock.Verify(x => x.CalculateChargesAsync("TXN001"), Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_InvalidMasterData_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            VehicleId = "INVALID_VEH",
            DriverId = "DRV001",
            SupplierId = "SUP001",
            ProductId = "PRD001",
            RouteId = "RTE001",
            WeighbridgeId = "WB001",
            OrganizationId = "ORG001"
        };

        // Setup invalid vehicle validation
        _masterDataServiceMock.Setup(x => x.ValidateVehicleAsync("INVALID_VEH"))
            .ReturnsAsync(false);
        SetupMasterDataValidationMocks(true, excludeVehicle: true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _transactionService.CreateTransactionAsync(request));

        exception.Message.Should().Contain("Invalid master data entities: Vehicle");
    }

    [Fact]
    public async Task GetTransactionAsync_ExistingId_ReturnsTransactionDto()
    {
        // Arrange
        var transactionId = "TXN001";
        var transaction = new WeighingTransaction { Id = transactionId };
        var expectedDto = new TransactionDto { Id = transactionId };

        _transactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);
        _mapperMock.Setup(x => x.Map<TransactionDto>(transaction))
            .Returns(expectedDto);

        // Act
        var result = await _transactionService.GetTransactionAsync(transactionId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(transactionId);
    }

    [Fact]
    public async Task GetTransactionAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var transactionId = "NONEXISTENT";
        _transactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId))
            .ReturnsAsync((WeighingTransaction?)null);

        // Act
        var result = await _transactionService.GetTransactionAsync(transactionId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CancelTransactionAsync_ExistingTransaction_ReturnsTrue()
    {
        // Arrange
        var transactionId = "TXN001";
        var reason = "Cancelled by user";
        var transaction = new WeighingTransaction
        {
            Id = transactionId,
            Status = TransactionStatus.Pending,
            Remarks = "Original remarks"
        };

        _transactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);
        _transactionRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<WeighingTransaction>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.CancelTransactionAsync(transactionId, reason);

        // Assert
        result.Should().BeTrue();
        transaction.Status.Should().Be(TransactionStatus.Cancelled);
        transaction.Remarks.Should().Contain("Cancelled by user");
    }

    private void SetupMasterDataValidationMocks(bool isValid, bool excludeVehicle = false)
    {
        if (!excludeVehicle)
            _masterDataServiceMock.Setup(x => x.ValidateVehicleAsync(It.IsAny<string>()))
                .ReturnsAsync(isValid);
        
        _masterDataServiceMock.Setup(x => x.ValidateDriverAsync(It.IsAny<string>()))
            .ReturnsAsync(isValid);
        _masterDataServiceMock.Setup(x => x.ValidateSupplierAsync(It.IsAny<string>()))
            .ReturnsAsync(isValid);
        _masterDataServiceMock.Setup(x => x.ValidateCustomerAsync(It.IsAny<string>()))
            .ReturnsAsync(isValid);
        _masterDataServiceMock.Setup(x => x.ValidateProductAsync(It.IsAny<string>()))
            .ReturnsAsync(isValid);
        _masterDataServiceMock.Setup(x => x.ValidateRouteAsync(It.IsAny<string>()))
            .ReturnsAsync(isValid);
        _masterDataServiceMock.Setup(x => x.ValidateWeighbridgeAsync(It.IsAny<string>()))
            .ReturnsAsync(isValid);
        _masterDataServiceMock.Setup(x => x.ValidateOrganizationAsync(It.IsAny<string>()))
            .ReturnsAsync(isValid);
    }
}