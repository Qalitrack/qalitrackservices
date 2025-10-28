using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Masterdata.Core.Services;
using Moq;
using Xunit;

namespace Masterdata.Tests.Services;

public class DriverServiceTests
{
    private readonly Mock<IRepository<Driver>> _mockDriverRepository;
    private readonly Mock<IRepository<DriverVehicle>> _mockDriverVehicleRepository;
    private readonly Mock<IRepository<Supplier>> _mockSupplierRepository;
    private readonly Mock<IRepository<Transporter>> _mockTransporterRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly DriverService _service;

    public DriverServiceTests()
    {
        _mockDriverRepository = new Mock<IRepository<Driver>>();
        _mockDriverVehicleRepository = new Mock<IRepository<DriverVehicle>>();
        _mockSupplierRepository = new Mock<IRepository<Supplier>>();
        _mockTransporterRepository = new Mock<IRepository<Transporter>>();
        _mockMapper = new Mock<IMapper>();

        _service = new DriverService(
            _mockDriverRepository.Object,
            _mockDriverVehicleRepository.Object,
            _mockSupplierRepository.Object,
            _mockTransporterRepository.Object,
            _mockMapper.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDriverRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DriverService(null!, _mockDriverVehicleRepository.Object, _mockSupplierRepository.Object,
                _mockTransporterRepository.Object, _mockMapper.Object));
        exception.ParamName.Should().Be("driverRepository");
    }

