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
                    new Role { Id = Guid.NewGuid().ToString(), Name = "Manager", Description = "Manager role with elevated access", CreatedAt = DateTime.UtcNow },
                    new Role { Id = Guid.NewGuid().ToString(), Name = "Supervisor", Description = "Supervisor role with oversight capabilities", CreatedAt = DateTime.UtcNow },
                    new Role { Id = Guid.NewGuid().ToString(), Name = "Analyst", Description = "Analyst role with data access", CreatedAt = DateTime.UtcNow }
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
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "shifts.assign", Description = "Permission to assign shifts", CreatedAt = DateTime.UtcNow },
                    //Roles Permissions
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "roles.view", Description = "Permission to view roles", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "roles.manage", Description = "Permission to manage roles", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "roles.assign", Description = "Permission to assign roles", CreatedAt = DateTime.UtcNow },
                    //Users Permissions
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "users.view", Description = "Permission to view users", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "users.manage", Description = "Permission to manage users", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "users.assign", Description = "Permission to assign users", CreatedAt = DateTime.UtcNow },
                    //Reports Permissions
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "reports.view", Description = "Permission to view reports", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "reports.manage", Description = "Permission to manage reports", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "reports.assign", Description = "Permission to assign reports", CreatedAt = DateTime.UtcNow },
                    //Permissions Permissions
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "permissions.view", Description = "Permission to view permissions", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "permissions.manage", Description = "Permission to manage permissions", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid().ToString(), Name = "permissions.assign", Description = "Permission to assign permissions", CreatedAt = DateTime.UtcNow }
                    //end
                };
                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var users = new[]
                {
                    // Admin role users (3)
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Admin", LastName = "One", Email = "admin1@userservice.com", MobileNumber = "1234567890", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Admin", LastName = "Two", Email = "admin2@userservice.com", MobileNumber = "1234567891", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Admin", LastName = "Three", Email = "admin3@userservice.com", MobileNumber = "1234567892", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    // User role users (3)
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe", Email = "john.doe@userservice.com", MobileNumber = "1234567893", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Jane", LastName = "Smith", Email = "jane.smith@userservice.com", MobileNumber = "1234567894", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@userservice.com", MobileNumber = "1234567895", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    // Manager role users (3)
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Alice", LastName = "Brown", Email = "alice.brown@userservice.com", MobileNumber = "1234567896", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Charlie", LastName = "Davis", Email = "charlie.davis@userservice.com", MobileNumber = "1234567897", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Emma", LastName = "Wilson", Email = "emma.wilson@userservice.com", MobileNumber = "1234567898", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    // Supervisor role users (3)
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "David", LastName = "Moore", Email = "david.moore@userservice.com", MobileNumber = "1234567899", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Sarah", LastName = "Taylor", Email = "sarah.taylor@userservice.com", MobileNumber = "1234567900", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Michael", LastName = "Anderson", Email = "michael.anderson@userservice.com", MobileNumber = "1234567901", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    // Analyst role users (3)
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Laura", LastName = "Martinez", Email = "laura.martinez@userservice.com", MobileNumber = "1234567902", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "James", LastName = "Garcia", Email = "james.garcia@userservice.com", MobileNumber = "1234567903", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@userservice.com", MobileNumber = "1234567904", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    // Additional User role users (5)
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Tom", LastName = "Lee", Email = "tom.lee@userservice.com", MobileNumber = "1234567905", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Olivia", LastName = "Hernandez", Email = "olivia.hernandez@userservice.com", MobileNumber = "1234567906", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "William", LastName = "Lopez", Email = "william.lopez@userservice.com", MobileNumber = "1234567907", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Sophia", LastName = "Gonzalez", Email = "sophia.gonzalez@userservice.com", MobileNumber = "1234567908", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Daniel", LastName = "Perez", Email = "daniel.perez@userservice.com", MobileNumber = "1234567909", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow }
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
                var roles = await context.Roles.ToListAsync();
                var users = await context.Users.ToListAsync();

                var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
                var userRole = roles.FirstOrDefault(r => r.Name == "User");
                var managerRole = roles.FirstOrDefault(r => r.Name == "Manager");
                var supervisorRole = roles.FirstOrDefault(r => r.Name == "Supervisor");
                var analystRole = roles.FirstOrDefault(r => r.Name == "Analyst");

                var userRoles = new List<UserRole>();

                // Assign Admin role to 3 users
                var adminUsers = users.Where(u => u.Email.Contains("admin")).ToList();
                if (adminRole != null)
                {
                    userRoles.AddRange(adminUsers.Select(u => new UserRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = u.Id,
                        RoleId = adminRole.Id,
                        CreatedAt = DateTime.UtcNow
                    }));
                }

                // Assign User role to 3 users + 5 additional
                var userRoleUsers = users.Where(u => u.Email.Contains("john.doe") || u.Email.Contains("jane.smith") || u.Email.Contains("bob.johnson") ||
                                                    u.Email.Contains("tom.lee") || u.Email.Contains("olivia.hernandez") || u.Email.Contains("william.lopez") ||
                                                    u.Email.Contains("sophia.gonzalez") || u.Email.Contains("daniel.perez")).ToList();
                if (userRole != null)
                {
                    userRoles.AddRange(userRoleUsers.Select(u => new UserRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = u.Id,
                        RoleId = userRole.Id,
                        CreatedAt = DateTime.UtcNow
                    }));
                }

                // Assign Manager role to 3 users
                var managerUsers = users.Where(u => u.Email.Contains("alice.brown") || u.Email.Contains("charlie.davis") || u.Email.Contains("emma.wilson")).ToList();
                if (managerRole != null)
                {
                    userRoles.AddRange(managerUsers.Select(u => new UserRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = u.Id,
                        RoleId = managerRole.Id,
                        CreatedAt = DateTime.UtcNow
                    }));
                }

                // Assign Supervisor role to 3 users
                var supervisorUsers = users.Where(u => u.Email.Contains("david.moore") || u.Email.Contains("sarah.taylor") || u.Email.Contains("michael.anderson")).ToList();
                if (supervisorRole != null)
                {
                    userRoles.AddRange(supervisorUsers.Select(u => new UserRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = u.Id,
                        RoleId = supervisorRole.Id,
                        CreatedAt = DateTime.UtcNow
                    }));
                }

                // Assign Analyst role to 3 users
                var analystUsers = users.Where(u => u.Email.Contains("laura.martinez") || u.Email.Contains("james.garcia") || u.Email.Contains("emily.rodriguez")).ToList();
                if (analystRole != null)
                {
                    userRoles.AddRange(analystUsers.Select(u => new UserRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = u.Id,
                        RoleId = analystRole.Id,
                        CreatedAt = DateTime.UtcNow
                    }));
                }

                if (userRoles.Any())
                {
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
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin1@userservice.com");
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