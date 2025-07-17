using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using UserService.Api.Controllers;
using UserService.Core.DTOs.Report;
using UserService.Core.Entities;
using UserService.Infrastructure.Data;
using Xunit;

namespace UserService.IntegrationTests.Controllers
{
    public class ReportsControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly UserServiceDbContext _dbContext;
        private readonly string _adminToken;

        public ReportsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace the database with an in-memory database for testing
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<UserServiceDbContext>));
                    
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<UserServiceDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_Reports");
                    });
                });
            });

            _client = _factory.CreateClient();
            
            // Get the DbContext
            var scope = _factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
            
            // Ensure the database is created and seeded
            _dbContext.Database.EnsureCreated();
            
            // Generate a test admin token
            _adminToken = TestHelpers.GenerateTestToken("test-admin", ["Admin"]);
            
            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Clear existing data
            _dbContext.Users.RemoveRange(_dbContext.Users);
            _dbContext.Roles.RemoveRange(_dbContext.Roles);
            _dbContext.Shifts.RemoveRange(_dbContext.Shifts);
            _dbContext.UserShifts.RemoveRange(_dbContext.UserShifts);
            
            // Add test roles
            var adminRole = new Role { Id = "1", Name = "Admin", Description = "Administrator role" };
            var userRole = new Role { Id = "2", Name = "User", Description = "Regular user role" };
            _dbContext.Roles.AddRange(adminRole, userRole);
            
            // Add test users
            var adminUser = new User 
            { 
                Id = "admin-1", 
                Email = "admin@example.com", 
                FirstName = "Admin", 
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var testUser1 = new User 
            { 
                Id = "user-1", 
                Email = "user1@example.com", 
                FirstName = "Test", 
                LastName = "User 1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var testUser2 = new User 
            { 
                Id = "user-2", 
                Email = "user2@example.com", 
                FirstName = "Test", 
                LastName = "User 2",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _dbContext.Users.AddRange(adminUser, testUser1, testUser2);
            
            // Add user roles
            _dbContext.UserRoles.AddRange(
                new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id },
                new UserRole { UserId = testUser1.Id, RoleId = userRole.Id },
                new UserRole { UserId = testUser2.Id, RoleId = userRole.Id }
            );
            
            // Add test shifts
            var morningShift = new Shift
            {
                Id = "shift-1",
                Name = "Morning Shift",
                Description = "Morning shift from 8 AM to 4 PM",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                Mode = ShiftMode.Strict,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var eveningShift = new Shift
            {
                Id = "shift-2",
                Name = "Evening Shift",
                Description = "Evening shift from 4 PM to 12 AM",
                StartTime = new TimeSpan(16, 0, 0),
                EndTime = new TimeSpan(0, 0, 0),
                Mode = ShiftMode.Open,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _dbContext.Shifts.AddRange(morningShift, eveningShift);
            
            // Add user shifts
            _dbContext.UserShifts.AddRange(
                new UserShift 
                { 
                    UserId = testUser1.Id, 
                    ShiftId = morningShift.Id, 
                    AssignedAt = DateTime.UtcNow.AddDays(-7) 
                },
                new UserShift 
                { 
                    UserId = testUser1.Id, 
                    ShiftId = eveningShift.Id, 
                    AssignedAt = DateTime.UtcNow.AddDays(-7) 
                },
                new UserShift 
                { 
                    UserId = testUser2.Id, 
                    ShiftId = eveningShift.Id, 
                    AssignedAt = DateTime.UtcNow.AddDays(-7) 
                }
            );
            
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetShiftReport_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            
            // Act
            var response = await _client.GetAsync("/api/Reports/shifts");
            
            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
            
            var report = await response.Content.ReadFromJsonAsync<ShiftReportResponse>();
            Assert.NotNull(report);
            Assert.Equal(2, report.TotalShifts);
            Assert.Equal(2, report.Shifts.Count);
            Assert.True(report.Shifts.All(s => !string.IsNullOrEmpty(s.Id)));
            Assert.True(report.Shifts.All(s => !string.IsNullOrEmpty(s.Name)));
        }
        
        [Fact]
        public async Task GetUserReport_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            
            // Act
            var response = await _client.GetAsync("/api/Reports/users");
            
            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
            
            var report = await response.Content.ReadFromJsonAsync<UserReportResponse>();
            Assert.NotNull(report);
            Assert.Equal(3, report.TotalUsers);
            Assert.Equal(3, report.Users.Count);
            
            // Test user 1 should have 2 shifts assigned
            var testUser1 = report.Users.FirstOrDefault(u => u.Id == "user-1");
            Assert.NotNull(testUser1);
            Assert.Equal(2, testUser1.AssignedShifts.Count);
            
            // Test user 2 should have 1 shift assigned
            var testUser2 = report.Users.FirstOrDefault(u => u.Id == "user-2");
            Assert.NotNull(testUser2);
            Assert.Single(testUser2.AssignedShifts);
        }
        
        [Fact]
        public async Task GetShiftDetails_WithValidId_ReturnsShiftDetails()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var shiftId = "shift-1";
            
            // Act
            var response = await _client.GetAsync($"/api/Reports/shifts/{shiftId}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var shift = await response.Content.ReadFromJsonAsync<ShiftReportDto>();
            
            Assert.NotNull(shift);
            Assert.Equal(shiftId, shift.Id);
            Assert.Equal("Morning Shift", shift.Name);
            Assert.Equal(1, shift.AssignedUsersCount); // Only testUser1 is assigned to this shift
        }
        
        [Fact]
        public async Task GetUserDetails_WithValidId_ReturnsUserDetails()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var userId = "user-1";
            
            // Act
            var response = await _client.GetAsync($"/api/Reports/users/{userId}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserReportDto>();
            
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
            Assert.Equal("Test", user.FirstName);
            Assert.Equal(2, user.AssignedShifts.Count); // user-1 is assigned to 2 shifts
            Assert.Contains(user.AssignedShifts, s => s.ShiftName == "Morning Shift");
            Assert.Contains(user.AssignedShifts, s => s.ShiftName == "Evening Shift");
        }
        
        [Fact]
        public async Task GetShiftDetails_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var invalidShiftId = "nonexistent-shift";
            
            // Act
            var response = await _client.GetAsync($"/api/Reports/shifts/{invalidShiftId}");
            
            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public async Task GetUserDetails_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var invalidUserId = "nonexistent-user";
            
            // Act
            var response = await _client.GetAsync($"/api/Reports/users/{invalidUserId}");
            
            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public async Task GetReports_WithoutAuthorization_ReturnsUnauthorized()
        {
            // Arrange - don't set the authorization header
            
            // Act - try to access reports without a token
            var shiftReportResponse = await _client.GetAsync("/api/Reports/shifts");
            var userReportResponse = await _client.GetAsync("/api/Reports/users");
            
            // Assert - both should return 401 Unauthorized
            Assert.Equal(HttpStatusCode.Unauthorized, shiftReportResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, userReportResponse.StatusCode);
        }

        public void Dispose()
        {
            // Clean up the test database
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            _client.Dispose();
        }
    }
}
