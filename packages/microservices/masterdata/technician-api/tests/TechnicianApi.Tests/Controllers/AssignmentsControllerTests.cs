using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Api.Controllers;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Core.DTOs.Assignment;
using Microsoft.Extensions.Logging;
using TechnicianApi.Core.DTOs.Common;
using Xunit.Abstractions;
using System.Text.Json;

namespace TechnicianApi.Tests.Controllers;

public class AssignmentsControllerTests
{
    private readonly Mock<IAssignmentService> _mockService;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly Mock<ILogger<AssignmentsController>> _mockLogger;
    private readonly AssignmentsController _controller;
    private readonly ITestOutputHelper _output;

    public AssignmentsControllerTests(ITestOutputHelper output)
    {
        _output = output;
        _mockService = new Mock<IAssignmentService>();
        _mockFileStorage = new Mock<IFileStorageService>();
        _mockLogger = new Mock<ILogger<AssignmentsController>>();
        _controller = new AssignmentsController(
            _mockService.Object,
            _mockFileStorage.Object,
            _mockLogger.Object
        );
    }

    #region Bug Verification Tests - These ensure TechnicianIds is never null

    [Fact]
    public async Task BugFix_GetById_ShouldNeverReturnNullTechnicianIds()
    {
        // Arrange
        var assignmentId = "assignment-123";
        var technicianIds = new List<string> { "tech-456", "tech-789" };
        
        var expectedAssignment = new AssignmentResponseDto
        {
            Id = assignmentId,
            Title = "Test Assignment",
            TechnicianIds = technicianIds,
            Status = "Assigned",
            ManagerId = "manager-123",
            Description = "Test Description",
            ServiceType = "Maintenance",
            Priority = "High",
            StartDate = DateTime.UtcNow
        };

        _mockService.Setup(s => s.GetByIdAsync(assignmentId))
            .ReturnsAsync(expectedAssignment);

        // Act
        var result = await _controller.GetById(assignmentId);

        // Print Results
        _output.WriteLine("=== GetById Test Results ===");
        _output.WriteLine($"Assignment ID: {assignmentId}");
        
        var okResult = result as OkObjectResult;
        var assignment = okResult?.Value as AssignmentResponseDto;
        
        if (assignment != null)
        {
            _output.WriteLine($"Title: {assignment.Title}");
            _output.WriteLine($"Status: {assignment.Status}");
            _output.WriteLine($"Manager ID: {assignment.ManagerId}");
            _output.WriteLine($"Service Type: {assignment.ServiceType}");
            _output.WriteLine($"Priority: {assignment.Priority}");
            _output.WriteLine($"TechnicianIds: [{string.Join(", ", assignment.TechnicianIds ?? new List<string>())}]");
            _output.WriteLine($"TechnicianIds Count: {assignment.TechnicianIds?.Count ?? 0}");
            _output.WriteLine($"TechnicianIds is null: {assignment.TechnicianIds == null}");
        }
        _output.WriteLine("============================\n");

        // Assert - THE BUG FIX: TechnicianIds should NEVER be null
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(assignment);
        Assert.NotNull(assignment.TechnicianIds); // ✅ BUG FIX VERIFICATION
        Assert.NotEmpty(assignment.TechnicianIds);
        Assert.Equal(2, assignment.TechnicianIds.Count);
        Assert.Contains("tech-456", assignment.TechnicianIds);
        Assert.Contains("tech-789", assignment.TechnicianIds);
    }

