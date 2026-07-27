using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UserService.Core.Enums;
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
            // Ensure system roles exist. Per-role existence check (not gated on the
            // whole table being empty) so adding/renaming a system role here still
            // takes effect on an already-seeded database, not just a fresh install.
            var systemRoleDefs = new (string Name, string Description)[]
            {
                ("Admin", "Administrator role with full access"),
                ("Manager", "Manager role with elevated access"),
                ("Supervisor", "Supervisor role with oversight capabilities"),
                ("Operator", "Frontline weighbridge operator role — factory floor operations only"),
            };
            foreach (var (name, description) in systemRoleDefs)
            {
                var roleExists = await context.Roles.AnyAsync(r => r.Name == name);
                if (!roleExists)
                {
                    await context.Roles.AddAsync(new Role
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = name,
                        Description = description,
                        IsSystem = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();

            // Seed Permissions
            // Ensure system permissions exist. Per-permission existence check (like
            // the roles above) so adding a new permission here — e.g. audit.view —
            // still takes effect on an already-seeded database.
            var systemPermissionDefs = new (string Name, string Description)[]
            {
                ("CreateUser", "Permission to create users"),
                ("EditUser", "Permission to edit users"),
                ("DeleteUser", "Permission to delete users"),
                ("ViewReports", "Permission to view reports"),
                ("shifts.view", "Permission to view shifts"),
                ("shifts.manage", "Permission to manage shifts"),
                ("shifts.assign", "Permission to assign shifts"),
                ("roles.view", "Permission to view roles"),
                ("roles.manage", "Permission to manage roles"),
                ("roles.assign", "Permission to assign roles"),
                ("users.view", "Permission to view users"),
                ("users.manage", "Permission to manage users"),
                ("users.assign", "Permission to assign users"),
                ("users.delete", "Permission to delete users"),
                ("users.create", "Permission to create users"),
                ("reports.view", "Permission to view reports"),
                ("reports.manage", "Permission to manage reports"),
                ("reports.assign", "Permission to assign reports"),
                ("permissions.view", "Permission to view permissions"),
                ("permissions.manage", "Permission to manage permissions"),
                ("permissions.assign", "Permission to assign permissions"),
                ("audit.view", "Permission to view audit logs"),
            };
            foreach (var (name, description) in systemPermissionDefs)
            {
                var permissionExists = await context.Permissions.AnyAsync(p => p.Name == name);
                if (!permissionExists)
                {
                    await context.Permissions.AddAsync(new Permission
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = name,
                        Description = description,
                        IsSystem = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();

            // Backfill IsSystem on databases that were seeded before this flag existed,
            // so upgrading doesn't leave already-seeded Admin/roles.manage etc. editable.
            var systemRoleNames = new[] { "Admin", "Manager", "Supervisor", "Operator" };
            var rolesToProtect = await context.Roles
                .Where(r => systemRoleNames.Contains(r.Name) && !r.IsSystem)
                .ToListAsync();
            foreach (var role in rolesToProtect) role.IsSystem = true;

            var systemPermissionNames = new[]
            {
                "CreateUser", "EditUser", "DeleteUser", "ViewReports",
                "shifts.view", "shifts.manage", "shifts.assign",
                "roles.view", "roles.manage", "roles.assign",
                "users.view", "users.manage", "users.assign", "users.delete", "users.create",
                "reports.view", "reports.manage", "reports.assign",
                "permissions.view", "permissions.manage", "permissions.assign"
            };
            var permissionsToProtect = await context.Permissions
                .Where(p => systemPermissionNames.Contains(p.Name) && !p.IsSystem)
                .ToListAsync();
            foreach (var permission in permissionsToProtect) permission.IsSystem = true;

            if (rolesToProtect.Any() || permissionsToProtect.Any())
            {
                await context.SaveChangesAsync();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var users = new[]
                {
                    // Admin role users (3)
                    new User { Id = Guid.NewGuid().ToString(), FirstName = "Admin", LastName = "One", Email = "joshuaiska@gmail.com", MobileNumber = "1234567890", Password = BCrypt.Net.BCrypt.HashPassword("password"), IsActive = true, IsFirstLogin = false, CreatedAt = DateTime.UtcNow },
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

            // Seed Shifts and their Instances
            if (!context.Shifts.Any())
            {
                var now = DateTime.UtcNow;
                var currentDate = DateTime.UtcNow.Date;

                
                // Create two shifts
                var morningShift = new Shift
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Morning Shift",
                    Description = "Standard morning working hours",
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(16, 0, 0),
                    Mode = ShiftMode.Open,
                    StartDate = currentDate,
                    EndDate = null,
                    Status = ShiftStatus.Active,
                    Type = ShiftType.Recurring,
                    RequiredStaffCount = 5,
                    RecurrenceType = RecurrenceType.Daily,
                    RecurrenceInterval = 1,
                    CustomDaysJson = JsonSerializer.Serialize(new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }),
                    ExceptionDatesJson = JsonSerializer.Serialize(Array.Empty<DateTime>()),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                var eveningShift = new Shift 
                { 
                    Id = Guid.NewGuid().ToString(),
                    Name = "Evening Shift", 
                    Description = "Standard evening working hours",
                    StartTime = new TimeSpan(16, 0, 0), 
                    EndTime = new TimeSpan(0, 0, 0), // Midnight
                    Mode = ShiftMode.Open,
                    StartDate = currentDate,
                    EndDate = null,
                    Status = ShiftStatus.Active,
                    Type = ShiftType.Recurring,
                    RequiredStaffCount = 5,
                    RecurrenceType = RecurrenceType.Daily,
                    RecurrenceInterval = 1,
                    CustomDaysJson = JsonSerializer.Serialize(new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }),
                    ExceptionDatesJson = JsonSerializer.Serialize(Array.Empty<DateTime>()),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                // Add shifts to context
                context.Shifts.AddRange(morningShift, eveningShift);
                await context.SaveChangesAsync();

                // Create shift instances for the next 7 days
                var shiftInstances = new List<ShiftInstance>();
                
                for (int i = 0; i < 7; i++)
                {
                    var date = currentDate.AddDays(i);
                    var dayOfWeek = date.DayOfWeek;
                    
                    // Skip weekends
                    if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // Morning shift instance
                    var morningStart = date.Add(morningShift.StartTime);
                    var morningEnd = date.Add(morningShift.EndTime);
                    if (morningShift.EndTime <= morningShift.StartTime) // Handle overnight shifts
                        morningEnd = morningEnd.AddDays(1);

                    shiftInstances.Add(new ShiftInstance
                    {
                        Id = Guid.NewGuid().ToString(),
                        ShiftId = morningShift.Id,
                        ScheduledDate = date,
                        ScheduledStartTime = morningStart,
                        ScheduledEndTime = morningEnd,
                        Status = ShiftInstanceStatus.Scheduled,
                        CreatedAt = now,
                        UpdatedAt = now
                    });

                    // Evening shift instance
                    var eveningStart = date.Add(eveningShift.StartTime);
                    var eveningEnd = date.Add(eveningShift.EndTime);
                    if (eveningShift.EndTime <= eveningShift.StartTime) // Handle overnight shifts
                        eveningEnd = eveningEnd.AddDays(1);

                    shiftInstances.Add(new ShiftInstance
                    {
                        Id = Guid.NewGuid().ToString(),
                        ShiftId = eveningShift.Id,
                        ScheduledDate = date,
                        ScheduledStartTime = eveningStart,
                        ScheduledEndTime = eveningEnd,
                        Status = ShiftInstanceStatus.Scheduled,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                await context.ShiftInstances.AddRangeAsync(shiftInstances);
                await context.SaveChangesAsync();
            }

            // Ensure Admin has every permission. Per-pair existence check (not
            // gated on the whole table being empty) so a newly added permission —
            // e.g. audit.view — actually reaches Admin on an already-seeded database,
            // not just a fresh install.
            var adminRoleForPermissions = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRoleForPermissions != null)
            {
                var allPermissions = await context.Permissions.ToListAsync();
                var existingAdminPermissionIds = (await context.RolePermissions
                    .Where(rp => rp.RoleId == adminRoleForPermissions.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync())
                    .ToHashSet();

                var missingRolePermissions = allPermissions
                    .Where(p => !existingAdminPermissionIds.Contains(p.Id))
                    .Select(p => new RolePermission
                    {
                        Id = Guid.NewGuid().ToString(),
                        RoleId = adminRoleForPermissions.Id,
                        PermissionId = p.Id,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                if (missingRolePermissions.Any())
                {
                    await context.RolePermissions.AddRangeAsync(missingRolePermissions);
                    await context.SaveChangesAsync();
                }
            }

            // Seed UserRoles
            if (!context.UserRoles.Any())
            {
                var roles = await context.Roles.ToListAsync();
                var users = await context.Users.ToListAsync();

                var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
                var managerRole = roles.FirstOrDefault(r => r.Name == "Manager");
                var supervisorRole = roles.FirstOrDefault(r => r.Name == "Supervisor");

                var userRoles = new List<UserRole>();

                // Assign Admin role to 3 users
                var adminUsers = users.Where(u => u.Email.Contains("admin")).ToList();
                // Add joshuiska@gmail.com to admin users if they exist
                var joshuaUser = users.FirstOrDefault(u => u.Email == "joshuiska@gmail.com");
                if (joshuaUser != null && !adminUsers.Contains(joshuaUser))
                {
                    adminUsers.Add(joshuaUser);
                }
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

                // Assign Manager role — the baseline non-admin role — to the general
                // staff seed users plus the 3 originally-manager-labelled ones.
                var managerUsers = users.Where(u => u.Email.Contains("alice.brown") || u.Email.Contains("charlie.davis") || u.Email.Contains("emma.wilson") ||
                                                    u.Email.Contains("john.doe") || u.Email.Contains("jane.smith") || u.Email.Contains("bob.johnson") ||
                                                    u.Email.Contains("tom.lee") || u.Email.Contains("olivia.hernandez") || u.Email.Contains("william.lopez") ||
                                                    u.Email.Contains("sophia.gonzalez") || u.Email.Contains("daniel.perez") ||
                                                    u.Email.Contains("laura.martinez") || u.Email.Contains("james.garcia") || u.Email.Contains("emily.rodriguez")).ToList();
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

            // Seed PasswordPolicies
            if (!context.PasswordPolicies.Any())
            {
                var passwordPolicy = new PasswordPolicy
                {
                    Id = Guid.NewGuid().ToString(),
                    MinimumLength = 6,
                    RequireUppercase = false,
                    RequireLowercase = false,
                    RequireDigit = false,
                    RequireSpecialCharacter = false,
                    CreatedAt = DateTime.UtcNow
                };
                await context.PasswordPolicies.AddAsync(passwordPolicy);
                await context.SaveChangesAsync();
            }
        }
        
    }
}