using Microsoft.EntityFrameworkCore;
using TestServiceV4.Core.Entities;

namespace TestServiceV4.Infrastructure.Data;

public class TestServiceV4DbContext : DbContext
{
    public TestServiceV4DbContext(DbContextOptions<TestServiceV4DbContext> options) : base(options)
    {
    }

    public DbSet<Testentity> Testentitys { get; set; }
    // TODO: Add additional DbSets for other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Testentity entity
        modelBuilder.Entity<Testentity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TODO: Configure additional entities here

        // Add global query filter for soft deletes
        modelBuilder.Entity<Testentity>().HasQueryFilter(e => !e.IsDeleted);
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