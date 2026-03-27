using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TechnicianApi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TechnicianApi.Infrastructure.Data;

public class TechnicianApiDbContext : DbContext
{
    public TechnicianApiDbContext(DbContextOptions<TechnicianApiDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<CheckIn> CheckIns { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<ServiceReport> ServiceReports { get; set; }
    public DbSet<Requisition> Requisitions { get; set; }

    // Financial forms
    public DbSet<PettyCashAdvanceForm> PettyCashAdvanceForms { get; set; }
    public DbSet<AdvanceReturnForm> AdvanceReturnForms { get; set; }
    public DbSet<AdvanceReturnLineItem> AdvanceReturnLineItems { get; set; }
    public DbSet<PerDiemReturnForm> PerDiemReturnForms { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Refund> Refunds { get; set; }
    public DbSet<AssignmentBalanceSummary> AssignmentBalanceSummaries { get; set; }

    // Performance tracking
    public DbSet<DailySummary> DailySummaries { get; set; }
    public DbSet<PerformanceMetrics> PerformanceMetrics { get; set; }
    
    // File attachments
    public DbSet<Attachment> Attachments { get; set; }

    // QTruck Entities - Driver Management
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<DriverProfile> DriverProfiles { get; set; }
    public DbSet<DriverActivity> DriverActivities { get; set; }
    public DbSet<DriverProfileChange> DriverProfileChanges { get; set; }
    public DbSet<LicenseClass> LicenseClasses { get; set; }

    // QTruck Entities - Fleet Management
    public DbSet<Truck> Trucks { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<MaterialVariant> MaterialVariants { get; set; }
    public DbSet<MaterialCost> MaterialCosts { get; set; }
    public DbSet<MaterialPhoto> MaterialPhotos { get; set; }
    public DbSet<MaterialVariantPhoto> MaterialVariantPhotos { get; set; }

    // QTruck Entities - Trip Management
    public DbSet<TripType> TripTypes { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<TripMaterial> TripMaterials { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<VehicleMileage> VehicleMileages { get; set; }

    // QTruck Entities - Feedback & Settings
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<SystemSettings> SystemSettingsTable { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Assignment entity
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ManagerId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LocationName).HasMaxLength(200);
            entity.Property(e => e.LocationAddress).HasMaxLength(500);

            // FIXED: TechnicianIds with correct ValueComparer for ICollection<string>
            entity.Property(e => e.TechnicianIds)
                .HasColumnName("TechnicianIds")
                .HasConversion(
                    v => string.Join(',', v),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<ICollection<string>>(
                        (c1, c2) => CompareCollections(c1, c2),
                        c => GetCollectionHashCode(c),
                        c => c != null ? new List<string>(c) : new List<string>()
                    )
                );

            entity.HasIndex(e => e.ManagerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Deadline);
        });

        // Configure CheckIn entity
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Assignment)
                .WithOne(a => a.CheckIn)
                .HasForeignKey<CheckIn>(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => e.CheckInTime);
        });

        // Configure Photo entity
        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.StorageUrl).HasMaxLength(1000);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.Caption).HasMaxLength(500);

            entity.HasOne(e => e.Assignment)
                .WithMany(a => a.Photos)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.CapturedAt);
        });

        // Configure ServiceReport entity
        modelBuilder.Entity<ServiceReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianName).IsRequired().HasMaxLength(200);

            // Customer & Location
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LocationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LocationAddress).IsRequired().HasMaxLength(500);

            // Contact Person
            entity.Property(e => e.ContactPerson).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Designation).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MobileNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(200);

            // Visit Information
            entity.Property(e => e.VehicleNo).HasMaxLength(50);

            // Machine & Service Details
            entity.Property(e => e.MachineDetails).HasMaxLength(1000);
            entity.Property(e => e.FaultReported).HasMaxLength(2000);
            entity.Property(e => e.Findings).HasMaxLength(2000);
            entity.Property(e => e.Correction).HasMaxLength(2000);
            entity.Property(e => e.FinalResult).HasMaxLength(2000);
            entity.Property(e => e.PartsOffered).HasMaxLength(2000);

            // Customer Feedback
            entity.Property(e => e.CustomerComments).HasMaxLength(2000);

            // Rejection
            entity.Property(e => e.RejectionReason).HasMaxLength(500);

            entity.HasOne(e => e.Assignment)
                .WithOne(a => a.ServiceReport)
                .HasForeignKey<ServiceReport>(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Technician>()
                .WithMany()
                .HasForeignKey(e => e.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.AssignmentId).IsUnique();
            entity.HasIndex(e => e.TechnicianId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NatureOfVisit);
            entity.HasIndex(e => e.SubmittedAt);
        });

        // Configure Requisition entity
        modelBuilder.Entity<Requisition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ItemsList).HasMaxLength(2000);
            entity.Property(e => e.Justification).HasMaxLength(1000);
            entity.Property(e => e.VoucherNumber).HasMaxLength(100);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);

            entity.HasOne(e => e.Assignment)
                .WithMany(a => a.Requisitions)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Technician>()
                .WithMany()
                .HasForeignKey(e => e.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => e.TechnicianId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
        });

        // Configure PettyCashAdvanceForm entity
        modelBuilder.Entity<PettyCashAdvanceForm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Sum).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.PreparedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ApprovedBy).HasMaxLength(200);
            entity.Property(e => e.ApprovalComments).HasMaxLength(1000);
            entity.Property(e => e.RejectedBy).HasMaxLength(200);
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);
            entity.Property(e => e.DisbursedBy).HasMaxLength(200);
            entity.Property(e => e.VoucherNumber).HasMaxLength(100);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);

            entity.HasOne(e => e.Assignment)
                .WithMany(a => a.PettyCashAdvanceForms)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PreparedBy);
        });

        // Configure AdvanceReturnForm entity
        modelBuilder.Entity<AdvanceReturnForm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.ApprovedBy).HasMaxLength(200);
            entity.Property(e => e.ApprovalComments).HasMaxLength(1000);
            entity.Property(e => e.RejectedBy).HasMaxLength(200);
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);

            entity.HasOne(e => e.Assignment)
                .WithMany(a => a.AdvanceReturnForms)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Technician>()
                .WithMany()
                .HasForeignKey(e => e.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => e.TechnicianId);
            entity.HasIndex(e => e.Status);
        });

        // Configure AdvanceReturnLineItem entity
        modelBuilder.Entity<AdvanceReturnLineItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AdvanceReturnFormId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Particular).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AmountInKsh).HasPrecision(18, 2).IsRequired();

            entity.HasOne(e => e.AdvanceReturnForm)
                .WithMany(f => f.LineItems)
                .HasForeignKey(e => e.AdvanceReturnFormId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AdvanceReturnFormId);
        });

        // Configure PerDiemReturnForm entity
        modelBuilder.Entity<PerDiemReturnForm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProjectName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FaresOrCarExpense).HasPrecision(18, 2);
            entity.Property(e => e.Mileage).HasPrecision(18, 2);
            entity.Property(e => e.Meals).HasPrecision(18, 2);
            entity.Property(e => e.Medical).HasPrecision(18, 2);
            entity.Property(e => e.Incidentals).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.ApprovedBy).HasMaxLength(200);
            entity.Property(e => e.ApprovalComments).HasMaxLength(1000);
            entity.Property(e => e.RejectedBy).HasMaxLength(200);
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);

            entity.HasOne(e => e.Assignment)
                .WithMany(a => a.PerDiemReturnForms)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Technician>()
                .WithMany()
                .HasForeignKey(e => e.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => e.TechnicianId);
            entity.HasIndex(e => e.Status);
        });

        // Configure Claim entity
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.SupportingDocuments).HasMaxLength(2000);
            entity.Property(e => e.Justification).HasMaxLength(1000);
            entity.Property(e => e.ManagerReviewedBy).HasMaxLength(200);
            entity.Property(e => e.ManagerComments).HasMaxLength(1000);
            entity.Property(e => e.CfoReviewedBy).HasMaxLength(200);
            entity.Property(e => e.CfoComments).HasMaxLength(1000);
            entity.Property(e => e.DisbursedBy).HasMaxLength(200);
            entity.Property(e => e.VoucherNumber).HasMaxLength(100);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.PaymentMethod).HasMaxLength(100);
            entity.Property(e => e.RejectedBy).HasMaxLength(200);
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);

            entity.HasOne(e => e.Assignment)
                .WithMany(a => a.Claims)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Technician>()
                .WithMany()
                .HasForeignKey(e => e.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => e.TechnicianId);
            entity.HasIndex(e => e.Status);
        });

        // Configure Refund entity
        modelBuilder.Entity<Refund>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.PaymentMethod).HasMaxLength(100);
            entity.Property(e => e.ReceiptNumber).HasMaxLength(100);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.ManagerReviewedBy).HasMaxLength(200);
            entity.Property(e => e.ManagerComments).HasMaxLength(1000);
            entity.Property(e => e.CfoReviewedBy).HasMaxLength(200);
            entity.Property(e => e.CfoComments).HasMaxLength(1000);
            entity.Property(e => e.ReceivedBy).HasMaxLength(200);
            entity.Property(e => e.RejectedBy).HasMaxLength(200);
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);

            entity.HasOne(e => e.Assignment)
                .WithMany(a => a.Refunds)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Technician>()
                .WithMany()
                .HasForeignKey(e => e.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => e.TechnicianId);
            entity.HasIndex(e => e.Status);
        });

        // Configure AssignmentBalanceSummary entity
        modelBuilder.Entity<AssignmentBalanceSummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignmentId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TotalAdvancesGiven).HasPrecision(18, 2);
            entity.Property(e => e.TotalReturns).HasPrecision(18, 2);
            entity.Property(e => e.TotalExpensesClaimed).HasPrecision(18, 2);
            entity.Property(e => e.TotalMaterialsRequisitioned).HasPrecision(18, 2);
            entity.Property(e => e.TotalRefunds).HasPrecision(18, 2);
            entity.Property(e => e.TotalMoneyOut).HasPrecision(18, 2);
            entity.Property(e => e.TotalMoneyAccountedFor).HasPrecision(18, 2);
            entity.Property(e => e.NetBalance).HasPrecision(18, 2);
            entity.Property(e => e.RecommendedAmount).HasPrecision(18, 2);
            entity.Property(e => e.RecommendationMessage).HasMaxLength(500);
            entity.Property(e => e.ReconciledBy).HasMaxLength(200);
            entity.Property(e => e.ReconciliationNotes).HasMaxLength(2000);
            entity.Property(e => e.ManagerApprovedBy).HasMaxLength(200);
            entity.Property(e => e.CfoApprovedBy).HasMaxLength(200);

            entity.HasOne(e => e.Assignment)
                .WithOne(a => a.BalanceSummary)
                .HasForeignKey<AssignmentBalanceSummary>(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Technician>()
                .WithMany()
                .HasForeignKey(e => e.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.AssignmentId).IsUnique();
            entity.HasIndex(e => e.TechnicianId);
            entity.HasIndex(e => e.BalanceStatus);
            entity.HasIndex(e => e.RecommendedAction);
            entity.HasIndex(e => e.IsReconciled);
        });

        // Configure DailySummary entity
        modelBuilder.Entity<DailySummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PerformanceScore).HasPrecision(5, 2);
            entity.Property(e => e.TotalRequisitionAmount).HasPrecision(18, 2);
            entity.Property(e => e.AutoGeneratedNotes).HasMaxLength(2000);

            entity.HasOne<Technician>()
                .WithMany()
                .HasForeignKey(e => e.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TechnicianId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.TechnicianId, e.Date }).IsUnique();
        });

        // Configure PerformanceMetrics entity
        modelBuilder.Entity<PerformanceMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TechnicianId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PerformanceScore).HasPrecision(5, 2);
            entity.Property(e => e.CompletionRate).HasPrecision(5, 2);
            entity.Property(e => e.OnTimeRate).HasPrecision(5, 2);
            entity.Property(e => e.ReportApprovalRate).HasPrecision(5, 2);
            entity.Property(e => e.TotalRequisitionAmount).HasPrecision(18, 2);

            entity.HasOne<Technician>()
                .WithMany()
                .HasForeignKey(e => e.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TechnicianId);
            entity.HasIndex(e => e.PeriodStart);
            entity.HasIndex(e => e.PeriodEnd);
        });

        // Configure QTruck Entities

        // Driver entity
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.UserId);
        });

        // DriverProfile entity
        modelBuilder.Entity<DriverProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DriverId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Driver)
                .WithMany(d => d.ProfileVersions)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.DriverId, e.IsCurrent });
        });

        // DriverActivity entity
        modelBuilder.Entity<DriverActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DriverId).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Driver)
                .WithMany(d => d.Activities)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.ActivityType);
            entity.HasIndex(e => e.CreatedAt);
        });

        // DriverProfileChange entity
        modelBuilder.Entity<DriverProfileChange>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NewProfileId).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.OldProfile)
                .WithMany()
                .HasForeignKey(e => e.OldProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NewProfile)
                .WithMany(p => p.Changes)
                .HasForeignKey(e => e.NewProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.NewProfileId);
        });

        // LicenseClass entity
        modelBuilder.Entity<LicenseClass>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Truck entity
        modelBuilder.Entity<Truck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Driver)
                .WithMany()
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.LicensePlate).IsUnique();
            entity.HasIndex(e => e.DriverId);
        });

        // Material entity
        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // MaterialVariant entity
        modelBuilder.Entity<MaterialVariant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MaterialId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.Variants)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.MaterialId);
            entity.HasIndex(e => new { e.MaterialId, e.Name }).IsUnique();
        });

        // MaterialCost entity
        modelBuilder.Entity<MaterialCost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MaterialId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Cost).HasPrecision(18, 2);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.Costs)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.MaterialId);
        });

        // MaterialPhoto entity
        modelBuilder.Entity<MaterialPhoto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MaterialId).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.Photos)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.MaterialId);
        });

        // MaterialVariantPhoto entity
        modelBuilder.Entity<MaterialVariantPhoto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MaterialVariantId).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.MaterialVariant)
                .WithMany(mv => mv.Photos)
                .HasForeignKey(e => e.MaterialVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.MaterialVariantId);
        });

        // TripType entity
        modelBuilder.Entity<TripType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // Trip entity
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TruckId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DriverId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.MaterialCost).HasPrecision(18, 2);

            entity.HasOne(e => e.Truck)
                .WithMany(t => t.Trips)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Driver)
                .WithMany(d => d.Trips)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TripType)
                .WithMany(tt => tt.Trips)
                .HasForeignKey(e => e.TripTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.Trips)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.MaterialVariant)
                .WithMany(mv => mv.Trips)
                .HasForeignKey(e => e.MaterialVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.TruckId);
            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Date);
        });

        // TripMaterial entity
        modelBuilder.Entity<TripMaterial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TripId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MaterialId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.TripMaterials)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.TripMaterials)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.MaterialVariant)
                .WithMany(mv => mv.TripMaterials)
                .HasForeignKey(e => e.MaterialVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.TripId);
            entity.HasIndex(e => new { e.TripId, e.MaterialId, e.MaterialVariantId }).IsUnique();
        });

        // Expense entity
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TripId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TripId);
        });

        // Receipt entity
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExpenseId).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Expense)
                .WithMany(ex => ex.Receipts)
                .HasForeignKey(e => e.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ExpenseId);
        });

        // VehicleMileage entity
        modelBuilder.Entity<VehicleMileage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TruckId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DriverId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StartMileage).HasPrecision(18, 2);
            entity.Property(e => e.EndMileage).HasPrecision(18, 2);
            entity.Property(e => e.Mileage).HasPrecision(18, 2);

            entity.HasOne(e => e.Truck)
                .WithMany(t => t.VehicleMileages)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Driver)
                .WithMany(d => d.VehicleMileages)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TruckId);
            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.Date);
        });

        // Feedback entity
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.FeedbackType);
        });

        // SystemSettings entity
        modelBuilder.Entity<SystemSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Global query filters for soft deletes
        modelBuilder.Entity<Technician>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Assignment>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<AssignmentTechnician>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<CheckIn>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Photo>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<ServiceReport>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Requisition>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<DailySummary>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<PerformanceMetrics>().HasQueryFilter(e => e.IsDeleted == false);

        // QTruck entities soft delete filters
        modelBuilder.Entity<Driver>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<DriverProfile>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<DriverActivity>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<DriverProfileChange>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<LicenseClass>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Truck>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Material>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<MaterialVariant>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<MaterialCost>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<MaterialPhoto>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<MaterialVariantPhoto>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<TripType>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Trip>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<TripMaterial>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Expense>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Receipt>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<VehicleMileage>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<Feedback>().HasQueryFilter(e => e.IsDeleted == false);
        modelBuilder.Entity<SystemSettings>().HasQueryFilter(e => e.IsDeleted == false);
    }

    // Helper for ValueComparer equality
    private static bool CompareCollections(ICollection<string>? left, ICollection<string>? right)
    {
        var l = left ?? new List<string>();
        var r = right ?? new List<string>();
        return l.SequenceEqual(r);
    }

    // Helper for ValueComparer hash code
    private static int GetCollectionHashCode(ICollection<string>? collection)
    {
        if (collection == null || collection.Count == 0) return 0;

        var hash = 0;
        foreach (var item in collection)
        {
            hash = HashCode.Combine(hash, item?.GetHashCode() ?? 0);
        }
        return hash;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}