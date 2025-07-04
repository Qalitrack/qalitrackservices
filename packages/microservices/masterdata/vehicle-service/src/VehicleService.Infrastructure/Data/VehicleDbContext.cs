using Microsoft.EntityFrameworkCore;
using VehicleService.Core.Entities;

namespace VehicleService.Infrastructure.Data;

public class VehicleDbContext : DbContext
{
    public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options) { }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleType> VehicleTypes { get; set; }
    public DbSet<VehicleRegistration> VehicleRegistrations { get; set; }
    public DbSet<VehicleSpecification> VehicleSpecifications { get; set; }
    public DbSet<VehicleOwner> VehicleOwners { get; set; }
    public DbSet<VehicleDocument> VehicleDocuments { get; set; }
    public DbSet<VehicleInspection> VehicleInspections { get; set; }
    public DbSet<VehicleInsurance> VehicleInsurance { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Vehicle Configuration
        modelBuilder.Entity<Vehicle>()
            .HasKey(v => v.Id);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.RegistrationNumber)
            .IsUnique();

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.VIN)
            .IsUnique();

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.VehicleType)
            .WithMany(vt => vt.Vehicles)
            .HasForeignKey(v => v.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Registration)
            .WithOne(vr => vr.Vehicle)
            .HasForeignKey<VehicleRegistration>(vr => vr.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Specification)
            .WithOne(vs => vs.Vehicle)
            .HasForeignKey<VehicleSpecification>(vs => vs.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // VehicleType Configuration
        modelBuilder.Entity<VehicleType>()
            .HasKey(vt => vt.Id);

        modelBuilder.Entity<VehicleType>()
            .HasIndex(vt => vt.Name)
            .IsUnique();

        // VehicleRegistration Configuration
        modelBuilder.Entity<VehicleRegistration>()
            .HasKey(vr => vr.Id);

        modelBuilder.Entity<VehicleRegistration>()
            .HasIndex(vr => vr.RegistrationNumber)
            .IsUnique();

        // VehicleSpecification Configuration
        modelBuilder.Entity<VehicleSpecification>()
            .HasKey(vs => vs.Id);

        modelBuilder.Entity<VehicleSpecification>()
            .HasIndex(vs => vs.VehicleId)
            .IsUnique();

        // VehicleOwner Configuration
        modelBuilder.Entity<VehicleOwner>()
            .HasKey(vo => vo.Id);

        modelBuilder.Entity<VehicleOwner>()
            .HasIndex(vo => vo.Email);

        modelBuilder.Entity<VehicleOwner>()
            .HasIndex(vo => vo.IdentificationNumber)
            .IsUnique();

        // VehicleDocument Configuration
        modelBuilder.Entity<VehicleDocument>()
            .HasKey(vd => vd.Id);

        modelBuilder.Entity<VehicleDocument>()
            .HasOne(vd => vd.Vehicle)
            .WithMany(v => v.Documents)
            .HasForeignKey(vd => vd.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VehicleDocument>()
            .HasIndex(vd => new { vd.VehicleId, vd.DocumentType, vd.DocumentNumber });

        // VehicleInspection Configuration
        modelBuilder.Entity<VehicleInspection>()
            .HasKey(vi => vi.Id);

        modelBuilder.Entity<VehicleInspection>()
            .HasOne(vi => vi.Vehicle)
            .WithMany(v => v.Inspections)
            .HasForeignKey(vi => vi.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VehicleInspection>()
            .HasIndex(vi => new { vi.VehicleId, vi.InspectionDate });

        // VehicleInsurance Configuration
        modelBuilder.Entity<VehicleInsurance>()
            .HasKey(vi => vi.Id);

        modelBuilder.Entity<VehicleInsurance>()
            .HasOne(vi => vi.Vehicle)
            .WithMany(v => v.InsurancePolicies)
            .HasForeignKey(vi => vi.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VehicleInsurance>()
            .HasIndex(vi => vi.PolicyNumber)
            .IsUnique();

        // Configure decimal precision
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.MaxWeight)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Vehicle>()
            .Property(v => v.TareWeight)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Vehicle>()
            .Property(v => v.CurrentMileage)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleType>()
            .Property(vt => vt.MaxWeightLimit)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleRegistration>()
            .Property(vr => vr.RegistrationFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleSpecification>()
            .Property(vs => vs.Length)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleSpecification>()
            .Property(vs => vs.Width)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleSpecification>()
            .Property(vs => vs.Height)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleSpecification>()
            .Property(vs => vs.Wheelbase)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleSpecification>()
            .Property(vs => vs.FuelTankCapacity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleSpecification>()
            .Property(vs => vs.LoadCapacity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleSpecification>()
            .Property(vs => vs.TowingCapacity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleInspection>()
            .Property(vi => vi.InspectionFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleInsurance>()
            .Property(vi => vi.PremiumAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleInsurance>()
            .Property(vi => vi.CoverageAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VehicleInsurance>()
            .Property(vi => vi.Deductible)
            .HasPrecision(18, 2);

        // Configure enum conversions
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Status)
            .HasConversion<string>();

        modelBuilder.Entity<VehicleDocument>()
            .Property(vd => vd.DocumentType)
            .HasConversion<string>();

        modelBuilder.Entity<VehicleInspection>()
            .Property(vi => vi.InspectionType)
            .HasConversion<string>();

        modelBuilder.Entity<VehicleInspection>()
            .Property(vi => vi.Result)
            .HasConversion<string>();

        modelBuilder.Entity<VehicleInsurance>()
            .Property(vi => vi.InsuranceType)
            .HasConversion<string>();
    }
}