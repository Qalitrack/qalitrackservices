using UserModule.Models;
using BCrypt.Net;

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

            // Seed Permissions
            if (!context.Permissions.Any())
            {
                var permissions = new[] {
                    new Permission { Name = "users.read", Description = "Read user data", Category = "Users" },
                    new Permission { Name = "users.create", Description = "Create users", Category = "Users" },
                    new Permission { Name = "users.update", Description = "Update users", Category = "Users" },
                    new Permission { Name = "users.delete", Description = "Delete users", Category = "Users" },
                    new Permission { Name = "users.assign-roles", Description = "Assign roles to users", Category = "Users" },
                    new Permission { Name = "users.remove-roles", Description = "Remove roles from users", Category = "Users" },
                    new Permission { Name = "users.read-permissions", Description = "Read user permissions", Category = "Users" },
                    new Permission { Name = "roles.read", Description = "Read roles", Category = "Roles" },
                    new Permission { Name = "roles.create", Description = "Create roles", Category = "Roles" },
                    new Permission { Name = "roles.update", Description = "Update roles", Category = "Roles" },
                    new Permission { Name = "roles.delete", Description = "Delete roles", Category = "Roles" },
                    new Permission { Name = "roles.assign-permissions", Description = "Assign permissions to roles", Category = "Roles" },
                    new Permission { Name = "permissions.read", Description = "Read permissions", Category = "Permissions" },
                    new Permission { Name = "permissions.create", Description = "Create permissions", Category = "Permissions" },
                    new Permission { Name = "permissions.update", Description = "Update permissions", Category = "Permissions" },
                    new Permission { Name = "permissions.delete", Description = "Delete permissions", Category = "Permissions" },
                    new Permission { Name = "shifts.read", Description = "Read shifts", Category = "Shifts" },
                    new Permission { Name = "shifts.create", Description = "Create shifts", Category = "Shifts" },
                    new Permission { Name = "shifts.update", Description = "Update shifts", Category = "Shifts" },
                    new Permission { Name = "shifts.delete", Description = "Delete shifts", Category = "Shifts" },
                    new Permission { Name = "shifts.mode.read", Description = "Read shift mode", Category = "Shifts" },
                    new Permission { Name = "shifts.assign-user", Description = "Assign users to shifts", Category = "Shifts" },
                    new Permission { Name = "shifts.remove-user", Description = "Remove users from shifts", Category = "Shifts" },
                    new Permission { Name = "shifts.read-user-shifts", Description = "Read user shifts", Category = "Shifts" },
                    new Permission { Name = "shifts.update-user-shifts", Description = "Update user shifts", Category = "Shifts" },
                    new Permission { Name = "shifts.delete-user-shifts", Description = "Delete user shifts", Category = "Shifts" },
                    new Permission { Name="shifts.mode.update", Description="Update shift mode", Category="Shifts" }
                };

                context.Permissions.AddRange(permissions);
                context.SaveChanges();
            }

            // Seed Roles
            if (!context.Roles.Any())
            {
                var adminRole = new Role { Name = "Admin", Description = "Full system access", IsActive = true };
                var userRole = new Role { Name = "User", Description = "Standard user access", IsActive = true };
                context.Roles.AddRange(adminRole, userRole);
                context.SaveChanges();

                // Assign all permissions to Admin role
                var allPermissions = context.Permissions.ToList();
                foreach (var permission in allPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id
                    });
                }
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
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
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
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
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
                context.SaveChanges();
            } 
            // Seed Shifts (optional, if needed)
            if (!context.Shifts.Any())
            {
                // Add sample shifts here
                var shifts = new[]
                {
                    new Shift { Name = "Shift 1", Description = "Description for Shift 1", IsActive = true , Mode = ShiftMode.NonStrict, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(16, 0, 0), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Shift { Name = "Shift 2", Description = "Description for Shift 2", IsActive = true, Mode = ShiftMode.NonStrict, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(16, 0, 0), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Shift { Name = "Shift 3", Description = "Description for Shift 3", IsActive = true, Mode = ShiftMode.NonStrict, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(16, 0, 0), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    
                };

                context.Shifts.AddRange(shifts);
                context.SaveChanges();
            }
            // Seed Roles
            if (!context.Roles.Any())
            {
                var adminRole = new Role { Name = "Admin", Description = "Full system access", IsActive = true };
                var userRole = new Role { Name = "User", Description = "Standard user access", IsActive = true };
                context.Roles.AddRange(adminRole, userRole);
                context.SaveChanges();

                // Assign all permissions to Admin role - this ensures any newly added permissions are included
                var allPermissions = context.Permissions.ToList();
                var existingRolePermissions = context.RolePermissions
                    .Where(rp => rp.RoleId == adminRole.Id)
                    .Select(rp => rp.PermissionId)
                    .ToList();

                foreach (var permission in allPermissions)
                {
                    if (!existingRolePermissions.Contains(permission.Id))
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = adminRole.Id,
                            PermissionId = permission.Id
                        });
                    }
                }
                context.SaveChanges();
            }
        }
    }
}