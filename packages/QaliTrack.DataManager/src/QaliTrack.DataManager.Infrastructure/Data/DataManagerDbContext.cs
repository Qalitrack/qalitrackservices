using Microsoft.EntityFrameworkCore;
using QaliTrack.DataManager.Core.Modules.Transactions.Entities;
using QaliTrack.DataManager.Core.Modules.WeightData.Entities;
using QaliTrack.DataManager.Core.Modules.Orders.Entities;
using QaliTrack.DataManager.Core.Modules.Quality.Entities;
using QaliTrack.DataManager.Core.Modules.Operations.Entities;

namespace QaliTrack.DataManager.Infrastructure.Data;

public class DataManagerDbContext : DbContext
{
    public DataManagerDbContext(DbContextOptions<DataManagerDbContext> options) : base(options)
    {
    }

    #region Transaction Module Entities
    public DbSet<WeighingTransaction> WeighingTransactions { get; set; }
    public DbSet<TransactionLine> TransactionLines { get; set; }
    public DbSet<TransactionAudit> TransactionAudits { get; set; }
    #endregion

    #region WeightData Module Entities
    public DbSet<WeightMeasurement> WeightMeasurements { get; set; }
    public DbSet<CalibrationRecord> CalibrationRecords { get; set; }
    #endregion

    #region Orders Module Entities
    public DbSet<CustomerOrder> CustomerOrders { get; set; }
    public DbSet<CustomerOrderLine> CustomerOrderLines { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
    public DbSet<InterPlantTransfer> InterPlantTransfers { get; set; }
    public DbSet<TransferLine> TransferLines { get; set; }
    #endregion

    #region Quality Module Entities
    public DbSet<QualityTestResult> QualityTestResults { get; set; }
    public DbSet<SealRecord> SealRecords { get; set; }
    public DbSet<VehicleIncident> VehicleIncidents { get; set; }
    #endregion


    #region Operations Module Entities (Keep existing) 
    public DbSet<OperationalAlert> OperationalAlerts { get; set; }
    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureTransactionModule(modelBuilder);
        ConfigureWeightDataModule(modelBuilder);
        ConfigureOrdersModule(modelBuilder);
        ConfigureQualityModule(modelBuilder);
        ConfigureOperationsModule(modelBuilder);
        ConfigureIndexes(modelBuilder);
        ConfigureGlobalFilters(modelBuilder);
    }

