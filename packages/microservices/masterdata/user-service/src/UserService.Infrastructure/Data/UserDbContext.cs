using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using System.Text.Json;

namespace UserService.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<OrganizationUser> OrganizationUsers { get; set; }
    public DbSet<UserInvitation> UserInvitations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.Language).HasMaxLength(10);

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.OrganizationId).HasMaxLength(36);

            entity.HasIndex(e => new { e.Name, e.OrganizationId }).IsUnique();
        });

        // Configure Permission entity
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Resource).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
        });

        // Configure UserRole entity
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.RoleId).IsRequired();
            entity.Property(e => e.OrganizationId).HasMaxLength(36);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.RoleId, e.OrganizationId }).IsUnique();
        });

        // Configure RolePermission entity
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleId).IsRequired();
            entity.Property(e => e.PermissionId).IsRequired();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        });

        // Configure UserSession entity
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RefreshToken).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.HasIndex(e => e.RefreshToken).IsUnique();
        });

        // Configure UserProfile entity
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.ZipCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.LinkedIn).HasMaxLength(500);
            entity.Property(e => e.Twitter).HasMaxLength(500);

            // Configure JSON properties
            entity.Property(e => e.Settings)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!));

            entity.Property(e => e.Preferences)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!));

            entity.HasOne(e => e.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Configure OrganizationUser entity
        modelBuilder.Entity<OrganizationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(36);
            entity.Property(e => e.DepartmentId).HasMaxLength(36);
            entity.Property(e => e.EmployeeId).HasMaxLength(50);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.InvitedBy).HasMaxLength(36);

            entity.HasOne(e => e.User)
                .WithMany(u => u.OrganizationUsers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.OrganizationId }).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.EmployeeId }).IsUnique();
        });

        // Configure UserInvitation entity
        modelBuilder.Entity<UserInvitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(36);
            entity.Property(e => e.InvitedBy).IsRequired().HasMaxLength(36);
            entity.Property(e => e.RoleId).HasMaxLength(36);
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.AcceptedBy).HasMaxLength(36);
            entity.Property(e => e.Message).HasMaxLength(1000);

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => new { e.Email, e.OrganizationId });
        });

        // Add global query filter for soft deletes
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserSession>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrganizationUser>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserInvitation>().HasQueryFilter(e => !e.IsDeleted);

        // Seed default data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed default permissions
        var permissions = new List<Permission>
        {
            new Permission { Id = Guid.NewGuid().ToString(), Name = "user.read", Description = "Read user information", Resource = "User", Action = "Read", Scope = PermissionScope.System },
            new Permission { Id = Guid.NewGuid().ToString(), Name = "user.write", Description = "Create and update users", Resource = "User", Action = "Write", Scope = PermissionScope.System },
            new Permission { Id = Guid.NewGuid().ToString(), Name = "user.delete", Description = "Delete users", Resource = "User", Action = "Delete", Scope = PermissionScope.System },
            new Permission { Id = Guid.NewGuid().ToString(), Name = "role.read", Description = "Read role information", Resource = "Role", Action = "Read", Scope = PermissionScope.System },
            new Permission { Id = Guid.NewGuid().ToString(), Name = "role.write", Description = "Create and update roles", Resource = "Role", Action = "Write", Scope = PermissionScope.System },
            new Permission { Id = Guid.NewGuid().ToString(), Name = "role.delete", Description = "Delete roles", Resource = "Role", Action = "Delete", Scope = PermissionScope.System },
            new Permission { Id = Guid.NewGuid().ToString(), Name = "organization.read", Description = "Read organization information", Resource = "Organization", Action = "Read", Scope = PermissionScope.Organization },
            new Permission { Id = Guid.NewGuid().ToString(), Name = "organization.write", Description = "Update organization information", Resource = "Organization", Action = "Write", Scope = PermissionScope.Organization },
            new Permission { Id = Guid.NewGuid().ToString(), Name = "organization.admin", Description = "Full organization administration", Resource = "Organization", Action = "Admin", Scope = PermissionScope.Organization }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);

        // Seed default roles
        var systemAdminRole = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "System Administrator",
            Description = "Full system administration privileges",
            Type = RoleType.System,
            IsDefault = false,
            IsActive = true
        };

        var userRole = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "User",
            Description = "Basic user privileges",
            Type = RoleType.System,
            IsDefault = true,
            IsActive = true
        };

        var orgAdminRole = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Organization Administrator",
            Description = "Organization administration privileges",
            Type = RoleType.Organization,
            IsDefault = false,
            IsActive = true
        };

        var orgUserRole = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Organization User",
            Description = "Standard organization user privileges",
            Type = RoleType.Organization,
            IsDefault = true,
            IsActive = true
        };

        modelBuilder.Entity<Role>().HasData(systemAdminRole, userRole, orgAdminRole, orgUserRole);

        // Seed role permissions
        var rolePermissions = new List<RolePermission>();

        // System Admin gets all permissions
        foreach (var permission in permissions)
        {
            rolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid().ToString(),
                RoleId = systemAdminRole.Id,
                PermissionId = permission.Id
            });
        }

        // User gets basic read permissions
        var userPermissions = permissions.Where(p => p.Action == "Read").ToList();
        foreach (var permission in userPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid().ToString(),
                RoleId = userRole.Id,
                PermissionId = permission.Id
            });
        }

        modelBuilder.Entity<RolePermission>().HasData(rolePermissions);
    }
}