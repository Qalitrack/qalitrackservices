using Microsoft.EntityFrameworkCore;
using Abuso.Core.Entities;

namespace Abuso.Infrastructure.Data;

public class AbusoDbContext : DbContext
{
    public AbusoDbContext(DbContextOptions<AbusoDbContext> options) : base(options)
    {
    }

    public DbSet<Abuso.Core.Entities.Abuso> Abusos { get; set; }
    // TODO: Add additional DbSets for other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Abuso entity
        modelBuilder.Entity<Abuso.Core.Entities.Abuso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TODO: Configure additional entities here

        // Add global query filter for soft deletes
        modelBuilder.Entity<Abuso.Core.Entities.Abuso>().HasQueryFilter(e => !e.IsDeleted);
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