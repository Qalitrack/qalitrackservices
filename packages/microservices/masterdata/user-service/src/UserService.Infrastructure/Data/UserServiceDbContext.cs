using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data.Configurations;

namespace UserService.Infrastructure.Data;

public class UserServiceDbContext : DbContext
{
    public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : base(options)
    {
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
    public DbSet<ShiftInstance> ShiftInstances { get; set; } = null!;
    public DbSet<ShiftAttendance> ShiftAttendances { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<EmailSettings> EmailSettings { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Suppress specific warnings
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(CoreEventId.MultipleNavigationProperties));
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure query splitting behavior for EF Core 9.0
        modelBuilder.HasDefaultSchema("users");
        
        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Configure entity types as needed
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Clear any query filters if needed
            entityType.SetQueryFilter(null);
        }

// Configure common properties for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configure DateTime properties for PostgreSQL
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("TIMESTAMPTZ");
                }
                else if (property.ClrType == typeof(bool))
                {
                    property.SetColumnType("BOOLEAN");
                }
                else if (property.ClrType == typeof(TimeSpan) || property.ClrType == typeof(TimeSpan?))
                {
                    property.SetColumnType("TIME");
                }
            }
        }
    }
}
