using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Api.Controllers;
using Masterdata.Core.DTOs.AuditLog;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Masterdata.Tests.Controllers;

public class AuditLogsControllerTests
{
    private readonly Mock<IAuditLogService> _mockAuditLogService;
    private readonly Mock<ILogger<AuditLogsController>> _mockLogger;
    private readonly AuditLogsController _controller;

    public AuditLogsControllerTests()
    {
        _mockAuditLogService = new Mock<IAuditLogService>();
        _mockLogger = new Mock<ILogger<AuditLogsController>>();
        _controller = new AuditLogsController(_mockAuditLogService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAuditLogs_WithValidFilter_ReturnsOkResult()
    {
        // Arrange
        var filter = new AuditLogFilterDto
        {
            PageNumber = 1,
            PageSize = 10,
            EntityName = "TestEntity",
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        var expectedResult = new PagedResult<AuditLogDto>
        {
            Items = new List<AuditLogDto>
            {
                new() { Id = Guid.NewGuid().ToString(), EntityName = "TestEntity", Action = "Create" },
                new() { Id = Guid.NewGuid().ToString(), EntityName = "TestEntity", Action = "Update" }
            },
            TotalItems = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mockAuditLogService
            .Setup(x => x.GetAuditLogsAsync(
                filter.EntityName,
                null,
                null,
                null,
                filter.StartDate,
                filter.EndDate,
                filter.PageNumber,
                filter.PageSize))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAuditLogs(filter);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var pagedResult = okResult.Value.Should().BeAssignableTo<PagedResult<AuditLogDto>>().Subject;
        pagedResult.Items.Should().HaveCount(2);
        pagedResult.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task GetAuditLogs_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var filter = new AuditLogFilterDto { PageNumber = 1, PageSize = 10 };
        
        _mockAuditLogService
            .Setup(x => x.GetAuditLogsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetAuditLogs(filter);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task GetAuditLogsForEntity_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var entityName = "Product";
        var entityId = "123";
        var pageNumber = 1;
        var pageSize = 10;

        var expectedResult = new PagedResult<AuditLogDto>
        {
            Items = new List<AuditLogDto>
            {
                new() { Id = Guid.NewGuid().ToString(), EntityName = entityName, EntityId = entityId, Action = "Update" }
            },
            TotalItems = 1,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _mockAuditLogService
            .Setup(x => x.GetAuditLogsAsync(
                entityName,
                entityId,
                null,
                null,
                null,
                null,
                pageNumber,
                pageSize))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAuditLogsForEntity(entityName, entityId, pageNumber, pageSize);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var pagedResult = okResult.Value.Should().BeAssignableTo<PagedResult<AuditLogDto>>().Subject;
        var items = pagedResult.Items.ToList();
        items.Should().ContainSingle();
        items[0].EntityName.Should().Be(entityName);
        items[0].EntityId.Should().Be(entityId);
    }

    [Fact]
    public async Task GetAuditLog_WithValidId_ReturnsAuditLog()
    {
        // Arrange
        var auditLogId = Guid.NewGuid().ToString();
        var expectedAuditLog = new AuditLogDto 
        { 
            Id = auditLogId, 
            EntityName = "Product", 
            Action = "Create", 
            CreatedAt = DateTime.UtcNow 
        };

        _mockAuditLogService
            .Setup(x => x.GetAuditLogByIdAsync(auditLogId))
            .ReturnsAsync(expectedAuditLog);

        // Act
        var result = await _controller.GetAuditLog(auditLogId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var auditLog = okResult.Value.Should().BeAssignableTo<AuditLogDto>().Subject;
        auditLog.Id.Should().Be(auditLogId);
    }

    [Fact]
    public async Task GetAuditLog_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        
        _mockAuditLogService
            .Setup(x => x.GetAuditLogByIdAsync(nonExistentId))
            .ReturnsAsync((AuditLogDto)null);

        // Act
        var result = await _controller.GetAuditLog(nonExistentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAuditLog_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var auditLogId = Guid.NewGuid().ToString();
        
        _mockAuditLogService
            .Setup(x => x.GetAuditLogByIdAsync(auditLogId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetAuditLog(auditLogId);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]      // PageNumber less than 1 should default to 1, keep pageSize
    [InlineData(-1, 10, 1, 10)]     // Negative PageNumber should default to 1, keep pageSize
    [InlineData(1, 0, 1, 1)]        // PageSize less than 1 should default to 1
    [InlineData(1, 101, 1, 100)]    // PageSize greater than 100 should be capped at 100
    [InlineData(5, 50, 5, 50)]      // Valid values should remain unchanged
    public async Task GetAuditLogs_WithInvalidPagination_ParametersAreAdjusted(
        int pageNumber, 
        int pageSize, 
        int expectedPageNumber,
        int expectedPageSize)
    {
        // Arrange
        var filter = new AuditLogFilterDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var expectedResult = new PagedResult<AuditLogDto>
        {
            Items = new List<AuditLogDto>(),
            TotalItems = 0,
            PageNumber = expectedPageNumber,
            PageSize = expectedPageSize
        };

        _mockAuditLogService
            .Setup(x => x.GetAuditLogsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAuditLogs(filter);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        
        // Verify the service was called with the expected parameters
        _mockAuditLogService.Verify(
            x => x.GetAuditLogsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                expectedPageNumber,
                expectedPageSize),
            Times.Once);
    }
}
