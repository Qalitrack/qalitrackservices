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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureWeighbridgeTransaction(modelBuilder);
        ConfigureReweighRecord(modelBuilder);

        // Add global query filter for soft deletes
        modelBuilder.Entity<WeighbridgeTransaction>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReweighRecord>().HasQueryFilter(e => !e.IsDeleted);
    }

    private void ConfigureWeighbridgeTransaction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeighbridgeTransaction>(entity =>
        {
            // Map to MySQL table name
            entity.ToTable("tickets");
            
            // Primary Key
            entity.HasKey(e => e.TicketID);
            entity.Property(e => e.TicketID)
                  .HasColumnName("TicketID")
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

            // Vehicle Information
            entity.Property(e => e.VehicleID).HasColumnName("vehicleID");
            entity.Property(e => e.NoPlate)
                  .IsRequired()
                  .HasMaxLength(7)
                  .HasColumnName("NoPlate");
            entity.Property(e => e.DriverName)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasColumnName("DriverName");

            // Commodity Information
            entity.Property(e => e.CommodityID).HasColumnName("CommodityID");
            entity.Property(e => e.CommodityName)
                  .HasMaxLength(100)
                  .HasColumnName("CommodityName");

            // Supplier Information
            entity.Property(e => e.SupplierID).HasColumnName("SupplierID");
            entity.Property(e => e.SupplierName)
                  .HasMaxLength(100)
                  .HasColumnName("SupplierName");

            // Customer Information
            entity.Property(e => e.CustomerID).HasColumnName("CustomerID");
            entity.Property(e => e.CustomerName)
                  .HasMaxLength(100)
                  .HasColumnName("CustomerName");

            // Transporter Information
            entity.Property(e => e.TransporterID)
                  .IsRequired()
                  .HasColumnName("TransporterID");
            entity.Property(e => e.TransporterName)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasColumnName("TransporterName");

            // Origin and Destination
            entity.Property(e => e.OriginID).HasColumnName("OriginID");
            entity.Property(e => e.OriginName)
                  .HasMaxLength(50)
                  .HasColumnName("OriginName");
            entity.Property(e => e.DestinationID).HasColumnName("DestinationID");
            entity.Property(e => e.DestinationName)
                  .HasMaxLength(100)
                  .HasColumnName("DestinationName");

            // Weighbridge Information - First Weighing
            entity.Property(e => e.WeighBridgeID).HasColumnName("WeighBridgeID");
            entity.Property(e => e.WeighBridgeName)
                  .HasMaxLength(50)
                  .HasColumnName("WeighBridgeName");
            entity.Property(e => e.ScaleName)
                  .HasMaxLength(100)
                  .HasColumnName("ScaleName");
            entity.Property(e => e.OperatorID).HasColumnName("OperatorID");
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
                  .HasMaxLength(10)
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
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAddOrUpdate();

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

            // API Integration
            entity.Property(e => e.ApiId).HasColumnName("api_id");

            // Indexes
            entity.HasIndex(e => e.ReceiptNo);
            entity.HasIndex(e => e.NoPlate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.FirstWeightDate);
        });
    }

    private void ConfigureReweighRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReweighRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Change foreign key to match the new int type
            entity.Property(e => e.WeighbridgeTransactionId)
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
}