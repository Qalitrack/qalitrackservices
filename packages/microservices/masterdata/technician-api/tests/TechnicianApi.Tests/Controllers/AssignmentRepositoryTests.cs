using Microsoft.EntityFrameworkCore;
using TechnicianApi.Core.Entities;
using TechnicianApi.Infrastructure.Data;
using TechnicianApi.Infrastructure.Repositories;

namespace TechnicianApi.Tests.Controllers
{
    public class AssignmentRepositoryTests
    {
        private readonly TechnicianApiDbContext _context;
        private readonly AssignmentRepository _repository;

        public AssignmentRepositoryTests()
        {
            // Use InMemory for fast, isolated testing
            var options = new DbContextOptionsBuilder<TechnicianApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            _context = new TechnicianApiDbContext(options);
            _repository = new AssignmentRepository(_context);

            // Seed one assignment
            var assignment = new Assignment
            {
                Id = "test-assignment-001",
                Title = "Test Assignment",
                Status = AssignmentStatus.Pending,
                ManagerId = "mgr-001",
                TechnicianIds = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Assignments.Add(assignment);
            _context.SaveChanges();
        }

        [Fact]
        public async Task AssignTechnicianAsync_AddsTechnicianAndPersistsToDatabase()
        {
            // Arrange
            var assignmentId = "test-assignment-001";
            var technicianId = "tech-12345";

            // Act
            var result = await _repository.AssignTechnicianAsync(assignmentId, technicianId);

            // Assert - in-memory result
            Assert.NotNull(result);
            Assert.Contains(technicianId, result.TechnicianIds);
            Assert.Equal(1, result.TechnicianIds.Count);

            // Critical: Reload from DB to confirm persistence
            var reloaded = await _context.Assignments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            Assert.NotNull(reloaded);
            Assert.Contains(technicianId, reloaded.TechnicianIds);
            Assert.Equal(1, reloaded.TechnicianIds.Count);
            Assert.True(reloaded.UpdatedAt > DateTime.UtcNow.AddMinutes(-1)); // recent update
        }

        [Fact]
        public async Task AssignTechnicianAsync_DuplicateId_DoesNotAddAgain()
        {
            var assignmentId = "test-assignment-001";
            var technicianId = "tech-12345";

            // First assign
            await _repository.AssignTechnicianAsync(assignmentId, technicianId);

            // Second assign (should do nothing)
            var result = await _repository.AssignTechnicianAsync(assignmentId, technicianId);

            Assert.NotNull(result);
            Assert.Single(result.TechnicianIds); // still only 1

            // Check DB too
            var reloaded = await _context.Assignments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            Assert.Single(reloaded!.TechnicianIds);
        }

        [Fact]
        public async Task AssignTechnicianAsync_NonExistingAssignment_ReturnsNull()
        {
            var result = await _repository.AssignTechnicianAsync("non-existing-id", "tech-xyz");
            Assert.Null(result);
        }
    }
}