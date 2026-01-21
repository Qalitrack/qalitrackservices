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
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36); // For GUID
            entity.Property(e => e.ReceiptNo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.NoPlate).HasMaxLength(20);
            entity.Property(e => e.DriverName).HasMaxLength(100);
            entity.Property(e => e.CommodityName).HasMaxLength(100);
            entity.Property(e => e.SupplierName).HasMaxLength(200);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.TransporterName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.OriginName).HasMaxLength(200);
            entity.Property(e => e.DestinationName).HasMaxLength(200);
            entity.Property(e => e.WeighBridgeName).HasMaxLength(100);
            entity.Property(e => e.ScaleName).HasMaxLength(100);
            entity.Property(e => e.OperatorName).HasMaxLength(100);
            entity.Property(e => e.WeighBridgeName2nd).HasMaxLength(100);
            entity.Property(e => e.ScaleName2nd).HasMaxLength(100);
            entity.Property(e => e.OperatorName2nd).HasMaxLength(100);
            entity.Property(e => e.WeighMode).HasMaxLength(50);
            entity.Property(e => e.ReweighPermissionReason).HasMaxLength(500);
            entity.Property(e => e.ChangeDescription).HasMaxLength(1000);
            entity.Property(e => e.Image)
                .HasColumnName("image");

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
            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .IsRequired(false);  // This makes the column nullable

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