using Microsoft.EntityFrameworkCore;
using Masterdata.Core.Entities;

namespace Masterdata.Infrastructure.Data;

public class MasterdataDbContext : DbContext
{
    public MasterdataDbContext(DbContextOptions<MasterdataDbContext> options) : base(options)
    {
    }

    public DbSet<Masterdata.Core.Entities.Base> Bases { get; set; }
    // TODO: Add additional DbSets for other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Base entity
        modelBuilder.Entity<Masterdata.Core.Entities.Base>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TODO: Configure additional entities here

        // Add global query filter for soft deletes
        modelBuilder.Entity<Masterdata.Core.Entities.Base>().HasQueryFilter(e => !e.IsDeleted);
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