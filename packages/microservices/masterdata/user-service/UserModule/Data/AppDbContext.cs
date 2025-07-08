using Microsoft.EntityFrameworkCore;
using UserModule.Models;

namespace UserModule.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<PersonalAccessToken> PersonalAccessTokens { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Shift> Shifts { get; set; } = null!;
    public DbSet<UserShift> UserShifts { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }
    public DbSet<BackupRecord> BackupRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity relationships
       // Configure User entity relationships
        modelBuilder.Entity<User>()
            .HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserShift relationships
        modelBuilder.Entity<UserShift>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserShifts)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<UserShift>()
            .HasOne(us => us.Shift)
            .WithMany(s => s.UserShifts)
            .HasForeignKey(us => us.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<UserShift>()
            .HasOne(us => us.AssignedByUser)
            .WithMany()
            .HasForeignKey(us => us.AssignedBy)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<UserShift>()
            .HasOne(us => us.CreatedByUser)
            .WithMany()
            .HasForeignKey(us => us.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<UserShift>()
            .HasOne(us => us.UpdatedByUser)
            .WithMany()
            .HasForeignKey(us => us.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Ensure User has a collection of UserShifts
        modelBuilder.Entity<User>()
            .HasMany(u => u.UserShifts)
            .WithOne(us => us.User)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Ensure Shift has a collection of UserShifts
        modelBuilder.Entity<Shift>()
            .HasMany(s => s.UserShifts)
            .WithOne(us => us.Shift)
            .HasForeignKey(us => us.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.AuditLogs)
            .WithOne(al => al.User)
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasMany(u => u.UserActivities)
            .WithOne(ua => ua.User)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure self-referencing relationships for CreatedByUser and UpdatedByUser
        modelBuilder.Entity<User>()
            .HasOne(u => u.CreatedByUser)
            .WithMany(u => u.CreatedUsers)
            .HasForeignKey(u => u.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(u => u.UpdatedByUser)
            .WithMany(u => u.UpdatedUsers)
            .HasForeignKey(u => u.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Role entity relationships
        modelBuilder.Entity<Role>()
            .HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Permission entity relationships
        modelBuilder.Entity<Permission>()
            .HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Shift entity relationships
        modelBuilder.Entity<Shift>()
            .HasMany(s => s.UserShifts)
            .WithOne(us => us.Shift)
            .HasForeignKey(us => us.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserShift unique constraint
        modelBuilder.Entity<UserShift>()
            .HasIndex(us => us.UserId)
            .IsUnique();

        // Configure BackupRecord entity relationships
        modelBuilder.Entity<BackupRecord>()
            .HasOne(br => br.CreatedByUser)
            .WithMany()
            .HasForeignKey(br => br.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<BackupRecord>()
            .HasOne(br => br.ParentBackup)
            .WithMany(br => br.ChildBackups)
            .HasForeignKey(br => br.ParentBackupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}