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

            // Global soft-delete filter
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

                entity.Property(e => e.RegistrationNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Make).HasMaxLength(50);
                entity.Property(e => e.Model).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.ChassisNumber).HasMaxLength(50);
                entity.Property(e => e.EngineNumber).HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Active");

                entity.Property(e => e.VehicleClass).HasMaxLength(50);
                entity.Property(e => e.BodyType).HasMaxLength(50);

                // Owner relationship – now properly nullable
                entity.Property(e => e.OwnerId)
                    .HasColumnType("text")
                    .IsRequired(false);

                entity.HasOne(v => v.Owner)
                    .WithMany(o => o.Vehicles)
                    .HasForeignKey(v => v.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);                // ← Fixed: allows null

                entity.Property(e => e.SupplierId).HasColumnType("text");
                entity.HasOne(v => v.Supplier)
                    .WithMany(s => s.Vehicles)         // ← Added inverse
                    .HasForeignKey(v => v.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.Property(e => e.TransporterId).HasColumnType("text");
                entity.HasOne(v => v.Transporter)
                    .WithMany(t => t.Vehicles)         // ← Added inverse
                    .HasForeignKey(v => v.TransporterId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(v => v.AxleConfiguration)
                    .WithMany()
                    .HasForeignKey(v => v.AxleConfigurationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                entity.HasIndex(e => e.RegistrationNumber).IsUnique();
                entity.HasIndex(e => e.ChassisNumber).IsUnique(); // optional
            });

            // ──────────────────────────────────────────────────────────────
            // Owner Configuration
            // ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<Owner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("text");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.Type)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
                entity.Property(e => e.TaxIdentificationNumber).HasMaxLength(100);
                entity.Property(e => e.NationalId).HasMaxLength(50);

                entity.HasIndex(e => e.Name).IsUnique();

                // Explicit inverse navigation (recommended)
                entity.HasMany(o => o.Vehicles)
                    .WithOne(v => v.Owner)
                    .HasForeignKey(v => v.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // ──────────────────────────────────────────────────────────────
            // DriverVehicle (Many-to-Many)
            // ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<DriverVehicle>(entity =>
            {
                entity.HasKey(e => new { e.DriverId, e.VehicleId });

                entity.Property(e => e.DriverId).HasColumnType("text");
                entity.Property(e => e.VehicleId).HasColumnType("text");

                entity.HasOne(e => e.Driver)
                    .WithMany(d => d.DriverVehicles)
                    .HasForeignKey(e => e.DriverId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.DriverVehicles)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ──────────────────────────────────────────────────────────────
            // Supplier (with inverse navigation)
            // ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("text");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.Name).IsUnique();

                entity.HasMany(s => s.Vehicles)
                    .WithOne(v => v.Supplier)
                    .HasForeignKey(v => v.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // ──────────────────────────────────────────────────────────────
            // Transporter (with inverse navigation)
            // ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<Transporter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("text");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContactInfo).HasColumnType("jsonb");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.Name).IsUnique();

                entity.HasMany(t => t.Vehicles)
                    .WithOne(v => v.Transporter)
                    .HasForeignKey(v => v.TransporterId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // ──────────────────────────────────────────────────────────────
            // Driver
            // ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<Driver>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("text");
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.LicenseNumber).IsUnique();

                entity.HasOne(e => e.Transporter)
                    .WithMany()
                    .HasForeignKey(e => e.TransporterId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            // AuditLog – minimal configuration (expand as needed)
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("text");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                // Add other properties as needed
            });

            // ... add other entity configurations (Product, Route, etc.) here as needed ...
        }
    }
}