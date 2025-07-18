using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;

namespace ProductService.Infrastructure.Data;

public class ProductServiceDbContext : DbContext
{
    public ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options) : base(options)
    {
    }

    public DbSet<ProductService.Core.Entities.Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Pricing> Pricings { get; set; }
    public DbSet<Specification> Specifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<ProductService.Core.Entities.Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(100);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.SKU).IsUnique();

            // Relationships
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Path).HasMaxLength(500);

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Path);

            // Self-referencing relationship
            entity.HasOne(e => e.ParentCategory)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(e => e.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Pricing entity
        modelBuilder.Entity<Pricing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.SalePrice).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);

            entity.HasIndex(e => new { e.ProductId, e.CustomerId, e.ValidFrom });
            entity.HasIndex(e => new { e.ProductId, e.Type, e.IsActive });

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Pricings)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Specification entity
        modelBuilder.Entity<Specification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(50);

            entity.HasIndex(e => new { e.ProductId, e.Name });
            entity.HasIndex(e => e.ComplianceStandard);

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Specifications)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Add global query filter for soft deletes
        modelBuilder.Entity<ProductService.Core.Entities.Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Pricing>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Specification>().HasQueryFilter(e => !e.IsDeleted);

        // Seed default data if needed
        // SeedData(modelBuilder);
    }

    // TODO: Implement SeedData method if needed
    // private void SeedData(ModelBuilder modelBuilder)
    // {
    //     // Add seed data here
    // }
}