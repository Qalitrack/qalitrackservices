using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Masterdata.Core.Entities;

namespace Masterdata.Infrastructure.Data;

public class MasterdataDbContext : DbContext
{
    public MasterdataDbContext(DbContextOptions<MasterdataDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Transporter> Transporters { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<Sacco> Saccos { get; set; }
    public DbSet<Organisation> Organisations { get; set; }
    public DbSet<Weighbridge> Weighbridges { get; set; }
    public DbSet<AxleConfiguration> AxleConfigurations { get; set; }
    public DbSet<Affiliation> Affiliations { get; set; }
    public DbSet<Owner> Owners { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply global query filter for soft deletes to all BaseEntity-derived entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            // Create a single parameter instance to use in both places
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(property), parameter);
            
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        // Vehicle configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");
            entity.Property(e => e.SupplierId).HasColumnType("text");
            entity.Property(e => e.TransporterId).HasColumnType("text");
            entity.Property(e => e.DriverId).HasColumnType("text");
            entity.Property(e => e.AxleConfigurationId).HasColumnType("text");
            entity.Property(e => e.OwnerId).HasColumnType("text");

            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            entity.HasOne(e => e.Supplier).WithMany(s => s.Vehicles).HasForeignKey(e => e.SupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Transporter).WithMany(t => t.Vehicles).HasForeignKey(e => e.TransporterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Driver).WithMany(d => d.AssignedVehicles).HasForeignKey(e => e.DriverId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AxleConfiguration).WithMany().HasForeignKey(e => e.AxleConfigurationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Owner).WithMany(o => o.Vehicles).HasForeignKey(e => e.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        // Supplier configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");
            entity.Property(e => e.Logo).HasMaxLength(500);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");
            entity.Property(e => e.Logo).HasMaxLength(500);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Transporter configuration
        modelBuilder.Entity<Transporter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");
            entity.Property(e => e.Logo).HasMaxLength(500);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Driver configuration
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LicenseExpiryDate);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");
            entity.Property(e => e.TransporterId).HasColumnType("text");
            entity.Property(e => e.SupplierId).HasColumnType("text");

            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.HasOne(e => e.Transporter).WithMany(t => t.Drivers).HasForeignKey(e => e.TransporterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier).WithMany(s => s.Drivers).HasForeignKey(e => e.SupplierId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Image).HasMaxLength(500);

            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Route configuration
        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.StartPoint).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EndPoint).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Sacco configuration
        modelBuilder.Entity<Sacco>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
            entity.Property(e => e.OtherDetails).HasColumnType("jsonb");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Organisation configuration
        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Weighbridge configuration
        modelBuilder.Entity<Weighbridge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");

            entity.HasIndex(e => e.Location).IsUnique();
        });

        // AxleConfiguration configuration
        modelBuilder.Entity<AxleConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Configuration).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasIndex(e => e.Configuration).IsUnique();
        });

        // Affiliation configuration
        modelBuilder.Entity<Affiliation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.Details).HasMaxLength(1000);
            entity.Property(e => e.SaccoId).HasColumnType("text");
            entity.Property(e => e.OrganisationId).HasColumnType("text");

            entity.HasOne(e => e.Sacco).WithMany(s => s.Affiliations).HasForeignKey(e => e.SaccoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Organisation).WithMany(o => o.Affiliations).HasForeignKey(e => e.OrganisationId).OnDelete(DeleteBehavior.Restrict);
        });

        // Owner configuration
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
            entity.Property(e => e.Type).HasMaxLength(100);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasMany(e => e.Vehicles).WithOne(v => v.Owner).HasForeignKey(v => v.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("text");
            entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(20);
            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.AffectedProperties).HasMaxLength(1000);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");

            entity.HasIndex(e => new { e.EntityName, e.EntityId });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Action);
        });

        // BaseEntity properties are configured in each derived entity
    }
}