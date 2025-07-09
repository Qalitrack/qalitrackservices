using Microsoft.EntityFrameworkCore;
using TestService.Core.Entities;

namespace TestService.Infrastructure.Data;

public class TestServiceDbContext : DbContext
{
    public TestServiceDbContext(DbContextOptions<TestServiceDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
    // TODO: Add additional DbSets for other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Item entity
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TODO: Configure additional entities here

        // Add global query filter for soft deletes
        modelBuilder.Entity<Item>().HasQueryFilter(e => !e.IsDeleted);
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