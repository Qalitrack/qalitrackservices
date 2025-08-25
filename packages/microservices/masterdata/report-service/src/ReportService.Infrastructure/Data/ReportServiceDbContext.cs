using Microsoft.EntityFrameworkCore;
using ReportService.Core.Entities;

namespace ReportService.Infrastructure.Data;

public class ReportServiceDbContext : DbContext
{
    public ReportServiceDbContext(DbContextOptions<ReportServiceDbContext> options) : base(options)
    {
    }

    public DbSet<Report> Reports { get; set; }
    // TODO: Add additional DbSets for other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Report entity
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasIndex(e => e.ReportName).IsUnique();
            
            // Ignore navigation properties to prevent mapping issues
            entity.Ignore(e => e.Template);
            entity.Ignore(e => e.Schedule);
            entity.Ignore(e => e.Exports);
        });

        // TODO: Configure additional entities here

        // Add global query filter for soft deletes
        modelBuilder.Entity<Report>().HasQueryFilter(e => !e.IsDeleted);
        // TODO: Add query filters for additional entities

        // Seed default data if needed
        // SeedData(modelBuilder);
    }

    // TODO: Implement SeedData method if needed
    // private void SeedData(ModelBuilder modelBuilder)
    // {
    //     // Add seed data here
    // }
}