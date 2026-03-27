using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechnicianApi.Api.Controllers;
using TechnicianApi.Core.DTOs.Assignment;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Interfaces;
using Xunit;
using Xunit.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechnicianApi.Tests.Controllers
{
    public class AssignmentFlowLocalTests
    {
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly Mock<IFileStorageService> _mockFileStorage;
        private readonly Mock<ILogger<AssignmentsController>> _mockLogger;
        private readonly AssignmentsController _controller;
        private readonly ITestOutputHelper _output;

        public AssignmentFlowLocalTests(ITestOutputHelper output)
        {
            _output = output;
            _mockAssignmentService = new Mock<IAssignmentService>();
            _mockFileStorage = new Mock<IFileStorageService>();
            _mockLogger = new Mock<ILogger<AssignmentsController>>();
            _controller = new AssignmentsController(
                _mockAssignmentService.Object,
                _mockFileStorage.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Local_AssignFlow_WithGetAllAndGetSingle_VerifyFullStructure()
        {
            // ───────────────────────────────────────────────────────────────
            // Arrange - Test data (two assignments to make GetAll more realistic)
            // ───────────────────────────────────────────────────────────────
            var targetAssignmentId = "75664b41-f418-4d98-9296-27c7ad8e76e5";
            var otherAssignmentId = "another-9999-aaaa-bbbb-cccccccccccc";
            var technicianId = "test-technician-789";

            var initialTarget = new AssignmentResponseDto
            {
                Id = targetAssignmentId,
                Title = "Signon Weighbridge Installation",
                Status = "Pending",
                TechnicianIds = new List<string>(),
                ManagerId = "manager-001",
                Description = "Install weighbridge at Signon site",
                ServiceType = "Installation",
                Priority = "Medium",
                StartDate = DateTime.UtcNow.AddDays(-2)
            };

            var otherAssignment = new AssignmentResponseDto
            {
                Id = otherAssignmentId,
                Title = "Routine Pump Maintenance - Kitengela",
                Status = "InProgress",
                TechnicianIds = new List<string> { "tech-already-123" },
                ManagerId = "manager-002",
                Description = "Monthly pump check",
                ServiceType = "Maintenance",
                Priority = "High",
                StartDate = DateTime.UtcNow.AddDays(-10)
            };

            var updatedTarget = new AssignmentResponseDto
            {
                Id = targetAssignmentId,
                Title = "Signon Weighbridge Installation",
                Status = "Assigned",
                TechnicianIds = new List<string> { technicianId },
                ManagerId = "manager-001",
                Description = "Install weighbridge at Signon site",
                ServiceType = "Installation",
                Priority = "Medium",
                StartDate = DateTime.UtcNow.AddDays(-2)
            };

            // Initial paged list (before assign)
            var initialPaged = new PagedResponseDto<AssignmentResponseDto>
            {
                Items = new List<AssignmentResponseDto> { initialTarget, otherAssignment },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            // Paged list after assign (updated target)
            var updatedPaged = new PagedResponseDto<AssignmentResponseDto>
            {
                Items = new List<AssignmentResponseDto> { updatedTarget, otherAssignment },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            // Mock setups
            _mockAssignmentService
                .SetupSequence(s => s.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), null, null))
                .ReturnsAsync(initialPaged)     // first GetAll
                .ReturnsAsync(updatedPaged);    // second GetAll (after assign)

            _mockAssignmentService
                .SetupSequence(s => s.GetByIdAsync(targetAssignmentId))
                .ReturnsAsync(initialTarget)    // GET before
                .ReturnsAsync(updatedTarget);   // GET after

            _mockAssignmentService
                .Setup(s => s.AssignTechnicianAsync(targetAssignmentId, technicianId))
                .ReturnsAsync(updatedTarget);

            _output.WriteLine("=== LOCAL FULL FLOW TEST: GET ALL + GET SINGLE + ASSIGN + GET ALL/SINGLE AGAIN ===");
            _output.WriteLine("Goal: See complete structure before/after assignment");
            _output.WriteLine("");

            // ───────────────────────────────────────────────────────────────
            // STEP 1: GET ALL (before)
            // ───────────────────────────────────────────────────────────────
            _output.WriteLine("STEP 1: GET ALL assignments (mocked paged list - BEFORE)");
            var getAllBefore = await _controller.GetPaged();
            var okAllBefore = Assert.IsType<OkObjectResult>(getAllBefore);
            var pagedBefore = Assert.IsType<PagedResponseDto<AssignmentResponseDto>>(okAllBefore.Value);

            _output.WriteLine($"→ Total assignments: {pagedBefore.TotalCount}");
            foreach (var ass in pagedBefore.Items)
            {
                PrintFullAssignment($"Assignment {ass.Id}", ass);
            }

            var targetBefore = pagedBefore.Items.First(a => a.Id == targetAssignmentId);

            // ───────────────────────────────────────────────────────────────
            // STEP 2: GET SINGLE (before)
            // ───────────────────────────────────────────────────────────────
            _output.WriteLine("");
            _output.WriteLine("STEP 2: GET SINGLE assignment (before assign)");
            var getSingleBefore = await _controller.GetById(targetAssignmentId);
            var okSingleBefore = Assert.IsType<OkObjectResult>(getSingleBefore);
            var singleBefore = Assert.IsType<AssignmentResponseDto>(okSingleBefore.Value);
            PrintFullAssignment("SINGLE GET - BEFORE", singleBefore);

            // ───────────────────────────────────────────────────────────────
            // STEP 3: ASSIGN technician
            // ───────────────────────────────────────────────────────────────
            _output.WriteLine("");
            _output.WriteLine("STEP 3: POST assign technician");
            _output.WriteLine($"  → Technician ID: {technicianId}");

            var assignResult = await _controller.AssignTechnician(targetAssignmentId, technicianId);
            var okAssign = Assert.IsType<OkObjectResult>(assignResult);
            var fromAssign = Assert.IsType<AssignmentResponseDto>(okAssign.Value);
            PrintFullAssignment("RETURNED FROM ASSIGN", fromAssign);

            // ───────────────────────────────────────────────────────────────
            // STEP 4: GET ALL (after)
            // ───────────────────────────────────────────────────────────────
            _output.WriteLine("");
            _output.WriteLine("STEP 4: GET ALL assignments (after assign)");
            var getAllAfter = await _controller.GetPaged();
            var okAllAfter = Assert.IsType<OkObjectResult>(getAllAfter);
            var pagedAfter = Assert.IsType<PagedResponseDto<AssignmentResponseDto>>(okAllAfter.Value);

            _output.WriteLine($"→ Total assignments: {pagedAfter.TotalCount}");
            foreach (var ass in pagedAfter.Items)
            {
                PrintFullAssignment($"Assignment {ass.Id}", ass);
            }

            // ───────────────────────────────────────────────────────────────
            // STEP 5: GET SINGLE (after)
            // ───────────────────────────────────────────────────────────────
            _output.WriteLine("");
            _output.WriteLine("STEP 5: GET SINGLE assignment (after assign)");
            var getSingleAfter = await _controller.GetById(targetAssignmentId);
            var okSingleAfter = Assert.IsType<OkObjectResult>(getSingleAfter);
            var singleAfter = Assert.IsType<AssignmentResponseDto>(okSingleAfter.Value);
            PrintFullAssignment("SINGLE GET - AFTER", singleAfter);

            // ───────────────────────────────────────────────────────────────
            // Final checks
            // ───────────────────────────────────────────────────────────────
            _output.WriteLine("");
            _output.WriteLine("FINAL VERIFICATIONS:");
            Assert.NotNull(singleAfter.TechnicianIds);
            _output.WriteLine("  ✓ TechnicianIds is NOT null");

            Assert.Contains(technicianId, singleAfter.TechnicianIds);
            _output.WriteLine("  ✓ Assigned technician is present");

            Assert.Equal("Assigned", singleAfter.Status);
            _output.WriteLine("  ✓ Status updated (if your logic changes it)");

            _output.WriteLine("");
            _output.WriteLine("========================================");
            _output.WriteLine("LOCAL TEST COMPLETE");
            _output.WriteLine("========================================");
        }

        private void PrintFullAssignment(string label, AssignmentResponseDto dto)
        {
            _output.WriteLine($"┌─ {label,-50} ─┐");
            _output.WriteLine($"  • ID                  : {dto.Id}");
            _output.WriteLine($"  • Title               : {dto.Title}");
            _output.WriteLine($"  • Status              : {dto.Status}");
            _output.WriteLine($"  • Manager ID          : {dto.ManagerId ?? "—"}");
            _output.WriteLine($"  • Description         : {(dto.Description?.Length > 60 ? dto.Description.Substring(0, 57) + "..." : dto.Description ?? "—")}");
            _output.WriteLine($"  • Service Type        : {dto.ServiceType ?? "—"}");
            _output.WriteLine($"  • Priority            : {dto.Priority ?? "—"}");
            _output.WriteLine($"  • Start Date          : {dto.StartDate:yyyy-MM-dd HH:mm} UTC");

            if (dto.TechnicianIds == null)
            {
                _output.WriteLine($"  • TechnicianIds       : NULL ⚠️");
                _output.WriteLine($"  • Technician Count    : -1");
            }
            else
            {
                _output.WriteLine($"  • TechnicianIds       : [{string.Join(", ", dto.TechnicianIds)}]");
                _output.WriteLine($"  • Technician Count    : {dto.TechnicianIds.Count}");
                if (dto.TechnicianIds.Count == 0) _output.WriteLine("    (empty list - allowed)");
            }

            _output.WriteLine($"└────────────────────────────────────────────────────┘");
            _output.WriteLine("");
        }
    }
}