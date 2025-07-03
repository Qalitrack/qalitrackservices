using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WeightDataService.Api.Controllers;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Interfaces;
using WeightDataService.Tests.Helpers;
using Xunit;

namespace WeightDataService.Tests.Unit.Controllers;

public class WeightMeasurementsControllerTests
{
    private readonly Mock<IWeightMeasurementService> _mockService;
    private readonly WeightMeasurementsController _controller;

    public WeightMeasurementsControllerTests()
    {
        _mockService = new Mock<IWeightMeasurementService>();
        _controller = new WeightMeasurementsController(_mockService.Object);

        // Setup HttpContext with headers
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Organization-Id"] = "org1";
        httpContext.Request.Headers["X-User-Id"] = "user1";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetMeasurement_ExistingId_ReturnsOkResult()
    {
        // Arrange
        var measurementId = Guid.NewGuid();
        var measurementDto = new WeightMeasurementDto
        {
            Id = measurementId,
            WeighbridgeId = "WB001",
            VehicleRegistration = "ABC123",
            Weight = 1000,
            OrganizationId = "org1"
        };

        _mockService.Setup(x => x.GetMeasurementByIdAsync(measurementId, "org1"))
            .ReturnsAsync(measurementDto);

        // Act
        var result = await _controller.GetMeasurement(measurementId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<WeightMeasurementDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(measurementId, apiResponse.Data.Id);
    }

    [Fact]
    public async Task GetMeasurement_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var measurementId = Guid.NewGuid();

        _mockService.Setup(x => x.GetMeasurementByIdAsync(measurementId, "org1"))
            .ReturnsAsync((WeightMeasurementDto?)null);

        // Act
        var result = await _controller.GetMeasurement(measurementId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<WeightMeasurementDto>>(notFoundResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task CreateMeasurement_ValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = TestDataFactory.CreateWeightMeasurementDto();
        var createdDto = new WeightMeasurementDto
        {
            Id = Guid.NewGuid(),
            WeighbridgeId = createDto.WeighbridgeId,
            VehicleRegistration = createDto.VehicleRegistration,
            Weight = createDto.Weight,
            OrganizationId = "org1"
        };

        _mockService.Setup(x => x.CreateMeasurementAsync(createDto, "org1", "user1"))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _controller.CreateMeasurement(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<WeightMeasurementDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(createdDto.Id, apiResponse.Data.Id);

        _mockService.Verify(x => x.CreateMeasurementAsync(createDto, "org1", "user1"), Times.Once);
    }

    [Fact]
    public async Task CreateMeasurement_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var createDto = TestDataFactory.CreateWeightMeasurementDto();

        _mockService.Setup(x => x.CreateMeasurementAsync(createDto, "org1", "user1"))
            .ThrowsAsync(new InvalidOperationException("Weighbridge not available"));

        // Act
        var result = await _controller.CreateMeasurement(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<WeightMeasurementDto>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("Weighbridge not available", apiResponse.Message);
    }

    [Fact]
    public async Task DeleteMeasurement_ExistingId_ReturnsOkResult()
    {
        // Arrange
        var measurementId = Guid.NewGuid();

        _mockService.Setup(x => x.DeleteMeasurementAsync(measurementId, "org1", "user1"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteMeasurement(measurementId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.True(apiResponse.Data);

        _mockService.Verify(x => x.DeleteMeasurementAsync(measurementId, "org1", "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteMeasurement_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var measurementId = Guid.NewGuid();

        _mockService.Setup(x => x.DeleteMeasurementAsync(measurementId, "org1", "user1"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteMeasurement(measurementId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<bool>>(notFoundResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task GetPendingMeasurements_ReturnsOkResult()
    {
        // Arrange
        var pendingMeasurements = new List<WeightMeasurementDto>
        {
            new() { Id = Guid.NewGuid(), WeighbridgeId = "WB001", VehicleRegistration = "ABC123" },
            new() { Id = Guid.NewGuid(), WeighbridgeId = "WB002", VehicleRegistration = "DEF456" }
        };

        _mockService.Setup(x => x.GetPendingMeasurementsAsync("org1"))
            .ReturnsAsync(pendingMeasurements);

        // Act
        var result = await _controller.GetPendingMeasurements();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<List<WeightMeasurementDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(2, apiResponse.Data.Count);
    }
}