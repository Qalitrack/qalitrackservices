using Microsoft.EntityFrameworkCore;
using System.Reflection;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Data;

public class UserServiceDbContext : DbContext
{
    public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Suppress pending model changes warning
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    // DbSet properties
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Shift> Shifts { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<UserShift> UserShifts { get; set; } = null!;
    public DbSet<PersonalAccessToken> PersonalAccessTokens { get; set; } = null!;
    public DbSet<PasswordPolicy> PasswordPolicies { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure entity relationships and constraints
        ConfigureUser(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigurePermission(modelBuilder);
        ConfigureRolePermission(modelBuilder);
        ConfigureUserRole(modelBuilder);
        ConfigureShift(modelBuilder);
        ConfigureUserShift(modelBuilder);
        ConfigurePersonalAccessToken(modelBuilder);
        ConfigurePasswordPolicy(modelBuilder);

        // Configure DateTime properties globally for PostgreSQL
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("TIMESTAMPTZ");
                }

                if (property.ClrType == typeof(bool))
                {
                    property.SetColumnType("BOOLEAN");
                }

                if (property.ClrType == typeof(TimeSpan) || property.ClrType == typeof(TimeSpan?))
                {
                    property.SetColumnType("TIME");
                }
            }
        }

        // Add global query filter for soft deletes
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Shift>().HasQueryFilter(e => !e.IsDeleted);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Password).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(false);
            entity.Property(e => e.IsFirstLogin).HasDefaultValue(false);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

        });
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.IsDeleted);
        });
    }

    private static void ConfigurePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsDeleted);
        });
    }

    private static void ConfigureRolePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(rp => rp.IsDeleted);
        });
    }

    private static void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(ur => ur.AssignedAt).HasDefaultValueSql("NOW()");
            entity.Property(ur => ur.IsDeleted);
        });
    }

    private static void ConfigureShift(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            entity.Property(e => e.Description).HasMaxLength(500);

            // Configure IsActive as a computed property (not mapped to database)
            entity.Ignore(e => e.IsActive);
            entity.Property(e => e.IsDeleted);
        });
    }

    private static void ConfigureUserShift(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserShift>(entity =>
        {
            entity.HasKey(us => new { us.UserId, us.ShiftId });

            entity.HasOne(us => us.User)
                .WithMany(u => u.UserShifts)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(us => us.Shift)
                .WithMany(s => s.UserShifts)
                .HasForeignKey(us => us.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(us => us.AssignedAt).HasDefaultValueSql("NOW()");
            entity.Property(us => us.IsDeleted);
        });
    }

    private static void ConfigurePersonalAccessToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonalAccessToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            entity.Property(e => e.IsDeleted);

            entity.HasOne(pat => pat.User)
                .WithMany(u => u.PersonalAccessTokens)
                .HasForeignKey(pat => pat.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    //lets configure password policy
    private static void ConfigurePasswordPolicy(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordPolicy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MinimumLength).HasDefaultValue(8);
            entity.Property(e => e.RequireUppercase).HasDefaultValue(true);
            entity.Property(e => e.RequireLowercase).HasDefaultValue(true);
            entity.Property(e => e.RequireDigit).HasDefaultValue(true);
            entity.Property(e => e.RequireSpecialCharacter).HasDefaultValue(true);
            entity.Property(e => e.MaxAgeDays).HasDefaultValue(90);
        });
    }
}