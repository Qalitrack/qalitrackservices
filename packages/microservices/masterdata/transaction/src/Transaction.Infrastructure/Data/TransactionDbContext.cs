using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Transaction.Core.Entities;

namespace Transaction.Infrastructure.Data;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
    {
    }

    public DbSet<WeighbridgeTransaction> Transactions { get; set; }
    public DbSet<ReweighRecord> ReweighRecords { get; set; }
    public DbSet<TransactionSettings> Settings { get; set; }
    public DbSet<TransactionAuditLog> TransactionAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("transactions");

        ConfigureWeighbridgeTransaction(modelBuilder);
        ConfigureReweighRecord(modelBuilder);
        ConfigureTransactionSettings(modelBuilder);
        ConfigureTransactionAuditLog(modelBuilder);

        // Add global query filter for soft deletes
        modelBuilder.Entity<WeighbridgeTransaction>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReweighRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransactionSettings>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransactionAuditLog>().HasQueryFilter(e => !e.IsDeleted);
    }

    private void ConfigureWeighbridgeTransaction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeighbridgeTransaction>(entity =>
        {
            // Map to MySQL table name
            entity.ToTable("tickets");

            // Optimistic concurrency token (Postgres's built-in xmin system
            // column) — without this, two operators editing the same ticket
            // at once (e.g. one calling Update while another calls
            // AddSecondWeight) silently last-write-wins with no error and no
            // audit trace of the lost change. With it, the loser's
            // SaveChangesAsync throws DbUpdateConcurrencyException instead.
            entity.Property<uint>("xmin")
                  .HasColumnName("xmin")
                  .HasColumnType("xid")
                  .ValueGeneratedOnAddOrUpdate()
                  .IsRowVersion();


            // Primary Key - Updated for GUID support
            entity.HasKey(e => e.TicketID);
            entity.Property(e => e.TicketID)
                  .HasColumnName("TicketID")
                  .HasMaxLength(50)
                  .ValueGeneratedOnAdd();

            // Receipt and Identification
            entity.Property(e => e.ReceiptNo)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasColumnName("ReceiptNo");

            // Weight Information
            entity.Property(e => e.FirstWeight)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasColumnName("FirstWeight");
                  
            entity.Property(e => e.SecondWeight)
                  .HasMaxLength(20)
                  .HasColumnName("SecondWeight");
                  
            entity.Property(e => e.NetWeight)
                  .HasMaxLength(20)
                  .HasColumnName("NetWeight");

            // Vehicle Information - Updated for GUID support
            entity.Property(e => e.VehicleID)
                  .HasMaxLength(50)
                  .HasColumnName("vehicleID");
            entity.Property(e => e.NoPlate)
                  .IsRequired()
                  .HasMaxLength(20)  // Increased from 7 to 20
                  .HasColumnName("NoPlate");
            entity.Property(e => e.DriverName)
                  .IsRequired()
                  .HasMaxLength(100)  // Increased from 50 to 100
                  .HasColumnName("DriverName");

            // Commodity Information - Updated for GUID support
            entity.Property(e => e.CommodityID)
                  .HasMaxLength(50)
                  .HasColumnName("CommodityID");
            entity.Property(e => e.CommodityName)
                  .HasMaxLength(100)
                  .HasColumnName("CommodityName");

            // Supplier Information - Updated for GUID support
            entity.Property(e => e.SupplierID)
                  .HasMaxLength(50)
                  .HasColumnName("SupplierID");
            entity.Property(e => e.SupplierName)
                  .HasMaxLength(100)
                  .HasColumnName("SupplierName");

            // Customer Information - Updated for GUID support
            entity.Property(e => e.CustomerID)
                  .HasMaxLength(50)
                  .HasColumnName("CustomerID");
            entity.Property(e => e.CustomerName)
                  .HasMaxLength(100)
                  .HasColumnName("CustomerName");

            // Transporter Information - Updated for GUID support
            entity.Property(e => e.TransporterID)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasColumnName("TransporterID");
            entity.Property(e => e.TransporterName)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasColumnName("TransporterName");

            // Origin and Destination - Updated for GUID support
            entity.Property(e => e.OriginID)
                  .HasMaxLength(50)
                  .HasColumnName("OriginID");
            entity.Property(e => e.OriginName)
                  .HasMaxLength(100)  // Increased from 50 to 100
                  .HasColumnName("OriginName");
            entity.Property(e => e.DestinationID)
                  .HasMaxLength(50)
                  .HasColumnName("DestinationID");
            entity.Property(e => e.DestinationName)
                  .HasMaxLength(100)
                  .HasColumnName("DestinationName");

            // Weighbridge Information - First Weighing - Updated for GUID support
            entity.Property(e => e.WeighBridgeID)
                  .HasMaxLength(50)
                  .HasColumnName("WeighBridgeID");
            entity.Property(e => e.WeighBridgeName)
                  .HasMaxLength(100)  // Increased from 50 to 100
                  .HasColumnName("WeighBridgeName");
            entity.Property(e => e.ScaleName)
                  .HasMaxLength(100)
                  .HasColumnName("ScaleName");
            entity.Property(e => e.OperatorID)
                  .HasMaxLength(50)
                  .HasColumnName("OperatorID");
            entity.Property(e => e.OperatorName)
                  .HasMaxLength(100)
                  .HasColumnName("OperatorName");

            // Weighbridge Information - Second Weighing
            entity.Property(e => e.WeighBridgeName2nd)
                  .HasMaxLength(100)
                  .HasColumnName("WeighBridgeName2nd");
            entity.Property(e => e.ScaleName2nd)
                  .HasMaxLength(100)
                  .HasColumnName("ScaleName2nd");
            entity.Property(e => e.OperatorID2nd)
                  .HasMaxLength(50)  // Increased from 10 to 50
                  .HasColumnName("OperatorID2nd");
            entity.Property(e => e.OperatorName2nd)
                  .HasMaxLength(100)
                  .HasColumnName("OperatorName2nd");

            // Operational Details
            entity.Property(e => e.WeighMode)
                  .HasMaxLength(20)
                  .HasColumnName("WeighMode");
            entity.Property(e => e.Operation)
                  .HasMaxLength(100)
                  .HasColumnName("Operation");

            // Date Information
            entity.Property(e => e.FirstWeightDate)
                  .HasColumnName("FirstWeightDate")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.SecondWeightDate)
                  .HasColumnName("SecondWeightDate")
                  .IsRequired(false);

            // Transaction Status and Modifications
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasDefaultValue("Active")
                  .HasColumnName("Status");
            entity.Property(e => e.ReweighPermission)
                  .HasMaxLength(200)
                  .HasColumnName("ReweighPermission");
            entity.Property(e => e.ChangeDesc)
                  .HasMaxLength(150)
                  .HasColumnName("ChangeDesc");
            entity.Property(e => e.ChangeDate)
                  .HasColumnName("ChangeDate")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAddOrUpdate();

            // API Integration - Updated for GUID support
            entity.Property(e => e.ApiId)
                  .HasMaxLength(50)
                  .HasColumnName("api_id");

            // NPR capture source
            entity.Property(e => e.NprSource)
                  .HasMaxLength(10)
                  .HasColumnName("npr_source");

            // Reweigh flag
            entity.Property(e => e.IsReweighed)
                  .HasDefaultValue(false)
                  .HasColumnName("is_reweighed");

            // Indexes
            // Unique among active tickets — matches IsReceiptNoAvailableAsync's
            // existing "!IsDeleted" semantics, and turns a receipt-number
            // collision (two concurrent creations computing the same "next"
            // number) into a loud constraint violation instead of a silently
            // duplicated receipt. CreateWithUniqueReceiptNoAsync retries on it.
            entity.HasIndex(e => e.ReceiptNo).IsUnique().HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(e => e.NoPlate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.FirstWeightDate);
            // Covers the default list/dashboard query shape: filter by
            // Status, sort by FirstWeightDate.
            entity.HasIndex(e => new { e.Status, e.FirstWeightDate });
            // Standalone dimension filters used by GetPagedAsync that had no
            // supporting index at all.
            entity.HasIndex(e => e.VehicleID);
            entity.HasIndex(e => e.CommodityID);
        });
    }

    private void ConfigureReweighRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReweighRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Change foreign key to support GUID strings
            entity.Property(e => e.WeighbridgeTransactionId)
                  .HasMaxLength(50)
                  .HasColumnName("WeighbridgeTransactionId");
                  
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.PerformedBy).HasMaxLength(100);
            entity.Property(e => e.Operator1).HasMaxLength(100);
            entity.Property(e => e.Operator2).HasMaxLength(100);

            // Configure precision for decimal properties
            entity.Property(e => e.Weight1).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Weight2).HasColumnType("decimal(18,2)");
            entity.Property(e => e.NetWeight).HasColumnType("decimal(18,2)");

            // Indexes
            entity.HasIndex(e => e.WeighbridgeTransactionId);
            entity.HasIndex(e => e.AttemptNumber);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.Status);
        });
    }

    private void ConfigureTransactionSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionSettings>(entity =>
        {
            entity.ToTable("transaction_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceiptPrefix)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("NCCU");
        });
    }

    private void ConfigureTransactionAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionAuditLog>(entity =>
        {
            entity.ToTable("transaction_audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeTransactionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(30);
            entity.Property(e => e.ChangedBy).HasMaxLength(200);
            entity.Property(e => e.ChangedFields).HasColumnType("text");
            entity.Property(e => e.OldValues).HasColumnType("text");
            entity.Property(e => e.NewValues).HasColumnType("text");
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasIndex(e => e.WeighbridgeTransactionId);
            entity.HasIndex(e => e.ChangeTimestamp);
        });
    }
}