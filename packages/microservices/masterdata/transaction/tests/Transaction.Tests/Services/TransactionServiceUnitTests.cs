using AutoMapper;
using FluentAssertions;
using Moq;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;
using Transaction.Core.Mappings;
using Transaction.Core.Services;
using Xunit;

namespace Transaction.Tests.Services;

/// <summary>
/// Comprehensive unit tests for TransactionService
/// Tests all business logic, validations, and workflows
/// </summary>
public class TransactionServiceUnitTests
{
    private readonly Mock<ITransactionRepository> _mockRepo;
    private readonly Mock<ITimeService> _mockTimeService;
    private readonly Mock<IReceiptNumberService> _mockReceiptService;
    private readonly IMapper _mapper;
    private readonly TransactionService _service;
    private readonly DateTime _testTime;

    public TransactionServiceUnitTests()
    {
        _mockRepo = new Mock<ITransactionRepository>();
        _mockTimeService = new Mock<ITimeService>();
        _mockReceiptService = new Mock<IReceiptNumberService>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<TransactionProfile>());
        _mapper = config.CreateMapper();

        _testTime = new DateTime(2026, 3, 20, 10, 0, 0, DateTimeKind.Utc);
        _mockTimeService.Setup(t => t.UtcNow).Returns(_testTime);
        _mockReceiptService.Setup(r => r.GenerateReceiptNumberAsync())
            .ReturnsAsync("QSL-20260320-000001");

