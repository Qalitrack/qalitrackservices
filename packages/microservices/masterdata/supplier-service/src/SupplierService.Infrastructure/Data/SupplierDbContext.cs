using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;

namespace SupplierService.Infrastructure.Data;

public class SupplierDbContext : DbContext
{
    public SupplierDbContext(DbContextOptions<SupplierDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<SupplierProduct> SupplierProducts { get; set; } = null!;
    public DbSet<SupplierPricing> SupplierPricing { get; set; } = null!;
    public DbSet<SupplierPerformance> SupplierPerformances { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Supplier Configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ContactPerson).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.TaxIdentificationNumber).HasMaxLength(50);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.TaxIdentificationNumber);
        });

        // Address Configuration
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SupplierId).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Street).IsRequired().HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AdditionalInfo).HasMaxLength(200);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Addresses)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsDefault);
        });

        // SupplierProduct Configuration
        modelBuilder.Entity<SupplierProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SupplierId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.SupplierSKU).HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => new { e.SupplierId, e.ProductId }).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsPreferred);
        });

        // SupplierPricing Configuration
        modelBuilder.Entity<SupplierPricing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SupplierProductId).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Notes).HasMaxLength(200);

            entity.HasOne(e => e.SupplierProduct)
                .WithMany(sp => sp.Pricing)
                .HasForeignKey(e => e.SupplierProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierProductId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.ValidFrom);
            entity.HasIndex(e => e.ValidTo);
        });

        // SupplierPerformance Configuration
        modelBuilder.Entity<SupplierPerformance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SupplierId).IsRequired();
            entity.Property(e => e.MetricType).HasConversion<string>();
            entity.Property(e => e.Score).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Period).HasConversion<string>();
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.PerformanceMetrics)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.MetricType);
            entity.HasIndex(e => e.Period);
            entity.HasIndex(e => new { e.PeriodStart, e.PeriodEnd });
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;
            
            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}