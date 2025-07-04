using Microsoft.EntityFrameworkCore;
using RouteService.Core.Entities;

namespace RouteService.Infrastructure.Data;

public class RouteDbContext : DbContext
{
    public RouteDbContext(DbContextOptions<RouteDbContext> options) : base(options)
    {
    }

    public DbSet<Route> Routes { get; set; }
    public DbSet<RouteWaypoint> RouteWaypoints { get; set; }
    public DbSet<RouteRestriction> RouteRestrictions { get; set; }
    public DbSet<RouteCondition> RouteConditions { get; set; }
    public DbSet<RouteToll> RouteTolls { get; set; }
    public DbSet<RoutePerformance> RoutePerformance { get; set; }
    public DbSet<RouteHazmat> RouteHazmat { get; set; }
    public DbSet<RouteSchedule> RouteSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Route entity
        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Origin).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Destination).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RouteType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            
            // Configure navigation properties
            entity.HasMany(e => e.Waypoints)
                .WithOne(e => e.Route)
                .HasForeignKey(e => e.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.Restrictions)
                .WithOne(e => e.Route)
                .HasForeignKey(e => e.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.Conditions)
                .WithOne(e => e.Route)
                .HasForeignKey(e => e.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.Tolls)
                .WithOne(e => e.Route)
                .HasForeignKey(e => e.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.Performance)
                .WithOne(e => e.Route)
                .HasForeignKey(e => e.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.HazmatRestrictions)
                .WithOne(e => e.Route)
                .HasForeignKey(e => e.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.Schedules)
                .WithOne(e => e.Route)
                .HasForeignKey(e => e.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RouteWaypoint entity
        modelBuilder.Entity<RouteWaypoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Address).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(50);
            
            entity.HasIndex(e => new { e.RouteId, e.Sequence }).IsUnique();
        });

        // Configure RouteRestriction entity
        modelBuilder.Entity<RouteRestriction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Severity).HasConversion<string>();
            entity.Property(e => e.DaysOfWeek).HasMaxLength(50);
            entity.Property(e => e.VehicleTypes).HasMaxLength(100);
            entity.Property(e => e.HazmatClasses).HasMaxLength(100);
            entity.Property(e => e.EnforcementAgency).HasMaxLength(100);
            entity.Property(e => e.PermitRequired).HasMaxLength(50);
        });

        // Configure RouteCondition entity
        modelBuilder.Entity<RouteCondition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.ReportedBy).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.RecommendedAction).HasMaxLength(500);
            entity.Property(e => e.WeatherCondition).HasMaxLength(100);
            entity.Property(e => e.TrafficInfo).HasMaxLength(200);
        });

        // Configure RouteToll entity
        modelBuilder.Entity<RouteToll>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.AcceptedPayments).HasConversion<string>();
            entity.Property(e => e.ETCProvider).HasMaxLength(50);
            entity.Property(e => e.SpecialConditions).HasMaxLength(500);
            entity.Property(e => e.AlternativeRouteDescription).HasMaxLength(200);
        });

        // Configure RoutePerformance entity
        modelBuilder.Entity<RoutePerformance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeatherConditions).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.VehicleType).HasMaxLength(100);
            entity.Property(e => e.DriverId).HasMaxLength(100);
            entity.Property(e => e.CompanyId).HasMaxLength(100);
        });

        // Configure RouteHazmat entity
        modelBuilder.Entity<RouteHazmat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HazmatClass).IsRequired().HasMaxLength(10);
            entity.Property(e => e.HazmatDescription).HasMaxLength(100);
            entity.Property(e => e.RestrictionType).HasConversion<string>();
            entity.Property(e => e.PermitType).HasMaxLength(100);
            entity.Property(e => e.IssuingAuthority).HasMaxLength(100);
            entity.Property(e => e.QuantityUnit).HasMaxLength(20);
            entity.Property(e => e.SpecialRequirements).HasMaxLength(500);
            entity.Property(e => e.AlternativeRoute).HasMaxLength(200);
            entity.Property(e => e.RestrictedDays).HasMaxLength(200);
            entity.Property(e => e.EmergencyContactInfo).HasMaxLength(200);
            entity.Property(e => e.SafetyRequirements).HasMaxLength(500);
            entity.Property(e => e.EscortRequirements).HasMaxLength(200);
        });

        // Configure RouteSchedule entity
        modelBuilder.Entity<RouteSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.DaysOfWeek).HasMaxLength(100);
            entity.Property(e => e.DaysOfMonth).HasMaxLength(100);
            entity.Property(e => e.MonthsOfYear).HasMaxLength(100);
            entity.Property(e => e.RecurrencePattern).HasMaxLength(50);
            entity.Property(e => e.Priority).HasMaxLength(100);
            entity.Property(e => e.ReservationContact).HasMaxLength(200);
            entity.Property(e => e.SpecialInstructions).HasMaxLength(500);
            entity.Property(e => e.WeatherDependency).HasMaxLength(100);
            entity.Property(e => e.SeasonalAdjustments).HasMaxLength(100);
        });

        // Configure global query filters to exclude soft-deleted entities
        modelBuilder.Entity<Route>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteWaypoint>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteRestriction>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteCondition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteToll>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RoutePerformance>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteHazmat>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteSchedule>().HasQueryFilter(e => !e.IsDeleted);
    }
}