using DriverService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Infrastructure.Data;

public class DriverDbContext : DbContext
{
    public DriverDbContext(DbContextOptions<DriverDbContext> options) : base(options)
    {
    }

    public DbSet<Driver> Drivers { get; set; }
    public DbSet<DriverLicense> DriverLicenses { get; set; }
    public DbSet<DriverProfile> DriverProfiles { get; set; }
    public DbSet<DriverDocument> DriverDocuments { get; set; }
    public DbSet<DriverTraining> DriverTrainings { get; set; }
    public DbSet<DriverMedical> DriverMedicals { get; set; }
    public DbSet<DriverViolation> DriverViolations { get; set; }
    public DbSet<DriverPerformance> DriverPerformances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Driver configuration
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Nationality).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(200);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactRelationship).HasMaxLength(50);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Supervisor).HasMaxLength(200);
            entity.Property(e => e.ProfilePhotoUrl).HasMaxLength(500);
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.EmploymentType).HasConversion<string>();

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.Status);

            // One-to-one relationships
            entity.HasOne(e => e.License)
                  .WithOne(e => e.Driver)
                  .HasForeignKey<DriverLicense>(e => e.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Profile)
                  .WithOne(e => e.Driver)
                  .HasForeignKey<DriverProfile>(e => e.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationships
            entity.HasMany(e => e.Documents)
                  .WithOne(e => e.Driver)
                  .HasForeignKey(e => e.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Trainings)
                  .WithOne(e => e.Driver)
                  .HasForeignKey(e => e.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.MedicalRecords)
                  .WithOne(e => e.Driver)
                  .HasForeignKey(e => e.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Violations)
                  .WithOne(e => e.Driver)
                  .HasForeignKey(e => e.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.PerformanceRecords)
                  .WithOne(e => e.Driver)
                  .HasForeignKey(e => e.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Driver License configuration
        modelBuilder.Entity<DriverLicense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IssuingCountry).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IssuingState).HasMaxLength(100);
            entity.Property(e => e.Categories).HasMaxLength(200);
            entity.Property(e => e.Restrictions).HasMaxLength(500);
            entity.Property(e => e.Endorsements).HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.DriverId).IsRequired();

            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.HasIndex(e => e.DriverId).IsUnique();
            entity.HasIndex(e => e.ExpiryDate);
            entity.HasIndex(e => e.Status);
        });

        // Driver Profile configuration
        modelBuilder.Entity<DriverProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.PreviousEmployers).HasMaxLength(2000);
            entity.Property(e => e.SpecialSkills).HasMaxLength(1000);
            entity.Property(e => e.Languages).HasMaxLength(500);
            entity.Property(e => e.AccidentHistory).HasMaxLength(2000);
            entity.Property(e => e.PreferredRoutes).HasMaxLength(1000);
            entity.Property(e => e.PreferredVehicleTypes).HasMaxLength(500);
            entity.Property(e => e.WorkSchedulePreference).HasMaxLength(500);
            entity.Property(e => e.SafetyRating).HasColumnType("decimal(3,2)");
            entity.Property(e => e.PerformanceRating).HasColumnType("decimal(3,2)");
            entity.Property(e => e.DriverId).IsRequired();

            entity.HasIndex(e => e.DriverId).IsUnique();
            entity.HasIndex(e => e.SafetyRating);
            entity.HasIndex(e => e.PerformanceRating);
            entity.HasIndex(e => e.YearsOfExperience);
        });

        // Driver Document configuration
        modelBuilder.Entity<DriverDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.DocumentName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DocumentNumber).HasMaxLength(100);
            entity.Property(e => e.IssuingAuthority).HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.VerifiedBy).HasMaxLength(200);
            entity.Property(e => e.DocumentType).HasConversion<string>();
            entity.Property(e => e.DriverId).IsRequired();

            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.DocumentType);
            entity.HasIndex(e => e.ExpiryDate);
            entity.HasIndex(e => e.IsVerified);
        });

        // Driver Training configuration
        modelBuilder.Entity<DriverTraining>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.TrainingName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Provider).HasMaxLength(200);
            entity.Property(e => e.CertificateNumber).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Instructor).HasMaxLength(200);
            entity.Property(e => e.Score).HasColumnType("decimal(5,2)");
            entity.Property(e => e.PassingScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Duration).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Cost).HasColumnType("decimal(10,2)");
            entity.Property(e => e.TrainingType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.DriverId).IsRequired();

            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.TrainingType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CertificateExpiryDate);
        });

        // Driver Medical configuration
        modelBuilder.Entity<DriverMedical>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.ExaminingPhysician).HasMaxLength(200);
            entity.Property(e => e.MedicalFacility).HasMaxLength(200);
            entity.Property(e => e.CertificateNumber).HasMaxLength(100);
            entity.Property(e => e.Restrictions).HasMaxLength(1000);
            entity.Property(e => e.Results).HasMaxLength(2000);
            entity.Property(e => e.Recommendations).HasMaxLength(1000);
            entity.Property(e => e.Cost).HasColumnType("decimal(10,2)");
            entity.Property(e => e.TestType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.DriverId).IsRequired();

            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.TestType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpiryDate);
        });

        // Driver Violation configuration
        modelBuilder.Entity<DriverViolation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.TicketNumber).HasMaxLength(100);
            entity.Property(e => e.IssuingOfficer).HasMaxLength(200);
            entity.Property(e => e.IssuingAgency).HasMaxLength(200);
            entity.Property(e => e.CourtLocation).HasMaxLength(300);
            entity.Property(e => e.Resolution).HasMaxLength(1000);
            entity.Property(e => e.ActionTaken).HasMaxLength(500);
            entity.Property(e => e.FineAmount).HasColumnType("decimal(10,2)");
            entity.Property(e => e.ViolationType).HasConversion<string>();
            entity.Property(e => e.Severity).HasConversion<string>();
            entity.Property(e => e.DriverId).IsRequired();

            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.ViolationType);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.ViolationDate);
            entity.HasIndex(e => e.IsPaid);
        });

        // Driver Performance configuration
        modelBuilder.Entity<DriverPerformance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.SafetyScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.FuelEfficiencyScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.OnTimePerformanceScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.CustomerSatisfactionScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.OverallScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TotalMiles).HasColumnType("decimal(12,2)");
            entity.Property(e => e.FuelConsumption).HasColumnType("decimal(10,2)");
            entity.Property(e => e.FuelCost).HasColumnType("decimal(12,2)");
            entity.Property(e => e.Revenue).HasColumnType("decimal(12,2)");
            entity.Property(e => e.Expenses).HasColumnType("decimal(12,2)");
            entity.Property(e => e.HoursWorked).HasColumnType("decimal(8,2)");
            entity.Property(e => e.OvertimeHours).HasColumnType("decimal(8,2)");
            entity.Property(e => e.Goals).HasMaxLength(2000);
            entity.Property(e => e.Achievements).HasMaxLength(2000);
            entity.Property(e => e.AreasForImprovement).HasMaxLength(2000);
            entity.Property(e => e.ManagerComments).HasMaxLength(2000);
            entity.Property(e => e.DriverComments).HasMaxLength(2000);
            entity.Property(e => e.ReviewedBy).HasMaxLength(200);
            entity.Property(e => e.Period).HasConversion<string>();
            entity.Property(e => e.DriverId).IsRequired();

            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.Period);
            entity.HasIndex(e => e.PeriodStart);
            entity.HasIndex(e => e.PeriodEnd);
            entity.HasIndex(e => e.OverallScore);
        });

        // Configure global query filters for soft delete
        modelBuilder.Entity<Driver>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverLicense>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverDocument>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverTraining>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverMedical>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverViolation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverPerformance>().HasQueryFilter(e => !e.IsDeleted);
    }
}