        _service = new TransactionService(_mockRepo.Object, _mapper, _mockTimeService.Object, _mockReceiptService.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidData_ShouldCreateTransaction()
    {
        // Arrange
        var dto = new CreateTransactionDto
        {
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            FirstWeight = "45000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.CreateWithUniqueReceiptNoAsync(It.IsAny<WeighbridgeTransaction>(), It.IsAny<Func<Task<string>>>()))
            .Returns(async (WeighbridgeTransaction t, Func<Task<string>> generateReceiptNo) =>
            {
                t.ReceiptNo = await generateReceiptNo();
                return t;
            });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.NoPlate.Should().Be("ABC-123");
        result.FirstWeight.Should().Be("45000");
        result.Status.Should().Be("Active");
        result.SecondWeightDate.Should().BeNull();
        result.TurnaroundTime.Should().BeNull();
        result.ReceiptNo.Should().Be("QSL-20260320-000001");
    }

    [Fact]
    public async Task CreateAsync_NegativeWeight_ShouldThrowArgumentException()
    {
        // Arrange
        var dto = new CreateTransactionDto
        {
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            FirstWeight = "-100",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_EmptyWeight_ShouldThrowArgumentException()
    {
        // Arrange
        var dto = new CreateTransactionDto
        {
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            FirstWeight = "",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_InvalidWeightFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var dto = new CreateTransactionDto
        {
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            FirstWeight = "not-a-number",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    #endregion

    #region AddSecondWeightAsync Tests

    [Fact]
    public async Task AddSecondWeightAsync_ValidData_ShouldCompleteTransaction()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            FirstWeightDate = _testTime.AddHours(-2),
            Status = "Active",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new AddSecondWeightDto
        {
            TicketID = ticketId,
            SecondWeight = "15000",
            OperatorName2nd = "Operator Bob"
        };

        // Act
        var result = await _service.AddSecondWeightAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.SecondWeight.Should().Be("15000");
        result.NetWeight.Should().Be("30000.00");
        result.SecondWeightDate.Should().NotBeNull();
        result.TurnaroundTime.Should().NotBeNull();
        result.TurnaroundTime!.Value.TotalHours.Should().Be(2);
    }

    [Fact]
    public async Task AddSecondWeightAsync_ToCompletedTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            SecondWeight = "15000",
            Status = "Completed",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new AddSecondWeightDto
        {
            TicketID = ticketId,
            SecondWeight = "16000"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddSecondWeightAsync(dto));
        exception.Message.Should().Contain("Cannot add second weight to a completed transaction");
    }

    [Fact]
    public async Task AddSecondWeightAsync_DuplicateSecondWeight_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            SecondWeight = "15000",
            Status = "Active",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new AddSecondWeightDto
        {
            TicketID = ticketId,
            SecondWeight = "16000"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddSecondWeightAsync(dto));
        exception.Message.Should().Contain("already been recorded");
    }

    [Fact]
    public async Task AddSecondWeightAsync_NegativeWeight_ShouldThrowArgumentException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            Status = "Active",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new AddSecondWeightDto
        {
            TicketID = ticketId,
            SecondWeight = "-100"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddSecondWeightAsync(dto));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_CompletedTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            Status = "Completed",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new UpdateTransactionDto
        {
            DriverName = "New Driver"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(ticketId, dto));
        exception.Message.Should().Contain("Cannot update a completed transaction");
    }

    [Fact]
    public async Task UpdateAsync_OverwriteSecondWeight_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            SecondWeight = "15000",
            Status = "Active",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new UpdateTransactionDto
        {
            SecondWeight = "16000"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(ticketId, dto));
        exception.Message.Should().Contain("already been recorded");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_CompletedTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            Status = "Completed",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(ticketId));
        exception.Message.Should().Contain("Cannot delete a completed transaction");
    }

    [Fact]
    public async Task DeleteAsync_ReweighRequestedTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            Status = "ReweighRequested",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(ticketId));
        exception.Message.Should().Contain("Cannot delete");
    }

    #endregion

    #region Reweigh Workflow Tests

    [Fact]
    public async Task RequestReweighAsync_CompletedTransaction_ShouldChangeStatusAndCreateRecord()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            SecondWeight = "15000",
            NetWeight = "30000.00",
            Status = "Completed",
            FirstWeightDate = _testTime.AddHours(-3),
            SecondWeightDate = _testTime.AddHours(-1),
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);
        _mockRepo.Setup(r => r.GetReweighRecordsAsync(ticketId)).ReturnsAsync(new List<ReweighRecord>());
        _mockRepo.Setup(r => r.CreateReweighRecordAsync(It.IsAny<ReweighRecord>()))
            .ReturnsAsync((ReweighRecord r) => r);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new RequestReweighDto
        {
            TicketID = ticketId,
            Reason = "Weight discrepancy"
        };

        // Act
        var result = await _service.RequestReweighAsync(dto);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.CreateReweighRecordAsync(It.Is<ReweighRecord>(rec =>
            rec.Status == "Pending" &&
            rec.Reason == "Weight discrepancy" &&
            rec.WeighbridgeTransactionId == ticketId
        )), Times.Once);
    }

    [Fact]
    public async Task RequestReweighAsync_DuplicateRequest_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            Status = "ReweighRequested",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new RequestReweighDto
        {
            TicketID = ticketId,
            Reason = "Another reason"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RequestReweighAsync(dto));
        exception.Message.Should().Contain("already been requested");
    }

    [Fact]
    public async Task RequestReweighAsync_EmptyReason_ShouldThrowArgumentException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            Status = "Completed",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new RequestReweighDto
        {
            TicketID = ticketId,
            Reason = ""
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RequestReweighAsync(dto));
    }

    [Fact]
    public async Task ApproveReweighAsync_ReweighRequested_ShouldResetToActive()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            SecondWeight = "15000",
            NetWeight = "30000.00",
            TurnaroundTime = TimeSpan.FromHours(2),
            Status = "ReweighRequested",
            FirstWeightDate = _testTime.AddHours(-3),
            SecondWeightDate = _testTime.AddHours(-1),
            OperatorName = "Operator 1",
            OperatorName2nd = "Operator 2",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport",
            ReweighPermission = "Weight discrepancy"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);
        _mockRepo.Setup(r => r.GetReweighRecordsAsync(ticketId)).ReturnsAsync(new List<ReweighRecord>());
        _mockRepo.Setup(r => r.CreateReweighRecordAsync(It.IsAny<ReweighRecord>()))
            .ReturnsAsync((ReweighRecord r) => r);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new ApproveReweighDto
        {
            TicketID = ticketId,
            ApprovedBy = "Supervisor Jane",
            Notes = "Approved for re-weighing"
        };

        // Act
        var result = await _service.ApproveReweighAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Active");
        result.SecondWeight.Should().BeNull();
        result.NetWeight.Should().BeNull();
        result.TurnaroundTime.Should().BeNull();
        result.SecondWeightDate.Should().BeNull();

        _mockRepo.Verify(r => r.CreateReweighRecordAsync(It.Is<ReweighRecord>(rec =>
            rec.Status == "Approved" &&
            rec.PerformedBy == "Supervisor Jane"
        )), Times.Once);
    }

    [Fact]
    public async Task ApproveReweighAsync_NonReweighRequested_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            Status = "Completed",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new ApproveReweighDto
        {
            TicketID = ticketId,
            ApprovedBy = "Supervisor"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ApproveReweighAsync(dto));
        exception.Message.Should().Contain("ReweighRequested");
    }

    [Fact]
    public async Task RejectReweighAsync_ReweighRequested_ShouldRestoreToCompleted()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            SecondWeight = "15000",
            NetWeight = "30000.00",
            Status = "ReweighRequested",
            FirstWeightDate = _testTime.AddHours(-3),
            SecondWeightDate = _testTime.AddHours(-1),
            OperatorName = "Operator 1",
            OperatorName2nd = "Operator 2",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport",
            ReweighPermission = "Driver complaint"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);
        _mockRepo.Setup(r => r.GetReweighRecordsAsync(ticketId)).ReturnsAsync(new List<ReweighRecord>());
        _mockRepo.Setup(r => r.CreateReweighRecordAsync(It.IsAny<ReweighRecord>()))
            .ReturnsAsync((ReweighRecord r) => r);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new RejectReweighDto
        {
            TicketID = ticketId,
            RejectionReason = "No evidence of error",
            RejectedBy = "Supervisor John",
            Notes = "Weights verified"
        };

        // Act
        var result = await _service.RejectReweighAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.SecondWeight.Should().Be("15000");
        result.NetWeight.Should().Be("30000.00");

        _mockRepo.Verify(r => r.CreateReweighRecordAsync(It.Is<ReweighRecord>(rec =>
            rec.Status == "Rejected" &&
            rec.PerformedBy == "Supervisor John"
        )), Times.Once);
    }

    [Fact]
    public async Task RejectReweighAsync_EmptyReason_ShouldThrowArgumentException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            Status = "ReweighRequested",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new RejectReweighDto
        {
            TicketID = ticketId,
            RejectionReason = "",
            RejectedBy = "Supervisor"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RejectReweighAsync(dto));
    }

    #endregion

    #region CompleteTransactionAsync Tests

    [Fact]
    public async Task CompleteTransactionAsync_ValidData_ShouldCompleteTransaction()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            SecondWeight = "15000",
            Status = "Active",
            FirstWeightDate = _testTime.AddHours(-2),
            SecondWeightDate = _testTime.AddHours(-1),
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<WeighbridgeTransaction>()))
            .ReturnsAsync((WeighbridgeTransaction t) => t);

        var dto = new CompleteTransactionDto
        {
            TicketID = ticketId
        };

        // Act
        var result = await _service.CompleteTransactionAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.NetWeight.Should().Be("30000.00");
        result.TurnaroundTime.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteTransactionAsync_AlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            SecondWeight = "15000",
            Status = "Completed",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new CompleteTransactionDto
        {
            TicketID = ticketId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CompleteTransactionAsync(dto));
        exception.Message.Should().Contain("already completed");
    }

    [Fact]
    public async Task CompleteTransactionAsync_MissingSecondWeight_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ticketId = Guid.NewGuid().ToString();
        var existingTransaction = new WeighbridgeTransaction
        {
            TicketID = ticketId,
            FirstWeight = "45000",
            SecondWeight = null,
            Status = "Active",
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            ReceiptNo = "QSL-20260320-000001",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTransaction);

        var dto = new CompleteTransactionDto
        {
            TicketID = ticketId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CompleteTransactionAsync(dto));
        exception.Message.Should().Contain("Both first and second weights are required");
    }

    #endregion
}
