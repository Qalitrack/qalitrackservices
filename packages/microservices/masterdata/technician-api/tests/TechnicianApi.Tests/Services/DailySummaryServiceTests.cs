using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Mappings;
using TechnicianApi.Core.Services;
using TechnicianApi.Infrastructure.Data;
using TechnicianApi.Infrastructure.Repositories;
using Xunit;

namespace TechnicianApi.Tests.Services;

public class DailySummaryServiceTests : IDisposable
{
    private readonly TechnicianApiDbContext _context;
    private readonly DailySummaryService _service;
    private readonly IMapper _mapper;

    public DailySummaryServiceTests()
    {
        var options = new DbContextOptionsBuilder<TechnicianApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TechnicianApiDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DailySummaryProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        var repository = new Repository<DailySummary>(_context);
        var assignmentRepository = new AssignmentRepository(_context);
        var checkInRepository = new Repository<CheckIn>(_context);
        var requisitionRepository = new Repository<Requisition>(_context);
        _service = new DailySummaryService(repository, assignmentRepository, checkInRepository, requisitionRepository, _mapper);
    }

    [Fact]
    public async Task GenerateSummaryAsync_ShouldCalculateCorrectly_WithNoDelays()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var technicianId = "tech1";

        var technician = new Technician { Id = technicianId, Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        // 3 completed assignments, all on time
        var assignments = new List<Assignment>
        {
            new Assignment
            {
                Id = Guid.NewGuid().ToString(),
                ManagerId = "manager1",
                Title = "Assignment 1",
                Status = AssignmentStatus.Completed,
                Deadline = date.AddHours(18),
                CompletedAt = date.AddHours(16),
                CreatedAt = date,
                LocationName = "Location 1",
                Description = "Description 1",
                ServiceType = "Maintenance",
                Priority = AssignmentPriority.Normal
            },
            new Assignment
            {
                Id = Guid.NewGuid().ToString(),
                ManagerId = "manager1",
                Title = "Assignment 2",
                Status = AssignmentStatus.Completed,
                Deadline = date.AddHours(20),
                CompletedAt = date.AddHours(19),
                CreatedAt = date,
                LocationName = "Location 2",
                Description = "Description 2",
                ServiceType = "Maintenance",
                Priority = AssignmentPriority.Normal
            },
            new Assignment
            {
                Id = Guid.NewGuid().ToString(),
                ManagerId = "manager1",
                Title = "Assignment 3",
                Status = AssignmentStatus.Completed,
                Deadline = date.AddHours(22),
                CompletedAt = date.AddHours(21),
                CreatedAt = date,
                LocationName = "Location 3",
                Description = "Description 3",
                ServiceType = "Maintenance",
                Priority = AssignmentPriority.Normal
            }
        };

        foreach (var assignment in assignments)
        {
            assignment.Technicians.Add(technician);
        }

        await _context.Assignments.AddRangeAsync(assignments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GenerateSummaryAsync(technicianId, date);

        // Assert
        result.Should().NotBeNull();
        result.TechnicianId.Should().Be(technicianId);
        result.Date.Should().Be(date);
        result.TotalAssignments.Should().Be(3);
        result.CompletedTasks.Should().Be(3);
        result.PendingTasks.Should().Be(0);
        result.DelayedTasks.Should().Be(0);
        result.AlertLevel.Should().Be("None");
        result.PerformanceScore.Should().Be(100); // 3/3 * 100
    }

    [Fact]
    public async Task GenerateSummaryAsync_ShouldDetectAmberAlert_With2Delays()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var technicianId = "tech1";

        var technician = new Technician { Id = technicianId, Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        // 2 delayed assignments (deadline passed, not completed)
        var assignments = new List<Assignment>
        {
            new Assignment
            {
                Id = Guid.NewGuid().ToString(),
                ManagerId = "manager1",
                Title = "Delayed Assignment 1",
                Status = AssignmentStatus.InProgress,
                Deadline = date.AddHours(-2), // 2 hours ago
                CreatedAt = date.AddHours(-24),
                LocationName = "Location 1",
                ServiceType = "Maintenance",
                Description = "Description 1",
                Priority = AssignmentPriority.High
            },
            new Assignment
            {
                Id = Guid.NewGuid().ToString(),
                ManagerId = "manager1",
                Title = "Delayed Assignment 2",
                Status = AssignmentStatus.InProgress,
                Deadline = date.AddHours(-1), // 1 hour ago
                CreatedAt = date.AddHours(-24),
                LocationName = "Location 2",
                ServiceType = "Maintenance",
                Description = "Description 2",
                Priority = AssignmentPriority.High
            }
        };

        foreach (var assignment in assignments)
        {
            assignment.Technicians.Add(technician);
        }

        await _context.Assignments.AddRangeAsync(assignments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GenerateSummaryAsync(technicianId, date);

        // Assert
        result.Should().NotBeNull();
        result.DelayedTasks.Should().Be(2);
        result.AlertLevel.Should().Be("Amber");
    }

    [Fact]
    public async Task GenerateSummaryAsync_ShouldDetectRedAlert_With4Delays()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var technicianId = "tech1";

        var technician = new Technician { Id = technicianId, Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        // 4 delayed assignments
        var assignments = new List<Assignment>();
        for (int i = 1; i <= 4; i++)
        {
            var assignment = new Assignment
            {
                Id = Guid.NewGuid().ToString(),
                ManagerId = "manager1",
                Title = $"Delayed Assignment {i}",
                Status = AssignmentStatus.InProgress,
                Deadline = date.AddHours(-i),
                CreatedAt = date.AddHours(-24),
                LocationName = $"Location {i}",
                ServiceType = "Maintenance",
                Description = $"Description {i}",
                Priority = AssignmentPriority.High
            };
            assignment.Technicians.Add(technician);
            assignments.Add(assignment);
        }

        await _context.Assignments.AddRangeAsync(assignments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GenerateSummaryAsync(technicianId, date);

        // Assert
        result.Should().NotBeNull();
        result.DelayedTasks.Should().Be(4);
        result.AlertLevel.Should().Be("Red");
    }

    [Fact]
    public async Task GenerateSummaryAsync_ShouldIncludeRequisitionCounts()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var technicianId = "tech1";
        var assignmentId = Guid.NewGuid().ToString();

        var technician = new Technician { Id = technicianId, Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        var assignment = new Assignment
        {
            Id = assignmentId,
            ManagerId = "manager1",
            Title = "Assignment with requisitions",
            Status = AssignmentStatus.InProgress,
            CreatedAt = date,
            LocationName = "Location",
            ServiceType = "Maintenance",
            Description = "Description",
            Priority = AssignmentPriority.Normal
        };
        assignment.Technicians.Add(technician);

        var requisitions = new List<Requisition>
        {
            new Requisition
            {
                Id = Guid.NewGuid().ToString(),
                AssignmentId = assignmentId,
                TechnicianId = technicianId,
                Type = RequisitionType.MaterialRequisition,
                Status = RequisitionStatus.Pending,
                CreatedAt = date,
                Description = "Test requisition",
                Amount = 100
            },
            new Requisition
            {
                Id = Guid.NewGuid().ToString(),
                AssignmentId = assignmentId,
                TechnicianId = technicianId,
                Type = RequisitionType.CashAdvance,
                Status = RequisitionStatus.TmApproved,
                CreatedAt = date,
                Description = "Test requisition 2",
                Amount = 200
            }
        };

        await _context.Assignments.AddAsync(assignment);
        await _context.Requisitions.AddRangeAsync(requisitions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GenerateSummaryAsync(technicianId, date);

        // Assert
        result.Should().NotBeNull();
        result.TotalRequisitions.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateSummaryAsync_ShouldUpdateExistingSummary()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var technicianId = "tech1";

        var existingSummary = new DailySummary
        {
            Id = Guid.NewGuid().ToString(),
            TechnicianId = technicianId,
            Date = date,
            TotalAssignments = 5,
            CompletedTasks = 3,
            PendingTasks = 2,
            DelayedTasks = 0,
            TotalRequisitions = 1,
            TotalRequisitionAmount = 100,
            AlertLevel = PerformanceAlertLevel.None,
            PerformanceScore = 60
        };

        await _context.DailySummaries.AddAsync(existingSummary);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GenerateSummaryAsync(technicianId, date);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingSummary.Id); // Should update same entity

        // Verify only one summary exists
        var summaries = await _context.DailySummaries
            .Where(ds => ds.TechnicianId == technicianId && ds.Date == date)
            .ToListAsync();
        summaries.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByTechnicianAndDateAsync_ShouldReturnSummary()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var technicianId = "tech1";

        var summary = new DailySummary
        {
            Id = Guid.NewGuid().ToString(),
            TechnicianId = technicianId,
            Date = date,
            TotalAssignments = 5,
            CompletedTasks = 5,
            PendingTasks = 0,
            DelayedTasks = 0,
            TotalRequisitions = 2,
            TotalRequisitionAmount = 300,
            TotalWorkingMinutes = 480,
            AlertLevel = PerformanceAlertLevel.None,
            PerformanceScore = 100
        };

        await _context.DailySummaries.AddAsync(summary);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByTechnicianAndDateAsync(technicianId, date);

        // Assert
        result.Should().NotBeNull();
        result!.TechnicianId.Should().Be(technicianId);
        result.Date.Should().Be(date);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
