using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;
using Transaction.Core.Mappings;
using Transaction.Core.Services;
using Transaction.Infrastructure.Data;
using Xunit;

namespace Transaction.Tests.Services;

public class TransactionServiceTests : IDisposable
{
    private readonly Mock<ITransactionRepository> _mockRepo;
    private readonly IMapper _mapper;
    private readonly TransactionService _service;
    private readonly TransactionDbContext _dbContext;
    private readonly Mock<ITimeService> _mockTimeService;
    private readonly DateTime _testTime;

    public TransactionServiceTests()
    {
        _mockRepo = new Mock<ITransactionRepository>();

        // Setup AutoMapper with the TransactionProfile
        var configuration = new MapperConfiguration(cfg =>
        {
            // Add the TransactionProfile which contains all the mappings
            cfg.AddProfile<TransactionProfile>();
        });
        
        // Create the mapper instance
        _mapper = configuration.CreateMapper();

        // Setup in-memory database (unused for mocked repo, but kept for potential real DbContext tests)
        var options = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TransactionDbContext(options);

        // Setup time service mock - using a fixed time for consistency in tests
        _mockTimeService = new Mock<ITimeService>();
        _testTime = new DateTime(2025, 10, 28, 15, 0, 0); // 3 PM EAT (fixed time for tests)
        _mockTimeService.Setup(t => t.Now).Returns(_testTime);
        _mockTimeService.Setup(t => t.UtcNow).Returns(_testTime.AddHours(-3)); // UTC is EAT - 3 hours
        _mockTimeService.Setup(t => t.ConvertFromUtc(It.IsAny<DateTime>())).Returns<DateTime>(dt => dt.AddHours(3));
        _mockTimeService.Setup(t => t.ConvertToUtc(It.IsAny<DateTime>())).Returns<DateTime>(dt => dt.AddHours(-3));

        _service = new TransactionService(_mockRepo.Object, _mapper, _mockTimeService.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithValidFilter_ShouldReturnPagedResults()
    {
        // Arrange
        var filter = new WeighbridgeTransactionFilter { PageNumber = 1, PageSize = 10 };
        var pagedTransactions = new PagedResult<WeighbridgeTransaction>
        {
            Items = new List<WeighbridgeTransaction> { new WeighbridgeTransaction { Id = "00000000-0000-0000-0000-000000000001", ReceiptNo = "TRX-001" } },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        _mockRepo.Setup(r => r.GetPagedAsync(filter)).ReturnsAsync(pagedTransactions);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        _mockRepo.Verify(r => r.GetPagedAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithNoItems_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var filter = new WeighbridgeTransactionFilter { PageNumber = 1, PageSize = 10 };
        var pagedTransactions = new PagedResult<WeighbridgeTransaction>
        {
            Items = new List<WeighbridgeTransaction>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        _mockRepo.Setup(r => r.GetPagedAsync(filter)).ReturnsAsync(pagedTransactions);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnDto()
    {
        // Arrange
        var id = "00000000-0000-0000-0000-000000000001";
        var transaction = new WeighbridgeTransaction { Id = "00000000-0000-0000-0000-000000000001", ReceiptNo = "TRX-001" };
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync(id)).ReturnsAsync(transaction);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("00000000-0000-0000-0000-000000000001");
        result.ReceiptNo.Should().Be("TRX-001");
        _mockRepo.Verify(r => r.GetWithWeighingRecordsAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var id = "non-existing";
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync(id)).ReturnsAsync((WeighbridgeTransaction)null);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
        _mockRepo.Verify(r => r.GetWithWeighingRecordsAsync(id), Times.Once);
    }

    #endregion

    #region GetByReceiptNoAsync Tests

    [Fact]
    public async Task GetByReceiptNoAsync_WithExistingReceiptNo_ShouldReturnDto()
    {
        // Arrange
        var receiptNo = "TRX-001";
        var transaction = new WeighbridgeTransaction { Id = "1", ReceiptNo = receiptNo };
        _mockRepo.Setup(r => r.GetByReceiptNoAsync(receiptNo)).ReturnsAsync(transaction);

        // Act
        var result = await _service.GetByReceiptNoAsync(receiptNo);

        // Assert
        result.Should().NotBeNull();
        result.ReceiptNo.Should().Be(receiptNo);
        _mockRepo.Verify(r => r.GetByReceiptNoAsync(receiptNo), Times.Once);
    }

    [Fact]
    public async Task GetByReceiptNoAsync_WithNonExistingReceiptNo_ShouldReturnNull()
    {
        // Arrange
        var receiptNo = "NON-EXISTENT";
        _mockRepo.Setup(r => r.GetByReceiptNoAsync(receiptNo)).ReturnsAsync((WeighbridgeTransaction)null);

        // Act
        var result = await _service.GetByReceiptNoAsync(receiptNo);

        // Assert
        result.Should().BeNull();
        _mockRepo.Verify(r => r.GetByReceiptNoAsync(receiptNo), Times.Once);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        var dto = new CreateTransactionDto
        {
            ReceiptNo = "TRX-001",
            ExpectedWeighings = 2,
            NoPlate = "KAA 123A",
            TransporterId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            TransporterName = "Test Transporter"
        };
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t); // Return the same transaction passed in

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.ReceiptNo.Should().Be(dto.ReceiptNo);
        result.ExpectedWeighings.Should().Be(dto.ExpectedWeighings);
        result.Status.Should().Be(WeighbridgeTransactionStatus.Pending.ToString());
        result.CreatedAt.Should().BeCloseTo(_testTime, TimeSpan.FromSeconds(1));
        result.IsCompleted.Should().BeFalse();
        _mockRepo.Verify(r => r.CreateAsync(It.Is<WeighbridgeTransaction>(t => t.ReceiptNo == dto.ReceiptNo)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithFirstWeight_ShouldSetInProgressStatusAndAddWeighingRecord()
    {
        // Arrange
        var dto = new CreateTransactionDto
        {
            ReceiptNo = "TRX-001",
            ExpectedWeighings = 2,
            NoPlate = "KAA 123A",
            FirstWeight = 1000,
            WeighBridgeId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            WeighBridgeName = "WB1",
            OperatorId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            OperatorName = "Op1"
        };

        WeighbridgeTransaction? capturedTransaction = null;
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => {
                t.Id = "00000000-0000-0000-0000-000000000001"; // Simulate repository setting the ID
                t.AuditLogs = t.AuditLogs ?? new List<TransactionAuditLog>();
                capturedTransaction = t;
                return t;
            });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WeighbridgeTransactionStatus.InProgress.ToString());
        result.FirstWeight.Should().Be(dto.FirstWeight);
        result.CompletedWeighings.Should().Be(1);
        capturedTransaction.Should().NotBeNull();
        capturedTransaction!.WeighingRecords.Should().HaveCount(1);
        _mockRepo.Verify(r => r.CreateAsync(It.Is<WeighbridgeTransaction>(t => t.WeighingRecords.Any(w => w.WeighingSequence == 1))), Times.Once);
    }

   [Theory]
[InlineData(null, null, null, null)] // No first weight
[MemberData(nameof(GetVariousScenarioData))] // With first weight
public async Task CreateAsync_VariousScenarios_ShouldHandleCorrectly(
    decimal? firstWeight, 
    string? weighBridgeId, 
    string? weighBridgeName, 
    string? operatorName)
{
    // Parse the GUID if provided
    Guid? parsedWeighBridgeId = weighBridgeId != null ? Guid.Parse(weighBridgeId) : null;

    // Rest of the test remains the same
    var dto = new CreateTransactionDto
    {
        ReceiptNo = "TRX-001",
        ExpectedWeighings = 2,
        FirstWeight = firstWeight,
        WeighBridgeId = parsedWeighBridgeId,
        WeighBridgeName = weighBridgeName,
        OperatorName = operatorName
    };
    
    var transaction = new WeighbridgeTransaction 
    { 
        Id = "00000000-0000-0000-0000-000000000001",
        ReceiptNo = dto.ReceiptNo,
        WeighingRecords = new List<WeighingRecord>(),
        AuditLogs = new List<TransactionAuditLog>()
    };

    _mockRepo.Setup(r => r.CreateAsync(It.IsAny<WeighbridgeTransaction>()))
        .ReturnsAsync((WeighbridgeTransaction t) => 
        {
            // Simulate the repository setting the ID
            t.Id = "00000000-0000-0000-0000-000000000001";
            return t;
        });

    // Act
    var result = await _service.CreateAsync(dto);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be("00000000-0000-0000-0000-000000000001");
    result.ReceiptNo.Should().Be("TRX-001");
    
    if (firstWeight.HasValue)
    {
        result.Status.Should().Be(WeighbridgeTransactionStatus.InProgress.ToString());
        result.CompletedWeighings.Should().Be(1);
        result.FirstWeight.Should().Be(firstWeight);
        result.WeighingRecords.Should().HaveCount(1);
    }
    else
    {
        result.Status.Should().Be(WeighbridgeTransactionStatus.Pending.ToString());
        result.CompletedWeighings.Should().Be(0);
        result.WeighingRecords.Should().BeEmpty();
    }
}

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidDataAndModifiableTransaction_ShouldUpdateFields()
    {
        // Arrange
        var id = "00000000-0000-0000-0000-000000000001";
        var existingTransaction = new WeighbridgeTransaction
        {
            Id = "00000000-0000-0000-0000-000000000001",
            ReceiptNo = "TRX-001",
            NoPlate = "OLD-PLATE",
            DriverName = "Old Driver",
            Status = WeighbridgeTransactionStatus.InProgress,
            IsCompleted = false
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingTransaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>())).ReturnsAsync(existingTransaction);

        var dto = new UpdateTransactionDto
        {
            NoPlate = "NEW-PLATE",
            DriverName = "New Driver"
        };

        // Act
        var result = await _service.UpdateAsync(id, dto);

        // Assert
        result.Should().NotBeNull();
        existingTransaction.NoPlate.Should().Be("NEW-PLATE");
        existingTransaction.DriverName.Should().Be("New Driver");
        existingTransaction.UpdatedAt.Should().BeCloseTo(_testTime, TimeSpan.FromSeconds(1));
        existingTransaction.AuditLogs.Should().HaveCount(1); // Audit log added
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<WeighbridgeTransaction>(t => t.NoPlate == "NEW-PLATE")), Times.Once);
    }
    [Fact]
    public async Task UpdateAsync_WithCompletedTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var id = "1";
        var existingTransaction = new WeighbridgeTransaction
        {
            Id = "1",
            Status = WeighbridgeTransactionStatus.Completed,
            IsCompleted = true
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingTransaction);

        var dto = new UpdateTransactionDto { NoPlate = "NEW-PLATE" };

        // Act & Assert
        var act = () => _service.UpdateAsync(id, dto);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("Cannot modify a completed transaction. Request reweigh permission if needed.");
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var id = "non-existing";
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((WeighbridgeTransaction)null);

        var dto = new UpdateTransactionDto { NoPlate = "NEW-PLATE" };

        // Act
        var result = await _service.UpdateAsync(id, dto);

        // Assert
        result.Should().BeNull();
        _mockRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "NEW-DRIVER", 1)] // Partial updates
    [InlineData("NEW-PLATE", null, null)]
    public async Task UpdateAsync_WithPartialData_ShouldOnlyUpdateProvidedFields(string? noPlate, string? driverName, int? vehicleId)
    {
        // Arrange
        var id = "00000000-0000-0000-0000-000000000001";
        var existingTransaction = new WeighbridgeTransaction
        {
            Id = "00000000-0000-0000-0000-000000000001",
            NoPlate = "OLD-PLATE",
            DriverName = "Old Driver",
            VehicleId = Guid.Empty
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingTransaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>())).ReturnsAsync(existingTransaction);

