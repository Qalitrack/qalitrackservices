using Microsoft.EntityFrameworkCore;
using ComplianceService.Core.Entities;

namespace ComplianceService.Infrastructure.Data
{
    public class ComplianceDbContext : DbContext
    {
        public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Compliance> Compliances { get; set; }
        public DbSet<ComplianceRule> ComplianceRules { get; set; }
        public DbSet<ComplianceViolation> ComplianceViolations { get; set; }
        public DbSet<ComplianceCheck> ComplianceChecks { get; set; }
        public DbSet<RegulatoryStandard> RegulatoryStandards { get; set; }
        public DbSet<ComplianceAudit> ComplianceAudits { get; set; }
        public DbSet<LicenseMonitoring> LicenseMonitoring { get; set; }
        public DbSet<WeightLimits> WeightLimits { get; set; }
        public DbSet<RouteRestrictions> RouteRestrictions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Compliance
            modelBuilder.Entity<Compliance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ComplianceName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OrganizationId).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.AssignedUserId).HasMaxLength(50);
                entity.Property(e => e.AssignedUserName).HasMaxLength(100);
                entity.Property(e => e.RiskAssessment).HasMaxLength(1000);
                entity.Property(e => e.RegulatoryFramework).HasMaxLength(100);
                entity.Property(e => e.RegulatoryBody).HasMaxLength(100);
                entity.Property(e => e.ComplianceStandardId).HasMaxLength(50);
                entity.Property(e => e.NonComplianceReason).HasMaxLength(1000);
                entity.Property(e => e.RecommendedActions).HasMaxLength(1000);
                entity.Property(e => e.CompletedActions).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                
                entity.Property(e => e.RiskScore).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ComplianceScore).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ComplianceType);
                entity.HasIndex(e => e.CheckDate);
                entity.HasIndex(e => e.DueDate);
            });

            // Configure ComplianceRule
            modelBuilder.Entity<ComplianceRule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.RuleType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ConfigurationJson).IsRequired();
                entity.Property(e => e.Severity).HasMaxLength(50).HasDefaultValue("MEDIUM");
                entity.Property(e => e.Unit).HasMaxLength(10);
                entity.Property(e => e.PenaltyCurrency).HasMaxLength(10).HasDefaultValue("KES");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.MinValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PenaltyAmount).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.RuleType);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure ComplianceViolation
            modelBuilder.Entity<ComplianceViolation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ViolationType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.VehicleId).HasMaxLength(100);
                entity.Property(e => e.DriverId).HasMaxLength(100);
                entity.Property(e => e.WeighbridgeId).HasMaxLength(100);
                entity.Property(e => e.OrganizationId).HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("ACTIVE");
                entity.Property(e => e.Severity).HasMaxLength(50).HasDefaultValue("MEDIUM");
                entity.Property(e => e.Unit).HasMaxLength(10);
                entity.Property(e => e.PenaltyCurrency).HasMaxLength(10).HasDefaultValue("KES");
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Details).HasMaxLength(2000);
                entity.Property(e => e.ResolvedBy).HasMaxLength(100);
                entity.Property(e => e.ResolutionNotes).HasMaxLength(500);
                entity.Property(e => e.AcknowledgedBy).HasMaxLength(100);
                entity.Property(e => e.ActualValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LimitValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ExcessValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PenaltyAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.ComplianceRule)
                      .WithMany(r => r.Violations)
                      .HasForeignKey(e => e.ComplianceRuleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ViolationType);
                entity.HasIndex(e => e.DetectedAt);
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => e.TransactionId);
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.DriverId);
            });

            // Configure ComplianceCheck
            modelBuilder.Entity<ComplianceCheck>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CheckType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.VehicleId).HasMaxLength(100);
                entity.Property(e => e.DriverId).HasMaxLength(100);
                entity.Property(e => e.OrganizationId).HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Result).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Unit).HasMaxLength(10);
                entity.Property(e => e.CheckDetails).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CheckedBy).HasMaxLength(100);
                entity.Property(e => e.CheckedValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LimitValue).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.ComplianceRule)
                      .WithMany(r => r.ComplianceChecks)
                      .HasForeignKey(e => e.ComplianceRuleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Result);
                entity.HasIndex(e => e.CheckedAt);
                entity.HasIndex(e => e.OrganizationId);
            });

            // Configure RegulatoryStandard
            modelBuilder.Entity<RegulatoryStandard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.StandardType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RegulatoryBody).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(50).HasDefaultValue("KENYA");
                entity.Property(e => e.Region).HasMaxLength(50);
                entity.Property(e => e.StandardDetails).IsRequired();
                entity.Property(e => e.Version).HasMaxLength(100).HasDefaultValue("1.0");
                entity.Property(e => e.DocumentReference).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                entity.HasIndex(e => e.StandardType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Country);
            });

            // Configure ComplianceAudit
            modelBuilder.Entity<ComplianceAudit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).HasMaxLength(100);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.OrganizationId).HasMaxLength(100);
                entity.Property(e => e.OldValues).IsRequired();
                entity.Property(e => e.NewValues).IsRequired();
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.IPAddress).HasMaxLength(100);
                entity.Property(e => e.UserAgent).HasMaxLength(500);

                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
            });

            // Configure LicenseMonitoring
            modelBuilder.Entity<LicenseMonitoring>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DriverId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DriverName).HasMaxLength(100);
                entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LicenseType).HasMaxLength(50);
                entity.Property(e => e.IssuingAuthority).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("ACTIVE");
                entity.Property(e => e.OrganizationId).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                entity.Ignore(e => e.DaysToExpiry);
                entity.Ignore(e => e.IsExpired);
                entity.Ignore(e => e.IsExpiringSoon);

                entity.HasIndex(e => e.DriverId);
                entity.HasIndex(e => e.LicenseNumber);
                entity.HasIndex(e => e.ExpiryDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.OrganizationId);
            });

            // Configure WeightLimits
            modelBuilder.Entity<WeightLimits>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.VehicleType).HasMaxLength(50);
                entity.Property(e => e.VehicleClass).HasMaxLength(50);
                entity.Property(e => e.WeighbridgeId).HasMaxLength(100);
                entity.Property(e => e.RouteId).HasMaxLength(100);
                entity.Property(e => e.OrganizationId).HasMaxLength(100);
                entity.Property(e => e.PenaltyCurrency).HasMaxLength(10).HasDefaultValue("KES");
                entity.Property(e => e.RegulatoryReference).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.MaxGrossWeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxAxleWeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxFrontAxleWeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxRearAxleWeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.OverweightPenaltyRate).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.VehicleType);
                entity.HasIndex(e => e.VehicleClass);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.OrganizationId);
            });

            // Configure RouteRestrictions
            modelBuilder.Entity<RouteRestrictions>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.RouteId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RouteName).HasMaxLength(100);
                entity.Property(e => e.RestrictionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.VehicleType).HasMaxLength(50);
                entity.Property(e => e.ProductType).HasMaxLength(50);
                entity.Property(e => e.DaysOfWeek).HasMaxLength(100);
                entity.Property(e => e.OrganizationId).HasMaxLength(100);
                entity.Property(e => e.PenaltyCurrency).HasMaxLength(10).HasDefaultValue("KES");
                entity.Property(e => e.Severity).HasMaxLength(50).HasDefaultValue("MEDIUM");
                entity.Property(e => e.RegulatoryReference).HasMaxLength(500);
                entity.Property(e => e.AdditionalDetails).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.MaxWeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxHeight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxWidth).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxLength).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ViolationPenalty).HasColumnType("decimal(18,2)");

                entity.Ignore(e => e.IsTimeRestricted);
                entity.Ignore(e => e.IsCurrentlyRestricted);

                entity.HasIndex(e => e.RouteId);
                entity.HasIndex(e => e.RestrictionType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.OrganizationId);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default weight limits for Kenya
            modelBuilder.Entity<WeightLimits>().HasData(
                new WeightLimits
                {
                    Id = 1,
                    Name = "Standard Truck Weight Limits",
                    Description = "Standard weight limits for trucks in Kenya",
                    VehicleType = "TRUCK",
                    VehicleClass = "STANDARD",
                    MaxGrossWeight = 56000, // 56 tons
                    MaxAxleWeight = 18000, // 18 tons
                    MaxFrontAxleWeight = 7000, // 7 tons
                    MaxRearAxleWeight = 18000, // 18 tons
                    MaxAxleCount = 5,
                    IsActive = true,
                    OverweightPenaltyRate = 5.0m,
                    PenaltyCurrency = "KES",
                    RegulatoryReference = "Traffic Act Cap 403",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SYSTEM",
                    UpdatedBy = "SYSTEM"
                }
            );

            // Seed default compliance rules
            modelBuilder.Entity<ComplianceRule>().HasData(
                new ComplianceRule
                {
                    Id = 1,
                    Name = "Gross Weight Compliance",
                    Description = "Check if vehicle gross weight exceeds legal limits",
                    RuleType = "WEIGHT",
                    Category = "GROSS_WEIGHT",
                    ConfigurationJson = "{\"maxWeight\": 56000, \"unit\": \"KG\", \"penaltyRate\": 5.0}",
                    IsActive = true,
                    Severity = "HIGH",
                    MaxValue = 56000,
                    Unit = "KG",
                    PenaltyAmount = 5.0m,
                    PenaltyCurrency = "KES",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SYSTEM",
                    UpdatedBy = "SYSTEM"
                },
                new ComplianceRule
                {
                    Id = 2,
                    Name = "License Expiry Check",
                    Description = "Monitor driver license expiry dates",
                    RuleType = "LICENSE",
                    Category = "EXPIRY",
                    ConfigurationJson = "{\"warningDays\": 30, \"criticalDays\": 7}",
                    IsActive = true,
                    Severity = "CRITICAL",
                    MinValue = 0,
                    Unit = "DAYS",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SYSTEM",
                    UpdatedBy = "SYSTEM"
                }
            );
        }
    }
}