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