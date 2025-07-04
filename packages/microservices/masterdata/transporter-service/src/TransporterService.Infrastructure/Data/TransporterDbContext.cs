using Microsoft.EntityFrameworkCore;
using TransporterService.Core.Entities;

namespace TransporterService.Infrastructure.Data;

public class TransporterDbContext : DbContext
{
    public TransporterDbContext(DbContextOptions<TransporterDbContext> options) : base(options)
    {
    }

    public DbSet<Transporter> Transporters { get; set; }
    public DbSet<TransporterContact> TransporterContacts { get; set; }
    public DbSet<TransporterFleet> TransporterFleet { get; set; }
    public DbSet<TransporterDriver> TransporterDrivers { get; set; }
    public DbSet<TransporterLicense> TransporterLicenses { get; set; }
    public DbSet<TransporterInsurance> TransporterInsurance { get; set; }
    public DbSet<TransporterContract> TransporterContracts { get; set; }
    public DbSet<TransporterPerformance> TransporterPerformance { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Transporter
        modelBuilder.Entity<Transporter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactPhone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.OperatingLicense).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Rating).HasColumnType("decimal(3,2)");
            entity.Property(e => e.TransporterType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
        });

        // Configure TransporterContact
        modelBuilder.Entity<TransporterContact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.AlternatePhone).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.ContactType).HasConversion<string>();
            entity.HasOne(e => e.Transporter)
                .WithMany(t => t.Contacts)
                .HasForeignKey(e => e.TransporterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransporterFleet
        modelBuilder.Entity<TransporterFleet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VehicleNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            entity.Property(e => e.Make).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.ChassisNumber).HasMaxLength(100);
            entity.Property(e => e.EngineNumber).HasMaxLength(100);
            entity.Property(e => e.LoadCapacity).HasColumnType("decimal(10,2)");
            entity.Property(e => e.FuelCapacity).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Mileage).HasColumnType("decimal(10,2)");
            entity.Property(e => e.CurrentDriverId).HasMaxLength(50);
            entity.Property(e => e.CurrentLocation).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.VehicleType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.Transporter)
                .WithMany(t => t.Fleet)
                .HasForeignKey(e => e.TransporterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransporterDriver
        modelBuilder.Entity<TransporterDriver>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Salary).HasColumnType("decimal(10,2)");
            entity.Property(e => e.AssignedVehicleId).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.Transporter)
                .WithMany(t => t.Drivers)
                .HasForeignKey(e => e.TransporterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransporterLicense
        modelBuilder.Entity<TransporterLicense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IssuingAuthority).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Restrictions).HasMaxLength(500);
            entity.Property(e => e.Fee).HasColumnType("decimal(10,2)");
            entity.Property(e => e.DocumentPath).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.LicenseType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.Transporter)
                .WithMany(t => t.Licenses)
                .HasForeignKey(e => e.TransporterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransporterInsurance
        modelBuilder.Entity<TransporterInsurance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PolicyNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.InsuranceCompany).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CoverageAmount).HasColumnType("decimal(15,2)");
            entity.Property(e => e.PremiumAmount).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Deductible).HasMaxLength(100);
            entity.Property(e => e.BeneficiaryName).HasMaxLength(200);
            entity.Property(e => e.AgentName).HasMaxLength(200);
            entity.Property(e => e.AgentContact).HasMaxLength(50);
            entity.Property(e => e.TotalClaimsAmount).HasColumnType("decimal(15,2)");
            entity.Property(e => e.DocumentPath).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.InsuranceType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.Transporter)
                .WithMany(t => t.Insurance)
                .HasForeignKey(e => e.TransporterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransporterContract
        modelBuilder.Entity<TransporterContract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ClientContactPerson).HasMaxLength(200);
            entity.Property(e => e.ClientPhone).HasMaxLength(20);
            entity.Property(e => e.ClientEmail).HasMaxLength(200);
            entity.Property(e => e.ContractValue).HasColumnType("decimal(15,2)");
            entity.Property(e => e.PaymentTerms).HasMaxLength(500);
            entity.Property(e => e.ServiceDescription).HasMaxLength(1000);
            entity.Property(e => e.DeliveryTerms).HasMaxLength(500);
            entity.Property(e => e.PerformanceMetrics).HasMaxLength(500);
            entity.Property(e => e.PenaltyClause).HasMaxLength(500);
            entity.Property(e => e.DocumentPath).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.ContractType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.Transporter)
                .WithMany(t => t.Contracts)
                .HasForeignKey(e => e.TransporterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransporterPerformance
        modelBuilder.Entity<TransporterPerformance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Period).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Value).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TargetValue).HasColumnType("decimal(10,2)");
            entity.Property(e => e.BenchmarkValue).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Grade).HasMaxLength(5);
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.TotalDistance).HasColumnType("decimal(12,2)");
            entity.Property(e => e.TotalFuelConsumed).HasColumnType("decimal(10,2)");
            entity.Property(e => e.CustomerRating).HasColumnType("decimal(3,2)");
            entity.Property(e => e.ImprovementAreas).HasMaxLength(1000);
            entity.Property(e => e.Recommendations).HasMaxLength(1000);
            entity.Property(e => e.MetricType).HasConversion<string>();
            entity.HasOne(e => e.Transporter)
                .WithMany(t => t.PerformanceRecords)
                .HasForeignKey(e => e.TransporterId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}