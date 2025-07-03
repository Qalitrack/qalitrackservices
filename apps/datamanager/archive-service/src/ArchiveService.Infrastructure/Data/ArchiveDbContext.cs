using Microsoft.EntityFrameworkCore;
using ArchiveService.Core.Entities;

namespace ArchiveService.Infrastructure.Data
{
    public class ArchiveDbContext : DbContext
    {
        public ArchiveDbContext(DbContextOptions<ArchiveDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<ArchiveMetadata> ArchiveMetadata { get; set; }
        public DbSet<RetentionPolicy> RetentionPolicies { get; set; }
        public DbSet<ArchiveStorage> ArchiveStorages { get; set; }
        public DbSet<DataMigration> DataMigrations { get; set; }
        public DbSet<ArchiveIndex> ArchiveIndexes { get; set; }
        public DbSet<ArchivedTransaction> ArchivedTransactions { get; set; }
        public DbSet<ArchivedWeightMeasurement> ArchivedWeightMeasurements { get; set; }
        public DbSet<ArchivedComplianceRecord> ArchivedComplianceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ArchiveMetadata
            modelBuilder.Entity<ArchiveMetadata>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ArchiveId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ArchiveName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CompressionType).HasMaxLength(50).HasDefaultValue("GZIP");
                entity.Property(e => e.ChecksumType).HasMaxLength(100).HasDefaultValue("SHA256");
                entity.Property(e => e.ChecksumValue).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("ACTIVE");
                entity.Property(e => e.StorageLocation).HasMaxLength(100);
                entity.Property(e => e.StorageTier).HasMaxLength(100).HasDefaultValue("HOT");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.AdditionalMetadata).HasMaxLength(1000);
                entity.Property(e => e.Tags).HasMaxLength(500);

