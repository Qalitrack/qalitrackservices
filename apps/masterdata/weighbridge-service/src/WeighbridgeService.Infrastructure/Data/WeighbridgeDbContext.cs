using Microsoft.EntityFrameworkCore;
using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Infrastructure.Data;

public class WeighbridgeDbContext : DbContext
{
    public WeighbridgeDbContext(DbContextOptions<WeighbridgeDbContext> options) : base(options)
    {
    }

    public DbSet<Weighbridge> Weighbridges { get; set; }
    public DbSet<WeighbridgeLocation> WeighbridgeLocations { get; set; }
    public DbSet<WeighbridgeConfiguration> WeighbridgeConfigurations { get; set; }
    public DbSet<WeighbridgeCalibration> WeighbridgeCalibrations { get; set; }
    public DbSet<WeighbridgeMaintenance> WeighbridgeMaintenances { get; set; }
    public DbSet<WeighbridgeOperator> WeighbridgeOperators { get; set; }
    public DbSet<WeighbridgeSchedule> WeighbridgeSchedules { get; set; }
    public DbSet<WeighbridgeCapacity> WeighbridgeCapacities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Weighbridge
        modelBuilder.Entity<Weighbridge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MaxCapacity).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinCapacity).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Accuracy).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Manufacturer).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Configure relationships
            entity.HasOne(e => e.WeighbridgeLocation)
                  .WithOne(l => l.Weighbridge)
                  .HasForeignKey<WeighbridgeLocation>(l => l.WeighbridgeId);

            entity.HasOne(e => e.Configuration)
                  .WithOne(c => c.Weighbridge)
                  .HasForeignKey<WeighbridgeConfiguration>(c => c.WeighbridgeId);

            entity.HasOne(e => e.CurrentCapacity)
                  .WithOne(c => c.Weighbridge)
                  .HasForeignKey<WeighbridgeCapacity>(c => c.WeighbridgeId);

            entity.HasMany(e => e.CalibrationHistory)
                  .WithOne(c => c.Weighbridge)
                  .HasForeignKey(c => c.WeighbridgeId);

            entity.HasMany(e => e.MaintenanceSchedules)
                  .WithOne(m => m.Weighbridge)
                  .HasForeignKey(m => m.WeighbridgeId);

            entity.HasMany(e => e.Operators)
                  .WithOne(o => o.Weighbridge)
                  .HasForeignKey(o => o.WeighbridgeId);
        });

        // Configure WeighbridgeLocation
        modelBuilder.Entity<WeighbridgeLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired();
            entity.Property(e => e.SiteName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AccessInstructions).HasMaxLength(1000);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.ContactEmail).HasMaxLength(100);
        });

        // Configure WeighbridgeConfiguration
        modelBuilder.Entity<WeighbridgeConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired();
            entity.Property(e => e.ConfigurationName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MinimumWeight).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaximumWeight).HasColumnType("decimal(18,2)");
            entity.Property(e => e.WeightUnit).HasMaxLength(10);
            entity.Property(e => e.DisplayFormat).HasMaxLength(20);
            entity.Property(e => e.BackupLocation).HasMaxLength(500);
            entity.Property(e => e.AlertEmail).HasMaxLength(100);
        });

        // Configure WeighbridgeCalibration
        modelBuilder.Entity<WeighbridgeCalibration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired();
            entity.Property(e => e.CalibratedBy).HasMaxLength(100);
            entity.Property(e => e.CertificationNumber).HasMaxLength(50);
            entity.Property(e => e.CalibrationCompany).HasMaxLength(200);
            entity.Property(e => e.TestWeight1).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ActualReading1).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TestWeight2).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ActualReading2).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TestWeight3).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ActualReading3).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Accuracy).HasColumnType("decimal(10,4)");
            entity.Property(e => e.LinearityError).HasColumnType("decimal(10,4)");
            entity.Property(e => e.RepeatabilityError).HasColumnType("decimal(10,4)");
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.CorrectionApplied).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CertificateFilePath).HasMaxLength(500);
            entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
        });

        // Configure WeighbridgeMaintenance
        modelBuilder.Entity<WeighbridgeMaintenance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.AssignedTo).HasMaxLength(100);
            entity.Property(e => e.MaintenanceCompany).HasMaxLength(200);
            entity.Property(e => e.EstimatedDuration).HasColumnType("decimal(10,2)");
            entity.Property(e => e.ActualDuration).HasColumnType("decimal(10,2)");
            entity.Property(e => e.EstimatedCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ActualCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.WorkPerformed).HasMaxLength(2000);
            entity.Property(e => e.Findings).HasMaxLength(2000);
            entity.Property(e => e.Recommendations).HasMaxLength(2000);
            entity.Property(e => e.WorkOrderNumber).HasMaxLength(50);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
        });

        // Configure WeighbridgeOperator
        modelBuilder.Entity<WeighbridgeOperator>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired();
            entity.Property(e => e.OperatorId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.EmployeeNumber).HasMaxLength(20);
            entity.Property(e => e.CertificationNumber).HasMaxLength(50);
            entity.Property(e => e.TrainingLevel).HasMaxLength(50);
            entity.Property(e => e.Shift).HasMaxLength(50);
            entity.Property(e => e.AccessLevel).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        // Configure WeighbridgeSchedule
        modelBuilder.Entity<WeighbridgeSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired();
            entity.Property(e => e.OperatorId).IsRequired();
            entity.Property(e => e.ShiftType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Operator)
                  .WithMany()
                  .HasForeignKey(e => e.OperatorId)
                  .HasPrincipalKey(o => o.Id);
        });

        // Configure WeighbridgeCapacity
        modelBuilder.Entity<WeighbridgeCapacity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired();
            entity.Property(e => e.CurrentLoad).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaxCapacity).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AvailableCapacity).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UsagePercentage).HasColumnType("decimal(10,2)");
            entity.Property(e => e.AlertLevel).HasMaxLength(20);
            entity.Property(e => e.EstimatedWaitTime).HasColumnType("decimal(10,2)");
            entity.Property(e => e.UnavailableReason).HasMaxLength(500);
        });
    }
}