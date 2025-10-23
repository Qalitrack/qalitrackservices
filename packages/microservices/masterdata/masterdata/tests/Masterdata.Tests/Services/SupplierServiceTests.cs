using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.DTOs.Supplier;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Masterdata.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Masterdata.Tests.Services;

public class SupplierServiceTests
{
    private readonly Mock<IRepository<Supplier>> _mockRepository;
    private readonly Mock<IRepository<Driver>> _mockDriverRepository;
    private readonly Mock<IRepository<Vehicle>> _mockVehicleRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<SupplierService>> _mockLogger;
    private readonly SupplierService _service;

    public SupplierServiceTests()
    {
        _mockRepository = new Mock<IRepository<Supplier>>();
        _mockDriverRepository = new Mock<IRepository<Driver>>();
        _mockVehicleRepository = new Mock<IRepository<Vehicle>>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<SupplierService>>();

        // Setup logger to do nothing
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));

        _service = new SupplierService(
            _mockRepository.Object,
            _mockDriverRepository.Object,
            _mockVehicleRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SupplierService(null, _mockDriverRepository.Object, _mockVehicleRepository.Object,
                _mockMapper.Object, _mockLogger.Object));
        exception.ParamName.Should().Be("repository");
    }

    [Fact]
    public void Constructor_WithNullDriverRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SupplierService(_mockRepository.Object, null, _mockVehicleRepository.Object,
                _mockMapper.Object, _mockLogger.Object));
        exception.ParamName.Should().Be("driverRepository");
    }

    [Fact]
    public void Constructor_WithNullVehicleRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SupplierService(_mockRepository.Object, _mockDriverRepository.Object, null,
                _mockMapper.Object, _mockLogger.Object));
        exception.ParamName.Should().Be("vehicleRepository");
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SupplierService(_mockRepository.Object, _mockDriverRepository.Object,
                _mockVehicleRepository.Object, null, _mockLogger.Object));
        exception.ParamName.Should().Be("mapper");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SupplierService(_mockRepository.Object, _mockDriverRepository.Object,
                _mockVehicleRepository.Object, _mockMapper.Object, null));
        exception.ParamName.Should().Be("logger");
    }

    #endregion

    #region GetPagedSuppliersAsync Tests

    [Fact]
    public async Task GetPagedSuppliersAsync_WithValidParameters_ReturnsPagedResult()
    {
        // Arrange
        var suppliers = new List<Supplier>
        {
            new() { Id = "1", Name = "Supplier 1", ContactInfo = "{\"email\":\"supplier1@test.com\",\"phone\":\"1234567890\"}", IsDeleted = false },
            new() { Id = "2", Name = "Supplier 2", ContactInfo = "{\"email\":\"supplier2@test.com\",\"phone\":\"0987654321\"}", IsDeleted = false }
        };

        var pagedResult = new PagedResult<Supplier>
        {
            Items = suppliers,
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 2
        };

        var supplierDtos = new List<SupplierReadDto>
        {
            new() { Id = "1", Name = "Supplier 1", Email = "supplier1@test.com", Phone = "1234567890" },
            new() { Id = "2", Name = "Supplier 2", Email = "supplier2@test.com", Phone = "0987654321" }
        };

        _mockRepository
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Supplier, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<SupplierReadDto>>(It.IsAny<IEnumerable<Supplier>>()))
            .Returns(supplierDtos);

        // Act
        var result = await _service.GetPagedSuppliersAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPagedSuppliersAsync_WithSearchTerm_FiltersResults()
    {
        // Arrange
        var searchTerm = "Supplier 1";
        var suppliers = new List<Supplier>
        {
            new() { Id = "1", Name = "Supplier 1", ContactInfo = "{\"email\":\"supplier1@test.com\",\"phone\":\"1234567890\"}", IsDeleted = false }
        };

        var pagedResult = new PagedResult<Supplier>
        {
            Items = suppliers,
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 1
        };

        var supplierDtos = new List<SupplierReadDto>
        {
            new() { Id = "1", Name = "Supplier 1", Email = "supplier1@test.com", Phone = "1234567890" }
        };

        _mockRepository
            .Setup(x => x.GetPagedAsync(
                1,
                10,
                searchTerm,
                It.IsAny<Expression<Func<Supplier, bool>>>(),
                It.Is<string[]>(props => props.Contains(nameof(Supplier.Name)) && props.Contains(nameof(Supplier.ContactInfo))),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(pagedResult);

        _mockMapper.Setup(x => x.Map<IEnumerable<SupplierReadDto>>(suppliers)).Returns(supplierDtos);

        // Act
        var result = await _service.GetPagedSuppliersAsync(1, 10, searchTerm);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Contain("Supplier 1");
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-1, 10, 1, 10)]
    [InlineData(1, 0, 1, 1)]
    [InlineData(1, 101, 1, 100)]
    public async Task GetPagedSuppliersAsync_WithInvalidPagination_AdjustsParameters(
        int pageNumber, int pageSize, int expectedPageNumber, int expectedPageSize)
    {
        // Arrange
        var pagedResult = new PagedResult<Supplier>
        {
            Items = new List<Supplier>(),
            PageNumber = expectedPageNumber,
            PageSize = expectedPageSize,
            TotalItems = 0
        };

        _mockRepository
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Supplier, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<bool>()
            ))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetPagedSuppliersAsync(pageNumber, pageSize);

        // Assert
        result.PageNumber.Should().Be(expectedPageNumber);
        result.PageSize.Should().Be(expectedPageSize);
    }

    [Fact]
    public async Task GetPagedSuppliersAsync_WithNoResults_ReturnsEmptyPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResult<Supplier>
        {
            Items = new List<Supplier>(),
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 0
        };

        _mockRepository
            .Setup(x => x.GetPagedAsync(
                1,
                10,
                null,
                null,
                It.Is<string[]>(props => props.Contains(nameof(Supplier.Name)) && props.Contains(nameof(Supplier.ContactInfo))),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetPagedSuppliersAsync(1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    #endregion

    #region GetSupplierByIdAsync Tests
    [Fact]
    public async Task GetSupplierByIdAsync_WithValidId_ReturnsSupplier()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier
        {
            Id = supplierId,
            Name = "Test Supplier",
            ContactInfo = "{\"email\":\"test@info.com\",\"phone\":\"1234567890\"}",
            IsDeleted = false
        };

        var supplierDto = new SupplierReadDto
        {
            Id = supplierId,
            Name = "Test Supplier",
            Email = "test@info.com",
            Phone = "1234567890"
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        _mockMapper.Setup(x => x.Map<SupplierReadDto>(supplier)).Returns(supplierDto);

        // Act
        var result = await _service.GetSupplierByIdAsync(supplierId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(supplierId);
        result.Name.Should().Be("Test Supplier");
        result.Email.Should().Be("test@info.com");
        result.Phone.Should().Be("1234567890");
    }

    [Fact]
    public async Task GetSupplierByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var supplierId = "nonexistent";

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier>());

        // Act
        var result = await _service.GetSupplierByIdAsync(supplierId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSupplierByIdAsync_WithDeletedSupplier_ReturnsNull()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier
        {
            Id = supplierId,
            Name = "Test Supplier",
            IsDeleted = true
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.GetSupplierByIdAsync(supplierId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateSupplierAsync Tests

    [Fact]
    public async Task CreateSupplierAsync_WithValidDto_CreatesSupplier()
    {
        // Arrange
        var createDto = new CreateSupplierDto
        {
            Name = "Test Supplier",
            Email = "test@info.com",
            Phone = "1234567890"
        };

        var supplier = new Supplier
        {
            Id = "1",
            Name = "Test Supplier",
            ContactInfo = "{\"email\":\"test@info.com\",\"phone\":\"1234567890\"}",
            Status = "active"
        };

        var supplierDto = new SupplierReadDto
        {
            Id = "1",
            Name = "Test Supplier",
            Email = "test@info.com",
            Phone = "1234567890"
        };

        _mockMapper.Setup(x => x.Map<Supplier>(createDto)).Returns(supplier);
        _mockMapper.Setup(x => x.Map<SupplierReadDto>(supplier)).Returns(supplierDto);

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<Supplier>()))
            .ReturnsAsync(supplier);

        // Act
        var result = await _service.CreateSupplierAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("1");
        result.Name.Should().Be("Test Supplier");
        result.Email.Should().Be("test@info.com");
        result.Phone.Should().Be("1234567890");
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Supplier>()), Times.Once);
    }

    [Fact]
public async Task CreateSupplierAsync_WhenCreateFails_ThrowsInvalidOperationException()
{
    // Arrange
    var createDto = new CreateSupplierDto
    {
        Name = "Test Supplier",
        Email = "test@info.com",
        Phone = "1234567890"
    };

    var supplier = new Supplier
    {
        Name = "Test Supplier",
        ContactInfo = "{\"email\":\"test@info.com\",\"phone\":\"1234567890\"}"
    };

    _mockMapper.Setup(x => x.Map<Supplier>(createDto)).Returns(supplier);

    _mockRepository
        .Setup(x => x.CreateAsync(It.IsAny<Supplier>()))
        .ReturnsAsync((Supplier)null);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => _service.CreateSupplierAsync(createDto));

    exception.Message.Should().Contain("Failed to create");
}

[Fact]
public async Task UpdateSupplierAsync_WithValidDto_UpdatesSuccessfully()
{
    // Arrange
    var supplierId = "1";
    var updateDto = new UpdateSupplierDto
    {
        Name = "Updated Supplier",
        Email = "updated@info.com",
        Phone = "0987654321"
    };

    var existingSupplier = new Supplier
    {
        Id = supplierId,
        Name = "Old Supplier",
        ContactInfo = "{\"email\":\"old@info.com\",\"phone\":\"1234567890\"}",
        IsDeleted = false
    };

    var updatedSupplier = new Supplier
    {
        Id = supplierId,
        Name = "Updated Supplier",
        ContactInfo = "{\"email\":\"updated@info.com\",\"phone\":\"0987654321\"}",
        IsDeleted = false
    };

    _mockRepository
        .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
        .ReturnsAsync(new List<Supplier> { existingSupplier });

    _mockMapper.Setup(x => x.Map(updateDto, existingSupplier)).Returns(updatedSupplier);

    _mockRepository
        .Setup(x => x.UpdateAsync(It.IsAny<Supplier>()))
        .ReturnsAsync(updatedSupplier);

    // Act
    var result = await _service.UpdateSupplierAsync(supplierId, updateDto);

    // Assert
    result.Should().BeTrue();
    _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Supplier>()), Times.Once);
}
    [Fact]
    public async Task UpdateSupplierAsync_WithNonExistentSupplier_ReturnsFalse()
    {
        // Arrange
        var supplierId = "nonexistent";
        var updateDto = new UpdateSupplierDto { Name = "Updated" };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier>());

        // Act
        var result = await _service.UpdateSupplierAsync(supplierId, updateDto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSupplierAsync_WithDeletedSupplier_ReturnsFalse()
    {
        // Arrange
        var supplierId = "1";
        var updateDto = new UpdateSupplierDto { Name = "Updated" };

        var deletedSupplier = new Supplier
        {
            Id = supplierId,
            IsDeleted = true
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { deletedSupplier });

        // Act
        var result = await _service.UpdateSupplierAsync(supplierId, updateDto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSupplierAsync_WhenUpdateFails_ReturnsFalse()
    {
        // Arrange
        var supplierId = "1";
        var updateDto = new UpdateSupplierDto { Name = "Updated" };

        var existingSupplier = new Supplier
        {
            Id = supplierId,
            IsDeleted = false
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { existingSupplier });

        _mockMapper.Setup(x => x.Map(updateDto, existingSupplier)).Returns(existingSupplier);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Supplier>()))
            .ReturnsAsync((Supplier)null);

        // Act
        var result = await _service.UpdateSupplierAsync(supplierId, updateDto);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DeleteSupplierAsync Tests

    [Fact]
    public async Task DeleteSupplierAsync_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier
        {
            Id = supplierId,
            IsDeleted = false
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        _mockRepository
            .Setup(x => x.DeleteAsync(supplierId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteSupplierAsync(supplierId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(supplierId), Times.Once);
    }

    [Fact]
    public async Task DeleteSupplierAsync_WithNonExistentSupplier_ReturnsFalse()
    {
        // Arrange
        var supplierId = "nonexistent";

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier>());

        // Act
        var result = await _service.DeleteSupplierAsync(supplierId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteSupplierAsync_WithAlreadyDeletedSupplier_ReturnsFalse()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier
        {
            Id = supplierId,
            IsDeleted = true
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.DeleteSupplierAsync(supplierId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteSupplierAsync_WhenDeleteFails_ReturnsFalse()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier
        {
            Id = supplierId,
            IsDeleted = false
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        _mockRepository
            .Setup(x => x.DeleteAsync(supplierId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteSupplierAsync(supplierId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingSupplier_ReturnsTrue()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier { Id = supplierId, IsDeleted = false };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.ExistsAsync(supplierId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentSupplier_ReturnsFalse()
    {
        // Arrange
        var supplierId = "nonexistent";

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier>());

        // Act
        var result = await _service.ExistsAsync(supplierId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithDeletedSupplier_ReturnsFalse()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier { Id = supplierId, IsDeleted = true };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.ExistsAsync(supplierId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetSuppliersByIdsAsync Tests

    [Fact]
    public async Task GetSuppliersByIdsAsync_WithValidIds_ReturnsSuppliers()
    {
        // Arrange
        var ids = new[] { "1", "2" };
        var suppliers = new List<Supplier>
        {
            new() { Id = "1", Name = "Sup 1", IsDeleted = false },
            new() { Id = "2", Name = "Sup 2", IsDeleted = true }
        };

        var expectedDtos = new List<SupplierReadDto>
        {
            new() { Id = "1", Name = "Sup 1" }
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(ids))
            .ReturnsAsync(suppliers);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<SupplierReadDto>>(It.Is<IEnumerable<Supplier>>(s => s.Count() == 1)))
            .Returns(expectedDtos);

        // Act
        var result = await _service.GetSuppliersByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Sup 1");
    }

    [Fact]
    public async Task GetSuppliersByIdsAsync_WithNoValidIds_ReturnsEmpty()
    {
        // Arrange
        var ids = new[] { "1", "2" };
        var suppliers = new List<Supplier>
        {
            new() { Id = "1", IsDeleted = true },
            new() { Id = "2", IsDeleted = true }
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(ids))
            .ReturnsAsync(suppliers);

        // Act
        var result = await _service.GetSuppliersByIdsAsync(ids);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetDriversBySupplierIdAsync Tests

    [Fact]
    public async Task GetDriversBySupplierIdAsync_WithValidId_ReturnsDrivers()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier { Id = supplierId, IsDeleted = false };

        var drivers = new List<Driver>
        {
            new() { Id = "D1", FullName = "Driver 1", SupplierId = supplierId, IsDeleted = false },
            new() { Id = "D2", FullName = "Driver 2", SupplierId = supplierId, IsDeleted = true }
        };

        var pagedResult = new PagedResult<Driver>
        {
            Items = drivers,
            TotalItems = 2
        };

        var expectedDtos = new List<DriverReadDto>
        {
            new() { Id = "D1", FullName = "Driver 1" }
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        _mockDriverRepository
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),  // pageNumber
                It.IsAny<int>(),  // pageSize
                It.IsAny<string>(),  // searchTerm
                It.IsAny<Expression<Func<Driver, bool>>>(),  // searchPredicate
                It.IsAny<string[]>(),  // searchProperties
                It.IsAny<string>(),  // sortBy
                It.IsAny<bool>()  // sortDescending
            ))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<DriverReadDto>>(It.IsAny<IEnumerable<Driver>>()))
            .Returns<IEnumerable<Driver>>(drivers => 
                drivers.Where(d => !d.IsDeleted)
                       .Select(d => new DriverReadDto { Id = d.Id, FullName = d.FullName }));

        // Act
        var result = await _service.GetDriversBySupplierIdAsync(supplierId);

        // Assert
        result.Should().HaveCount(1);
        result.First().FullName.Should().Be("Driver 1");
    }

    [Fact]
    public async Task GetDriversBySupplierIdAsync_WithNonExistentId_ReturnsEmpty()
    {
        // Arrange
        var supplierId = "nonexistent";

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier>());

        // Act
        var result = await _service.GetDriversBySupplierIdAsync(supplierId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDriversBySupplierIdAsync_WithDeletedSupplier_ReturnsEmpty()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier { Id = supplierId, IsDeleted = true };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.GetDriversBySupplierIdAsync(supplierId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDriversBySupplierIdAsync_WithNoDrivers_ReturnsEmpty()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier { Id = supplierId, IsDeleted = false };

        var pagedResult = new PagedResult<Driver>
        {
            Items = new List<Driver>(),
            TotalItems = 0
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        _mockDriverRepository
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),  // pageNumber
                It.IsAny<int>(),  // pageSize
                It.IsAny<string>(),  // searchTerm
                It.IsAny<Expression<Func<Driver, bool>>>(),  // searchPredicate
                It.IsAny<string[]>(),  // searchProperties
                It.IsAny<string>(),  // sortBy
                It.IsAny<bool>()  // sortDescending
            ))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<DriverReadDto>>(It.IsAny<IEnumerable<Driver>>()))
            .Returns(Enumerable.Empty<DriverReadDto>());

        // Act
        var result = await _service.GetDriversBySupplierIdAsync(supplierId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetVehiclesBySupplierIdAsync Tests

    [Fact]
    public async Task GetVehiclesBySupplierIdAsync_WithValidId_ReturnsVehicles()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier { Id = supplierId, IsDeleted = false };

        var vehicles = new List<Vehicle>
        {
            new() { Id = "V1", RegistrationNumber = "REG1", SupplierId = supplierId, IsDeleted = false },
            new() { Id = "V2", RegistrationNumber = "REG2", SupplierId = supplierId, IsDeleted = true }
        };

        var pagedResult = new PagedResult<Vehicle>
        {
            Items = vehicles,
            PageNumber = 1,
            PageSize = int.MaxValue,
            TotalItems = 2
        };

        var expectedDtos = new List<VehicleReadDto>
        {
            new() { Id = "V1", RegistrationNumber = "REG1" }
        };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        _mockVehicleRepository
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),  // pageNumber
                It.IsAny<int>(),  // pageSize
                It.IsAny<string>(),  // searchTerm
                It.IsAny<Expression<Func<Vehicle, bool>>>(),  // searchPredicate
                It.IsAny<string[]>(),  // searchProperties
                It.IsAny<string>(),  // sortBy
                It.IsAny<bool>()  // sortDescending
            ))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<VehicleReadDto>>(It.IsAny<IEnumerable<Vehicle>>()))
            .Returns<IEnumerable<Vehicle>>(vehicles => 
                vehicles.Where(v => !v.IsDeleted)
                       .Select(v => new VehicleReadDto { Id = v.Id, RegistrationNumber = v.RegistrationNumber }));

        // Act
        var result = await _service.GetVehiclesBySupplierIdAsync(supplierId);

        // Assert
        result.First().RegistrationNumber.Should().Be("REG1");
    }

    [Fact]
    public async Task GetVehiclesBySupplierIdAsync_WithNonExistentId_ReturnsEmpty()
    {
        // Arrange
        var supplierId = "nonexistent";

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier>());

        // Act
        var result = await _service.GetVehiclesBySupplierIdAsync(supplierId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVehiclesBySupplierIdAsync_WithDeletedSupplier_ReturnsEmpty()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier { Id = supplierId, IsDeleted = true };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        // Act
        var result = await _service.GetVehiclesBySupplierIdAsync(supplierId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVehiclesBySupplierIdAsync_WithNoVehicles_ReturnsEmpty_Updated()
    {
        // Arrange
        var supplierId = "1";
        var supplier = new Supplier { Id = supplierId, IsDeleted = false };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(new[] { supplierId }))
            .ReturnsAsync(new List<Supplier> { supplier });

        var emptyPagedResult = new PagedResult<Vehicle>
        {
            Items = new List<Vehicle>(),
            PageNumber = 1,
            PageSize = int.MaxValue,
            TotalItems = 0
        };

        _mockVehicleRepository
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Vehicle, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _service.GetVehiclesBySupplierIdAsync(supplierId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}