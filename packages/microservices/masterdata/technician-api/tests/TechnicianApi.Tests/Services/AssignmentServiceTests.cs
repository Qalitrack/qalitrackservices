using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechnicianApi.Core.DTOs.Assignment;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Core.Mappings;
using TechnicianApi.Core.Services;
using TechnicianApi.Infrastructure.Data;
using TechnicianApi.Infrastructure.Repositories;
using Xunit;

namespace TechnicianApi.Tests.Services;

public class AssignmentServiceTests : IDisposable
{
    private readonly TechnicianApiDbContext _context;
    private readonly AssignmentService _service;
    private readonly IMapper _mapper;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public AssignmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<TechnicianApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TechnicianApiDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AssignmentProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _mockFileStorage = new Mock<IFileStorageService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        
        // Setup default empty attachments for tests
        _mockFileStorage.Setup(x => x.GetAttachmentsForEntityAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Attachment>());
            
        var repository = new AssignmentRepository(_context);
        _service = new AssignmentService(
            repository, 
            _mockFileStorage.Object, 
            _mapper, 
            _mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAssignment_WhenExists()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(assignment.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(assignment.Id);
        result.Title.Should().Be("Test Assignment");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetByIdAsync("non-existent-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAssignment()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var dto = new CreateAssignmentDto
        {
            TechnicianIds = new List<string> { "tech1" },
            ManagerId = "manager1",
            Title = "New Assignment",
            Description = "Test Description",
            ServiceType = "Maintenance",
            Priority = "High",
            LocationName = "Test Location",
            LocationAddress = "Test Address",
            Deadline = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Assignment");
        result.Status.Should().Be("Pending");
        result.Priority.Should().Be("High");
        result.TechnicianIds.Should().Contain("tech1");

        var dbEntity = await _context.Assignments.Include(a => a.Technicians).FirstOrDefaultAsync(a => a.Id == result.Id);
        dbEntity.Should().NotBeNull();
        dbEntity!.Technicians.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAssignment()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Original Title");
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateAssignmentDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = "Low"
        };

        // Act
        var result = await _service.UpdateAsync(assignment.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        result.Priority.Should().Be("Low");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteAssignment()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(assignment.Id);

        // Assert
        result.Should().BeTrue();

        var getResult = await _service.GetByIdAsync(assignment.Id);
        getResult.Should().BeNull();
    }

    [Fact]
    public async Task AcceptAssignmentAsync_ShouldUpdateStatus_WhenPending()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        assignment.Status = AssignmentStatus.Pending;
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AcceptAssignmentAsync(assignment.Id, "tech1");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Accepted");
        result.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task AcceptAssignmentAsync_ShouldReturnNull_WhenNotPending()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        assignment.Status = AssignmentStatus.Completed;
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AcceptAssignmentAsync(assignment.Id, "tech1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AcceptAssignmentAsync_ShouldReturnNull_WhenWrongTechnician()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        assignment.Status = AssignmentStatus.Pending;
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AcceptAssignmentAsync(assignment.Id, "tech2");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeclineAssignmentAsync_ShouldUpdateStatus_WhenPending()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        assignment.Status = AssignmentStatus.Pending;
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeclineAssignmentAsync(assignment.Id, "tech1");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Declined");
    }

    [Fact]
    public async Task StartAssignmentAsync_ShouldUpdateStatus_WhenAccepted()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        assignment.Status = AssignmentStatus.Accepted;
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.StartAssignmentAsync(assignment.Id, "tech1");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task CompleteAssignmentAsync_ShouldUpdateStatus_WhenInProgress()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        assignment.Status = AssignmentStatus.InProgress;
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CompleteAssignmentAsync(assignment.Id, "tech1");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByTechnicianIdAsync_ShouldReturnOnlyTechnicianAssignments()
    {
        // Arrange
        var tech1 = new Technician { Id = "tech1", Name = "Technician 1", Status = TechnicianStatus.Active };
        var tech2 = new Technician { Id = "tech2", Name = "Technician 2", Status = TechnicianStatus.Active };
        await _context.Technicians.AddRangeAsync(tech1, tech2);
        await _context.SaveChangesAsync();

        var assignment1 = CreateTestAssignment(tech1, "Assignment 1");
        var assignment2 = CreateTestAssignment(tech1, "Assignment 2");
        var assignment3 = CreateTestAssignment(tech2, "Assignment 3");

        await _context.Assignments.AddRangeAsync(assignment1, assignment2, assignment3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByTechnicianIdAsync("tech1");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.TechnicianIds.Should().Contain("tech1"));
    }

    [Fact]
    public async Task GetByManagerIdAsync_ShouldReturnOnlyManagerAssignments()
    {
        // Arrange
        var tech1 = new Technician { Id = "tech1", Name = "Technician 1", Status = TechnicianStatus.Active };
        var tech2 = new Technician { Id = "tech2", Name = "Technician 2", Status = TechnicianStatus.Active };
        var tech3 = new Technician { Id = "tech3", Name = "Technician 3", Status = TechnicianStatus.Active };
        await _context.Technicians.AddRangeAsync(tech1, tech2, tech3);
        await _context.SaveChangesAsync();

        var assignment1 = CreateTestAssignment(tech1, "Assignment 1", "manager1");
        var assignment2 = CreateTestAssignment(tech2, "Assignment 2", "manager1");
        var assignment3 = CreateTestAssignment(tech3, "Assignment 3", "manager2");

        await _context.Assignments.AddRangeAsync(assignment1, assignment2, assignment3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByManagerIdAsync("manager1");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.ManagerId.Should().Be("manager1"));
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var tech1 = new Technician { Id = "tech1", Name = "Technician 1", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(tech1);
        await _context.SaveChangesAsync();

        for (int i = 1; i <= 25; i++)
        {
            var assignment = CreateTestAssignment(tech1, $"Assignment {i}");
            await _context.Assignments.AddAsync(assignment);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPagedAsync(2, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetPagedAsync_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var tech1 = new Technician { Id = "tech1", Name = "Technician 1", Status = TechnicianStatus.Active };
        var tech2 = new Technician { Id = "tech2", Name = "Technician 2", Status = TechnicianStatus.Active };
        await _context.Technicians.AddRangeAsync(tech1, tech2);
        await _context.SaveChangesAsync();

        var assignment1 = CreateTestAssignment(tech1, "Assignment 1");
        assignment1.Status = AssignmentStatus.Pending;
        var assignment2 = CreateTestAssignment(tech1, "Assignment 2");
        assignment2.Status = AssignmentStatus.Completed;
        var assignment3 = CreateTestAssignment(tech2, "Assignment 3");
        assignment3.Status = AssignmentStatus.Pending;

        await _context.Assignments.AddRangeAsync(assignment1, assignment2, assignment3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPagedAsync(1, 10, "tech1", "Pending");

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items.First().TechnicianIds.Should().Contain("tech1");
        result.Items.First().Status.Should().Be("Pending");
    }

    private static Assignment CreateTestAssignment(
        Technician technician,
        string title,
        string managerId = "manager1")
    {
        var assignment = new Assignment
        {
            Id = Guid.NewGuid().ToString(),
            ManagerId = managerId,
            Title = title,
            Description = "Test Description",
            ServiceType = "Maintenance",
            Priority = AssignmentPriority.Normal,
            Status = AssignmentStatus.Pending,
            LocationName = "Test Location",
        };
        assignment.Technicians.Add(technician);
        return assignment;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
