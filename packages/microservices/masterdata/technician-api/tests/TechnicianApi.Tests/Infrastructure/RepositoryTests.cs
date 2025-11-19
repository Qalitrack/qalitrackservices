using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechnicianApi.Core.Entities;
using TechnicianApi.Infrastructure.Data;
using TechnicianApi.Infrastructure.Repositories;
using Xunit;

namespace TechnicianApi.Tests.Infrastructure;

public class RepositoryTests : IDisposable
{
    private readonly TechnicianApiDbContext _context;
    private readonly Repository<Assignment> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TechnicianApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TechnicianApiDbContext(options);
        _repository = new Repository<Assignment>(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedEntities()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment1 = CreateTestAssignment(technician, "Assignment 1");
        var assignment2 = CreateTestAssignment(technician, "Assignment 2");
        var assignment3 = CreateTestAssignment(technician, "Assignment 3");
        assignment3.IsDeleted = true; // Soft deleted

        await _context.Assignments.AddRangeAsync(assignment1, assignment2, assignment3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(a => a.IsDeleted);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(assignment.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(assignment.Id);
        result.Title.Should().Be("Test Assignment");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityIsDeleted()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        assignment.IsDeleted = true;
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(assignment.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddEntity()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "New Assignment");

        // Act
        var result = await _repository.CreateAsync(assignment);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var dbEntity = await _context.Assignments.FindAsync(result.Id);
        dbEntity.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyEntity()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Original Title");
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        assignment.Title = "Updated Title";
        var result = await _repository.UpdateAsync(assignment);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteEntity()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        await _context.Assignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(assignment.Id);

        // Assert
        result.Should().BeTrue();

        var dbEntity = await _context.Assignments.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == assignment.Id);
        dbEntity.Should().NotBeNull();
        dbEntity!.IsDeleted.Should().BeTrue();

        var publicEntity = await _repository.GetByIdAsync(assignment.Id);
        publicEntity.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment1 = CreateTestAssignment(technician, "Assignment 1");
        assignment1.Priority = AssignmentPriority.High;
        var assignment2 = CreateTestAssignment(technician, "Assignment 2");
        assignment2.Priority = AssignmentPriority.High;
        var assignment3 = CreateTestAssignment(technician, "Assignment 3");
        assignment3.Priority = AssignmentPriority.Low;

        await _context.Assignments.AddRangeAsync(assignment1, assignment2, assignment3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(a => a.Priority == AssignmentPriority.High);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.Priority.Should().Be(AssignmentPriority.High));
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatch()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment1 = CreateTestAssignment(technician, "Assignment 1");
        assignment1.Priority = AssignmentPriority.High;
        var assignment2 = CreateTestAssignment(technician, "Assignment 2");
        assignment2.Priority = AssignmentPriority.Low;

        await _context.Assignments.AddRangeAsync(assignment1, assignment2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FirstOrDefaultAsync(a => a.Priority == AssignmentPriority.High);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Assignment 1");
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment1 = CreateTestAssignment(technician, "Assignment 1");
        assignment1.Priority = AssignmentPriority.High;
        var assignment2 = CreateTestAssignment(technician, "Assignment 2");
        assignment2.Priority = AssignmentPriority.High;
        var assignment3 = CreateTestAssignment(technician, "Assignment 3");
        assignment3.Priority = AssignmentPriority.Low;

        await _context.Assignments.AddRangeAsync(assignment1, assignment2, assignment3);
        await _context.SaveChangesAsync();

        // Act
        var totalCount = await _repository.CountAsync();
        var highPriorityCount = await _repository.CountAsync(a => a.Priority == AssignmentPriority.High);

        // Assert
        totalCount.Should().Be(3);
        highPriorityCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        for (int i = 1; i <= 25; i++)
        {
            var assignment = CreateTestAssignment(technician, $"Assignment {i}");
            await _context.Assignments.AddAsync(assignment);
        }
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(2, 10);

        // Assert
        items.Should().HaveCount(10);
        totalCount.Should().Be(25);
    }

    [Fact]
    public async Task GetPagedAsync_WithFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        for (int i = 1; i <= 10; i++)
        {
            var priority = i % 2 == 0 ? AssignmentPriority.High : AssignmentPriority.Low;
            var assignment = CreateTestAssignment(technician, $"Assignment {i}");
            assignment.Priority = priority;
            await _context.Assignments.AddAsync(assignment);
        }
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(
            1,
            10,
            a => a.Priority == AssignmentPriority.High);

        // Assert
        items.Should().HaveCount(5);
        totalCount.Should().Be(5);
        items.Should().AllSatisfy(a => a.Priority.Should().Be(AssignmentPriority.High));
    }

    [Fact]
    public async Task GetByIdWithIncludesAsync_ShouldIncludeRelatedEntities()
    {
        // Arrange
        var technician = new Technician { Id = "tech1", Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = CreateTestAssignment(technician, "Test Assignment");
        var checkIn = new CheckIn
        {
            Id = Guid.NewGuid().ToString(),
            AssignmentId = assignment.Id,
            Latitude = 0.0,
            Longitude = 0.0,
            CheckInTime = DateTime.UtcNow,
            Status = CheckInStatus.OnSite
        };

        await _context.Assignments.AddAsync(assignment);
        await _context.CheckIns.AddAsync(checkIn);
        await _context.SaveChangesAsync();

        // Clear tracking to test include
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdWithIncludesAsync(
            assignment.Id,
            a => a.CheckIn!);

        // Assert
        result.Should().NotBeNull();
        result!.CheckIn.Should().NotBeNull();
        result.CheckIn!.Id.Should().Be(checkIn.Id);
    }

    private Assignment CreateTestAssignment(Technician technician, string title)
    {
        var assignment = new Assignment
        {
            Id = Guid.NewGuid().ToString(),
            ManagerId = "manager1",
            Title = title,
            Description = "Test Description",
            Priority = AssignmentPriority.Normal,
            Status = AssignmentStatus.Pending,
            LocationName = "Test Location",
            ServiceType = "Maintenance"
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
