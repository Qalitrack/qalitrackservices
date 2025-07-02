using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserModule.Data;
using UserModule.Models;
using System;
using System.Linq;

namespace UserModule.Data
{
    public class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>());
            }
        }

        private static void SeedData(AppDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Database context is null.");
            }

            // Seed Roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Admin",
                        Description = "Administrator role with full access",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "User",
                        Description = "Standard user role with limited access",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }

            // Seed Permissions
            if (!context.Permissions.Any())
            {
                context.Permissions.AddRange(
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = "ManageUsers",
                        Description = "Permission to manage user accounts",
                        Category = "Administration",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = "ViewReports",
                        Description = "Permission to view reports",
                        Category = "General",
                        CreatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
                var userRole = context.Roles.FirstOrDefault(r => r.Name == "User");

                if (adminRole == null || userRole == null)
                {
                    throw new InvalidOperationException("Roles must be seeded before users.");
                }

                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = "hashedpassword123", // Placeholder: Use proper hashing (e.g., BCrypt) in production
                    FirstName = "Admin",
                    LastName = "User",
                    Department = "IT",
                    IsActive = true,
                    LoginStatus = false,
                    HasAssignedShift = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var standardUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "user",
                    Email = "user@example.com",
                    PasswordHash = "hashedpassword456", // Placeholder: Use proper hashing
                    FirstName = "Standard",
                    LastName = "User",
                    Department = "HR",
                    IsActive = true,
                    LoginStatus = false,
                    HasAssignedShift = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id
                };

                context.Users.AddRange(adminUser, standardUser);
                context.SaveChanges();

                // Seed UserRoles
                context.UserRoles.AddRange(
                    new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id,
                        AssignedAt = DateTime.UtcNow
                    },
                    new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = standardUser.Id,
                        RoleId = userRole.Id,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = adminUser.Id
                    }
                );

                // Seed RolePermissions
                var manageUsersPermission = context.Permissions.FirstOrDefault(p => p.Name == "ManageUsers");
                var viewReportsPermission = context.Permissions.FirstOrDefault(p => p.Name == "ViewReports");

                if (manageUsersPermission == null || viewReportsPermission == null)
                {
                    throw new InvalidOperationException("Permissions must be seeded before role permissions.");
                }

                context.RolePermissions.AddRange(
                    new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = adminRole.Id,
                        PermissionId = manageUsersPermission.Id,
                        AssignedAt = DateTime.UtcNow
                    },
                    new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = adminRole.Id,
                        PermissionId = viewReportsPermission.Id,
                        AssignedAt = DateTime.UtcNow
                    },
                    new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = userRole.Id,
                        PermissionId = viewReportsPermission.Id,
                        AssignedAt = DateTime.UtcNow
                    }
                );

                context.SaveChanges();
            }

            // Seed Shifts (optional, if needed)
            if (!context.Shifts.Any())
            {
                context.Shifts.AddRange(
                    new Shift
                    {
                        Id = Guid.NewGuid(),
                        Name = "Morning Shift",
                        Description = "Morning work shift",
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(16, 0, 0),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }
        }
    }
}