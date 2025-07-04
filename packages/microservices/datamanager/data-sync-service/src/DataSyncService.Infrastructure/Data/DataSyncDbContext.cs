using Microsoft.EntityFrameworkCore;
using DataSyncService.Core.Entities;

namespace DataSyncService.Infrastructure.Data;

public class DataSyncDbContext : DbContext
{
    public DataSyncDbContext(DbContextOptions<DataSyncDbContext> options) : base(options)
    {
    }

    public DbSet<SyncSession> SyncSessions { get; set; }
    public DbSet<SyncSite> SyncSites { get; set; }
    public DbSet<ChangeRecord> ChangeRecords { get; set; }
    public DbSet<SyncConflict> SyncConflicts { get; set; }
    public DbSet<SyncLog> SyncLogs { get; set; }
    public DbSet<SiteHealthCheck> SiteHealthChecks { get; set; }
    public DbSet<SyncConfiguration> SyncConfigurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SyncSession entity
        modelBuilder.Entity<SyncSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.Property(e => e.SessionId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SourceSiteId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetSiteId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Relationships
            entity.HasMany(e => e.SyncLogs)
                .WithOne(e => e.SyncSession)
                .HasForeignKey(e => e.SessionId)
                .HasPrincipalKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ChangeRecords)
                .WithOne(e => e.SyncSession)
                .HasForeignKey(e => e.SessionId)
                .HasPrincipalKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.SyncConflicts)
                .WithOne(e => e.SyncSession)
                .HasForeignKey(e => e.SessionId)
                .HasPrincipalKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SyncSite entity
        modelBuilder.Entity<SyncSite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SiteId).IsUnique();
            entity.Property(e => e.SiteId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ConnectionString).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ApiEndpoint).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TimeZone).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AuthTokenHash).HasMaxLength(256);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Relationships
            entity.HasMany(e => e.HealthChecks)
                .WithOne(e => e.SyncSite)
                .HasForeignKey(e => e.SiteId)
                .HasPrincipalKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ChangeRecord entity
        modelBuilder.Entity<ChangeRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChangeId).IsUnique();
            entity.HasIndex(e => new { e.TableName, e.RecordId });
            entity.HasIndex(e => e.ChangeTimestamp);
            entity.HasIndex(e => e.SequenceNumber);
            entity.Property(e => e.ChangeId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SessionId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TableName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RecordId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SourceSiteId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetSiteId).HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.ChecksumHash).HasMaxLength(64);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Relationships
            entity.HasMany(e => e.RelatedConflicts)
                .WithMany(e => e.RelatedChanges)
                .UsingEntity(j => j.ToTable("ChangeRecordConflicts"));
        });

        // Configure SyncConflict entity
        modelBuilder.Entity<SyncConflict>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConflictId).IsUnique();
            entity.HasIndex(e => new { e.TableName, e.RecordId });
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.ConflictId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SessionId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TableName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RecordId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ConflictType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ResolutionReason).HasMaxLength(500);
            entity.Property(e => e.ResolvedBy).HasMaxLength(100);
            entity.Property(e => e.SourceSiteId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetSiteId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
        });

        // Configure SyncLog entity
        modelBuilder.Entity<SyncLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LogId).IsUnique();
            entity.HasIndex(e => e.LogTimestamp);
            entity.HasIndex(e => e.Level);
            entity.Property(e => e.LogId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SessionId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Level).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.CorrelationId).HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
        });

        // Configure SiteHealthCheck entity
        modelBuilder.Entity<SiteHealthCheck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.HealthCheckId).IsUnique();
            entity.HasIndex(e => e.CheckTime);
            entity.Property(e => e.HealthCheckId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SiteId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.CheckType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
        });

        // Configure SyncConfiguration entity
        modelBuilder.Entity<SyncConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConfigId).IsUnique();
            entity.HasIndex(e => new { e.SourceSiteId, e.TargetSiteId, e.TableName });
            entity.Property(e => e.ConfigId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SourceSiteId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetSiteId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TableName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FilterCondition).HasMaxLength(1000);
            entity.Property(e => e.FieldMapping).HasMaxLength(2000);
            entity.Property(e => e.TransformationRules).HasMaxLength(2000);
            entity.Property(e => e.NotificationEmails).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
        });

        // Configure enum conversions
        modelBuilder.Entity<SyncSession>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<SyncSession>()
            .Property(e => e.Direction)
            .HasConversion<string>();

        modelBuilder.Entity<SyncSession>()
            .Property(e => e.Mode)
            .HasConversion<string>();

        modelBuilder.Entity<SyncSession>()
            .Property(e => e.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<SyncSite>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ChangeRecord>()
            .Property(e => e.Operation)
            .HasConversion<string>();

        modelBuilder.Entity<ChangeRecord>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<SyncConflict>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<SyncConflict>()
            .Property(e => e.ResolutionStrategy)
            .HasConversion<string>();

        modelBuilder.Entity<SiteHealthCheck>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<SyncConfiguration>()
            .Property(e => e.Direction)
            .HasConversion<string>();

        modelBuilder.Entity<SyncConfiguration>()
            .Property(e => e.Mode)
            .HasConversion<string>();

        modelBuilder.Entity<SyncConfiguration>()
            .Property(e => e.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<SyncConfiguration>()
            .Property(e => e.DefaultConflictResolution)
            .HasConversion<string>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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

        return await base.SaveChangesAsync(cancellationToken);
    }
}