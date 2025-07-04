using Microsoft.EntityFrameworkCore;
using WeightDataService.Core.Entities;

namespace WeightDataService.Infrastructure.Data;

public class WeightDataContext : DbContext
{
    public WeightDataContext(DbContextOptions<WeightDataContext> options) : base(options)
    {
    }

    public DbSet<WeightMeasurement> WeightMeasurements { get; set; }
    public DbSet<WeighbridgeStatus> WeighbridgeStatuses { get; set; }
    public DbSet<WeightCorrection> WeightCorrections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure WeightMeasurement
        modelBuilder.Entity<WeightMeasurement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.VehicleRegistration).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DriverId).HasMaxLength(50);
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.Property(e => e.TareWeight).HasColumnType("decimal(10,2)");
            entity.Property(e => e.NetWeight).HasColumnType("decimal(10,2)");
            entity.Property(e => e.TicketReference).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProductType).HasMaxLength(100);
            entity.Property(e => e.CustomerReference).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasIndex(e => e.WeighbridgeId);
            entity.HasIndex(e => e.VehicleRegistration);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.MeasurementDateTime);
            entity.HasIndex(e => new { e.OrganizationId, e.IsDeleted });

            entity.HasMany(e => e.Corrections)
                  .WithOne(c => c.WeightMeasurement)
                  .HasForeignKey(c => c.WeightMeasurementId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WeighbridgeStatus
        modelBuilder.Entity<WeighbridgeStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MaxCapacity).HasColumnType("decimal(10,2)");
            entity.Property(e => e.MinCapacity).HasColumnType("decimal(10,2)");
            entity.Property(e => e.CurrentWeight).HasColumnType("decimal(10,2)");
            entity.Property(e => e.MaintenanceNotes).HasMaxLength(1000);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.AccuracyTolerance).HasColumnType("decimal(5,3)");
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasIndex(e => e.WeighbridgeId).IsUnique();
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.Status);
        });

        // Configure WeightCorrection
        modelBuilder.Entity<WeightCorrection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalWeight).HasColumnType("decimal(10,2)");
            entity.Property(e => e.CorrectedWeight).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AuthorizedBy).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ApprovedBy).HasMaxLength(50);
            entity.Property(e => e.ApprovalNotes).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasIndex(e => e.WeightMeasurementId);
            entity.HasIndex(e => e.CorrectionDateTime);
        });
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
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}