    [Fact]
    public void Constructor_WithNullDriverVehicleRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DriverService(_mockDriverRepository.Object, null!, _mockSupplierRepository.Object,
                _mockTransporterRepository.Object, _mockMapper.Object));
        exception.ParamName.Should().Be("driverVehicleRepository");
    }

    [Fact]
    public void Constructor_WithNullSupplierRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DriverService(_mockDriverRepository.Object, _mockDriverVehicleRepository.Object, null!,
                _mockTransporterRepository.Object, _mockMapper.Object));
        exception.ParamName.Should().Be("supplierRepository");
    }

    [Fact]
    public void Constructor_WithNullTransporterRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DriverService(_mockDriverRepository.Object, _mockDriverVehicleRepository.Object, _mockSupplierRepository.Object,
                null!, _mockMapper.Object));
        exception.ParamName.Should().Be("transporterRepository");
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DriverService(_mockDriverRepository.Object, _mockDriverVehicleRepository.Object,
                _mockSupplierRepository.Object, _mockTransporterRepository.Object, null!));
        exception.ParamName.Should().Be("mapper");
    }

    #endregion

    #region GetPagedDriversAsync Tests

    [Fact]
    public async Task GetPagedDriversAsync_WithValidParameters_ReturnsPagedResult()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new() { Id = "1", FullName = "John Doe", LicenseNumber = "LIC001", IsDeleted = false },
            new() { Id = "2", FullName = "Jane Smith", LicenseNumber = "LIC002", IsDeleted = false }
        };

        var pagedResult = new PagedResult<Driver>
        {
            Items = drivers,
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 2
        };

        var driverDtos = new List<DriverReadDto>
        {
            new() { Id = "1", FullName = "John Doe", LicenseNumber = "LIC001" },
            new() { Id = "2", FullName = "Jane Smith", LicenseNumber = "LIC002" }
        };

        _mockDriverRepository
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Driver, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(x => x.Map<List<DriverReadDto>>(It.IsAny<List<Driver>>()))
            .Returns(driverDtos);

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<DriverVehicle>());

        // Act
        var result = await _service.GetPagedDriversAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPagedDriversAsync_WithSearchTerm_FiltersResults()
    {
        // Arrange
        var searchTerm = "John";
        var drivers = new List<Driver>
        {
            new() { Id = "1", FullName = "John Doe", LicenseNumber = "LIC001", IsDeleted = false }
        };

        var pagedResult = new PagedResult<Driver>
        {
            Items = drivers,
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 1
        };

        var driverDtos = new List<DriverReadDto>
        {
            new() { Id = "1", FullName = "John Doe", LicenseNumber = "LIC001" }
        };

        _mockDriverRepository
            .Setup(x => x.GetPagedAsync(
                1, 
                10, 
                searchTerm, 
                It.IsAny<Expression<Func<Driver, bool>>>(),
                It.IsAny<string[]>(),
                "CreatedAt",
                false))
            .ReturnsAsync(pagedResult);

        _mockMapper.Setup(x => x.Map<List<DriverReadDto>>(drivers)).Returns(driverDtos);

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new List<DriverVehicle>());

        // Act
        var result = await _service.GetPagedDriversAsync(1, 10, searchTerm);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().FullName.Should().Contain("John");
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-1, 10, 1, 10)]
    [InlineData(1, 0, 1, 1)]
    [InlineData(1, 101, 1, 100)]
    public async Task GetPagedDriversAsync_WithInvalidPagination_AdjustsParameters(
        int pageNumber, int pageSize, int expectedPageNumber, int expectedPageSize)
    {
        // Arrange
        var pagedResult = new PagedResult<Driver>
        {
            Items = new List<Driver>(),
            PageNumber = expectedPageNumber,
            PageSize = expectedPageSize,
            TotalItems = 0
        };

        _mockDriverRepository
            .Setup(x => x.GetPagedAsync(
                expectedPageNumber, 
                expectedPageSize, 
                null, 
                It.IsAny<Expression<Func<Driver, bool>>>(),
                It.IsAny<string[]>(),
                "CreatedAt",
                false))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetPagedDriversAsync(pageNumber, pageSize);

        // Assert
        result.PageNumber.Should().Be(expectedPageNumber);
        result.PageSize.Should().Be(expectedPageSize);
    }

    [Fact]
    public async Task GetPagedDriversAsync_WithVehicleAssignments_ReturnsDriversWithVehicleIds()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new() { Id = "1", FullName = "John Doe", LicenseNumber = "LIC001", IsDeleted = false }
        };

        var pagedResult = new PagedResult<Driver>
        {
            Items = drivers,
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 1
        };

        var driverDtos = new List<DriverReadDto>
        {
            new() { Id = "1", FullName = "John Doe", LicenseNumber = "LIC001" }
        };

        var vehicleAssignments = new List<DriverVehicle>
        {
            new() { DriverId = "1", VehicleId = "V1" },
            new() { DriverId = "1", VehicleId = "V2" }
        };

        _mockDriverRepository
            .Setup(x => x.GetPagedAsync(
                1, 
                10, 
                null, 
                It.IsAny<Expression<Func<Driver, bool>>>(),
                It.IsAny<string[]>(),
                "CreatedAt",
                false))
            .ReturnsAsync(pagedResult);

        _mockMapper.Setup(x => x.Map<List<DriverReadDto>>(drivers)).Returns(driverDtos);

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(new[] { "1" }))
            .ReturnsAsync(vehicleAssignments);

        // Act
        var result = await _service.GetPagedDriversAsync(1, 10);

        // Assert
        result.Items.First().AssignedVehicleIds.Should().HaveCount(2);
        result.Items.First().AssignedVehicleIds.Should().Contain(new[] { "V1", "V2" });
    }

    [Fact]
    public async Task GetPagedDriversAsync_WithNoResults_ReturnsEmptyPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResult<Driver>
        {
            Items = new List<Driver>(),
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 0
        };

        _mockDriverRepository
            .Setup(x => x.GetPagedAsync(
                1, 
                10, 
                null, 
                It.IsAny<Expression<Func<Driver, bool>>>(),
                It.IsAny<string[]>(),
                "CreatedAt",
                false))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetPagedDriversAsync(1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsDriver()
    {
        // Arrange
        var driverId = "1";
        var driver = new Driver
        {
            Id = driverId,
            FullName = "John Doe",
            LicenseNumber = "LIC001",
            IsDeleted = false
        };

        var driverDto = new DriverReadDto
        {
            Id = driverId,
            FullName = "John Doe",
            LicenseNumber = "LIC001"
        };

        var vehicleAssignments = new List<DriverVehicle>
        {
            new() { DriverId = driverId, VehicleId = "V1" }
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockMapper.Setup(x => x.Map<DriverReadDto>(driver)).Returns(driverDto);

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(vehicleAssignments);

        // Act
        var result = await _service.GetByIdAsync(driverId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(driverId);
        result.FullName.Should().Be("John Doe");
        result.AssignedVehicleIds.Should().HaveCount(1);
        result.AssignedVehicleIds.Should().Contain("V1");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var driverId = "nonexistent";

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver>());

        // Act
        var result = await _service.GetByIdAsync(driverId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedDriver_ReturnsNull()
    {
        // Arrange
        var driverId = "1";
        var driver = new Driver
        {
            Id = driverId,
            FullName = "John Doe",
            IsDeleted = true
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        // Act
        var result = await _service.GetByIdAsync(driverId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesDriver()
    {
        // Arrange
        var createDto = new CreateDriverDto
        {
            FullName = "John Doe",
            LicenseNumber = "LIC001"
        };

        var driver = new Driver
        {
            Id = "1",
            FullName = "John Doe",
            LicenseNumber = "LIC001",
            Status = "active"
        };

        var driverDto = new DriverReadDto
        {
            Id = "1",
            FullName = "John Doe",
            LicenseNumber = "LIC001"
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { createDto.LicenseNumber }))
            .ReturnsAsync(new List<Driver>());

        _mockMapper.Setup(x => x.Map<Driver>(createDto)).Returns(driver);

        _mockDriverRepository
            .Setup(x => x.CreateAsync(It.IsAny<Driver>()))
            .ReturnsAsync(driver);

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driver.Id }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockMapper.Setup(x => x.Map<DriverReadDto>(driver)).Returns(driverDto);

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(new[] { driver.Id }))
            .ReturnsAsync(new List<DriverVehicle>());

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("1");
        result.FullName.Should().Be("John Doe");
        _mockDriverRepository.Verify(x => x.CreateAsync(It.IsAny<Driver>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateLicenseNumber_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = new CreateDriverDto
        {
            FullName = "John Doe",
            LicenseNumber = "LIC001"
        };

        var existingDriver = new Driver
        {
            Id = "1",
            LicenseNumber = "LIC001"
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { createDto.LicenseNumber }))
            .ReturnsAsync(new List<Driver> { existingDriver });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(createDto));

        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateAsync_WhenCreateFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = new CreateDriverDto
        {
            FullName = "John Doe",
            LicenseNumber = "LIC001"
        };

        var driver = new Driver
        {
            FullName = "John Doe",
            LicenseNumber = "LIC001"
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { createDto.LicenseNumber }))
            .ReturnsAsync(new List<Driver>());

        _mockMapper.Setup(x => x.Map<Driver>(createDto)).Returns(driver);

#pragma warning disable CS8620
        _mockDriverRepository
            .Setup(x => x.CreateAsync(It.IsAny<Driver>()))
            .ReturnsAsync((Driver?)null);
#pragma warning restore CS8620

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(createDto));

        exception.Message.Should().Contain("Failed to create driver");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidDto_UpdatesDriver()
    {
        // Arrange
        var driverId = "1";
        var updateDto = new UpdateDriverDto
        {
            FullName = "John Updated",
            LicenseNumber = "LIC001"
        };

        var existingDriver = new Driver
        {
            Id = driverId,
            FullName = "John Doe",
            LicenseNumber = "LIC001",
            IsDeleted = false
        };

        var updatedDriver = new Driver
        {
            Id = driverId,
            FullName = "John Updated",
            LicenseNumber = "LIC001",
            IsDeleted = false
        };

        var driverDto = new DriverReadDto
        {
            Id = driverId,
            FullName = "John Updated",
            LicenseNumber = "LIC001"
        };

        // Setup GetByIdsAsync to return the existing driver first, then the updated driver
        _mockDriverRepository
            .SetupSequence(x => x.GetByIdsAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new List<Driver> { existingDriver })
            .ReturnsAsync(new List<Driver> { updatedDriver });
            
        // Setup for the license number check - using GetByIdsAsync with the license number
        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(It.Is<string[]>(ids => ids.Length > 0 && ids[0] == updateDto.LicenseNumber)))
            .ReturnsAsync(new List<Driver>());

        _mockMapper.Setup(x => x.Map(updateDto, existingDriver)).Returns(updatedDriver);

        _mockDriverRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Driver>()))
            .ReturnsAsync(updatedDriver);

        _mockMapper.Setup(x => x.Map<DriverReadDto>(updatedDriver)).Returns(driverDto);

        // Setup for vehicle assignments
        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new List<DriverVehicle>());

        // Act
        var result = await _service.UpdateAsync(driverId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("John Updated");
        _mockDriverRepository.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNullOrEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var updateDto = new UpdateDriverDto { FullName = "John Doe" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync("", updateDto));

        exception.ParamName.Should().Be("id");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentDriver_ReturnsNull()
    {
        // Arrange
        var driverId = "nonexistent";
        var updateDto = new UpdateDriverDto { FullName = "John Doe" };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver>());

        // Act
        var result = await _service.UpdateAsync(driverId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithDeletedDriver_ReturnsNull()
    {
        // Arrange
        var driverId = "1";
        var updateDto = new UpdateDriverDto { FullName = "John Doe" };

        var deletedDriver = new Driver
        {
            Id = driverId,
            IsDeleted = true
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { deletedDriver });

        // Act
        var result = await _service.UpdateAsync(driverId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateLicenseNumber_ThrowsInvalidOperationException()
    {
        // Arrange
        var driverId = "1";
        var updateDto = new UpdateDriverDto
        {
            LicenseNumber = "LIC002"
        };

        var existingDriver = new Driver
        {
            Id = driverId,
            LicenseNumber = "LIC001",
            IsDeleted = false
        };

        var driverWithSameLicense = new Driver
        {
            Id = "2",
            LicenseNumber = "LIC002"
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { existingDriver });

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { updateDto.LicenseNumber }))
            .ReturnsAsync(new List<Driver> { driverWithSameLicense });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(driverId, updateDto));

        exception.Message.Should().Contain("already exists");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_SoftDeletesDriver()
    {
        // Arrange
        var driverId = "1";
        var driver = new Driver
        {
            Id = driverId,
            FullName = "John Doe",
            IsDeleted = false
        };

        var vehicleAssignments = new List<DriverVehicle>
        {
            new() { Id = "DV1", DriverId = driverId, VehicleId = "V1" },
            new() { Id = "DV2", DriverId = driverId, VehicleId = "V2" }
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(vehicleAssignments);

        _mockDriverRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Driver>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _service.DeleteAsync(driverId);

        // Assert
        result.Should().BeTrue();
        _mockDriverVehicleRepository.Verify(x => x.DeleteAsync("DV1"), Times.Once);
        _mockDriverVehicleRepository.Verify(x => x.DeleteAsync("DV2"), Times.Once);
        _mockDriverRepository.Verify(x => x.UpdateAsync(It.Is<Driver>(d => d.IsDeleted)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentDriver_ReturnsFalse()
    {
        // Arrange
        var driverId = "nonexistent";

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver>());

        // Act
        var result = await _service.DeleteAsync(driverId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithAlreadyDeletedDriver_ReturnsFalse()
    {
        // Arrange
        var driverId = "1";
        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = true
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        // Act
        var result = await _service.DeleteAsync(driverId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Vehicle Assignment Tests

    [Fact]
    public async Task AssignVehicleAsync_WithValidIds_AssignsVehicle()
    {
        // Arrange
        var driverId = "1";
        var vehicleId = "V1";

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<DriverVehicle>());

        _mockDriverVehicleRepository
            .Setup(x => x.CreateAsync(It.IsAny<DriverVehicle>()))
            .ReturnsAsync(new DriverVehicle { DriverId = driverId, VehicleId = vehicleId });

        // Act
        await _service.AssignVehicleAsync(driverId, vehicleId);

        // Assert
        _mockDriverVehicleRepository.Verify(
            x => x.CreateAsync(It.Is<DriverVehicle>(
                dv => dv.DriverId == driverId && dv.VehicleId == vehicleId)),
            Times.Once);
    }

    [Fact]
    public async Task AssignVehicleAsync_WithEmptyDriverId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AssignVehicleAsync("", "V1"));
    }

    [Fact]
    public async Task AssignVehicleAsync_WithEmptyVehicleId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AssignVehicleAsync("1", ""));
    }

    [Fact]
    public async Task AssignVehicleAsync_WithExistingAssignment_IsIdempotent()
    {
        // Arrange
        var driverId = "1";
        var vehicleId = "V1";

        var existingAssignment = new List<DriverVehicle>
        {
            new() { DriverId = driverId, VehicleId = vehicleId }
        };

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(existingAssignment);

        // Act
        await _service.AssignVehicleAsync(driverId, vehicleId);

        // Assert
        _mockDriverVehicleRepository.Verify(
            x => x.CreateAsync(It.IsAny<DriverVehicle>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveVehicleAsync_WithValidIds_RemovesAssignment()
    {
        // Arrange
        var driverId = "1";
        var vehicleId = "V1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false
        };

        var assignment = new DriverVehicle
        {
            Id = "DV1",
            DriverId = driverId,
            VehicleId = vehicleId
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId, vehicleId }))
            .ReturnsAsync(new List<DriverVehicle> { assignment });

        // Act
        var result = await _service.RemoveVehicleAsync(driverId, vehicleId);

        // Assert
        result.Should().BeTrue();
        _mockDriverVehicleRepository.Verify(x => x.DeleteAsync("DV1"), Times.Once);
    }

    [Fact]
    public async Task RemoveVehicleAsync_WithNonExistentAssignment_ReturnsTrue()
    {
        // Arrange
        var driverId = "1";
        var vehicleId = "V1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockDriverVehicleRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId, vehicleId }))
            .ReturnsAsync(new List<DriverVehicle>());

        // Act
        var result = await _service.RemoveVehicleAsync(driverId, vehicleId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Supplier Assignment Tests

    [Fact]
    public async Task AssignToSupplierAsync_WithValidIds_AssignsSupplier()
    {
        // Arrange
        var driverId = "1";
        var supplierId = "S1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            SupplierId = null
        };

        var supplier = new Supplier
        {
            Id = supplierId,
            Name = "Test Supplier",
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        _mockDriverRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Driver>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _service.AssignToSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeTrue();
        _mockDriverRepository.Verify(
            x => x.UpdateAsync(It.Is<Driver>(d => d.SupplierId == supplierId)),
            Times.Once);
    }

    [Fact]
    public async Task AssignToSupplierAsync_WithNonExistentDriver_ReturnsFalse()
    {
        // Arrange
        var driverId = "nonexistent";
        var supplierId = "S1";

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver>());

        // Act
        var result = await _service.AssignToSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeFalse();
        _mockDriverRepository.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
    }

    [Fact]
    public async Task AssignToSupplierAsync_WithNonExistentSupplier_ReturnsFalse()
    {
        // Arrange
        var driverId = "1";
        var supplierId = "nonexistent";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier>());

        // Act
        var result = await _service.AssignToSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AssignToSupplierAsync_WithDeletedDriver_ReturnsFalse()
    {
        // Arrange
        var driverId = "1";
        var supplierId = "S1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = true,
            SupplierId = null
        };

        var supplier = new Supplier
        {
            Id = supplierId,
            Name = "Test Supplier",
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.AssignToSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeFalse();
        _mockDriverRepository.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
    }

    [Fact]
    public async Task AssignToSupplierAsync_WithDeletedSupplier_ReturnsFalse()
    {
        // Arrange
        var driverId = "1";
        var supplierId = "S1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            SupplierId = null
        };

        var supplier = new Supplier
        {
            Id = supplierId,
            Name = "Test Supplier",
            IsDeleted = true
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.AssignToSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeFalse();
        _mockDriverRepository.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
    }

    [Fact]
    public async Task AssignToSupplierAsync_WhenAlreadyAssignedToSameSupplier_ReturnsTrue()
    {
        // Arrange
        var driverId = "1";
        var supplierId = "S1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            SupplierId = supplierId
        };

        var supplier = new Supplier
        {
            Id = supplierId,
            Name = "Test Supplier",
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.AssignToSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeTrue();
        _mockDriverRepository.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
    }

    [Fact]
    public async Task AssignToSupplierAsync_WhenAlreadyAssignedToDifferentSupplier_ThrowsInvalidOperationException()
    {
        // Arrange
        var driverId = "1";
        var currentSupplierId = "S1";
        var newSupplierId = "S2";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            SupplierId = currentSupplierId
        };

        var currentSupplier = new Supplier
        {
            Id = currentSupplierId,
            Name = "Current Supplier",
            IsDeleted = false
        };

        var newSupplier = new Supplier
        {
            Id = newSupplierId,
            Name = "New Supplier",
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { newSupplierId }))
            .ReturnsAsync(new List<Supplier> { newSupplier });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { currentSupplierId }))
            .ReturnsAsync(new List<Supplier> { currentSupplier });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AssignToSupplierAsync(driverId, newSupplierId));

        exception.Message.Should().Contain("already assigned to supplier");
        exception.Message.Should().Contain("Current Supplier");
        exception.Message.Should().Contain("New Supplier");
    }

    [Fact]
    public async Task RemoveFromSupplierAsync_WithValidIds_RemovesSupplierAssignment()
    {
        // Arrange
        var driverId = "1";
        var supplierId = "S1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            SupplierId = supplierId
        };

        var supplier = new Supplier
        {
            Id = supplierId,
            Name = "Test Supplier",
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        _mockDriverRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Driver>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _service.RemoveFromSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeTrue();
        _mockDriverRepository.Verify(
            x => x.UpdateAsync(It.Is<Driver>(d => d.SupplierId == null)),
            Times.Once);
    }

    [Fact]
    public async Task RemoveFromSupplierAsync_WhenNotAssignedToSupplier_ReturnsTrue()
    {
        // Arrange
        var driverId = "1";
        var supplierId = "S1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            SupplierId = "S2"  // Different supplier
        };

        var supplier = new Supplier
        {
            Id = supplierId,
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.RemoveFromSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeTrue();
        _mockDriverRepository.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFromSupplierAsync_WithDeletedDriver_ReturnsFalse()
    {
        // Arrange
        var driverId = "1";
        var supplierId = "S1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = true,
            SupplierId = supplierId
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        // Act
        var result = await _service.RemoveFromSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveFromSupplierAsync_WithDeletedSupplier_ReturnsFalse()
    {
        // Arrange
        var driverId = "1";
        var supplierId = "S1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            SupplierId = supplierId
        };

        var supplier = new Supplier
        {
            Id = supplierId,
            IsDeleted = true
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockSupplierRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.RemoveFromSupplierAsync(driverId, supplierId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Transporter Assignment Tests

    [Fact]
    public async Task AssignToTransporterAsync_WithValidIds_AssignsTransporter()
    {
        // Arrange
        var driverId = "1";
        var transporterId = "T1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            TransporterId = null
        };

        var transporter = new Transporter
        {
            Id = transporterId,
            Name = "Test Transporter",
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockTransporterRepository
            .Setup(x => x.GetByIdsAsync(new[] { transporterId }))
            .ReturnsAsync(new List<Transporter> { transporter });

        _mockDriverRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Driver>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _service.AssignToTransporterAsync(driverId, transporterId);

        // Assert
        result.Should().BeTrue();
        _mockDriverRepository.Verify(
            x => x.UpdateAsync(It.Is<Driver>(d => d.TransporterId == transporterId)),
            Times.Once);
    }

    [Fact]
    public async Task AssignToTransporterAsync_WithNonExistentDriver_ReturnsFalse()
    {
        // Arrange
        var driverId = "nonexistent";
        var transporterId = "T1";

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver>());

        // Act
        var result = await _service.AssignToTransporterAsync(driverId, transporterId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AssignToTransporterAsync_WithNonExistentTransporter_ReturnsFalse()
    {
        // Arrange
        var driverId = "1";
        var transporterId = "nonexistent";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockTransporterRepository
            .Setup(x => x.GetByIdsAsync(new[] { transporterId }))
            .ReturnsAsync(new List<Transporter>());

        // Act
        var result = await _service.AssignToTransporterAsync(driverId, transporterId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AssignToTransporterAsync_WhenAlreadyAssignedToSameTransporter_ReturnsTrue()
    {
        // Arrange
        var driverId = "1";
        var transporterId = "T1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            TransporterId = transporterId
        };

        var transporter = new Transporter
        {
            Id = transporterId,
            Name = "Test Transporter",
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockTransporterRepository
            .Setup(x => x.GetByIdsAsync(new[] { transporterId }))
            .ReturnsAsync(new List<Transporter> { transporter });

        // Act
        var result = await _service.AssignToTransporterAsync(driverId, transporterId);

        // Assert
        result.Should().BeTrue();
        _mockDriverRepository.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
    }

    [Fact]
    public async Task AssignToTransporterAsync_WhenAlreadyAssignedToDifferentTransporter_ThrowsInvalidOperationException()
    {
        // Arrange
        var driverId = "1";
        var currentTransporterId = "T1";
        var newTransporterId = "T2";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            TransporterId = currentTransporterId
        };

        var currentTransporter = new Transporter
        {
            Id = currentTransporterId,
            Name = "Current Transporter",
            IsDeleted = false
        };

        var newTransporter = new Transporter
        {
            Id = newTransporterId,
            Name = "New Transporter",
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockTransporterRepository
            .Setup(x => x.GetByIdsAsync(new[] { newTransporterId }))
            .ReturnsAsync(new List<Transporter> { newTransporter });

        _mockTransporterRepository
            .Setup(x => x.GetByIdsAsync(new[] { currentTransporterId }))
            .ReturnsAsync(new List<Transporter> { currentTransporter });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AssignToTransporterAsync(driverId, newTransporterId));

        exception.Message.Should().Contain("already assigned to transporter");
        exception.Message.Should().Contain("Current Transporter");
        exception.Message.Should().Contain("New Transporter");
    }

    [Fact]
    public async Task RemoveFromTransporterAsync_WithValidIds_RemovesTransporterAssignment()
    {
        // Arrange
        var driverId = "1";
        var transporterId = "T1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            TransporterId = transporterId
        };

        var transporter = new Transporter
        {
            Id = transporterId,
            Name = "Test Transporter",
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockTransporterRepository
            .Setup(x => x.GetByIdsAsync(new[] { transporterId }))
            .ReturnsAsync(new List<Transporter> { transporter });

        _mockDriverRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Driver>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _service.RemoveFromTransporterAsync(driverId, transporterId);

        // Assert
        result.Should().BeTrue();
        _mockDriverRepository.Verify(
            x => x.UpdateAsync(It.Is<Driver>(d => d.TransporterId == null)),
            Times.Once);
    }

    [Fact]
    public async Task RemoveFromTransporterAsync_WhenNotAssignedToTransporter_ReturnsTrue()
    {
        // Arrange
        var driverId = "1";
        var transporterId = "T1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            TransporterId = "T2"  // Different transporter
        };

        var transporter = new Transporter
        {
            Id = transporterId,
            IsDeleted = false
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockTransporterRepository
            .Setup(x => x.GetByIdsAsync(new[] { transporterId }))
            .ReturnsAsync(new List<Transporter> { transporter });

        // Act
        var result = await _service.RemoveFromTransporterAsync(driverId, transporterId);

        // Assert
        result.Should().BeTrue();
        _mockDriverRepository.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFromTransporterAsync_WithDeletedDriver_ReturnsFalse()
    {
        // Arrange
        var driverId = "1";
        var transporterId = "T1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = true,
            TransporterId = transporterId
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        // Act
        var result = await _service.RemoveFromTransporterAsync(driverId, transporterId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveFromTransporterAsync_WithDeletedTransporter_ReturnsFalse()
    {
        // Arrange
        var driverId = "1";
        var transporterId = "T1";

        var driver = new Driver
        {
            Id = driverId,
            IsDeleted = false,
            TransporterId = transporterId
        };

        var transporter = new Transporter
        {
            Id = transporterId,
            IsDeleted = true
        };

        _mockDriverRepository
            .Setup(x => x.GetByIdsAsync(new[] { driverId }))
            .ReturnsAsync(new List<Driver> { driver });

        _mockTransporterRepository
            .Setup(x => x.GetByIdsAsync(new[] { transporterId }))
            .ReturnsAsync(new List<Transporter> { transporter });

        // Act
        var result = await _service.RemoveFromTransporterAsync(driverId, transporterId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsLicenseNumberAvailableAsync Tests

    [Fact]
    public async Task IsLicenseNumberAvailableAsync_WithAvailableLicense_ReturnsTrue()
    {
        // Arrange
        var licenseNumber = "LIC001";

        var pagedResult = new PagedResult<Driver>
        {
            Items = new List<Driver>(),
            TotalItems = 0
        };

        _mockDriverRepository
            .Setup(x => x.GetPagedAsync(
                1,
                1,
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Driver, bool>>>(),
                new[] { nameof(Driver.LicenseNumber) },
                "CreatedAt",
                false))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.IsLicenseNumberAvailableAsync(licenseNumber);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsLicenseNumberAvailableAsync_WithTakenLicense_ReturnsFalse()
    {
        // Arrange
        var licenseNumber = "LIC001";

        var pagedResult = new PagedResult<Driver>
        {
            Items = new List<Driver>
            {
                new() { Id = "1", LicenseNumber = licenseNumber }
            },
            TotalItems = 1
        };

        _mockDriverRepository
            .Setup(x => x.GetPagedAsync(
                1,
                1,
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Driver, bool>>>(),
                new[] { nameof(Driver.LicenseNumber) },
                "CreatedAt",
                false))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.IsLicenseNumberAvailableAsync(licenseNumber);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}