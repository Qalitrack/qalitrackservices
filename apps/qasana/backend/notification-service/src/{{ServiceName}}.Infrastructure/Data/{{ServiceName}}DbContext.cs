using Microsoft.EntityFrameworkCore;
using {{ServiceName}}.Core.Entities;

namespace {{ServiceName}}.Infrastructure.Data;

public class {{ServiceName}}DbContext : DbContext
{
    public {{ServiceName}}DbContext(DbContextOptions<{{ServiceName}}DbContext> options) : base(options)
    {
    }

    public DbSet<{{ServiceName}}.Core.Entities.{{EntityName}}> {{EntityName}}s { get; set; }
    // TODO: Add additional DbSets for other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure {{EntityName}} entity
        modelBuilder.Entity<{{ServiceName}}.Core.Entities.{{EntityName}}>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TODO: Configure additional entities here

        // Add global query filter for soft deletes
        modelBuilder.Entity<{{ServiceName}}.Core.Entities.{{EntityName}}>().HasQueryFilter(e => !e.IsDeleted);
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