using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;

namespace SupplierService.Infrastructure.Data;

public class SupplierDbContext : DbContext
{
    public SupplierDbContext(DbContextOptions<SupplierDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<SupplierContact> SupplierContacts { get; set; } = null!;
    public DbSet<SupplierContract> SupplierContracts { get; set; } = null!;
    public DbSet<SupplierProduct> SupplierProducts { get; set; } = null!;
    public DbSet<SupplierLocation> SupplierLocations { get; set; } = null!;
    public DbSet<SupplierDocument> SupplierDocuments { get; set; } = null!;
    public DbSet<SupplierPerformance> SupplierPerformances { get; set; } = null!;
    public DbSet<SupplierFinancial> SupplierFinancials { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Supplier Configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.TaxNumber).IsUnique().HasFilter("[TaxNumber] IS NOT NULL");
            entity.HasIndex(e => e.RegistrationNumber).IsUnique().HasFilter("[RegistrationNumber] IS NOT NULL");
            entity.HasIndex(e => e.ContactEmail);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SupplierType);
        });

        // SupplierContact Configuration
        modelBuilder.Entity<SupplierContact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Mobile).HasMaxLength(20);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Contacts)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.ContactType);
            entity.HasIndex(e => e.IsPrimary);
        });

        // SupplierContract Configuration
        modelBuilder.Entity<SupplierContract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Terms).HasMaxLength(2000);
            entity.Property(e => e.Conditions).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.ContractValue).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Contracts)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ContractNumber).IsUnique();
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.EndDate);
        });

        // SupplierProduct Configuration
        modelBuilder.Entity<SupplierProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SupplierProductCode).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.Specifications).HasMaxLength(2000);
            entity.Property(e => e.QualityCertifications).HasMaxLength(500);
            entity.Property(e => e.ComplianceStandards).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
        });

        // SupplierLocation Configuration
        modelBuilder.Entity<SupplierLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AddressLine2).HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.OperatingHours).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Locations)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.LocationType);
            entity.HasIndex(e => e.IsPrimary);
        });

        // SupplierDocument Configuration
        modelBuilder.Entity<SupplierDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.Version).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Documents)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.DocumentType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpiryDate);
        });

        // SupplierPerformance Configuration
        modelBuilder.Entity<SupplierPerformance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Comments).HasMaxLength(1000);
            entity.Property(e => e.EvaluatedBy).HasMaxLength(100);
            entity.Property(e => e.QualityRating).HasColumnType("decimal(3,1)");
            entity.Property(e => e.DeliveryRating).HasColumnType("decimal(3,1)");
            entity.Property(e => e.ServiceRating).HasColumnType("decimal(3,1)");
            entity.Property(e => e.OverallRating).HasColumnType("decimal(3,1)");
            entity.Property(e => e.OnTimeDeliveryRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DefectRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.AverageLeadTime).HasColumnType("decimal(8,2)");
            entity.Property(e => e.TotalOrderValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CustomerSatisfactionScore).HasColumnType("decimal(3,1)");

            entity.HasOne(e => e.Supplier)
                .WithOne(s => s.Performance)
                .HasForeignKey<SupplierPerformance>(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => new { e.Year, e.Month });
            entity.HasIndex(e => e.OverallRating);
        });

        // SupplierFinancial Configuration
        modelBuilder.Entity<SupplierFinancial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankRoutingNumber).HasMaxLength(50);
            entity.Property(e => e.BankAddress).HasMaxLength(200);
            entity.Property(e => e.SwiftCode).HasMaxLength(20);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.VatNumber).HasMaxLength(50);
            entity.Property(e => e.TaxExemptionCertificate).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.AnnualRevenue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OutstandingBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LastPaymentAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Supplier)
                .WithOne(s => s.Financial)
                .HasForeignKey<SupplierFinancial>(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.CreditRating);
            entity.HasIndex(e => e.Status);
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