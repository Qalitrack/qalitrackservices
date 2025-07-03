using Microsoft.EntityFrameworkCore;
using OperationalDataService.Core.Entities;
using System.Text.Json;

namespace OperationalDataService.Infrastructure.Data;

public class OperationalDataDbContext : DbContext
{
    public OperationalDataDbContext(DbContextOptions<OperationalDataDbContext> options) : base(options) { }

    // DbSets for all entities
    public DbSet<WeighbridgeOperation> WeighbridgeOperations { get; set; }
    public DbSet<ProductCatalog> ProductCatalogs { get; set; }
    public DbSet<RouteConfiguration> RouteConfigurations { get; set; }
    public DbSet<OperationalSchedule> OperationalSchedules { get; set; }
    public DbSet<CapacityManagement> CapacityManagements { get; set; }
    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<OperationalAlert> OperationalAlerts { get; set; }
    public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure WeighbridgeOperation
        modelBuilder.Entity<WeighbridgeOperation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.OperationalNotes).HasMaxLength(1000);
            
            // Configure JSON columns for SQLite
            entity.Property(e => e.QueuedVehicles)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.HasIndex(e => new { e.WeighbridgeId, e.OrganizationId });
            entity.HasIndex(e => e.Status);
        });

        // Configure ProductCatalog
        modelBuilder.Entity<ProductCatalog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ComplianceRequirements).HasMaxLength(1000);
            entity.Property(e => e.QualitySpecifications).HasMaxLength(1000);
            entity.Property(e => e.SupplierIds).HasMaxLength(200);
            entity.Property(e => e.MasterDataVersion).HasMaxLength(100);

            // Configure JSON columns
            entity.Property(e => e.AllowedVehicleTypes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.AdditionalProperties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.HasIndex(e => new { e.ProductId, e.OrganizationId }).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Category);
        });

        // Configure RouteConfiguration
        modelBuilder.Entity<RouteConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RouteId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Origin).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Destination).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SafetyNotes).HasMaxLength(1000);

            // Configure JSON columns
            entity.Property(e => e.AllowedVehicleTypes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.RestrictedVehicleTypes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.Waypoints)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<RouteWaypoint>>(v, (JsonSerializerOptions?)null) ?? new List<RouteWaypoint>());

            entity.Property(e => e.Restrictions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<RouteRestriction>>(v, (JsonSerializerOptions?)null) ?? new List<RouteRestriction>());

            entity.Property(e => e.OperatingHours)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<TimeSpan[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<TimeSpan>());

            entity.Property(e => e.WeatherRestrictions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.OwnsOne(e => e.PerformanceMetrics, pm =>
            {
                pm.Property(p => p.AverageSpeed);
                pm.Property(p => p.AverageDelay);
                pm.Property(p => p.ReliabilityScore);
                pm.Property(p => p.UsageCount);
                pm.Property(p => p.CustomerSatisfactionScore);
                pm.Property(p => p.LastCalculated);
            });

            entity.HasIndex(e => new { e.Origin, e.Destination });
            entity.HasIndex(e => e.Status);
        });

        // Configure OperationalSchedule
        modelBuilder.Entity<OperationalSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ScheduleId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.WeighbridgeId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.AssignedTo).HasMaxLength(100);
            entity.Property(e => e.VehicleId).HasMaxLength(100);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CompletedBy).HasMaxLength(100);

            // Configure JSON columns
            entity.Property(e => e.RequiredResources)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.Dependencies)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.Alerts)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ScheduleAlert>>(v, (JsonSerializerOptions?)null) ?? new List<ScheduleAlert>());

            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.OwnsOne(e => e.RecurrencePattern, rp =>
            {
                rp.Property(p => p.Type);
                rp.Property(p => p.Interval);
                rp.Property(p => p.DayOfMonth);
                rp.Property(p => p.EndDate);
                rp.Property(p => p.MaxOccurrences);
                rp.Property(p => p.DaysOfWeek)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions?)null) ?? new List<DayOfWeek>());
            });

            entity.HasIndex(e => e.ScheduleId).IsUnique();
            entity.HasIndex(e => new { e.WeighbridgeId, e.StartDate });
            entity.HasIndex(e => e.Status);
        });

        // Configure CapacityManagement
        modelBuilder.Entity<CapacityManagement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeighbridgeId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(200);

            // Configure JSON columns
            entity.Property(e => e.Forecasts)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<CapacityForecast>>(v, (JsonSerializerOptions?)null) ?? new List<CapacityForecast>());

            entity.Property(e => e.LoadBalancingMetrics)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<LoadBalancingMetric>>(v, (JsonSerializerOptions?)null) ?? new List<LoadBalancingMetric>());

            entity.Property(e => e.HourlyUtilization)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, decimal>());

            entity.Property(e => e.BottleneckFactors)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.OwnsOne(e => e.Alert, alert =>
            {
                alert.Property(a => a.Severity);
                alert.Property(a => a.Message).HasMaxLength(500);
                alert.Property(a => a.TriggeredAt);
                alert.Property(a => a.ResolvedAt);
                alert.Property(a => a.ResolvedBy).HasMaxLength(100);
                alert.Property(a => a.Resolution).HasMaxLength(1000);
                alert.Property(a => a.IsActive);
                alert.Property(a => a.NotificationsSent)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            });

            entity.OwnsOne(e => e.Recommendation, rec =>
            {
                rec.Property(r => r.Type).HasMaxLength(100);
                rec.Property(r => r.Description).HasMaxLength(500);
                rec.Property(r => r.Priority);
                rec.Property(r => r.CreatedAt);
                rec.Property(r => r.ImplementedAt);
                rec.Property(r => r.ImplementedBy).HasMaxLength(100);
                rec.Property(r => r.EstimatedImpact);
                rec.Property(r => r.Implementation).HasMaxLength(1000);
                rec.Property(r => r.IsImplemented);
            });

            entity.HasIndex(e => new { e.WeighbridgeId, e.MeasurementDate });
        });

        // Configure MaintenanceSchedule
        modelBuilder.Entity<MaintenanceSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MaintenanceId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.WeighbridgeId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.AssignedTechnician).HasMaxLength(100);
            entity.Property(e => e.SupervisorId).HasMaxLength(100);
            entity.Property(e => e.PreMaintenanceNotes).HasMaxLength(1000);
            entity.Property(e => e.PostMaintenanceNotes).HasMaxLength(1000);

            // Configure JSON columns
            entity.Property(e => e.Tasks)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<MaintenanceTask>>(v, (JsonSerializerOptions?)null) ?? new List<MaintenanceTask>());

            entity.Property(e => e.RequiredResources)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<MaintenanceResource>>(v, (JsonSerializerOptions?)null) ?? new List<MaintenanceResource>());

            entity.Property(e => e.RequiredParts)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.Alerts)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<MaintenanceAlert>>(v, (JsonSerializerOptions?)null) ?? new List<MaintenanceAlert>());

            entity.Property(e => e.DocumentPaths)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.OwnsOne(e => e.Recurrence, rec =>
            {
                rec.Property(r => r.Type);
                rec.Property(r => r.Interval);
                rec.Property(r => r.DayOfMonth);
                rec.Property(r => r.EndDate);
                rec.Property(r => r.MaxOccurrences);
                rec.Property(r => r.IsActive);
                rec.Property(r => r.DaysOfWeek)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions?)null) ?? new List<DayOfWeek>());
            });

            entity.OwnsOne(e => e.Result, result =>
            {
                result.Property(r => r.Status);
                result.Property(r => r.Summary).HasMaxLength(1000);
                result.Property(r => r.NextRecommendedMaintenance);
                result.Property(r => r.QualityScore);
                result.Property(r => r.CompletedBy).HasMaxLength(100);
                result.Property(r => r.VerifiedBy).HasMaxLength(100);
                result.Property(r => r.CompletedAt);
                result.Property(r => r.IssuesFound)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
                result.Property(r => r.IssuesResolved)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
                result.Property(r => r.Recommendations)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
                result.Property(r => r.PartsReplaced)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            });

            entity.HasIndex(e => e.MaintenanceId).IsUnique();
            entity.HasIndex(e => new { e.WeighbridgeId, e.ScheduledDate });
            entity.HasIndex(e => e.Status);
        });

        // Configure OperationalAlert
        modelBuilder.Entity<OperationalAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.WeighbridgeId).HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.AcknowledgedBy).HasMaxLength(100);
            entity.Property(e => e.ResolvedBy).HasMaxLength(100);
            entity.Property(e => e.Resolution).HasMaxLength(1000);
            entity.Property(e => e.SourceId).HasMaxLength(100);

            // Configure JSON columns
            entity.Property(e => e.Recipients)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.Actions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<AlertAction>>(v, (JsonSerializerOptions?)null) ?? new List<AlertAction>());

            entity.Property(e => e.Notifications)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<AlertNotification>>(v, (JsonSerializerOptions?)null) ?? new List<AlertNotification>());

            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.OwnsOne(e => e.Rule, rule =>
            {
                rule.Property(r => r.RuleId).HasMaxLength(100);
                rule.Property(r => r.Name).HasMaxLength(200);
                rule.Property(r => r.Description).HasMaxLength(500);
                rule.Property(r => r.Condition).HasMaxLength(1000);
                rule.Property(r => r.TriggerType);
                rule.Property(r => r.Severity);
                rule.Property(r => r.IsActive);
                rule.Property(r => r.CooldownPeriod);
                rule.Property(r => r.MaxOccurrences);
                rule.Property(r => r.SuppressDuration);
                rule.Property(r => r.NotificationChannels)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
                rule.Property(r => r.Parameters)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());
            });

            entity.HasIndex(e => e.AlertId).IsUnique();
            entity.HasIndex(e => new { e.Type, e.Status });
            entity.HasIndex(e => e.Severity);
        });

        // Configure SystemConfiguration
        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConfigurationId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.DefaultValue).HasMaxLength(2000);
            entity.Property(e => e.ValidationRules).HasMaxLength(1000);
            entity.Property(e => e.DisplayName).HasMaxLength(500);
            entity.Property(e => e.HelpText).HasMaxLength(1000);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            entity.Property(e => e.ChangeReason).HasMaxLength(500);

            // Configure JSON columns
            entity.Property(e => e.AllowedValues)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.Dependencies)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ConfigurationDependency>>(v, (JsonSerializerOptions?)null) ?? new List<ConfigurationDependency>());

            entity.Property(e => e.History)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ConfigurationHistory>>(v, (JsonSerializerOptions?)null) ?? new List<ConfigurationHistory>());

            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.OwnsOne(e => e.Constraints, constraints =>
            {
                constraints.Property(c => c.MinValue);
                constraints.Property(c => c.MaxValue);
                constraints.Property(c => c.MinLength);
                constraints.Property(c => c.MaxLength);
                constraints.Property(c => c.RegexPattern).HasMaxLength(500);
                constraints.Property(c => c.IsRequired);
                constraints.Property(c => c.CustomValidation).HasMaxLength(1000);
                constraints.Property(c => c.AllowedValues)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
                constraints.Property(c => c.ExcludedValues)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            });

            entity.HasIndex(e => new { e.ConfigurationId, e.OrganizationId }).IsUnique();
            entity.HasIndex(e => new { e.Category, e.Key });
            entity.HasIndex(e => e.IsActive);
        });

        // Configure global query filters for soft delete
        modelBuilder.Entity<WeighbridgeOperation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductCatalog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteConfiguration>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OperationalSchedule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CapacityManagement>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<MaintenanceSchedule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OperationalAlert>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SystemConfiguration>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
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
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;
            }
        }
    }
}