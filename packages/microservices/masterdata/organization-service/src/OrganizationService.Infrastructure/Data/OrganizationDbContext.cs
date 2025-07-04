using Microsoft.EntityFrameworkCore;
using OrganizationService.Core.Entities;
using System.Text.Json;

namespace OrganizationService.Infrastructure.Data;

public class OrganizationDbContext : DbContext
{
    public OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationUser> OrganizationUsers { get; set; }
    public DbSet<OrganizationSettings> OrganizationSettings { get; set; }
    public DbSet<OrganizationBilling> OrganizationBilling { get; set; }
    public DbSet<OrganizationSubscription> OrganizationSubscriptions { get; set; }
    public DbSet<OrganizationDepartment> OrganizationDepartments { get; set; }
    public DbSet<OrganizationLocation> OrganizationLocations { get; set; }
    public DbSet<OrganizationHierarchy> OrganizationHierarchies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Organization Configuration
        modelBuilder.Entity<Organization>()
            .HasKey(o => o.Id);

        modelBuilder.Entity<Organization>()
            .HasIndex(o => o.Code)
            .IsUnique();

        modelBuilder.Entity<Organization>()
            .HasIndex(o => o.ContactEmail);

        modelBuilder.Entity<Organization>()
            .HasMany(o => o.ChildOrganizations)
            .WithOne(o => o.ParentOrganization)
            .HasForeignKey(o => o.ParentOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Organization User Configuration
        modelBuilder.Entity<OrganizationUser>()
            .HasKey(ou => ou.Id);

        modelBuilder.Entity<OrganizationUser>()
            .HasIndex(ou => new { ou.OrganizationId, ou.Email })
            .IsUnique();

        modelBuilder.Entity<OrganizationUser>()
            .HasOne(ou => ou.Organization)
            .WithMany(o => o.OrganizationUsers)
            .HasForeignKey(ou => ou.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrganizationUser>()
            .Property(ou => ou.Permissions)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());

        // Organization Settings Configuration
        modelBuilder.Entity<OrganizationSettings>()
            .HasKey(os => os.Id);

        modelBuilder.Entity<OrganizationSettings>()
            .HasIndex(os => os.OrganizationId)
            .IsUnique();

        modelBuilder.Entity<OrganizationSettings>()
            .HasOne(os => os.Organization)
            .WithOne(o => o.Settings)
            .HasForeignKey<OrganizationSettings>(os => os.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrganizationSettings>()
            .Property(os => os.CustomSettings)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, object>());

        // Organization Billing Configuration
        modelBuilder.Entity<OrganizationBilling>()
            .HasKey(ob => ob.Id);

        modelBuilder.Entity<OrganizationBilling>()
            .HasIndex(ob => ob.OrganizationId)
            .IsUnique();

        modelBuilder.Entity<OrganizationBilling>()
            .HasOne(ob => ob.Organization)
            .WithOne(o => o.Billing)
            .HasForeignKey<OrganizationBilling>(ob => ob.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Organization Subscription Configuration
        modelBuilder.Entity<OrganizationSubscription>()
            .HasKey(os => os.Id);

        modelBuilder.Entity<OrganizationSubscription>()
            .HasOne(os => os.Organization)
            .WithMany(o => o.Subscriptions)
            .HasForeignKey(os => os.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrganizationSubscription>()
            .Property(os => os.FeatureLimits)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, int>());

        modelBuilder.Entity<OrganizationSubscription>()
            .Property(os => os.Features)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());

        // Organization Department Configuration
        modelBuilder.Entity<OrganizationDepartment>()
            .HasKey(od => od.Id);

        modelBuilder.Entity<OrganizationDepartment>()
            .HasIndex(od => new { od.OrganizationId, od.Code })
            .IsUnique();

        modelBuilder.Entity<OrganizationDepartment>()
            .HasOne(od => od.Organization)
            .WithMany(o => o.Departments)
            .HasForeignKey(od => od.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrganizationDepartment>()
            .HasMany(od => od.ChildDepartments)
            .WithOne(od => od.ParentDepartment)
            .HasForeignKey(od => od.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrganizationDepartment>()
            .HasOne(od => od.Manager)
            .WithMany()
            .HasForeignKey(od => od.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Organization Location Configuration
        modelBuilder.Entity<OrganizationLocation>()
            .HasKey(ol => ol.Id);

        modelBuilder.Entity<OrganizationLocation>()
            .HasIndex(ol => new { ol.OrganizationId, ol.Code })
            .IsUnique();

        modelBuilder.Entity<OrganizationLocation>()
            .HasOne(ol => ol.Organization)
            .WithMany(o => o.Locations)
            .HasForeignKey(ol => ol.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Organization Hierarchy Configuration
        modelBuilder.Entity<OrganizationHierarchy>()
            .HasKey(oh => oh.Id);

        modelBuilder.Entity<OrganizationHierarchy>()
            .HasIndex(oh => new { oh.ParentOrganizationId, oh.ChildOrganizationId })
            .IsUnique();

        modelBuilder.Entity<OrganizationHierarchy>()
            .HasOne(oh => oh.ParentOrganization)
            .WithMany()
            .HasForeignKey(oh => oh.ParentOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrganizationHierarchy>()
            .HasOne(oh => oh.ChildOrganization)
            .WithMany()
            .HasForeignKey(oh => oh.ChildOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure decimal precision
        modelBuilder.Entity<OrganizationBilling>()
            .Property(ob => ob.CurrentBalance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrganizationBilling>()
            .Property(ob => ob.CreditLimit)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrganizationSubscription>()
            .Property(os => os.PlanPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrganizationSubscription>()
            .Property(os => os.DiscountAmount)
            .HasPrecision(18, 2);
    }
}