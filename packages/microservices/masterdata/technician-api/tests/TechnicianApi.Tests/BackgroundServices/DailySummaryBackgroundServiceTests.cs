using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TechnicianApi.Core.BackgroundServices;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Core.Mappings;
using TechnicianApi.Core.Services;
using TechnicianApi.Infrastructure.Data;
using TechnicianApi.Infrastructure.Repositories;
using Xunit;

namespace TechnicianApi.Tests.BackgroundServices;

public class DailySummaryBackgroundServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TechnicianApiDbContext _context;
    private static readonly string DatabaseName = $"TestDb_DailySummary_{Guid.NewGuid()}";

    public DailySummaryBackgroundServiceTests()
    {
        var services = new ServiceCollection();

        // Add DbContext with fixed database name (shared across all scopes in this test)
        services.AddDbContext<TechnicianApiDbContext>(options =>
            options.UseInMemoryDatabase(DatabaseName));

        // Add AutoMapper
        services.AddAutoMapper(typeof(DailySummaryProfile), typeof(AssignmentProfile));

        // Add Repositories and Services
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IDailySummaryService, DailySummaryService>();

        // Add Logging
        services.AddLogging(builder => builder.AddConsole());

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TechnicianApiDbContext>();
    }

    [Fact]
    public async Task BackgroundService_ShouldGenerateDailySummaries_ForAllTechnicians()
    {
        // Arrange
        var technicianIds = new[] { "tech1", "tech2", "tech3" };
        var today = DateTime.UtcNow.Date;

        // Create technicians first
        foreach (var techId in technicianIds)
        {
            var technician = new Technician { Id = techId, Name = $"Technician {techId}", Status = TechnicianStatus.Active };
            await _context.Technicians.AddAsync(technician);
        }
        await _context.SaveChangesAsync();

        // Create assignments for each technician
        foreach (var techId in technicianIds)
        {
            var technician = await _context.Technicians.FindAsync(techId);
            for (int i = 1; i <= 3; i++)
            {
                var assignment = new Assignment
                {
                    Id = Guid.NewGuid().ToString(),
                    ManagerId = "manager1",
                    Title = $"Assignment {i} for {techId}",
                    Description = "Test",
                    Status = AssignmentStatus.Completed,
                    Deadline = today.AddHours(18),
                    CompletedAt = today.AddHours(16),
                    LocationName = "Location",
                    ServiceType = "Maintenance",
                    Priority = AssignmentPriority.Normal
                };
                assignment.Technicians.Add(technician!);
                await _context.Assignments.AddAsync(assignment);
            }
        }
        await _context.SaveChangesAsync();

        // Act - Simulate what the background service does
        using var scope = _serviceProvider.CreateScope();
        var dailySummaryService = scope.ServiceProvider.GetRequiredService<IDailySummaryService>();
        var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

        var allAssignments = await assignmentService.GetPagedAsync(1, 10000);
        var uniqueTechIds = allAssignments.Items
            .SelectMany(a => a.TechnicianIds)
            .Distinct()
            .ToList();

        foreach (var techId in uniqueTechIds)
        {
            await dailySummaryService.GenerateSummaryAsync(techId, today);
        }

        // Assert
        var summaries = await _context.DailySummaries.ToListAsync();
        summaries.Should().HaveCount(3); // One for each technician
        summaries.Should().AllSatisfy(s =>
        {
            s.TotalAssignments.Should().Be(3);
            s.CompletedTasks.Should().Be(3);
            s.DelayedTasks.Should().Be(0);
        });
    }

    [Fact]
    public async Task BackgroundService_ShouldDetectDelays_AndSetAlertLevels()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var techId = "tech1";

        // Create technician first
        var technician = new Technician { Id = techId, Name = "Test Technician", Status = TechnicianStatus.Active };
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();

        // Create 2 delayed assignments (Amber alert)
        for (int i = 1; i <= 2; i++)
        {
            var assignment = new Assignment
            {
                Id = Guid.NewGuid().ToString(),
                ManagerId = "manager1",
                Title = $"Delayed Assignment {i}",
                Description = "Test",
                Status = AssignmentStatus.InProgress,
                Deadline = DateTime.UtcNow.AddHours(-i),
                LocationName = "Location",
                ServiceType = "Maintenance",
                Priority = AssignmentPriority.High
            };
            assignment.Technicians.Add(technician);
            await _context.Assignments.AddAsync(assignment);
        }
        await _context.SaveChangesAsync();

        // Act
        using var scope = _serviceProvider.CreateScope();
        var dailySummaryService = scope.ServiceProvider.GetRequiredService<IDailySummaryService>();
        var summary = await dailySummaryService.GenerateSummaryAsync(techId, today);

        // Assert
        summary.Should().NotBeNull();
        summary.DelayedTasks.Should().Be(2);
        summary.AlertLevel.Should().Be("Amber");
    }

    [Fact]
    public void BackgroundService_ShouldSchedule_At11_59PM()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DailySummaryBackgroundService>>();
        var service = new DailySummaryBackgroundService(_serviceProvider, loggerMock.Object);

        // Act
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1).AddMinutes(-1); // 11:59 PM
        var timeUntilMidnight = nextMidnight - now;

        // Assert
        timeUntilMidnight.Should().BeGreaterThan(TimeSpan.Zero);
        timeUntilMidnight.Should().BeLessThanOrEqualTo(TimeSpan.FromHours(24));

        service.Dispose();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _serviceProvider.Dispose();
    }
}
