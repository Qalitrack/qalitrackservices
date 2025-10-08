using Microsoft.EntityFrameworkCore;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Infrastructure.Data;

public class TechnicianApiDbContext : DbContext
{
    public TechnicianApiDbContext(DbContextOptions<TechnicianApiDbContext> options) : base(options)
    {
    }

    public DbSet<TechnicianApi.Core.Entities.Technician> Technicians { get; set; }
    // TODO: Add additional DbSets for other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Technician entity
        modelBuilder.Entity<TechnicianApi.Core.Entities.Technician>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TODO: Configure additional entities here

        // Add global query filter for soft deletes
        modelBuilder.Entity<TechnicianApi.Core.Entities.Technician>().HasQueryFilter(e => !e.IsDeleted);
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