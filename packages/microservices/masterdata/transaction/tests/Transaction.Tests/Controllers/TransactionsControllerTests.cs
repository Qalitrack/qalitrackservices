using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Transaction.Api.Controllers;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;
using Xunit;

namespace Transaction.Tests.Controllers;

public class TransactionsControllerTests
{
    private readonly Mock<ITransactionService> _mockService;
    private readonly Mock<ILogger<TransactionsController>> _mockLogger;
    private readonly TransactionsController _controller;

    public TransactionsControllerTests()
    {
        _mockService = new Mock<ITransactionService>();
        _mockLogger = new Mock<ILogger<TransactionsController>>();
        _controller = new TransactionsController(_mockService.Object, _mockLogger.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidFilter_ShouldReturnOkWithPagedResults()
    {
        // Arrange
        var filter = new WeighbridgeTransactionFilter { PageNumber = 1, PageSize = 10 };
        var pagedResult = new PagedResult<TransactionReadDto>
        {
            Items = new List<TransactionReadDto>
            {
                new() { Id = "1", ReceiptNo = "R001" },
                new() { Id = "2", ReceiptNo = "R002" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };
        _mockService.Setup(s => s.GetAllAsync(filter)).ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<PagedResult<TransactionReadDto>>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.Items.Should().HaveCount(2);
        wrapper.Data.TotalCount.Should().Be(2);
        _mockService.Verify(s => s.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var filter = new WeighbridgeTransactionFilter();
        _mockService.Setup(s => s.GetAllAsync(filter)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturnOkWithTransaction()
    {
        // Arrange
        var transactionDto = new TransactionReadDto { Id = "1", ReceiptNo = "R001" };
        _mockService.Setup(s => s.GetByIdAsync("1")).ReturnsAsync(transactionDto);

        // Act
        var result = await _controller.GetById("1");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<TransactionReadDto>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.Id.Should().Be("1");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync("non-existing")).ReturnsAsync((TransactionReadDto?)null);

        // Act
        var result = await _controller.GetById("non-existing");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetByReceiptNo Tests

    [Fact]
    public async Task GetByReceiptNo_WithExistingReceipt_ShouldReturnOkWithTransaction()
    {
        // Arrange
        var receiptNo = "R001";
        var transactionDto = new TransactionReadDto { Id = "1", ReceiptNo = receiptNo };
        _mockService.Setup(s => s.GetByReceiptNoAsync(receiptNo)).ReturnsAsync(transactionDto);

        // Act
        var result = await _controller.GetByReceiptNo(receiptNo);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<TransactionReadDto>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.ReceiptNo.Should().Be(receiptNo);
    }

    [Fact]
    public async Task GetByReceiptNo_WithNonExistingReceipt_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByReceiptNoAsync("XXX")).ReturnsAsync((TransactionReadDto?)null);

        // Act
        var result = await _controller.GetByReceiptNo("XXX");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreatedWithTransaction()
    {
        // Arrange
        var createDto = new CreateTransactionDto
        {
            ReceiptNo = "R001",
            NoPlate = "ABC-123",
            TransporterId = 1,
            TransporterName = "Test Transport"
        };
        var createdDto = new TransactionReadDto { Id = "1", ReceiptNo = "R001" };
        _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues!["id"].Should().Be("1");
        var returnValue = createdResult.Value.Should().BeOfType<TransactionReadDto>().Subject;
        returnValue.ReceiptNo.Should().Be("R001");
    }

    [Fact]
    public async Task Create_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var createDto = new CreateTransactionDto { ReceiptNo = "R001" };
        _mockService.Setup(s => s.CreateAsync(createDto)).ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)result).StatusCode.Should().Be(500);
    }

    #endregion

    #region AddWeighing Tests

    [Fact]
    public async Task AddWeighing_WithValidData_ShouldReturnOkWithUpdatedTransaction()
    {
        // Arrange
        var dto = new AddWeighingDto
        {
            TransactionId = "1",
            Weight = 45000,
            WeighBridgeId = 1,
            WeighBridgeName = "Main WB",
            ScaleName = "Scale 1",
            OperatorId = 101,
            OperatorName = "Alice"
        };
        var updatedDto = new TransactionReadDto
        {
            Id = "1",
            FirstWeight = 45000,
            CompletedWeighings = 1
        };
        _mockService.Setup(s => s.AddWeighingAsync(dto)).ReturnsAsync(updatedDto);

        // Act
        var result = await _controller.AddWeighing(dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<TransactionReadDto>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.Id.Should().Be("1");
        wrapper.Data.FirstWeight.Should().Be(45000);
    }

    [Fact]
    public async Task AddWeighing_WithNonExistingTransaction_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new AddWeighingDto { TransactionId = "999", Weight = 1000 };
        _mockService.Setup(s => s.AddWeighingAsync(dto)).ReturnsAsync((TransactionReadDto?)null);

        // Act
        var result = await _controller.AddWeighing(dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddWeighing_WhenInvalidOperation_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new AddWeighingDto { TransactionId = "1", Weight = 1000 };
        _mockService.Setup(s => s.AddWeighingAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Transaction already completed"));

        // Act
        var result = await _controller.AddWeighing(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Complete Tests

    [Fact]
    public async Task Complete_WithValidTransaction_ShouldReturnOkWithCompletedTransaction()
    {
        // Arrange
        var dto = new CompleteTransactionDto { TransactionId = "1" };
        var completedDto = new TransactionReadDto
        {
            Id = "1",
            IsCompleted = true,
            Status = "Completed"
        };
        _mockService.Setup(s => s.CompleteTransactionAsync(dto)).ReturnsAsync(completedDto);

        // Act
        var result = await _controller.Complete(dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<TransactionReadDto>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.Id.Should().Be("1");
        wrapper.Data.IsCompleted.Should().BeTrue();
        wrapper.Data.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Complete_WhenInsufficientWeighings_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CompleteTransactionDto { TransactionId = "1" };
        _mockService.Setup(s => s.CompleteTransactionAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Insufficient weighings"));

        // Act
        var result = await _controller.Complete(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region RequestReweigh Tests

    [Fact]
    public async Task RequestReweigh_WithValidTransaction_ShouldReturnOk()
    {
        // Arrange
        var dto = new RequestReweighDto
        {
            TransactionId = "1",
            Reason = "Suspected discrepancy"
        };
        _mockService.Setup(s => s.RequestReweighAsync(dto)).ReturnsAsync(true);

        // Act
        var result = await _controller.RequestReweigh(dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task RequestReweigh_WhenIncompleteTransaction_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new RequestReweighDto { TransactionId = "1", Reason = "Test" };
        _mockService.Setup(s => s.RequestReweighAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Transaction not completed"));

        // Act
        var result = await _controller.RequestReweigh(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region StartReweigh Tests

    [Fact]
    public async Task StartReweigh_WithRequestedReweigh_ShouldReturnOk()
    {
        // Arrange
        var transactionId = "1";
        var dto = new StartReweighDto { StartedBy = "Operator1" };
        var resultDto = new TransactionReadDto
        {
            Id = "1",
            Status = "ReweighInProgress"
        };
        _mockService.Setup(s => s.StartReweighAsync(transactionId, dto.StartedBy))
            .ReturnsAsync(resultDto);

        // Act
        var result = await _controller.StartReweigh(transactionId, dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<TransactionReadDto>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.Id.Should().Be("1");
        wrapper.Data.Status.Should().Be("ReweighInProgress");
    }

    [Fact]
    public async Task StartReweigh_WhenReweighNotRequested_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new StartReweighDto { StartedBy = "Operator1" };
        _mockService.Setup(s => s.StartReweighAsync("1", dto.StartedBy))
            .ThrowsAsync(new InvalidOperationException("Reweigh not requested"));

        // Act
        var result = await _controller.StartReweigh("1", dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update Tests
    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var id = "1";
        var updateDto = new UpdateTransactionDto { NoPlate = "XYZ-789" };
        var updatedDto = new TransactionReadDto { Id = "1", NoPlate = "XYZ-789" };
        _mockService.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(updatedDto);

        // Act
        var result = await _controller.Update(id, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<TransactionReadDto>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.NoPlate.Should().Be("XYZ-789");
    }

    [Fact]
    public async Task Update_WithNonExistingTransaction_ShouldReturnNotFound()
    {
        // Arrange
        var id = "non-existing";
        var updateDto = new UpdateTransactionDto { NoPlate = "XYZ-789" };
        _mockService.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync((TransactionReadDto?)null);

        // Act
        var result = await _controller.Update(id, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WhenCompletedTransaction_ShouldReturnBadRequest()
    {
        // Arrange
        var id = "1";
        var updateDto = new UpdateTransactionDto { NoPlate = "XYZ-789" };
        _mockService.Setup(s => s.UpdateAsync(id, updateDto))
            .ThrowsAsync(new InvalidOperationException("Cannot update completed transaction"));

        // Act
        var result = await _controller.Update(id, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithExistingId_ShouldReturnOk()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync("1")).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete("1");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<object>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Message.Should().Be("Transaction deleted successfully");
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync("non-existing")).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete("non-existing");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region CheckReceiptNo Tests

    [Fact]
    public async Task CheckReceiptNo_WhenAvailable_ShouldReturnOkWithTrue()
    {
        // Arrange
        _mockService.Setup(s => s.IsReceiptNoAvailableAsync("R001")).ReturnsAsync(true);

        // Act
        var result = await _controller.CheckReceiptNo("R001");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckReceiptNo_WhenNotAvailable_ShouldReturnOkWithFalse()
    {
        // Arrange
        _mockService.Setup(s => s.IsReceiptNoAvailableAsync("R001")).ReturnsAsync(false);

        // Act
        var result = await _controller.CheckReceiptNo("R001");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    #endregion

    #region GetIncompleteByVehicle Tests

    [Fact]
    public async Task GetIncompleteByVehicle_WithExistingVehicle_ShouldReturnOkWithTransactions()
    {
        // Arrange
        var noPlate = "ABC-123";
        var transactions = new List<TransactionReadDto>
        {
            new() { Id = "1", NoPlate = noPlate, IsCompleted = false },
            new() { Id = "2", NoPlate = noPlate, IsCompleted = false }
        };
        _mockService.Setup(s => s.GetIncompleteTransactionsByVehicleAsync(noPlate))
            .ReturnsAsync(transactions);

        // Act
        var result = await _controller.GetIncompleteByVehicle(noPlate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<IEnumerable<TransactionReadDto>>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.Should().HaveCount(2);
    }

    #endregion

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatus_WithValidStatus_ShouldReturnOkWithTransactions()
    {
        // Arrange
        var status = "InProgress";
        var transactions = new List<TransactionReadDto>
        {
            new() { Id = "1", Status = status },
            new() { Id = "2", Status = status }
        };
        _mockService.Setup(s => s.GetTransactionsByStatusAsync(status, 100))
            .ReturnsAsync(transactions);

        // Act
        var result = await _controller.GetByStatus(status);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var wrapper = okResult.Value.Should().BeOfType<ApiResponseDto<IEnumerable<TransactionReadDto>>>().Subject;
        wrapper.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStatus_WithInvalidStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var status = "InvalidStatus";
        _mockService.Setup(s => s.GetTransactionsByStatusAsync(status, 100))
            .ThrowsAsync(new ArgumentException("Invalid status"));

        // Act
        var result = await _controller.GetByStatus(status);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}