    private void ConfigureTransactionModule(ModelBuilder modelBuilder)
    {
        // WeighingTransaction configuration
        modelBuilder.Entity<WeighingTransaction>(entity =>
        {
            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.HasIndex(e => new { e.SiteId, e.TransactionDate });
            entity.HasIndex(e => new { e.VehicleId, e.Status });
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.TransactionNumber).HasMaxLength(100);
            entity.Property(e => e.TransactionType).HasMaxLength(50);
            entity.Property(e => e.Direction).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.CurrentState).HasMaxLength(50);
        });

        // TransactionLine configuration
        modelBuilder.Entity<TransactionLine>(entity =>
        {
            entity.HasIndex(e => new { e.TransactionId, e.ProductVariantId });
            
            entity.HasOne(tl => tl.Transaction)
                .WithMany(t => t.TransactionLines)
                .HasForeignKey(tl => tl.TransactionId);
        });

        // TransactionAudit configuration
        modelBuilder.Entity<TransactionAudit>(entity =>
        {
            entity.HasIndex(e => new { e.TransactionId, e.PerformedAt });
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.PerformedBy).HasMaxLength(200);

            entity.HasOne(ta => ta.Transaction)
                .WithMany(t => t.AuditTrail)
                .HasForeignKey(ta => ta.TransactionId);
        });
    }

    private void ConfigureWeightDataModule(ModelBuilder modelBuilder)
    {
        // WeightMeasurement configuration
        modelBuilder.Entity<WeightMeasurement>(entity =>
        {
            entity.HasIndex(e => new { e.TransactionId, e.MeasurementType });
            entity.HasIndex(e => new { e.WeighbridgeId, e.MeasurementTime });
            entity.Property(e => e.Weight).HasPrecision(18, 4);
            entity.Property(e => e.Temperature).HasPrecision(18, 4);
            entity.Property(e => e.Humidity).HasPrecision(18, 4);
            entity.Property(e => e.StabilityVariance).HasPrecision(18, 6);
        });

        // CalibrationRecord configuration
        modelBuilder.Entity<CalibrationRecord>(entity =>
        {
            entity.HasIndex(e => new { e.WeighbridgeId, e.CalibrationDate });
            entity.Property(e => e.AccuracyValue).HasPrecision(18, 6);
            entity.Property(e => e.ToleranceLevel).HasPrecision(18, 6);
        });
    }

    private void ConfigureOrdersModule(ModelBuilder modelBuilder)
    {
        // CustomerOrder configuration
        modelBuilder.Entity<CustomerOrder>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => new { e.CustomerId, e.OrderDate });
            entity.HasIndex(e => new { e.FromSiteId, e.Status });
            entity.Property(e => e.OrderNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 4);
        });

        // CustomerOrderLine configuration
        modelBuilder.Entity<CustomerOrderLine>(entity =>
        {
            entity.HasIndex(e => new { e.CustomerOrderId, e.ProductVariantId });
            entity.Property(e => e.QuantityOrdered).HasPrecision(18, 4);
            entity.Property(e => e.QuantityDelivered).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);

            entity.HasOne(col => col.CustomerOrder)
                .WithMany(co => co.OrderLines)
                .HasForeignKey(col => col.CustomerOrderId);
        });

        // PurchaseOrder configuration
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => new { e.SupplierId, e.OrderDate });
            entity.Property(e => e.OrderNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 4);
        });

        // PurchaseOrderLine configuration
        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.HasIndex(e => new { e.PurchaseOrderId, e.ProductVariantId });
            entity.Property(e => e.QuantityOrdered).HasPrecision(18, 4);
            entity.Property(e => e.QuantityReceived).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);

            entity.HasOne(pol => pol.PurchaseOrder)
                .WithMany(po => po.OrderLines)
                .HasForeignKey(pol => pol.PurchaseOrderId);
        });

        // InterPlantTransfer configuration
        modelBuilder.Entity<InterPlantTransfer>(entity =>
        {
            entity.HasIndex(e => e.TransferNumber).IsUnique();
            entity.HasIndex(e => new { e.FromSiteId, e.ToSiteId, e.TransferDate });
            entity.Property(e => e.TransferNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransferType).HasMaxLength(50);
        });

        // TransferLine configuration
        modelBuilder.Entity<TransferLine>(entity =>
        {
            entity.HasIndex(e => new { e.InterPlantTransferId, e.ProductVariantId });
            entity.Property(e => e.QuantityRequested).HasPrecision(18, 4);
            entity.Property(e => e.QuantityTransferred).HasPrecision(18, 4);

            entity.HasOne(tl => tl.InterPlantTransfer)
                .WithMany(ipt => ipt.TransferLines)
                .HasForeignKey(tl => tl.InterPlantTransferId);
        });
    }

    private void ConfigureQualityModule(ModelBuilder modelBuilder)
    {
        // QualityTestResult configuration
        modelBuilder.Entity<QualityTestResult>(entity =>
        {
            entity.HasIndex(e => new { e.TransactionId, e.ProductSpecificationId });
            entity.HasIndex(e => e.BatchNumber);
            entity.Property(e => e.TestValue).HasPrecision(18, 6);
            entity.Property(e => e.BatchNumber).HasMaxLength(100);
            entity.Property(e => e.TestUnit).HasMaxLength(50);
            entity.Property(e => e.TestResult).HasMaxLength(50);
        });

        // SealRecord configuration
        modelBuilder.Entity<SealRecord>(entity =>
        {
            entity.HasIndex(e => new { e.TransactionId, e.VehicleId });
            entity.HasIndex(e => e.SealNumber).IsUnique();
            entity.Property(e => e.SealNumber).HasMaxLength(100);
            entity.Property(e => e.SealType).HasMaxLength(50);
            entity.Property(e => e.SealStatus).HasMaxLength(50);
        });

        // VehicleIncident configuration
        modelBuilder.Entity<VehicleIncident>(entity =>
        {
            entity.HasIndex(e => new { e.VehicleId, e.Status });
            entity.HasIndex(e => new { e.Status, e.BlocksVehicle });
            entity.Property(e => e.IncidentType).HasMaxLength(100);
            entity.Property(e => e.Severity).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(vi => vi.SealRecord)
                .WithMany(sr => sr.VehicleIncidents)
                .HasForeignKey(vi => vi.SealRecordId)
                .IsRequired(false);
        });
    }


    private void ConfigureOperationsModule(ModelBuilder modelBuilder)
    {
        // Keep simplified Operations configuration
        modelBuilder.Entity<OperationalAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertType).HasMaxLength(100);
            entity.Property(e => e.Severity).HasMaxLength(50);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<MaintenanceSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MaintenanceType).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.EstimatedCost).HasPrecision(18, 4);
            entity.Property(e => e.ActualCost).HasPrecision(18, 4);
            
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.MaintenanceSchedule)
                .HasForeignKey(e => e.MaintenanceScheduleId);
        });

        modelBuilder.Entity<MaintenanceTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskName).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TaskCost).HasPrecision(18, 4);
        });
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Performance indexes for cement operations
        modelBuilder.Entity<WeighingTransaction>()
            .HasIndex(e => new { e.SiteId, e.Status, e.TransactionDate })
            .HasDatabaseName("IX_WeighingTransactions_Site_Status_Date");

        modelBuilder.Entity<WeighingTransaction>()
            .HasIndex(e => new { e.VehicleId, e.DriverId })
            .HasDatabaseName("IX_WeighingTransactions_Vehicle_Driver");

        modelBuilder.Entity<CustomerOrder>()
            .HasIndex(e => new { e.FromSiteId, e.Status, e.OrderDate })
            .HasDatabaseName("IX_CustomerOrders_Site_Status_Date");

        modelBuilder.Entity<QualityTestResult>()
            .HasIndex(e => new { e.BatchNumber, e.TestDate })
            .HasDatabaseName("IX_QualityTestResults_Batch_Date");

        modelBuilder.Entity<VehicleIncident>()
            .HasIndex(e => new { e.VehicleId, e.Status, e.BlocksVehicle })
            .HasDatabaseName("IX_VehicleIncidents_Vehicle_Status_Blocks");

        // Global soft delete indexes
        modelBuilder.Entity<WeighingTransaction>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<CustomerOrder>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<PurchaseOrder>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<InterPlantTransfer>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<QualityTestResult>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<SealRecord>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<VehicleIncident>().HasIndex(e => e.IsDeleted);
    }

    private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
    {
        // Global soft delete filters for all BaseEntity entities
        modelBuilder.Entity<WeighingTransaction>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransactionLine>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransactionAudit>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WeightMeasurement>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CalibrationRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CustomerOrder>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CustomerOrderLine>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PurchaseOrderLine>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InterPlantTransfer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransferLine>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<QualityTestResult>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SealRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleIncident>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is QaliTrack.DataManager.Core.Common.BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (QaliTrack.DataManager.Core.Common.BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}