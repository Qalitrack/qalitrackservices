using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Masterdata.Core.Entities;
using Masterdata.Core.Enums;

namespace Masterdata.Infrastructure.Data
{
    public class MasterdataDbContext : DbContext
    {
        public MasterdataDbContext(DbContextOptions<MasterdataDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Owner> Owners { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Transporter> Transporters { get; set; } = null!;
        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<DriverVehicle> DriverVehicles { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Route> Routes { get; set; } = null!;
        public DbSet<Sacco> Saccos { get; set; } = null!;
        public DbSet<Organisation> Organisations { get; set; } = null!;
        public DbSet<Weighbridge> Weighbridges { get; set; } = null!;
        public DbSet<AxleConfiguration> AxleConfigurations { get; set; } = null!;
        public DbSet<Affiliation> Affiliations { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Global soft-delete filter
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(property), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }

            // ──────────────────────────────────────────────────────────────
            // Vehicle Configuration
            // ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("text");
                entity.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                
                // Weights and Capacities (Decimal Precision)
                entity.Property(e => e.FuelTankCapacity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetWeightCapacity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TareWeight).HasColumnType("decimal(18,2)");

                // FK Relationships
                entity.HasOne(v => v.AxleConfiguration)
                    .WithMany(a => a.Vehicles)
                    .HasForeignKey(v => v.AxleConfigurationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // 🛠 FIX: Prevents the 23503 FK Violation if ID is missing

                entity.HasOne(v => v.Owner)
                    .WithMany(o => o.Vehicles)
                    .HasForeignKey(v => v.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(v => v.Supplier)
                    .WithMany(s => s.Vehicles)
                    .HasForeignKey(v => v.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(v => v.Transporter)
                    .WithMany(t => t.Vehicles)
                    .HasForeignKey(v => v.TransporterId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            });

            // ──────────────────────────────────────────────────────────────
            // Driver & DriverVehicle (Many-to-Many)
            // ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<DriverVehicle>(entity =>
            {
                entity.HasKey(e => new { e.DriverId, e.VehicleId });
                entity.HasOne(e => e.Driver).WithMany(d => d.DriverVehicles).HasForeignKey(e => e.DriverId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Vehicle).WithMany(v => v.DriverVehicles).HasForeignKey(e => e.VehicleId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Driver>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.LicenseNumber).IsUnique();
                entity.HasOne(e => e.Transporter).WithMany(t => t.Drivers).HasForeignKey(e => e.TransporterId).OnDelete(DeleteBehavior.Restrict);
            });

            // ──────────────────────────────────────────────────────────────
            // Master Data Entities (Product, Route, Sacco, etc.)
            // ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<Product>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            modelBuilder.Entity<Route>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StartPoint).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EndPoint).IsRequired().HasMaxLength(200);
            });

            modelBuilder.Entity<Weighbridge>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Scales).HasColumnType("jsonb");
            });

            modelBuilder.Entity<Sacco>(entity => {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Organisation>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
            });

            modelBuilder.Entity<AxleConfiguration>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // ──────────────────────────────────────────────────────────────
            // Audit and Logs
            // ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OldValues).HasColumnType("jsonb");
                entity.Property(e => e.NewValues).HasColumnType("jsonb");
                entity.HasIndex(e => new { e.EntityName, e.EntityId });
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}