                entity.HasIndex(e => e.ArchiveId).IsUnique();
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ArchiveDate);
                entity.HasIndex(e => e.StorageTier);
            });

            // Configure RetentionPolicy
            modelBuilder.Entity<RetentionPolicy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.StorageTier).HasMaxLength(50).HasDefaultValue("HOT");
                entity.Property(e => e.CompressionType).HasMaxLength(50).HasDefaultValue("GZIP");
                entity.Property(e => e.ExecutionSchedule).HasMaxLength(100).HasDefaultValue("DAILY");
                entity.Property(e => e.FilterCriteria).HasMaxLength(1000);
                entity.Property(e => e.OrganizationIds).HasMaxLength(500);
                entity.Property(e => e.Priority).HasMaxLength(100).HasDefaultValue("MEDIUM");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsAutomatic);
                entity.HasIndex(e => e.NextExecution);
            });

            // Configure ArchiveStorage
            modelBuilder.Entity<ArchiveStorage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.StorageType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StorageTier).HasMaxLength(50).HasDefaultValue("HOT");
                entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ContainerName).HasMaxLength(100);
                entity.Property(e => e.BasePath).HasMaxLength(500);
                entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("USD");
                entity.Property(e => e.Region).HasMaxLength(100);
                entity.Property(e => e.AvailabilityZone).HasMaxLength(100);
                entity.Property(e => e.ReplicationLevel).HasMaxLength(50).HasDefaultValue("NONE");
                entity.Property(e => e.EncryptionType).HasMaxLength(50).HasDefaultValue("AES256");
                entity.Property(e => e.EncryptionKey).HasMaxLength(500);
                entity.Property(e => e.HealthStatus).HasMaxLength(50).HasDefaultValue("UNKNOWN");
                entity.Property(e => e.HealthDetails).HasMaxLength(500);
                entity.Property(e => e.ConfigurationJson).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CostPerGB).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.StorageType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsDefault);
                entity.HasIndex(e => e.HealthStatus);
            });

            // Configure DataMigration
            modelBuilder.Entity<DataMigration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MigrationId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MigrationType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("PENDING");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.ErrorDetails).HasMaxLength(2000);
                entity.Property(e => e.MigrationCriteria).HasMaxLength(1000);
                entity.Property(e => e.SourceLocation).HasMaxLength(500);
                entity.Property(e => e.DestinationLocation).HasMaxLength(500);
                entity.Property(e => e.Priority).HasMaxLength(100).HasDefaultValue("MEDIUM");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.ProgressPercentage).HasColumnType("decimal(5,2)");

                entity.HasIndex(e => e.MigrationId).IsUnique();
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure ArchiveIndex
            modelBuilder.Entity<ArchiveIndex>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IndexId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IndexType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IndexName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IndexFields).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.IndexConfiguration).HasMaxLength(2000);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("ACTIVE");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.ErrorDetails).HasMaxLength(1000);
                entity.Property(e => e.Version).HasMaxLength(100).HasDefaultValue("1.0");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.AverageSearchTime).HasColumnType("decimal(10,2)");

                entity.HasOne(e => e.ArchiveMetadata)
                      .WithMany(a => a.ArchiveIndexes)
                      .HasForeignKey(e => e.ArchiveMetadataId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.IndexId).IsUnique();
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.Status);
            });

            // Configure ArchivedTransaction
            modelBuilder.Entity<ArchivedTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalTransactionId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.VehicleId).HasMaxLength(100);
                entity.Property(e => e.DriverId).HasMaxLength(100);
                entity.Property(e => e.WeighbridgeId).HasMaxLength(100);
                entity.Property(e => e.OrganizationId).HasMaxLength(100);
                entity.Property(e => e.CustomerId).HasMaxLength(100);
                entity.Property(e => e.ProductId).HasMaxLength(100);
                entity.Property(e => e.WeightUnit).HasMaxLength(10).HasDefaultValue("KG");
                entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("KES");
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.TransactionDetails).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.ArchivedBy).HasMaxLength(100);
                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TareWeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetWeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TransactionAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.ArchiveMetadata)
                      .WithMany(a => a.ArchivedTransactions)
                      .HasForeignKey(e => e.ArchiveMetadataId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.OriginalTransactionId);
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.DriverId);
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => e.OriginalTransactionDate);
                entity.HasIndex(e => e.ArchivedDate);
            });

            // Configure ArchivedWeightMeasurement
            modelBuilder.Entity<ArchivedWeightMeasurement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalMeasurementId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.VehicleId).HasMaxLength(100);
                entity.Property(e => e.WeighbridgeId).HasMaxLength(100);
                entity.Property(e => e.OrganizationId).HasMaxLength(100);
                entity.Property(e => e.MeasurementType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.WeightUnit).HasMaxLength(10).HasDefaultValue("KG");
                entity.Property(e => e.OperatorId).HasMaxLength(100);
                entity.Property(e => e.OperatorName).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.CalibrationDetails).HasMaxLength(500);
                entity.Property(e => e.MeasurementDetails).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.ArchivedBy).HasMaxLength(100);
                entity.Property(e => e.Weight).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.ArchiveMetadata)
                      .WithMany(a => a.ArchivedWeightMeasurements)
                      .HasForeignKey(e => e.ArchiveMetadataId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.OriginalMeasurementId);
                entity.HasIndex(e => e.TransactionId);
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.MeasurementType);
                entity.HasIndex(e => e.OriginalMeasurementDate);
                entity.HasIndex(e => e.ArchivedDate);
            });

            // Configure ArchivedComplianceRecord
            modelBuilder.Entity<ArchivedComplianceRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalComplianceId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.VehicleId).HasMaxLength(100);
                entity.Property(e => e.DriverId).HasMaxLength(100);
                entity.Property(e => e.OrganizationId).HasMaxLength(100);
                entity.Property(e => e.ComplianceType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ComplianceStatus).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Severity).HasMaxLength(50).HasDefaultValue("MEDIUM");
                entity.Property(e => e.ViolationType).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Details).HasMaxLength(2000);
                entity.Property(e => e.Unit).HasMaxLength(10);
                entity.Property(e => e.PenaltyCurrency).HasMaxLength(10).HasDefaultValue("KES");
                entity.Property(e => e.ResolutionStatus).HasMaxLength(50);
                entity.Property(e => e.ResolvedBy).HasMaxLength(100);
                entity.Property(e => e.ResolutionNotes).HasMaxLength(500);
                entity.Property(e => e.ArchivedBy).HasMaxLength(100);
                entity.Property(e => e.ActualValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LimitValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ExcessValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PenaltyAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.ArchiveMetadata)
                      .WithMany(a => a.ArchivedComplianceRecords)
                      .HasForeignKey(e => e.ArchiveMetadataId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ArchivedTransaction)
                      .WithMany(t => t.ComplianceRecords)
                      .HasForeignKey(e => e.ArchivedTransactionId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.OriginalComplianceId);
                entity.HasIndex(e => e.TransactionId);
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.ComplianceType);
                entity.HasIndex(e => e.ComplianceStatus);
                entity.HasIndex(e => e.OriginalDetectionDate);
                entity.HasIndex(e => e.ArchivedDate);
            });

            // Configure relationships
            modelBuilder.Entity<ArchiveMetadata>()
                .HasOne(a => a.RetentionPolicy)
                .WithMany(r => r.ArchiveMetadatas)
                .HasForeignKey(a => a.RetentionPolicyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ArchiveMetadata>()
                .HasOne(a => a.ArchiveStorage)
                .WithMany(s => s.ArchiveMetadatas)
                .HasForeignKey(a => a.ArchiveStorageId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DataMigration>()
                .HasOne(d => d.RetentionPolicy)
                .WithMany(r => r.DataMigrations)
                .HasForeignKey(d => d.RetentionPolicyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default storage configuration
            modelBuilder.Entity<ArchiveStorage>().HasData(
                new ArchiveStorage
                {
                    Id = 1,
                    Name = "Local File Storage",
                    Description = "Default local file storage for development",
                    StorageType = "LOCAL",
                    StorageTier = "HOT",
                    ConnectionString = "filesystem",
                    ContainerName = "archives",
                    BasePath = "/var/archives",
                    MaxStorageSize = 100L * 1024 * 1024 * 1024, // 100GB
                    CurrentStorageSize = 0,
                    CostPerGB = 0.0m,
                    Currency = "USD",
                    IsActive = true,
                    IsDefault = true,
                    Region = "LOCAL",
                    ReplicationLevel = "NONE",
                    EncryptionType = "AES256",
                    HealthStatus = "HEALTHY",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SYSTEM",
                    UpdatedBy = "SYSTEM"
                }
            );

            // Seed default retention policies
            modelBuilder.Entity<RetentionPolicy>().HasData(
                new RetentionPolicy
                {
                    Id = 1,
                    Name = "Transaction Data Retention",
                    Description = "Default retention policy for transaction data",
                    EntityType = "TRANSACTIONS",
                    Category = "FINANCIAL",
                    RetentionDays = 2555, // 7 years
                    ArchiveAfterDays = 365, // 1 year
                    DeleteAfterDays = 3650, // 10 years
                    StorageTier = "HOT",
                    CompressionType = "GZIP",
                    IsActive = true,
                    IsAutomatic = true,
                    ExecutionSchedule = "DAILY",
                    FilterCriteria = "{}",
                    OrganizationIds = "",
                    Priority = "HIGH",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SYSTEM",
                    UpdatedBy = "SYSTEM",
                    NextExecution = DateTime.UtcNow.Date.AddDays(1).AddHours(2)
                },
                new RetentionPolicy
                {
                    Id = 2,
                    Name = "Weight Measurement Retention",
                    Description = "Default retention policy for weight measurement data",
                    EntityType = "WEIGHT_MEASUREMENTS",
                    Category = "OPERATIONAL",
                    RetentionDays = 1825, // 5 years
                    ArchiveAfterDays = 180, // 6 months
                    DeleteAfterDays = 2555, // 7 years
                    StorageTier = "WARM",
                    CompressionType = "GZIP",
                    IsActive = true,
                    IsAutomatic = true,
                    ExecutionSchedule = "DAILY",
                    FilterCriteria = "{}",
                    OrganizationIds = "",
                    Priority = "MEDIUM",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SYSTEM",
                    UpdatedBy = "SYSTEM",
                    NextExecution = DateTime.UtcNow.Date.AddDays(1).AddHours(2)
                },
                new RetentionPolicy
                {
                    Id = 3,
                    Name = "Compliance Record Retention",
                    Description = "Default retention policy for compliance records",
                    EntityType = "COMPLIANCE_RECORDS",
                    Category = "REGULATORY",
                    RetentionDays = 3650, // 10 years
                    ArchiveAfterDays = 90, // 3 months
                    DeleteAfterDays = 0, // Never delete
                    StorageTier = "HOT",
                    CompressionType = "GZIP",
                    IsActive = true,
                    IsAutomatic = true,
                    ExecutionSchedule = "DAILY",
                    FilterCriteria = "{}",
                    OrganizationIds = "",
                    Priority = "CRITICAL",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SYSTEM",
                    UpdatedBy = "SYSTEM",
                    NextExecution = DateTime.UtcNow.Date.AddDays(1).AddHours(2)
                }
            );
        }
    }
}