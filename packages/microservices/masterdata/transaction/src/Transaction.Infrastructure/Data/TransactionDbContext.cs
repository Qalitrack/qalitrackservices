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
    public DbSet<WeighingRecord> WeighingRecords { get; set; }
    public DbSet<TransactionAuditLog> AuditLogs { get; set; }
    public DbSet<ReweighRecord> ReweighRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add DateTime converter for PostgreSQL
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new ValueConverter<DateTime, DateTime>(
                            v => v.Kind == DateTimeKind.Unspecified 
                                ? DateTime.SpecifyKind(v, DateTimeKind.Utc) 
                                : v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        ));
                }
            }
        }

        ConfigureWeighbridgeTransaction(modelBuilder);
        ConfigureWeighingRecord(modelBuilder);
        ConfigureAuditLog(modelBuilder);
        ConfigureReweighRecord(modelBuilder);

        // Add global query filter for soft deletes
        modelBuilder.Entity<WeighbridgeTransaction>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WeighingRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransactionAuditLog>().HasQueryFilter(e => !e.IsDeleted);
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
            entity.Property(e => e.Operation).HasMaxLength(50);
            entity.Property(e => e.ReweighPermissionReason).HasMaxLength(500);
            entity.Property(e => e.ChangeDescription).HasMaxLength(1000);

            // Indexes
            entity.HasIndex(e => e.ReceiptNo).IsUnique();
            entity.HasIndex(e => e.NoPlate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.CreatedAt);

            // Relationships
            entity.HasMany(t => t.WeighingRecords)
                  .WithOne(w => w.Transaction)
                  .HasForeignKey(w => w.WeighbridgeTransactionId);

            entity.HasMany(t => t.AuditLogs)
                  .WithOne(a => a.Transaction)
                  .HasForeignKey(a => a.WeighbridgeTransactionId);
                  
            entity.HasMany(t => t.ReweighRecords)
                  .WithOne(r => r.WeighbridgeTransaction)
                  .HasForeignKey(r => r.WeighbridgeTransactionId);
        });
    }

    private void ConfigureWeighingRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeighingRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighBridgeName).HasMaxLength(100);
            entity.Property(e => e.ScaleName).HasMaxLength(100);
            entity.Property(e => e.OperatorName).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Indexes
            entity.HasIndex(e => e.WeighbridgeTransactionId);
            entity.HasIndex(e => e.WeighingDate);
        });
    }

    private void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);

            // Indexes
            entity.HasIndex(e => e.WeighbridgeTransactionId);
            entity.HasIndex(e => e.ChangeTimestamp);
            entity.HasIndex(e => e.Action);
        });
    }

    private void ConfigureReweighRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReweighRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
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

            // Configure relationship with WeighbridgeTransaction
            entity.HasOne(r => r.WeighbridgeTransaction)
                  .WithMany(t => t.ReweighRecords)
                  .HasForeignKey(r => r.WeighbridgeTransactionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}