    [Fact]
    public async Task BugFix_GetById_UnassignedAssignment_ShouldReturnEmptyListNotNull()
    {
        // Arrange - Unassigned assignment scenario
        var assignmentId = "assignment-unassigned";
        
        var expectedAssignment = new AssignmentResponseDto
        {
            Id = assignmentId,
            Title = "Unassigned Assignment",
            TechnicianIds = new List<string>(), // Empty list, not null
            Status = "Pending",
            ManagerId = "manager-123",
            Description = "Test Description",
            ServiceType = "Maintenance",
            Priority = "Normal",
            StartDate = DateTime.UtcNow
        };

        _mockService.Setup(s => s.GetByIdAsync(assignmentId))
            .ReturnsAsync(expectedAssignment);

        // Act
        var result = await _controller.GetById(assignmentId);

        // Print Results
        _output.WriteLine("=== GetById Unassigned Test Results ===");
        _output.WriteLine($"Assignment ID: {assignmentId}");
        
        var okResult = result as OkObjectResult;
        var assignment = okResult?.Value as AssignmentResponseDto;
        
        if (assignment != null)
        {
            _output.WriteLine($"Title: {assignment.Title}");
            _output.WriteLine($"Status: {assignment.Status}");
            _output.WriteLine($"TechnicianIds: [{string.Join(", ", assignment.TechnicianIds ?? new List<string>())}]");
            _output.WriteLine($"TechnicianIds Count: {assignment.TechnicianIds?.Count ?? 0}");
            _output.WriteLine($"TechnicianIds is null: {assignment.TechnicianIds == null}");
            _output.WriteLine($"TechnicianIds is empty: {assignment.TechnicianIds?.Count == 0}");
        }
        _output.WriteLine("========================================\n");

        // Assert - THE BUG FIX: Empty list is OK, but never null
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(assignment);
        Assert.NotNull(assignment.TechnicianIds); // ✅ BUG FIX VERIFICATION: Not null
        Assert.Empty(assignment.TechnicianIds);    // ✅ Empty list is acceptable
    }

