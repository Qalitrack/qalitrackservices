using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Modules.SiteManagement.Entities;
using QaliTrack.MasterData.Core.Modules.BusinessEntities.Entities;
using QaliTrack.MasterData.Core.Modules.Driver.Entities;
using QaliTrack.MasterData.Core.Modules.Vehicle.Entities;
using QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;
using QaliTrack.MasterData.Core.Modules.HardwareManagement.Entities;

namespace QaliTrack.MasterData.Infrastructure.Data;

public class MasterDataDbContext : DbContext
{
    public MasterDataDbContext(DbContextOptions<MasterDataDbContext> options) : base(options)
    {
    }

    #region Site Management Module Entities
    public DbSet<Zone> Zones { get; set; }
    public DbSet<LocationType> LocationTypes { get; set; }
    public DbSet<Site> Sites { get; set; }
    #endregion

    #region Business Entities Module
    public DbSet<BusinessEntity> BusinessEntities { get; set; }
    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<SupplierProfile> SupplierProfiles { get; set; }
    public DbSet<BusinessEntityContact> BusinessEntityContacts { get; set; }
    public DbSet<BusinessEntityLocation> BusinessEntityLocations { get; set; }
    public DbSet<BusinessEntityDocument> BusinessEntityDocuments { get; set; }
    public DbSet<CustomerContract> CustomerContracts { get; set; }
    public DbSet<CustomerOrder> CustomerOrders { get; set; }
    public DbSet<SupplierContract> SupplierContracts { get; set; }
    public DbSet<SupplierPerformance> SupplierPerformances { get; set; }
    public DbSet<ScheduleAgreement> ScheduleAgreements { get; set; }
    #endregion

    #region Driver Module Entities
    public DbSet<Driver> Drivers { get; set; }
    #endregion

    #region Vehicle Module Entities
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<DriverVehicleAssignment> DriverVehicleAssignments { get; set; }
    #endregion

    #region Product Catalog Module Entities
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<PackagingType> PackagingTypes { get; set; }
    public DbSet<ProductBase> ProductBases { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductSpecification> ProductSpecifications { get; set; }
    public DbSet<ProductUsagePermission> ProductUsagePermissions { get; set; }
    public DbSet<SiteCapability> SiteCapabilities { get; set; }
    public DbSet<SiteProductConstraint> SiteProductConstraints { get; set; }
    #endregion

    #region Hardware Management Module Entities
    public DbSet<Weighbridge> Weighbridges { get; set; }
    public DbSet<PlcConfiguration> PlcConfigurations { get; set; }
    public DbSet<AnprCamera> AnprCameras { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureSiteManagementModule(modelBuilder);
        ConfigureBusinessEntitiesModule(modelBuilder);
        ConfigureDriverModule(modelBuilder);
        ConfigureVehicleModule(modelBuilder);
        ConfigureProductCatalogModule(modelBuilder);
        ConfigureHardwareManagementModule(modelBuilder);
        ConfigureIndexes(modelBuilder);
        ConfigureGlobalFilters(modelBuilder);
    }

    private void ConfigureSiteManagementModule(ModelBuilder modelBuilder)
    {
        // Zone configuration
        modelBuilder.Entity<Zone>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
        });

