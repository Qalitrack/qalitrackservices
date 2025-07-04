using Microsoft.EntityFrameworkCore;
using TransactionService.Core.Entities;

namespace TransactionService.Infrastructure.Data;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
    {
    }

    public DbSet<WeighingTransaction> Transactions { get; set; }
    public DbSet<TransactionWorkflow> TransactionWorkflows { get; set; }
    public DbSet<TransactionCharge> TransactionCharges { get; set; }
    public DbSet<TransactionDocument> TransactionDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure WeighingTransaction
        modelBuilder.Entity<WeighingTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.VehicleId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DriverId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SupplierId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProductId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RouteId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.WeighbridgeId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomerId).HasMaxLength(50);
            entity.Property(e => e.DeliveryNoteNumber).HasMaxLength(100);
            entity.Property(e => e.PermitNumber).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.MetadataJson).HasColumnType("TEXT");
            entity.Property(e => e.GrossWeight).HasPrecision(18, 2);
            entity.Property(e => e.TareWeight).HasPrecision(18, 2);
            entity.Property(e => e.NetWeight).HasPrecision(18, 2);
            
            // Ignore the computed property
            entity.Ignore(e => e.Metadata);
            
            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.TransactionDate);

            entity.HasMany(e => e.WorkflowSteps)
                .WithOne(w => w.Transaction)
                .HasForeignKey(w => w.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Charges)
                .WithOne(c => c.Transaction)
                .HasForeignKey(c => c.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Documents)
                .WithOne(d => d.Transaction)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransactionWorkflow
        modelBuilder.Entity<TransactionWorkflow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProcessedBy).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.ValidationDataJson).HasColumnType("TEXT");

            entity.HasIndex(e => new { e.TransactionId, e.WorkflowStep }).IsUnique();
            entity.HasIndex(e => e.Status);
            
            // Ignore the computed property
            entity.Ignore(e => e.ValidationData);
        });

        // Configure TransactionCharge
        modelBuilder.Entity<TransactionCharge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 4);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);

            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.ChargeType);
        });

        // Configure TransactionDocument
        modelBuilder.Entity<TransactionDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ChecksumMd5).HasMaxLength(32);
            entity.Property(e => e.ChecksumSha256).HasMaxLength(64);

            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.DocumentType);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure base entity properties for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property<string>("Id").HasMaxLength(50);
                modelBuilder.Entity(entityType.ClrType).Property<string>("CreatedBy").HasMaxLength(100);
                modelBuilder.Entity(entityType.ClrType).Property<string>("UpdatedBy").HasMaxLength(100);
                modelBuilder.Entity(entityType.ClrType).HasIndex("IsDeleted");
                modelBuilder.Entity(entityType.ClrType).HasIndex("CreatedAt");
            }
        }
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}