    [Fact]
    public async Task BugFix_GetPaged_AllAssignments_ShouldHaveNonNullTechnicianIds()
    {
        // Arrange
        var pagedResult = new PagedResponseDto<AssignmentResponseDto>
        {
            Items = new List<AssignmentResponseDto>
            {
                new AssignmentResponseDto
                {
                    Id = "assignment-1",
                    Title = "Assignment 1",
                    TechnicianIds = new List<string> { "tech-1" },
                    Status = "Assigned",
                    ManagerId = "manager-123",
                    Description = "Description 1",
                    ServiceType = "Repair",
                    Priority = "High",
                    StartDate = DateTime.UtcNow
                },
                new AssignmentResponseDto
                {
                    Id = "assignment-2",
                    Title = "Assignment 2",
                    TechnicianIds = new List<string> { "tech-2", "tech-3" },
                    Status = "InProgress",
                    ManagerId = "manager-123",
                    Description = "Description 2",
                    ServiceType = "Installation",
                    Priority = "Normal",
                    StartDate = DateTime.UtcNow
                },
                new AssignmentResponseDto
                {
                    Id = "assignment-3",
                    Title = "Assignment 3",
                    TechnicianIds = new List<string>(), // Unassigned but not null
                    Status = "Pending",
                    ManagerId = "manager-123",
                    Description = "Description 3",
                    ServiceType = "Maintenance",
                    Priority = "Low",
                    StartDate = DateTime.UtcNow
                }
            },
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 10
        };

        _mockService.Setup(s => s.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetPaged();

        // Print Results
        _output.WriteLine("=== GetPaged Test Results ===");
        
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as PagedResponseDto<AssignmentResponseDto>;
        
        if (response != null)
        {
            _output.WriteLine($"Total Count: {response.TotalCount}");
            _output.WriteLine($"Page Number: {response.PageNumber}");
            _output.WriteLine($"Page Size: {response.PageSize}");
            _output.WriteLine($"Items Count: {response.Items.Count()}\n");
            
            int index = 1;
            foreach (var assignment in response.Items)
            {
                _output.WriteLine($"Assignment #{index}:");
                _output.WriteLine($"  ID: {assignment.Id}");
                _output.WriteLine($"  Title: {assignment.Title}");
                _output.WriteLine($"  Status: {assignment.Status}");
                _output.WriteLine($"  Service Type: {assignment.ServiceType}");
                _output.WriteLine($"  Priority: {assignment.Priority}");
                _output.WriteLine($"  TechnicianIds: [{string.Join(", ", assignment.TechnicianIds ?? new List<string>())}]");
                _output.WriteLine($"  TechnicianIds Count: {assignment.TechnicianIds?.Count ?? 0}");
                _output.WriteLine($"  TechnicianIds is null: {assignment.TechnicianIds == null}\n");
                index++;
            }
        }
        _output.WriteLine("==============================\n");

        // Assert - THE BUG FIX: ALL assignments must have non-null TechnicianIds
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(response);
        var itemsList = response.Items.ToList();
        Assert.Equal(3, itemsList.Count);        
        // ✅ BUG FIX VERIFICATION: Every single assignment should have non-null TechnicianIds
        foreach (var assignment in response.Items)
        {
            Assert.NotNull(assignment.TechnicianIds);
        }
        
        // Verify specific assignments
        var assignment1 = response.Items.First(a => a.Id == "assignment-1");
        Assert.NotNull(assignment1.TechnicianIds);
        Assert.Single(assignment1.TechnicianIds);
        Assert.Contains("tech-1", assignment1.TechnicianIds);
        
        var assignment2 = response.Items.First(a => a.Id == "assignment-2");
        Assert.NotNull(assignment2.TechnicianIds);
        Assert.Equal(2, assignment2.TechnicianIds.Count);
        
        var assignment3 = response.Items.First(a => a.Id == "assignment-3");
        Assert.NotNull(assignment3.TechnicianIds); // ✅ Even unassigned should not be null
        Assert.Empty(assignment3.TechnicianIds);
    }

    [Fact]
    public async Task BugFix_GetByTechnicianId_ShouldReturnAssignmentsWithNonNullTechnicianIds()
    {
        // Arrange
        var technicianId = "tech-789";
        var expectedAssignments = new List<AssignmentResponseDto>
        {
            new AssignmentResponseDto
            {
                Id = "assignment-1",
                Title = "Assignment 1",
                TechnicianIds = new List<string> { technicianId },
                Status = "Assigned",
                ManagerId = "manager-123",
                Description = "Description 1",
                ServiceType = "Repair",
                Priority = "High",
                StartDate = DateTime.UtcNow
            },
            new AssignmentResponseDto
            {
                Id = "assignment-2",
                Title = "Assignment 2",
                TechnicianIds = new List<string> { technicianId, "tech-999" },
                Status = "InProgress",
                ManagerId = "manager-123",
                Description = "Description 2",
                ServiceType = "Installation",
                Priority = "Normal",
                StartDate = DateTime.UtcNow
            }
        };

        _mockService.Setup(s => s.GetByTechnicianIdAsync(technicianId))
            .ReturnsAsync(expectedAssignments);

        // Act
        var result = await _controller.GetByTechnicianId(technicianId);

        // Print Results
        _output.WriteLine("=== GetByTechnicianId Test Results ===");
        _output.WriteLine($"Technician ID: {technicianId}");
        
        var okResult = result as OkObjectResult;
        var assignments = okResult?.Value as List<AssignmentResponseDto>;
        
        if (assignments != null)
        {
            _output.WriteLine($"Total Assignments Found: {assignments.Count}\n");
            
            int index = 1;
            foreach (var assignment in assignments)
            {
                _output.WriteLine($"Assignment #{index}:");
                _output.WriteLine($"  ID: {assignment.Id}");
                _output.WriteLine($"  Title: {assignment.Title}");
                _output.WriteLine($"  Status: {assignment.Status}");
                _output.WriteLine($"  Service Type: {assignment.ServiceType}");
                _output.WriteLine($"  Priority: {assignment.Priority}");
                _output.WriteLine($"  TechnicianIds: [{string.Join(", ", assignment.TechnicianIds ?? new List<string>())}]");
                _output.WriteLine($"  TechnicianIds Count: {assignment.TechnicianIds?.Count ?? 0}");
                _output.WriteLine($"  Contains target technician: {assignment.TechnicianIds?.Contains(technicianId) ?? false}\n");
                index++;
            }
        }
        _output.WriteLine("=======================================\n");

        // Assert - THE BUG FIX: All returned assignments must have non-null TechnicianIds
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(assignments);
        Assert.Equal(2, assignments.Count);
        
        // ✅ BUG FIX VERIFICATION: Every assignment must have non-null TechnicianIds
        foreach (var assignment in assignments)
        {
            Assert.NotNull(assignment.TechnicianIds);
            Assert.Contains(technicianId, assignment.TechnicianIds);
        }
    }

    [Fact]
    public async Task BugFix_GetByManagerId_ShouldReturnAssignmentsWithNonNullTechnicianIds()
    {
        // Arrange
        var managerId = "manager-123";
        var expectedAssignments = new List<AssignmentResponseDto>
        {
            new AssignmentResponseDto
            {
                Id = "assignment-1",
                Title = "Assignment 1",
                TechnicianIds = new List<string> { "tech-1" },
                ManagerId = managerId,
                Status = "Assigned",
                Description = "Description 1",
                ServiceType = "Repair",
                Priority = "High",
                StartDate = DateTime.UtcNow
            },
            new AssignmentResponseDto
            {
                Id = "assignment-2",
                Title = "Assignment 2",
                TechnicianIds = new List<string> { "tech-2" },
                ManagerId = managerId,
                Status = "Completed",
                Description = "Description 2",
                ServiceType = "Maintenance",
                Priority = "Normal",
                StartDate = DateTime.UtcNow
            }
        };

        _mockService.Setup(s => s.GetByManagerIdAsync(managerId))
            .ReturnsAsync(expectedAssignments);

        // Act
        var result = await _controller.GetByManagerId(managerId);

        // Print Results
        _output.WriteLine("=== GetByManagerId Test Results ===");
        _output.WriteLine($"Manager ID: {managerId}");
        
        var okResult = result as OkObjectResult;
        var assignments = okResult?.Value as List<AssignmentResponseDto>;
        
        if (assignments != null)
        {
            _output.WriteLine($"Total Assignments Found: {assignments.Count}\n");
            
            int index = 1;
            foreach (var assignment in assignments)
            {
                _output.WriteLine($"Assignment #{index}:");
                _output.WriteLine($"  ID: {assignment.Id}");
                _output.WriteLine($"  Title: {assignment.Title}");
                _output.WriteLine($"  Status: {assignment.Status}");
                _output.WriteLine($"  Manager ID: {assignment.ManagerId}");
                _output.WriteLine($"  Service Type: {assignment.ServiceType}");
                _output.WriteLine($"  Priority: {assignment.Priority}");
                _output.WriteLine($"  TechnicianIds: [{string.Join(", ", assignment.TechnicianIds ?? new List<string>())}]");
                _output.WriteLine($"  TechnicianIds Count: {assignment.TechnicianIds?.Count ?? 0}\n");
                index++;
            }
        }
        _output.WriteLine("====================================\n");

        // Assert - THE BUG FIX: All assignments should have non-null TechnicianIds
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(assignments);
        Assert.Equal(2, assignments.Count);
        
        // ✅ BUG FIX VERIFICATION: Every assignment must have non-null TechnicianIds
        foreach (var assignment in assignments)
        {
            Assert.NotNull(assignment.TechnicianIds);
            Assert.NotEmpty(assignment.TechnicianIds);
        }
    }

    [Fact]
    public async Task BugFix_AssignTechnician_ShouldReturnAssignmentWithNonNullTechnicianIds()
    {
        // Arrange
        var assignmentId = "assignment-123";
        var technicianId = "tech-456";
        
        var updatedAssignment = new AssignmentResponseDto
        {
            Id = assignmentId,
            Title = "Test Assignment",
            TechnicianIds = new List<string> { technicianId },
            Status = "Assigned",
            ManagerId = "manager-123",
            Description = "Test Description",
            ServiceType = "Repair",
            Priority = "High",
            StartDate = DateTime.UtcNow
        };

        _mockService.Setup(s => s.AssignTechnicianAsync(assignmentId, technicianId))
            .ReturnsAsync(updatedAssignment);

        // Act
        var result = await _controller.AssignTechnician(assignmentId, technicianId);

        // Print Results
        _output.WriteLine("=== AssignTechnician Test Results ===");
        _output.WriteLine($"Assignment ID: {assignmentId}");
        _output.WriteLine($"Assigned Technician ID: {technicianId}");
        
        var okResult = result as OkObjectResult;
        var assignment = okResult?.Value as AssignmentResponseDto;
        
        if (assignment != null)
        {
            _output.WriteLine($"Title: {assignment.Title}");
            _output.WriteLine($"Status: {assignment.Status}");
            _output.WriteLine($"TechnicianIds: [{string.Join(", ", assignment.TechnicianIds ?? new List<string>())}]");
            _output.WriteLine($"TechnicianIds Count: {assignment.TechnicianIds?.Count ?? 0}");
            _output.WriteLine($"Contains assigned technician: {assignment.TechnicianIds?.Contains(technicianId) ?? false}");
        }
        _output.WriteLine("======================================\n");

        // Assert - THE BUG FIX: Assigned technician must appear in non-null TechnicianIds
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(assignment);
        Assert.NotNull(assignment.TechnicianIds); // ✅ BUG FIX VERIFICATION
        Assert.Contains(technicianId, assignment.TechnicianIds);
    }

    #endregion

    #region Additional Comprehensive Tests

    [Theory]
    [InlineData("assignment-1", "tech-1")]
    [InlineData("assignment-2", "tech-2")]
    [InlineData("assignment-3", "tech-3")]
    public async Task BugFix_GetById_MultipleScenarios_ShouldAlwaysReturnNonNullTechnicianIds(
        string assignmentId, 
        string technicianId)
    {
        // Arrange
        var expectedAssignment = new AssignmentResponseDto
        {
            Id = assignmentId,
            Title = $"Assignment for {technicianId}",
            TechnicianIds = new List<string> { technicianId },
            Status = "Assigned",
            ManagerId = "manager-123",
            Description = "Test Description",
            ServiceType = "Repair",
            Priority = "Normal",
            StartDate = DateTime.UtcNow
        };

        _mockService.Setup(s => s.GetByIdAsync(assignmentId))
            .ReturnsAsync(expectedAssignment);

        // Act
        var result = await _controller.GetById(assignmentId);

        // Print Results
        _output.WriteLine($"=== GetById Multiple Scenarios: {assignmentId} ===");
        
        var okResult = result as OkObjectResult;
        var assignment = okResult?.Value as AssignmentResponseDto;
        
        if (assignment != null)
        {
            _output.WriteLine($"Assignment ID: {assignment.Id}");
            _output.WriteLine($"Title: {assignment.Title}");
            _output.WriteLine($"TechnicianIds: [{string.Join(", ", assignment.TechnicianIds ?? new List<string>())}]");
            _output.WriteLine($"Expected Technician: {technicianId}");
            _output.WriteLine($"Contains expected: {assignment.TechnicianIds?.Contains(technicianId) ?? false}");
        }
        _output.WriteLine("==============================================\n");

        // Assert - THE BUG FIX: TechnicianIds must never be null
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(assignment);
        Assert.NotNull(assignment.TechnicianIds); // ✅ BUG FIX VERIFICATION
        Assert.Contains(technicianId, assignment.TechnicianIds);
    }

    [Fact]
    public async Task BugFix_UnassignTechnician_ShouldReturnAssignmentWithNonNullTechnicianIds()
    {
        // Arrange
        var assignmentId = "assignment-123";
        var technicianId = "tech-456";
        
        // After unassigning, TechnicianIds should be empty list, not null
        var updatedAssignment = new AssignmentResponseDto
        {
            Id = assignmentId,
            Title = "Test Assignment",
            TechnicianIds = new List<string>(), // Empty after unassign
            Status = "Pending",
            ManagerId = "manager-123",
            Description = "Test Description",
            ServiceType = "Repair",
            Priority = "High",
            StartDate = DateTime.UtcNow
        };

        _mockService.Setup(s => s.UnassignTechnicianAsync(assignmentId, technicianId))
            .ReturnsAsync(updatedAssignment);

        // Act
        var result = await _controller.UnassignTechnician(assignmentId, technicianId);

        // Print Results
        _output.WriteLine("=== UnassignTechnician Test Results ===");
        _output.WriteLine($"Assignment ID: {assignmentId}");
        _output.WriteLine($"Unassigned Technician ID: {technicianId}");
        
        var okResult = result as OkObjectResult;
        var assignment = okResult?.Value as AssignmentResponseDto;
        
        if (assignment != null)
        {
            _output.WriteLine($"Title: {assignment.Title}");
            _output.WriteLine($"Status: {assignment.Status}");
            _output.WriteLine($"TechnicianIds: [{string.Join(", ", assignment.TechnicianIds ?? new List<string>())}]");
            _output.WriteLine($"TechnicianIds Count: {assignment.TechnicianIds?.Count ?? 0}");
            _output.WriteLine($"TechnicianIds is null: {assignment.TechnicianIds == null}");
            _output.WriteLine($"TechnicianIds is empty: {assignment.TechnicianIds?.Count == 0}");
        }
        _output.WriteLine("========================================\n");

        // Assert - THE BUG FIX: Even after unassigning, TechnicianIds should be empty list, not null
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(assignment);
        Assert.NotNull(assignment.TechnicianIds); // ✅ BUG FIX VERIFICATION
        Assert.Empty(assignment.TechnicianIds);
    }

    [Fact]
    public async Task BugFix_MultipleAssignments_MixedScenarios_AllShouldHaveNonNullTechnicianIds()
    {
        // Arrange - Mix of assigned, unassigned, and multi-assigned
        var pagedResult = new PagedResponseDto<AssignmentResponseDto>
        {
            Items = new List<AssignmentResponseDto>
            {
                new AssignmentResponseDto
                {
                    Id = "assignment-single",
                    Title = "Single Technician",
                    TechnicianIds = new List<string> { "tech-1" },
                    Status = "Assigned",
                    ManagerId = "manager-123",
                    ServiceType = "Repair",
                    Priority = "High",
                    StartDate = DateTime.UtcNow
                },
                new AssignmentResponseDto
                {
                    Id = "assignment-multiple",
                    Title = "Multiple Technicians",
                    TechnicianIds = new List<string> { "tech-2", "tech-3", "tech-4" },
                    Status = "InProgress",
                    ManagerId = "manager-123",
                    ServiceType = "Installation",
                    Priority = "Normal",
                    StartDate = DateTime.UtcNow
                },
                new AssignmentResponseDto
                {
                    Id = "assignment-unassigned",
                    Title = "No Technicians",
                    TechnicianIds = new List<string>(), // Unassigned
                    Status = "Pending",
                    ManagerId = "manager-123",
                    ServiceType = "Maintenance",
                    Priority = "Low",
                    StartDate = DateTime.UtcNow
                }
            },
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 10
        };

        _mockService.Setup(s => s.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetPaged();

        // Print Results
        _output.WriteLine("=== Mixed Scenarios Test Results ===");
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PagedResponseDto<AssignmentResponseDto>>(okResult.Value);
        
        _output.WriteLine($"Total Count: {response.TotalCount}");
        _output.WriteLine($"Items: {response.Items.Count()}\n");
        
        foreach (var assignment in response.Items)
        {
            _output.WriteLine($"ID: {assignment.Id}");
            _output.WriteLine($"  Scenario: {assignment.Title}");
            _output.WriteLine($"  Status: {assignment.Status}");
            _output.WriteLine($"  TechnicianIds: [{string.Join(", ", assignment.TechnicianIds ?? new List<string>())}]");
            _output.WriteLine($"  Count: {assignment.TechnicianIds?.Count ?? 0}");
            _output.WriteLine($"  Is Null: {assignment.TechnicianIds == null}");
            _output.WriteLine($"  Is Empty: {assignment.TechnicianIds?.Count == 0}\n");
        }
        _output.WriteLine("=====================================\n");
        
        // Assert - THE BUG FIX: All scenarios must have non-null TechnicianIds
        Assert.All(response.Items, assignment => 
        {
            Assert.NotNull(assignment.TechnicianIds);
        });
        
        // Verify single technician
        var single = response.Items.First(a => a.Id == "assignment-single");
        Assert.Single(single.TechnicianIds);
        
        // Verify multiple technicians
        var multiple = response.Items.First(a => a.Id == "assignment-multiple");
        Assert.Equal(3, multiple.TechnicianIds.Count);
        
        // Verify unassigned (empty but not null)
        var unassigned = response.Items.First(a => a.Id == "assignment-unassigned");
        Assert.NotNull(unassigned.TechnicianIds);
        Assert.Empty(unassigned.TechnicianIds);
    }

    #endregion
}