        // LocationType configuration
        modelBuilder.Entity<LocationType>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
        });

        // Site configuration
        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => new { e.LocationTypeId, e.ZoneId });

            entity.HasOne(s => s.LocationType)
                .WithMany(lt => lt.Sites)
                .HasForeignKey(s => s.LocationTypeId);

            entity.HasOne(s => s.Zone)
                .WithMany(z => z.Sites)
                .HasForeignKey(s => s.ZoneId)
                .IsRequired(false);
        });
    }

    private void ConfigureBusinessEntitiesModule(ModelBuilder modelBuilder)
    {
        // BusinessEntity configuration
        modelBuilder.Entity<BusinessEntity>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.RegistrationNumber);
            entity.HasIndex(e => e.ContactEmail);
        });

        // One-to-one profile relationships
        modelBuilder.Entity<CustomerProfile>()
            .HasOne(cp => cp.BusinessEntity)
            .WithOne(be => be.CustomerProfile)
            .HasForeignKey<CustomerProfile>(cp => cp.BusinessEntityId);

        modelBuilder.Entity<SupplierProfile>()
            .HasOne(sp => sp.BusinessEntity)
            .WithOne(be => be.SupplierProfile)
            .HasForeignKey<SupplierProfile>(sp => sp.BusinessEntityId);

        // Schedule Agreement configuration
        modelBuilder.Entity<ScheduleAgreement>()
            .HasOne(sa => sa.Supplier)
            .WithMany()
            .HasForeignKey(sa => sa.SupplierId);

        // Other relationships remain the same as original configuration
        ConfigureBusinessEntityRelationships(modelBuilder);
    }

    private void ConfigureBusinessEntityRelationships(ModelBuilder modelBuilder)
    {
        // One-to-many relationships
        modelBuilder.Entity<BusinessEntityContact>()
            .HasOne(bec => bec.BusinessEntity)
            .WithMany(be => be.Contacts)
            .HasForeignKey(bec => bec.BusinessEntityId);

        modelBuilder.Entity<BusinessEntityLocation>()
            .HasOne(bel => bel.BusinessEntity)
            .WithMany(be => be.Locations)
            .HasForeignKey(bel => bel.BusinessEntityId);

        modelBuilder.Entity<BusinessEntityDocument>()
            .HasOne(bed => bed.BusinessEntity)
            .WithMany(be => be.Documents)
            .HasForeignKey(bed => bed.BusinessEntityId);

        // Profile-specific relationships
        modelBuilder.Entity<CustomerContract>()
            .HasOne(cc => cc.CustomerProfile)
            .WithMany(cp => cp.Contracts)
            .HasForeignKey(cc => cc.CustomerProfileId);

        modelBuilder.Entity<CustomerOrder>()
            .HasOne(co => co.CustomerProfile)
            .WithMany(cp => cp.Orders)
            .HasForeignKey(co => co.CustomerProfileId);

        modelBuilder.Entity<SupplierContract>()
            .HasOne(sc => sc.SupplierProfile)
            .WithMany(sp => sp.Contracts)
            .HasForeignKey(sc => sc.SupplierProfileId);

        modelBuilder.Entity<SupplierPerformance>()
            .HasOne(sp => sp.SupplierProfile)
            .WithMany(spp => spp.PerformanceRecords)
            .HasForeignKey(sp => sp.SupplierProfileId);
    }

    private void ConfigureDriverModule(ModelBuilder modelBuilder)
    {
        // Driver configuration
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Status);
        });
    }

    private void ConfigureVehicleModule(ModelBuilder modelBuilder)
    {
        // Vehicle configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(e => e.NumberPlate).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // Driver-Vehicle assignment
        modelBuilder.Entity<DriverVehicleAssignment>(entity =>
        {
            entity.HasIndex(e => new { e.DriverId, e.VehicleId, e.IsActive });
        });
    }

    private void ConfigureProductCatalogModule(ModelBuilder modelBuilder)
    {
        // ProductCategory configuration with self-referencing relationship
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);

            entity.HasOne(pc => pc.ParentCategory)
                .WithMany(pc => pc.SubCategories)
                .HasForeignKey(pc => pc.ParentCategoryId)
                .IsRequired(false);
        });

        // PackagingType configuration
        modelBuilder.Entity<PackagingType>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
        });

        // ProductBase configuration
        modelBuilder.Entity<ProductBase>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.CategoryId, e.Name });

            entity.HasOne(pb => pb.Category)
                .WithMany(pc => pc.Products)
                .HasForeignKey(pb => pb.CategoryId);
        });

        // ProductVariant configuration
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(e => e.VariantCode).IsUnique();
            entity.HasIndex(e => new { e.ProductBaseId, e.PackagingTypeId });

            entity.HasOne(pv => pv.ProductBase)
                .WithMany(pb => pb.Variants)
                .HasForeignKey(pv => pv.ProductBaseId);

            entity.HasOne(pv => pv.PackagingType)
                .WithMany(pt => pt.ProductVariants)
                .HasForeignKey(pv => pv.PackagingTypeId);
        });

        // ProductSpecification configuration
        modelBuilder.Entity<ProductSpecification>(entity =>
        {
            entity.HasOne(ps => ps.ProductBase)
                .WithMany(pb => pb.Specifications)
                .HasForeignKey(ps => ps.ProductBaseId);
        });

        // ProductUsagePermission configuration
        modelBuilder.Entity<ProductUsagePermission>(entity =>
        {
            entity.HasIndex(e => new { e.ProductVariantId, e.SiteId, e.UsageType });

            entity.HasOne(pup => pup.ProductVariant)
                .WithMany(pv => pv.UsagePermissions)
                .HasForeignKey(pup => pup.ProductVariantId);
        });

        // SiteCapability configuration
        modelBuilder.Entity<SiteCapability>(entity =>
        {
            entity.HasIndex(e => new { e.SiteId, e.ProductCategoryId, e.CapabilityType });

            entity.HasOne(sc => sc.ProductCategory)
                .WithMany()
                .HasForeignKey(sc => sc.ProductCategoryId);
        });

        // SiteProductConstraint configuration
        modelBuilder.Entity<SiteProductConstraint>(entity =>
        {
            entity.HasIndex(e => new { e.FromSiteId, e.ToSiteId, e.ProductCategoryId });

            entity.HasOne(spc => spc.ProductCategory)
                .WithMany()
                .HasForeignKey(spc => spc.ProductCategoryId);
        });
    }

    private void ConfigureHardwareManagementModule(ModelBuilder modelBuilder)
    {
        // Weighbridge configuration
        modelBuilder.Entity<Weighbridge>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.SiteId, e.Name });
        });

        // PlcConfiguration configuration
        modelBuilder.Entity<PlcConfiguration>(entity =>
        {
            entity.HasIndex(e => e.PlcAddress);

            entity.HasOne(pc => pc.Weighbridge)
                .WithMany(w => w.PlcConfigurations)
                .HasForeignKey(pc => pc.WeighbridgeId);
        });

        // AnprCamera configuration
        modelBuilder.Entity<AnprCamera>(entity =>
        {
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => new { e.WeighbridgeId, e.Position });

            entity.HasOne(ac => ac.Weighbridge)
                .WithMany(w => w.AnprCameras)
                .HasForeignKey(ac => ac.WeighbridgeId);
        });
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Global performance indexes for soft delete
        modelBuilder.Entity<Zone>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<LocationType>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Site>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<BusinessEntity>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Driver>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Vehicle>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<ProductCategory>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<ProductBase>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<ProductVariant>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Weighbridge>().HasIndex(e => e.IsDeleted);

        // Timestamp indexes for common queries
        modelBuilder.Entity<Zone>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Site>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<BusinessEntity>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Driver>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Vehicle>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<ProductBase>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Weighbridge>().HasIndex(e => e.CreatedAt);
    }

    private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
    {
        // Global soft delete filter for ALL entities that inherit BaseEntity
        
        // Site Management Module
        modelBuilder.Entity<Zone>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LocationType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Site>().HasQueryFilter(e => !e.IsDeleted);
        
        // BusinessEntity Module
        modelBuilder.Entity<BusinessEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CustomerProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SupplierProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BusinessEntityContact>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BusinessEntityLocation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BusinessEntityDocument>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CustomerContract>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CustomerOrder>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SupplierContract>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SupplierPerformance>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ScheduleAgreement>().HasQueryFilter(e => !e.IsDeleted);
        
        // Driver Module
        modelBuilder.Entity<Driver>().HasQueryFilter(e => !e.IsDeleted);
        
        // Vehicle Module
        modelBuilder.Entity<Vehicle>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverVehicleAssignment>().HasQueryFilter(e => !e.IsDeleted);
        
        // Product Catalog Module
        modelBuilder.Entity<ProductCategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PackagingType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductBase>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductVariant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductSpecification>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductUsagePermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SiteCapability>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SiteProductConstraint>().HasQueryFilter(e => !e.IsDeleted);
        
        // Hardware Management Module
        modelBuilder.Entity<Weighbridge>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PlcConfiguration>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AnprCamera>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is QaliTrack.MasterData.Core.Common.BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (QaliTrack.MasterData.Core.Common.BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}