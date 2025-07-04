using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;

namespace ProductService.Infrastructure.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductSpecification> ProductSpecifications { get; set; }
    public DbSet<ProductPricing> ProductPricing { get; set; }
    public DbSet<ProductHazmat> ProductHazmat { get; set; }
    public DbSet<ProductCompliance> ProductCompliance { get; set; }
    public DbSet<ProductInventory> ProductInventory { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product entity configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.UnitOfMeasure).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Weight).HasPrecision(18, 4);
            entity.Property(e => e.Density).HasPrecision(18, 4);
            entity.Property(e => e.HazmatClass).HasMaxLength(20);

            // Relationships
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId);

            entity.HasMany(e => e.Specifications)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.ProductId);

            entity.HasMany(e => e.Pricing)
                .WithOne(p => p.Product)
                .HasForeignKey(p => p.ProductId);

            entity.HasOne(e => e.HazmatInfo)
                .WithOne(h => h.Product)
                .HasForeignKey<ProductHazmat>(h => h.ProductId);

            entity.HasMany(e => e.ComplianceRequirements)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId);

            entity.HasOne(e => e.Inventory)
                .WithOne(i => i.Product)
                .HasForeignKey<ProductInventory>(i => i.ProductId);

            entity.HasMany(e => e.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId);
        });

        // ProductCategory entity configuration
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();

            // Self-referencing relationship
            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId);
        });

        // ProductSpecification entity configuration
        modelBuilder.Entity<ProductSpecification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        // ProductPricing entity configuration
        modelBuilder.Entity<ProductPricing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).IsRequired().HasPrecision(18, 4);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.MinimumQuantity).HasPrecision(18, 4);
            entity.Property(e => e.MaximumQuantity).HasPrecision(18, 4);
            entity.Property(e => e.CustomerGroup).HasMaxLength(50);
            entity.Property(e => e.Region).HasMaxLength(50);
        });

        // ProductHazmat entity configuration
        modelBuilder.Entity<ProductHazmat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HazmatClass).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PackingGroup).HasMaxLength(10);
            entity.Property(e => e.UnNumber).HasMaxLength(10);
            entity.Property(e => e.ProperShippingName).HasMaxLength(200);
            entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            entity.Property(e => e.EmergencyPhone).HasMaxLength(20);
        });

        // ProductCompliance entity configuration
        modelBuilder.Entity<ProductCompliance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ComplianceType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Regulation).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Authority).HasMaxLength(100);
            entity.Property(e => e.CertificationNumber).HasMaxLength(50);
        });

        // ProductInventory entity configuration
        modelBuilder.Entity<ProductInventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrentStock).HasPrecision(18, 4);
            entity.Property(e => e.MinimumStock).HasPrecision(18, 4);
            entity.Property(e => e.MaximumStock).HasPrecision(18, 4);
            entity.Property(e => e.ReorderPoint).HasPrecision(18, 4);
            entity.Property(e => e.ReorderQuantity).HasPrecision(18, 4);
            entity.Property(e => e.Location).HasMaxLength(50);
            entity.Property(e => e.Warehouse).HasMaxLength(50);
        });

        // ProductVariant entity configuration
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.VariantType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.VariantValue).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PriceAdjustment).HasPrecision(18, 4);
        });

        // Configure soft delete filter
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductCategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductSpecification>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductPricing>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductHazmat>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductCompliance>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductInventory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductVariant>().HasQueryFilter(e => !e.IsDeleted);
    }
}