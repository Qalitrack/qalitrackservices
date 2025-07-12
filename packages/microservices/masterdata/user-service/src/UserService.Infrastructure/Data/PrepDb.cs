using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Data
{
    public static class PrepDb
    {
        public static async Task PrepPopulation(IApplicationBuilder app, bool isProduction)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetService<UserServiceDbContext>();
            
            if (context == null)
            {
                throw new InvalidOperationException("UserServiceDbContext is not registered in the service provider.");
            }

            // Apply migrations if not in production
            if (!isProduction)
            {
                await context.Database.MigrateAsync();
            }

            // Seed data
            await SeedData(context);
        }

        private static async Task SeedData(UserServiceDbContext context)
        {
            // Seed Roles
            if (!context.Roles.Any())
            {
                var roles = new[]
                {
                    new Role { Id = Guid.NewGuid().ToString(), Name = "Admin", Description = "Administrator role with full access", CreatedAt = DateTime.UtcNow },
                    new Role { Id = Guid.NewGuid().ToString(), Name = "User", Description = "Standard user role with limited access", CreatedAt = DateTime.UtcNow },
                    new Role { Id = Guid.NewGuid().ToString(), Name = "Manager", Description = "Manager role with elevated access", CreatedAt = DateTime.UtcNow }
                };
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Seed Permissions
            if (!context.Permissions.Any())
            {
                var permissions = new[]
                {
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "CreateUser", Description = "Permission to create users", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "EditUser", Description = "Permission to edit users", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "DeleteUser", Description = "Permission to delete users", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "ViewReports", Description = "Permission to view reports", CreatedAt = DateTime.UtcNow },
                    //shiftpermissions
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "shifts.view", Description = "Permission to view shifts", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "shifts.manage", Description = "Permission to manage shifts", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "shifts.assign", Description = "Permission to assign shifts", CreatedAt = DateTime.UtcNow }
                };
                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var users = new[]
                {
                    new User 
                    { 
                        Id = Guid.NewGuid().ToString(), 
                        FirstName = "Admin",
                        LastName = "Admin",
                        Email = "admin@userservice.com", 
                        MobileNumber = "1234567890", 
                        Password = BCrypt.Net.BCrypt.HashPassword("password"), // Hash password using BCrypt
                        Status = UserStatus.Active,
                        CreatedAt = DateTime.UtcNow 
                    },
                    new User 
                    { 
                        Id = Guid.NewGuid().ToString(), 
                        FirstName = "JohnDoe", 
                        LastName = "Doe",
                        Email = "john.doe@userservice.com", 
                        Password = BCrypt.Net.BCrypt.HashPassword("password"), // Hash password using BCrypt
                        CreatedAt = DateTime.UtcNow,
                        Status = UserStatus.Active
                    }
                };
                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }

            // Seed Shifts
            if (!context.Shifts.Any())
            {
                var shifts = new[]
                {
                    new Shift
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Morning Shift", 
                        StartTime = new TimeSpan(8, 0, 0), 
                        EndTime = new TimeSpan(16, 0, 0), 
                        CreatedAt = DateTime.UtcNow ,
                        Mode = ShiftMode.Open,

                         
                    },
                    new Shift { Id = Guid.NewGuid().ToString(), Name = "Evening Shift", StartTime = new TimeSpan(16, 0, 0), EndTime = new TimeSpan(0, 0, 0), CreatedAt = DateTime.UtcNow }
                };
                await context.Shifts.AddRangeAsync(shifts);
                await context.SaveChangesAsync();
            }

            // Seed RolePermissions
            if (!context.RolePermissions.Any())
            {
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                var permissions = await context.Permissions.ToListAsync();

                if (adminRole != null && permissions.Any())
                {
                    var rolePermissions = permissions.Select(p => new RolePermission
                    {
                        Id = Guid.NewGuid().ToString(),
                        RoleId = adminRole.Id,
                        PermissionId = p.Id,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await context.RolePermissions.AddRangeAsync(rolePermissions);
                    await context.SaveChangesAsync();
                }
            }

            // Seed UserRoles
            if (!context.UserRoles.Any())
            {
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@userservice.com");
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "john.doe@userservice.com");
                var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

                if (adminUser != null && adminRole != null && user != null && userRole != null)
                {
                    var userRoles = new[]
                    {
                        new UserRole { Id = Guid.NewGuid().ToString(), UserId = adminUser.Id, RoleId = adminRole.Id, CreatedAt = DateTime.UtcNow },
                        new UserRole { Id = Guid.NewGuid().ToString(), UserId = user.Id, RoleId = userRole.Id, CreatedAt = DateTime.UtcNow }
                    };
                    await context.UserRoles.AddRangeAsync(userRoles);
                    await context.SaveChangesAsync();
                }
            }

            // Seed UserShifts
            if (!context.UserShifts.Any())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "john.doe@userservice.com");
                var morningShift = await context.Shifts.FirstOrDefaultAsync(s => s.Name == "Morning Shift");

                if (user != null && morningShift != null)
                {
                    var userShift = new UserShift
                    {
                        UserId = user.Id,
                        ShiftId = morningShift.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.UserShifts.AddAsync(userShift);
                    await context.SaveChangesAsync();
                }
            }

            // Seed PersonalAccessTokens (Optional, depending on use case)
            if (!context.PersonalAccessTokens.Any())
            {
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@userservice.com");
                if (adminUser != null)
                {
                    var token = new PersonalAccessToken
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = adminUser.Id,
                        Token = Guid.NewGuid().ToString(), // Replace with proper token generation logic
                        CreatedAt = DateTime.UtcNow,
                    };
                    await context.PersonalAccessTokens.AddAsync(token);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}