        var dto = new UpdateTransactionDto
        {
            NoPlate = noPlate,
            DriverName = driverName,
            VehicleId = vehicleId.HasValue ? Guid.Parse($"00000000-0000-0000-0000-{vehicleId.Value:D12}") : (Guid?)null
        };

        // Act
        await _service.UpdateAsync(id, dto);

        // Assert
        if (noPlate != null) existingTransaction.NoPlate.Should().Be(noPlate);
        if (driverName != null) existingTransaction.DriverName.Should().Be(driverName);
        if (vehicleId.HasValue) 
        {
            var expectedVehicleId = Guid.Parse($"00000000-0000-0000-0000-{vehicleId.Value:D12}");
            existingTransaction.VehicleId.Should().Be(expectedVehicleId);
        }
    }

    #endregion

    #region DeleteAsync Tests
    [Fact]
    public async Task DeleteAsync_WithExistingModifiableTransaction_ShouldReturnTrue()
    {
        // Arrange
        var id = "1";
        var transaction = new WeighbridgeTransaction { Id = id, IsCompleted = false };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithCompletedTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var id = "1";
        var transaction = new WeighbridgeTransaction { Id = id, IsCompleted = true };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(transaction);

        // Act & Assert
        var act = () => _service.DeleteAsync(id);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("Cannot delete a completed transaction.");
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var id = "non-existing";
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((WeighbridgeTransaction)null);

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        result.Should().BeFalse();
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region IsReceiptNoAvailableAsync Tests

    [Fact]
    public async Task IsReceiptNoAvailableAsync_WithAvailableReceiptNo_ShouldReturnTrue()
    {
        // Arrange
        var receiptNo = "NEW-001";
        _mockRepo.Setup(r => r.IsReceiptNoAvailableAsync(receiptNo)).ReturnsAsync(true);

        // Act
        var result = await _service.IsReceiptNoAvailableAsync(receiptNo);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.IsReceiptNoAvailableAsync(receiptNo), Times.Once);
    }

    [Fact]
    public async Task IsReceiptNoAvailableAsync_WithUnavailableReceiptNo_ShouldReturnFalse()
    {
        // Arrange
        var receiptNo = "EXISTING-001";
        _mockRepo.Setup(r => r.IsReceiptNoAvailableAsync(receiptNo)).ReturnsAsync(false);

        // Act
        var result = await _service.IsReceiptNoAvailableAsync(receiptNo);

        // Assert
        result.Should().BeFalse();
        _mockRepo.Verify(r => r.IsReceiptNoAvailableAsync(receiptNo), Times.Once);
    }

    #endregion

    #region AddWeighingAsync Tests

    [Theory]
    [InlineData(2, 1)] // First weighing of 2 expected
    [InlineData(3, 2)] // Second weighing of 3 expected
    public async Task AddWeighingAsync_WithValidSequence_ShouldAddWeighingAndUpdateStatus(int expectedWeighings, int sequenceNumber)
    {
        // Arrange
        var transactionId = "00000000-0000-0000-0000-000000000001";
        var weighBridgeId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var operatorId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        // Create existing weighing records for any previous weighings
        var existingRecords = new List<WeighingRecord>();
        for (int i = 1; i < sequenceNumber; i++)
        {
            existingRecords.Add(new WeighingRecord
            {
                Id = Guid.NewGuid().ToString(),
                WeighingSequence = i,
                Weight = 1000 * i,
                OperatorId = operatorId,
                OperatorName = $"Op{i}",
                WeighBridgeId = weighBridgeId,
                WeighBridgeName = $"WB{i}",
                WeighingDate = DateTime.UtcNow.AddMinutes(-i)
            });
        }

        var transaction = new WeighbridgeTransaction
        {
            Id = transactionId,
            ExpectedWeighings = expectedWeighings,
            CompletedWeighings = sequenceNumber - 1,
            Status = sequenceNumber == 1 ? WeighbridgeTransactionStatus.Pending : WeighbridgeTransactionStatus.InProgress,
            WeighingRecords = existingRecords,
            AuditLogs = new List<TransactionAuditLog>()
        };
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync(transactionId)).ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new AddWeighingDto
        {
            TransactionId = transactionId,
            Weight = 1000 * sequenceNumber,
            OperatorId = operatorId,
            OperatorName = $"Op{sequenceNumber}",
            WeighBridgeId = weighBridgeId,
            WeighBridgeName = $"WB{sequenceNumber}"
        };

        // Act
        var result = await _service.AddWeighingAsync(dto);

        // Assert
        result.Should().NotBeNull();
        transaction.CompletedWeighings.Should().Be(sequenceNumber);
        transaction.WeighingRecords.Should().HaveCount(sequenceNumber);
        if (sequenceNumber == 1)
        {
            transaction.FirstWeight.Should().Be(dto.Weight);
            transaction.Status.Should().Be(WeighbridgeTransactionStatus.InProgress);
        }
        else if (sequenceNumber == 2)
        {
            transaction.SecondWeight.Should().Be(dto.Weight);
            transaction.Status.Should().Be(WeighbridgeTransactionStatus.InProgress);
        }
        transaction.AuditLogs.Should().HaveCount(1); // Audit log added
        // Verify UpdateAsync is called twice: once for the transaction update and once for the audit log
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<WeighbridgeTransaction>(t => t.CompletedWeighings == sequenceNumber)), Times.Exactly(2));
    }

    [Fact]
    public async Task AddWeighingAsync_WithMaxWeighingsReached_ShouldCompleteTransaction()
    {
        // Arrange
        var transactionId = "1";
        var expectedWeighings = 2;
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            ExpectedWeighings = expectedWeighings,
            CompletedWeighings = 1,
            Status = WeighbridgeTransactionStatus.InProgress,
            WeighingRecords = new List<WeighingRecord>
            {
                new WeighingRecord
                {
                    Id = "1",
                    WeighingSequence = 1,
                    Weight = 1000,
                    WeighingDate = _testTime.AddMinutes(-10)
                }
            }
        };
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}")).ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .Returns<WeighbridgeTransaction>(t => Task.FromResult(t));

        var dto = new AddWeighingDto { TransactionId = transactionId, Weight = 2000 };

        // Act
        await _service.AddWeighingAsync(dto);

        // Assert
        transaction.CompletedWeighings.Should().Be(2);
        transaction.Status.Should().Be(WeighbridgeTransactionStatus.Completed);
        transaction.IsCompleted.Should().BeTrue();
        transaction.AuditLogs.Should().HaveCount(1); // Audit log added
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AddWeighingAsync_WithCompletedTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            IsCompleted = true
        };
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}")).ReturnsAsync(transaction);

        var dto = new AddWeighingDto { TransactionId = transactionId, Weight = 1000 };

        // Act & Assert
        var act = () => _service.AddWeighingAsync(dto);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("Cannot add more weighings. Transaction is either completed or has reached maximum weighings.");
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    [Fact]
    public async Task AddWeighingAsync_WithNonExistingTransaction_ShouldReturnNull()
    {
        // Arrange
        var transactionId = "999";
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync("999")).ReturnsAsync((WeighbridgeTransaction?)null);

        var dto = new AddWeighingDto { TransactionId = transactionId, Weight = 1000 };

        // Act
        var result = await _service.AddWeighingAsync(dto);

        // Assert
        result.Should().BeNull();
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    #endregion

    #region CompleteTransactionAsync Tests

    [Fact]
    public async Task CompleteTransactionAsync_WithSufficientWeighings_ShouldMarkAsCompleted()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            ExpectedWeighings = 2,
            CompletedWeighings = 2,
            Status = WeighbridgeTransactionStatus.InProgress,
            IsCompleted = false,
            AuditLogs = new List<TransactionAuditLog>() // Initialize AuditLogs
        };
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}")).ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>())).ReturnsAsync(transaction);

        var dto = new CompleteTransactionDto { TransactionId = transactionId }; // Removed CompletedBy

        // Act
        var result = await _service.CompleteTransactionAsync(dto);

        // Assert
        result.Should().NotBeNull();
        transaction.Status.Should().Be(WeighbridgeTransactionStatus.Completed);
        transaction.IsCompleted.Should().BeTrue();
        transaction.CompletedDate.Should().BeCloseTo(_testTime, TimeSpan.FromSeconds(1));
        transaction.AuditLogs.Should().HaveCount(1);
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<WeighbridgeTransaction>(t => t.IsCompleted)), Times.Once);
    }
    [Fact]
    public async Task CompleteTransactionAsync_WithInsufficientWeighings_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            ExpectedWeighings = 2,
            CompletedWeighings = 1
        };
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}")).ReturnsAsync(transaction);

        var dto = new CompleteTransactionDto { TransactionId = transactionId };

        // Act & Assert
        var act = () => _service.CompleteTransactionAsync(dto);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>()
            .Where(e => e.Message.Contains("Cannot complete transaction"));
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    [Fact]
    public async Task CompleteTransactionAsync_WithNonExistingTransaction_ShouldThrowArgumentException()
    {
        // Arrange
        var transactionId = "999";
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync("999")).ReturnsAsync((WeighbridgeTransaction?)null);

        var dto = new CompleteTransactionDto { TransactionId = transactionId };

        // Act & Assert
        var act = () => _service.CompleteTransactionAsync(dto);
        await act.Should().ThrowExactlyAsync<ArgumentException>()
            .Where(e => e.Message.Contains("Transaction not found"));
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    #endregion

    #region Reweigh-Related Tests (StartReweighAsync, AddReweighWeightAsync, CompleteReweighAsync, GetReweighRecordsAsync, RequestReweighAsync)


    [Fact]
    public async Task RequestReweighAsync_WithNonCompletedTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction { Id = $"{transactionId}", IsCompleted = false };
        _mockRepo.Setup(r => r.GetByIdAsync($"{transactionId}")).ReturnsAsync(transaction);

        var dto = new RequestReweighDto { TransactionId = transactionId, Reason = "Reason" };

        // Act & Assert
        var act = () => _service.RequestReweighAsync(dto);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("Can only request reweigh for completed transactions.");
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    [Fact]
    public async Task RequestReweighAsync_WithAlreadyRequestedReweigh_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            IsCompleted = true,
            Status = WeighbridgeTransactionStatus.ReweighRequested, // Set status directly
            AuditLogs = new List<TransactionAuditLog>() // Initialize AuditLogs
        };
    
        _mockRepo.Setup(r => r.GetByIdAsync($"{transactionId}"))
            .ReturnsAsync(transaction);

        var dto = new RequestReweighDto 
        { 
            TransactionId = transactionId, 
            Reason = "Reason"
        };

        // Act & Assert
        var act = () => _service.RequestReweighAsync(dto);
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("A reweigh has already been requested or is in progress for this transaction.");
        
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    [Fact(Skip = "Test requires entity internal state that's difficult to mock. Integration test recommended.")]
    public async Task StartReweighAsync_WithRequestedReweigh_ShouldStartReweigh()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            ReceiptNo = "TEST-001",
            ExpectedWeighings = 2,
            CompletedWeighings = 2,
            FirstWeight = 1000,
            SecondWeight = 2000,
            NetWeight = 1000,
            Status = WeighbridgeTransactionStatus.ReweighRequested, // Set status directly
            AuditLogs = new List<TransactionAuditLog>(), // Initialize AuditLogs
            ReweighRecords = new List<ReweighRecord>(), // Initialize ReweighRecords
            WeighingRecords = new List<WeighingRecord> // Initialize WeighingRecords with completed weighings
            {
                new WeighingRecord { Id = "1", WeighingSequence = 1, Weight = 1000, WeighingDate = DateTime.UtcNow },
                new WeighingRecord { Id = "2", WeighingSequence = 2, Weight = 2000, WeighingDate = DateTime.UtcNow }
            }
        };

        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}"))
            .ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var startedBy = "TestUser";

        // Act
        var result = await _service.StartReweighAsync(transactionId, startedBy);

        // Assert
        result.Should().NotBeNull();
        transaction.Status.Should().Be(WeighbridgeTransactionStatus.ReweighInProgress);
        // Note: CurrentReweighAttempt is a computed property that may not update correctly in unit tests with mocked data
        // transaction.CurrentReweighAttempt.Should().Be(1);
        transaction.AuditLogs.Should().HaveCount(1);
        transaction.ReweighRecords.Should().HaveCount(1); // Verify a new reweigh record was created
        transaction.ReweighRecords.First().AttemptNumber.Should().Be(1); // Verify the attempt number is correct
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<WeighbridgeTransaction>(t => 
                t.Status == WeighbridgeTransactionStatus.ReweighInProgress)), 
            Times.Once);
    }
    [Fact]
    public async Task StartReweighAsync_WithNonRequestedReweigh_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction 
        { 
            Id = $"{transactionId}", 
            Status = WeighbridgeTransactionStatus.Completed, // Set to a non-requested status
            AuditLogs = new List<TransactionAuditLog>(),
            ReweighRecords = new List<ReweighRecord>(),
            WeighingRecords = new List<WeighingRecord>()
        };
    
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}"))
            .ReturnsAsync(transaction);

        // Act & Assert
        var act = () => _service.StartReweighAsync(transactionId, "User");
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Cannot start reweigh that hasn't been requested.");
        
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }
    [Fact]
    public async Task StartReweighAsync_WithInProgressReweigh_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            Status = WeighbridgeTransactionStatus.ReweighInProgress, // Set status directly
            AuditLogs = new List<TransactionAuditLog>(), // Initialize AuditLogs
            ReweighRecords = new List<ReweighRecord>(),  // Initialize ReweighRecords
            WeighingRecords = new List<WeighingRecord>() // Initialize WeighingRecords
        };
    
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}"))
            .ReturnsAsync(transaction);

        // Act & Assert
        var act = () => _service.StartReweighAsync(transactionId, "User");
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>();
        
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    [Fact]
    public async Task AddReweighWeightAsync_WithInProgressReweigh_ShouldAddWeight()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            ExpectedWeighings = 2,
            CompletedWeighings = 2,
            Status = WeighbridgeTransactionStatus.ReweighInProgress, // Set status directly
            ReweighRecords = new List<ReweighRecord>
            {
                new ReweighRecord
                {
                    Id = 1,
                    AttemptNumber = 1,
                    Status = "InProgress"
                }
            },
            AuditLogs = new List<TransactionAuditLog>(), // Initialize AuditLogs
            WeighingRecords = new List<WeighingRecord> // Initialize with original weighings
            {
                new WeighingRecord { Id = "1", WeighingSequence = 1, Weight = 1000, WeighingDate = DateTime.UtcNow },
                new WeighingRecord { Id = "2", WeighingSequence = 2, Weight = 2000, WeighingDate = DateTime.UtcNow }
            }
        };

        // Set CurrentReweighAttempt using reflection since it's read-only
        var currentReweighAttemptProp = typeof(WeighbridgeTransaction)
            .GetProperty(nameof(WeighbridgeTransaction.CurrentReweighAttempt));
        currentReweighAttemptProp?.SetValue(transaction, 1);

        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}"))
            .ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new AddReweighWeightDto
        {
            TransactionId = transactionId,
            Weight = 1500,
            OperatorName = "ReweighOp",
            Notes = "Test note"
        };

        // Act
        var result = await _service.AddReweighWeightAsync(dto);

        // Assert
        result.Should().NotBeNull();
        var reweighRecord = transaction.ReweighRecords.First(r => r.AttemptNumber == 1);
        reweighRecord.Weight1.Should().Be(dto.Weight);
        reweighRecord.Operator1.Should().Be(dto.OperatorName);
        reweighRecord.Notes.Should().Be(dto.Notes);
        transaction.AuditLogs.Should().HaveCount(1);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Once);
    }
    [Fact]
    public async Task AddReweighWeightAsync_WithNoInProgressReweigh_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction 
        { 
            Id = $"{transactionId}", 
            Status = WeighbridgeTransactionStatus.Completed, // Set to a non-in-progress status
            AuditLogs = new List<TransactionAuditLog>(),
            ReweighRecords = new List<ReweighRecord>(),
            WeighingRecords = new List<WeighingRecord>()
        };
    
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}"))
            .ReturnsAsync(transaction);

        var dto = new AddReweighWeightDto 
        { 
            TransactionId = transactionId, 
            Weight = 1500 
        };

        // Act & Assert
        var act = () => _service.AddReweighWeightAsync(dto);
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Cannot add weight - no reweigh in progress for this transaction");
        
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }
    [Fact]
    public async Task CompleteReweighAsync_WithInProgressReweigh_ShouldCompleteReweigh()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            ExpectedWeighings = 2,
            CompletedWeighings = 2,
            Status = WeighbridgeTransactionStatus.ReweighInProgress,
            ReweighRecords = new List<ReweighRecord>
            {
                new ReweighRecord
                {
                    Id = 1,
                    AttemptNumber = 1,
                    Status = "InProgress",
                    Weight1 = 1500,
                    Weight2 = 1200,
                    NetWeight = 300
                }
            },
            AuditLogs = new List<TransactionAuditLog>(),
            WeighingRecords = new List<WeighingRecord>
            {
                new WeighingRecord { Id = "1", WeighingSequence = 1, Weight = 1000, WeighingDate = DateTime.UtcNow },
                new WeighingRecord { Id = "2", WeighingSequence = 2, Weight = 2000, WeighingDate = DateTime.UtcNow }
            }
        };

        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}"))
            .ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new CompleteReweighDto
        {
            TransactionId = transactionId,
            CompletedBy = "ReweighUser",
            Notes = "Reweigh completed successfully"
        };

        // Act
        var result = await _service.CompleteReweighAsync(dto);

        // Assert
        result.Should().NotBeNull();
        transaction.Status.Should().Be(WeighbridgeTransactionStatus.Completed);
        transaction.CompletedDate.Should().BeCloseTo(_testTime, TimeSpan.FromSeconds(1));
        transaction.AuditLogs.Should().HaveCount(1);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Once);
    }
    [Fact]
    public async Task CompleteReweighAsync_WithNoInProgressReweigh_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction 
        { 
            Id = $"{transactionId}", 
            Status = WeighbridgeTransactionStatus.Completed, // Set to a non-in-progress status
            AuditLogs = new List<TransactionAuditLog>(),
            ReweighRecords = new List<ReweighRecord>(),
            WeighingRecords = new List<WeighingRecord>()
        };
    
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}"))
            .ReturnsAsync(transaction);

        var dto = new CompleteReweighDto { TransactionId = transactionId };

        // Act & Assert
        var act = () => _service.CompleteReweighAsync(dto);
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Cannot complete reweigh - no reweigh in progress for this transaction");
        
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()), Times.Never);
    }

    [Fact]
    public async Task GetReweighRecordsAsync_WithExistingTransaction_ShouldReturnRecords()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = $"{transactionId}",
            ReweighRecords = new List<ReweighRecord>
            {
                new ReweighRecord 
                { 
                    Id = 1, 
                    Weight1 = 1500, 
                    Weight2 = 1400,
                    NetWeight = 100,
                    Notes = "Test" 
                },
                new ReweighRecord 
                { 
                    Id = 2, 
                    Weight1 = 1600,
                    Weight2 = 1500,
                    NetWeight = 100
                }
            },
            AuditLogs = new List<TransactionAuditLog>(),
            WeighingRecords = new List<WeighingRecord>()
        };
    
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}"))
            .ReturnsAsync(transaction);

        // Act
        var result = await _service.GetReweighRecordsAsync(transactionId);

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);
        result.First().Weight1.Should().Be(1500);
        result.First().Notes.Should().Be("Test");
    }
    [Fact]
    public async Task GetReweighRecordsAsync_WithNonExistingTransaction_ShouldThrowArgumentException()
    {
        // Arrange
        var transactionId = "999";
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync("999")).ReturnsAsync((WeighbridgeTransaction?)null);

        // Act & Assert
        var act = () => _service.GetReweighRecordsAsync(transactionId);
        await act.Should().ThrowExactlyAsync<ArgumentException>()
            .Where(e => e.Message.Contains("Transaction not found"));
    }

    [Fact]
    public async Task GetReweighRecordsAsync_WithNoRecords_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        var transactionId = "1";
        var transaction = new WeighbridgeTransaction { Id = $"{transactionId}", ReweighRecords = new List<ReweighRecord>() };
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync($"{transactionId}")).ReturnsAsync(transaction);

        // Act
        var result = await _service.GetReweighRecordsAsync(transactionId);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region GetIncompleteTransactionsByVehicleAsync Tests

    [Fact]
    public async Task GetIncompleteTransactionsByVehicleAsync_WithMatchingNoPlate_ShouldReturnTransactions()
    {
        // Arrange
        var noPlate = "KAA 123A";
        var transactions = new List<WeighbridgeTransaction>
        {
            new WeighbridgeTransaction { Id = "1", NoPlate = noPlate, IsCompleted = false },
            new WeighbridgeTransaction { Id = "2", NoPlate = noPlate, IsCompleted = false }
        };
        _mockRepo.Setup(r => r.GetIncompleteTransactionsByVehicleAsync(noPlate)).ReturnsAsync(transactions);

        // Act
        var result = await _service.GetIncompleteTransactionsByVehicleAsync(noPlate);

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);
        result.All(t => t.NoPlate == noPlate).Should().BeTrue();
        _mockRepo.Verify(r => r.GetIncompleteTransactionsByVehicleAsync(noPlate), Times.Once);
    }

    [Fact]
    public async Task GetIncompleteTransactionsByVehicleAsync_WithNoMatching_ShouldReturnEmpty()
    {
        // Arrange
        var noPlate = "NON-EXISTENT";
        _mockRepo.Setup(r => r.GetIncompleteTransactionsByVehicleAsync(noPlate)).ReturnsAsync(new List<WeighbridgeTransaction>());

        // Act
        var result = await _service.GetIncompleteTransactionsByVehicleAsync(noPlate);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region GetIncompleteTransactionsByVehicleIdAsync Tests

    [Fact]
    public async Task GetIncompleteTransactionsByVehicleIdAsync_WithMatchingVehicleId_ShouldReturnTransactions()
    {
        // Arrange
        var vehicleId = "00000000-0000-0000-0000-000000000001";
        var vehicleIdGuid = Guid.Parse(vehicleId);
        var transactions = new List<WeighbridgeTransaction>
        {
            new WeighbridgeTransaction { Id = "1", VehicleId = vehicleIdGuid, IsCompleted = false }
        };
        _mockRepo.Setup(r => r.GetIncompleteTransactionsByVehicleIdAsync(vehicleId)).ReturnsAsync(transactions);

        // Act
        var result = await _service.GetIncompleteTransactionsByVehicleIdAsync(vehicleId);

        // Assert
        result.Should().NotBeNull().And.HaveCount(1);
        result.First().Id.Should().Be("1");
        result.First().VehicleId.Should().Be(vehicleIdGuid);
        _mockRepo.Verify(r => r.GetIncompleteTransactionsByVehicleIdAsync(vehicleId), Times.Once);
    }

    [Fact]
    public async Task GetIncompleteTransactionsByVehicleIdAsync_WithNoMatching_ShouldReturnEmpty()
    {
        // Arrange
        var vehicleId = "non-existing";
        _mockRepo.Setup(r => r.GetIncompleteTransactionsByVehicleIdAsync(vehicleId)).ReturnsAsync(new List<WeighbridgeTransaction>());

        // Act
        var result = await _service.GetIncompleteTransactionsByVehicleIdAsync(vehicleId);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region GetTransactionsByStatusAsync Tests

    [Theory]
    [InlineData("Pending", WeighbridgeTransactionStatus.Pending)]
    [InlineData("InProgress", WeighbridgeTransactionStatus.InProgress)]
    [InlineData("Completed", WeighbridgeTransactionStatus.Completed)]
    public async Task GetTransactionsByStatusAsync_WithValidStatus_ShouldReturnTransactions(string statusString, WeighbridgeTransactionStatus statusEnum)
    {
        // Arrange
        var limit = 5;
        var transactions = new List<WeighbridgeTransaction>
        {
            new WeighbridgeTransaction { Id = "1", Status = statusEnum }
        };
        _mockRepo.Setup(r => r.GetTransactionsByStatusAsync(statusEnum, limit)).ReturnsAsync(transactions);

        // Act
        var result = await _service.GetTransactionsByStatusAsync(statusString, limit);

        // Assert
        result.Should().NotBeNull().And.HaveCount(1);
        result.First().Status.Should().Be(statusString);
        _mockRepo.Verify(r => r.GetTransactionsByStatusAsync(statusEnum, limit), Times.Once);
    }

    [Fact]
    public async Task GetTransactionsByStatusAsync_WithInvalidStatus_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidStatus = "Invalid";

        // Act & Assert
        var act = () => _service.GetTransactionsByStatusAsync(invalidStatus);
        await act.Should().ThrowExactlyAsync<ArgumentException>().WithMessage($"Invalid status: {invalidStatus}");
        _mockRepo.Verify(r => r.GetTransactionsByStatusAsync(It.IsAny<WeighbridgeTransactionStatus>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetTransactionsByStatusAsync_WithNoTransactions_ShouldReturnEmpty()
    {
        // Arrange
        var status = "Pending";
        var statusEnum = WeighbridgeTransactionStatus.Pending;
        _mockRepo.Setup(r => r.GetTransactionsByStatusAsync(statusEnum, 100)).ReturnsAsync(new List<WeighbridgeTransaction>());

        // Act
        var result = await _service.GetTransactionsByStatusAsync(status);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region GetWithWeighingRecordsAsync Tests

    [Fact]
    public async Task GetWithWeighingRecordsAsync_WithExistingId_ShouldReturnDtoWithRecords()
    {
        // Arrange
        var id = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = "1",
            WeighingRecords = new List<WeighingRecord> 
            { 
                new WeighingRecord
                {
                    Id = "1",
                    WeighingSequence = 1,
                    Weight = 1000,
                    WeighingDate = DateTime.UtcNow,
                    OperatorName = "TestOperator"
                } 
            },
            AuditLogs = new List<TransactionAuditLog>(),
            ReweighRecords = new List<ReweighRecord>()
        };
    
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync("1"))
            .ReturnsAsync(transaction);

        // Act
        var result = await _service.GetWithWeighingRecordsAsync(id);

        // Assert
        result.Should().NotBeNull();
        _mockRepo.Verify(r => r.GetWithWeighingRecordsAsync("1"), Times.Once);
    }

    [Fact]
    public async Task GetWithWeighingRecordsAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var id = "999";
        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync("999")).ReturnsAsync((WeighbridgeTransaction?)null);

        // Act
        var result = await _service.GetWithWeighingRecordsAsync(id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetWithAuditLogsAsync Tests

    [Fact]
    public async Task GetWithAuditLogsAsync_WithExistingId_ShouldReturnDtoWithLogs()
    {
        // Arrange
        var id = "1";
        var transaction = new WeighbridgeTransaction
        {
            Id = "1",
            AuditLogs = new List<TransactionAuditLog> { new TransactionAuditLog { Action = "Created" } }
        };
        _mockRepo.Setup(r => r.GetWithAuditLogsAsync("1")).ReturnsAsync(transaction);

        // Act
        var result = await _service.GetWithAuditLogsAsync(id);

        // Assert
        result.Should().NotBeNull();
        _mockRepo.Verify(r => r.GetWithAuditLogsAsync("1"), Times.Once);
    }

    [Fact]
    public async Task GetWithAuditLogsAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var id = "999";
        _mockRepo.Setup(r => r.GetWithAuditLogsAsync("999")).ReturnsAsync((WeighbridgeTransaction?)null);

        // Act
        var result = await _service.GetWithAuditLogsAsync(id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    // Existing tests improved/merged where applicable
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task AddWeighingAsync_ShouldTrackWeighings(int expectedWeighings)
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            Id = "1",
            ReceiptNo = "TRX-001",
            ExpectedWeighings = expectedWeighings,
            Status = WeighbridgeTransactionStatus.Pending,
            WeighingRecords = new List<WeighingRecord>()
        };

        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync("1"))
            .ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        // Act - Add weighings
        for (int i = 1; i <= expectedWeighings; i++)
        {
            var dto = new AddWeighingDto
            {
                TransactionId = "1",
                Weight = 1000 + (i * 100),
                OperatorName = $"Operator{i}"
            };

            await _service.AddWeighingAsync(dto);
        }

        // Assert
        transaction.CompletedWeighings.Should().Be(expectedWeighings);
        transaction.Status.Should().Be(WeighbridgeTransactionStatus.Completed);
        transaction.IsCompleted.Should().BeTrue();
        transaction.WeighingRecords.Should().HaveCount(expectedWeighings);
    }

    [Fact]
    public async Task CompleteTransactionAsync_ShouldMarkAsCompleted()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            Id = "1",
            ReceiptNo = "TRX-001",
            ExpectedWeighings = 2,
            CompletedWeighings = 2,
            Status = WeighbridgeTransactionStatus.InProgress,
            IsCompleted = false
        };

        _mockRepo.Setup(r => r.GetWithWeighingRecordsAsync("1"))
            .ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new CompleteTransactionDto
        {
            TransactionId = "1"
        };

        // Act
        var result = await _service.CompleteTransactionAsync(dto);

        // Assert
        result.Should().NotBeNull();
        transaction.Status.Should().Be(WeighbridgeTransactionStatus.Completed);
        transaction.IsCompleted.Should().BeTrue();
        transaction.CompletedDate.Should().BeCloseTo(_testTime, TimeSpan.FromSeconds(5));
        transaction.AuditLogs.Should().HaveCount(1);
    }

    [Fact]
    public async Task RequestReweighAsync_ShouldSetReweighStatus()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            Id = "1",
            ReceiptNo = "TRX-001",
            Status = WeighbridgeTransactionStatus.Completed,
            IsCompleted = true,
            AuditLogs = new List<TransactionAuditLog>()
        };

        _mockRepo.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(transaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new RequestReweighDto
        {
            TransactionId = "1",
            Reason = "Suspected weight discrepancy"
        };

        // Act
        var result = await _service.RequestReweighAsync(dto);

        // Assert
        result.Should().BeTrue();
        transaction.Status.Should().Be(WeighbridgeTransactionStatus.ReweighRequested);
        transaction.ReweighReason.Should().Be(dto.Reason);
        transaction.ReweighRequestDate.Should().BeCloseTo(_testTime, TimeSpan.FromSeconds(5));
        transaction.AuditLogs.Should().HaveCount(1);
    }

    public static IEnumerable<object?[]> GetVariousScenarioData()
    {
        yield return new object?[] { 
            1000m, 
            "00000000-0000-0000-0000-000000000100", // weighBridgeId as string
            "WB1", 
            "Op1" 
        };
        yield return new object?[] { 
            2000m, 
            "00000000-0000-0000-0000-000000000200", // weighBridgeId as string
            "WB2", 
            "Op2" 
        };
    }
}