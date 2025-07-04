using AutoMapper;
using Moq;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;
using WeightDataService.Core.Mappings;
using WeightDataService.Core.Services;
using WeightDataService.Tests.Helpers;
using Xunit;

namespace WeightDataService.Tests.Unit.Services;

public class WeightMeasurementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IWeighbridgeStatusService> _mockWeighbridgeService;
    private readonly Mock<IWeightMeasurementRepository> _mockMeasurementRepo;
    private readonly IMapper _mapper;
    private readonly WeightMeasurementService _service;

    public WeightMeasurementServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWeighbridgeService = new Mock<IWeighbridgeStatusService>();
        _mockMeasurementRepo = new Mock<IWeightMeasurementRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<WeightMeasurementProfile>();
            cfg.AddProfile<WeighbridgeStatusProfile>();
            cfg.AddProfile<WeightCorrectionProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _mockUnitOfWork.Setup(x => x.WeightMeasurements).Returns(_mockMeasurementRepo.Object);

        _service = new WeightMeasurementService(_mockUnitOfWork.Object, _mapper, _mockWeighbridgeService.Object);
    }

    [Fact]
    public async Task GetMeasurementByIdAsync_ExistingMeasurement_ReturnsDto()
    {
        // Arrange
        var measurementId = Guid.NewGuid();
        var organizationId = "org1";
        var measurement = TestDataFactory.CreateWeightMeasurement(organizationId: organizationId);
        measurement.Id = measurementId;

        _mockMeasurementRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WeightMeasurement, bool>>>()))
            .ReturnsAsync(measurement);

        // Act
        var result = await _service.GetMeasurementByIdAsync(measurementId, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(measurementId, result.Id);
        Assert.Equal(organizationId, result.OrganizationId);
    }

    [Fact]
    public async Task GetMeasurementByIdAsync_NonExistingMeasurement_ReturnsNull()
    {
        // Arrange
        var measurementId = Guid.NewGuid();
        var organizationId = "org1";

        _mockMeasurementRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WeightMeasurement, bool>>>()))
            .ReturnsAsync((WeightMeasurement?)null);

        // Act
        var result = await _service.GetMeasurementByIdAsync(measurementId, organizationId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateMeasurementAsync_ValidData_CreatesMeasurement()
    {
        // Arrange
        var createDto = TestDataFactory.CreateWeightMeasurementDto();
        var organizationId = "org1";
        var userId = "user1";

        _mockWeighbridgeService.Setup(x => x.IsWeighbridgeAvailableAsync(createDto.WeighbridgeId, organizationId))
            .ReturnsAsync(true);

        var weighbridge = TestDataFactory.CreateWeighbridgeStatus(createDto.WeighbridgeId, organizationId);
        _mockUnitOfWork.Setup(x => x.WeighbridgeStatuses.GetByWeighbridgeIdAsync(createDto.WeighbridgeId))
            .ReturnsAsync(weighbridge);

        _mockMeasurementRepo.Setup(x => x.AddAsync(It.IsAny<WeightMeasurement>()))
            .ReturnsAsync((WeightMeasurement m) => m);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        _mockWeighbridgeService.Setup(x => x.UpdateCurrentWeightAsync(createDto.WeighbridgeId, createDto.Weight, organizationId))
            .ReturnsAsync(new WeighbridgeStatusDto());

        // Act
        var result = await _service.CreateMeasurementAsync(createDto, organizationId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.WeighbridgeId, result.WeighbridgeId);
        Assert.Equal(createDto.VehicleRegistration, result.VehicleRegistration);
        Assert.Equal(createDto.Weight, result.Weight);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(userId, result.CreatedBy);

        _mockMeasurementRepo.Verify(x => x.AddAsync(It.IsAny<WeightMeasurement>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateMeasurementAsync_WeighbridgeNotAvailable_ThrowsException()
    {
        // Arrange
        var createDto = TestDataFactory.CreateWeightMeasurementDto();
        var organizationId = "org1";
        var userId = "user1";

        _mockWeighbridgeService.Setup(x => x.IsWeighbridgeAvailableAsync(createDto.WeighbridgeId, organizationId))
            .ReturnsAsync(false);

        var weighbridge = TestDataFactory.CreateWeighbridgeStatus(createDto.WeighbridgeId, organizationId);
        _mockUnitOfWork.Setup(x => x.WeighbridgeStatuses.GetByWeighbridgeIdAsync(createDto.WeighbridgeId))
            .ReturnsAsync(weighbridge);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateMeasurementAsync(createDto, organizationId, userId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task ValidateWeightAsync_InvalidWeight_ReturnsFalse(decimal weight)
    {
        // Arrange
        var weighbridgeId = "WB001";

        // Act
        var result = await _service.ValidateWeightAsync(weight, weighbridgeId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateWeightAsync_WeightExceedsCapacity_ReturnsFalse()
    {
        // Arrange
        var weighbridgeId = "WB001";
        var weight = 60000m; // Exceeds max capacity of 50000
        var weighbridge = TestDataFactory.CreateWeighbridgeStatus(weighbridgeId);

        _mockUnitOfWork.Setup(x => x.WeighbridgeStatuses.GetByWeighbridgeIdAsync(weighbridgeId))
            .ReturnsAsync(weighbridge);

        // Act
        var result = await _service.ValidateWeightAsync(weight, weighbridgeId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateWeightAsync_ValidWeight_ReturnsTrue()
    {
        // Arrange
        var weighbridgeId = "WB001";
        var weight = 25000m; // Within capacity
        var weighbridge = TestDataFactory.CreateWeighbridgeStatus(weighbridgeId);

        _mockUnitOfWork.Setup(x => x.WeighbridgeStatuses.GetByWeighbridgeIdAsync(weighbridgeId))
            .ReturnsAsync(weighbridge);

        // Act
        var result = await _service.ValidateWeightAsync(weight, weighbridgeId);

        // Assert
        Assert.True(result);
    }
}