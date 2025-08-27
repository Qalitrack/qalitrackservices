using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Modules.Organization.Entities;
using QaliTrack.MasterData.Core.Modules.BusinessEntities.Entities;
using QaliTrack.MasterData.Core.Modules.Driver.Entities;
using QaliTrack.MasterData.Core.Modules.Vehicle.Entities;
using QaliTrack.MasterData.Core.Modules.Sacco.Entities;
using QaliTrack.MasterData.Core.Modules.Product.Entities;
using QaliTrack.MasterData.Core.Modules.Route.Entities;
using QaliTrack.MasterData.Core.Modules.Weighbridge.Entities;
using QaliTrack.MasterData.Core.Modules.Report.Entities;
using QaliTrack.MasterData.Core.Modules.Relationships.Entities;

namespace QaliTrack.MasterData.Infrastructure.Data;

public class MasterDataDbContext : DbContext
{
    public MasterDataDbContext(DbContextOptions<MasterDataDbContext> options) : base(options)
    {
    }

    #region Organization Module Entities
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationUser> OrganizationUsers { get; set; }
    public DbSet<OrganizationLocation> OrganizationLocations { get; set; }
    public DbSet<OrganizationSettings> OrganizationSettings { get; set; }
    public DbSet<OrganizationSubscription> OrganizationSubscriptions { get; set; }
    #endregion

    #region Business Entities Module
    public DbSet<BusinessEntity> BusinessEntities { get; set; }
    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<SupplierProfile> SupplierProfiles { get; set; }
    public DbSet<TransporterProfile> TransporterProfiles { get; set; }
    public DbSet<BusinessEntityContact> BusinessEntityContacts { get; set; }
    public DbSet<BusinessEntityLocation> BusinessEntityLocations { get; set; }
    public DbSet<BusinessEntityDocument> BusinessEntityDocuments { get; set; }
    public DbSet<CustomerContract> CustomerContracts { get; set; }
    public DbSet<CustomerOrder> CustomerOrders { get; set; }
    public DbSet<SupplierContract> SupplierContracts { get; set; }
    public DbSet<SupplierPerformance> SupplierPerformances { get; set; }
    public DbSet<TransporterContract> TransporterContracts { get; set; }
    public DbSet<TransporterPerformance> TransporterPerformances { get; set; }
    public DbSet<TransporterInsurance> TransporterInsurances { get; set; }
    #endregion

    #region Driver Module Entities
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<DriverLicense> DriverLicenses { get; set; }
    public DbSet<DriverProfile> DriverProfiles { get; set; }
    public DbSet<DriverDocument> DriverDocuments { get; set; }
    public DbSet<DriverTraining> DriverTrainings { get; set; }
    public DbSet<DriverMedical> DriverMedicals { get; set; }
    public DbSet<DriverViolation> DriverViolations { get; set; }
    public DbSet<DriverPerformance> DriverPerformances { get; set; }
    #endregion

    #region Vehicle Module Entities
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleType> VehicleTypes { get; set; }
    public DbSet<VehicleRegistration> VehicleRegistrations { get; set; }
    public DbSet<VehicleSpecification> VehicleSpecifications { get; set; }
    public DbSet<VehicleDocument> VehicleDocuments { get; set; }
    public DbSet<VehicleInspection> VehicleInspections { get; set; }
    public DbSet<VehicleInsurance> VehicleInsurances { get; set; }
    public DbSet<VehicleMaintenance> VehicleMaintenances { get; set; }
    #endregion

    #region SACCO Module Entities
    public DbSet<Sacco> Saccos { get; set; }
    public DbSet<SaccoMember> SaccoMembers { get; set; }
    public DbSet<SaccoCommittee> SaccoCommittees { get; set; }
    public DbSet<SaccoCommitteeMember> SaccoCommitteeMembers { get; set; }
    public DbSet<SaccoMeeting> SaccoMeetings { get; set; }
    public DbSet<SaccoFinancial> SaccoFinancials { get; set; }
    public DbSet<SaccoShare> SaccoShares { get; set; }
    public DbSet<SaccoLoan> SaccoLoans { get; set; }
    public DbSet<SaccoService> SaccoServices { get; set; }
    #endregion

    #region Product Module Entities
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductSpecification> ProductSpecifications { get; set; }
    public DbSet<ProductDocument> ProductDocuments { get; set; }
    public DbSet<ProductPricing> ProductPricings { get; set; }
    #endregion

    #region Route Module Entities
    public DbSet<Route> Routes { get; set; }
    public DbSet<RouteWaypoint> RouteWaypoints { get; set; }
    public DbSet<RouteSchedule> RouteSchedules { get; set; }
    public DbSet<RouteHistory> RouteHistories { get; set; }
    #endregion

    #region Weighbridge Module Entities
    public DbSet<Weighbridge> Weighbridges { get; set; }
    public DbSet<WeighbridgeCalibration> WeighbridgeCalibrations { get; set; }
    public DbSet<WeighbridgeMaintenance> WeighbridgeMaintenances { get; set; }
    public DbSet<WeighbridgeTransaction> WeighbridgeTransactions { get; set; }
    public DbSet<WeighbridgeDocument> WeighbridgeDocuments { get; set; }
    #endregion

    #region Report Module Entities
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportTemplate> ReportTemplates { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }
    public DbSet<ReportExecution> ReportExecutions { get; set; }
    public DbSet<ReportPermission> ReportPermissions { get; set; }
    #endregion

    #region Cross-Module Relationships
    public DbSet<DriverSaccoMembership> DriverSaccoMemberships { get; set; }
    public DbSet<VehicleTransporterOwnership> VehicleTransporterOwnerships { get; set; }
    public DbSet<DriverVehicleAssignment> DriverVehicleAssignments { get; set; }
    public DbSet<DriverTransporterEmployment> DriverTransporterEmployments { get; set; }
    public DbSet<ProductSupplierCatalog> ProductSupplierCatalogs { get; set; }
    public DbSet<RouteWeighbridgeAssociation> RouteWeighbridgeAssociations { get; set; }
    public DbSet<OrganizationWeighbridgeOwnership> OrganizationWeighbridgeOwnerships { get; set; }
    public DbSet<VehicleSaccoRegistration> VehicleSaccoRegistrations { get; set; }
    public DbSet<UserOrganizationRole> UserOrganizationRoles { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureOrganizationModule(modelBuilder);
        ConfigureBusinessEntitiesModule(modelBuilder);
        ConfigureDriverModule(modelBuilder);
        ConfigureVehicleModule(modelBuilder);
        ConfigureSaccoModule(modelBuilder);
        ConfigureProductModule(modelBuilder);
        ConfigureRouteModule(modelBuilder);
        ConfigureWeighbridgeModule(modelBuilder);
        ConfigureReportModule(modelBuilder);
        ConfigureCrossModuleRelationships(modelBuilder);
        ConfigureIndexes(modelBuilder);
        ConfigureGlobalFilters(modelBuilder);
    }

    private void ConfigureOrganizationModule(ModelBuilder modelBuilder)
    {
        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);
        });

        // One-to-many relationships
        modelBuilder.Entity<OrganizationUser>()
            .HasOne(ou => ou.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(ou => ou.OrganizationId);

        modelBuilder.Entity<OrganizationLocation>()
            .HasOne(ol => ol.Organization)
            .WithMany(o => o.Locations)
            .HasForeignKey(ol => ol.OrganizationId);

        // One-to-one relationships
        modelBuilder.Entity<OrganizationSettings>()
            .HasOne(os => os.Organization)
            .WithOne(o => o.Settings)
            .HasForeignKey<OrganizationSettings>(os => os.OrganizationId);

        modelBuilder.Entity<OrganizationSubscription>()
            .HasOne(os => os.Organization)
            .WithOne(o => o.Subscription)
            .HasForeignKey<OrganizationSubscription>(os => os.OrganizationId);
    }

    private void ConfigureBusinessEntitiesModule(ModelBuilder modelBuilder)
    {
        // BusinessEntity configuration
        modelBuilder.Entity<BusinessEntity>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Name });
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

        modelBuilder.Entity<TransporterProfile>()
            .HasOne(tp => tp.BusinessEntity)
            .WithOne(be => be.TransporterProfile)
            .HasForeignKey<TransporterProfile>(tp => tp.BusinessEntityId);

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

        modelBuilder.Entity<TransporterContract>()
            .HasOne(tc => tc.TransporterProfile)
            .WithMany(tp => tp.Contracts)
            .HasForeignKey(tc => tc.TransporterProfileId);

        modelBuilder.Entity<TransporterPerformance>()
            .HasOne(tp => tp.TransporterProfile)
            .WithMany(tpp => tpp.PerformanceRecords)
            .HasForeignKey(tp => tp.TransporterProfileId);

        modelBuilder.Entity<TransporterInsurance>()
            .HasOne(ti => ti.TransporterProfile)
            .WithMany(tp => tp.InsurancePolicies)
            .HasForeignKey(ti => ti.TransporterProfileId);
    }

    private void ConfigureDriverModule(ModelBuilder modelBuilder)
    {
        // Driver configuration
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Status });
        });

        // One-to-one relationships
        modelBuilder.Entity<DriverLicense>()
            .HasOne(dl => dl.Driver)
            .WithOne(d => d.License)
            .HasForeignKey<DriverLicense>(dl => dl.DriverId);

        modelBuilder.Entity<DriverProfile>()
            .HasOne(dp => dp.Driver)
            .WithOne(d => d.Profile)
            .HasForeignKey<DriverProfile>(dp => dp.DriverId);

        // One-to-many relationships
        modelBuilder.Entity<DriverDocument>()
            .HasOne(dd => dd.Driver)
            .WithMany(d => d.Documents)
            .HasForeignKey(dd => dd.DriverId);

        modelBuilder.Entity<DriverTraining>()
            .HasOne(dt => dt.Driver)
            .WithMany(d => d.Trainings)
            .HasForeignKey(dt => dt.DriverId);

        modelBuilder.Entity<DriverMedical>()
            .HasOne(dm => dm.Driver)
            .WithMany(d => d.MedicalRecords)
            .HasForeignKey(dm => dm.DriverId);

        modelBuilder.Entity<DriverViolation>()
            .HasOne(dv => dv.Driver)
            .WithMany(d => d.Violations)
            .HasForeignKey(dv => dv.DriverId);

        modelBuilder.Entity<DriverPerformance>()
            .HasOne(dp => dp.Driver)
            .WithMany(d => d.PerformanceRecords)
            .HasForeignKey(dp => dp.DriverId);
    }

    private void ConfigureVehicleModule(ModelBuilder modelBuilder)
    {
        // Vehicle configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            entity.HasIndex(e => e.VIN).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Status });
        });

        // Vehicle-VehicleType relationship
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.VehicleType)
            .WithMany(vt => vt.Vehicles)
            .HasForeignKey(v => v.VehicleTypeId);

        // One-to-one relationships
        modelBuilder.Entity<VehicleRegistration>()
            .HasOne(vr => vr.Vehicle)
            .WithOne(v => v.Registration)
            .HasForeignKey<VehicleRegistration>(vr => vr.VehicleId);

        modelBuilder.Entity<VehicleSpecification>()
            .HasOne(vs => vs.Vehicle)
            .WithOne(v => v.Specification)
            .HasForeignKey<VehicleSpecification>(vs => vs.VehicleId);

        // One-to-many relationships
        modelBuilder.Entity<VehicleDocument>()
            .HasOne(vd => vd.Vehicle)
            .WithMany(v => v.Documents)
            .HasForeignKey(vd => vd.VehicleId);

        modelBuilder.Entity<VehicleInspection>()
            .HasOne(vi => vi.Vehicle)
            .WithMany(v => v.Inspections)
            .HasForeignKey(vi => vi.VehicleId);

        modelBuilder.Entity<VehicleInsurance>()
            .HasOne(vi => vi.Vehicle)
            .WithMany(v => v.InsurancePolicies)
            .HasForeignKey(vi => vi.VehicleId);

        modelBuilder.Entity<VehicleMaintenance>()
            .HasOne(vm => vm.Vehicle)
            .WithMany(v => v.MaintenanceRecords)
            .HasForeignKey(vm => vm.VehicleId);
    }

    private void ConfigureSaccoModule(ModelBuilder modelBuilder)
    {
        // SACCO configuration
        modelBuilder.Entity<Sacco>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            entity.HasIndex(e => e.Name);
        });

        // One-to-many relationships
        modelBuilder.Entity<SaccoMember>()
            .HasOne(sm => sm.Sacco)
            .WithMany(s => s.Members)
            .HasForeignKey(sm => sm.SaccoId);

        modelBuilder.Entity<SaccoCommittee>()
            .HasOne(sc => sc.Sacco)
            .WithMany(s => s.Committees)
            .HasForeignKey(sc => sc.SaccoId);

        modelBuilder.Entity<SaccoMeeting>()
            .HasOne(sm => sm.Sacco)
            .WithMany(s => s.Meetings)
            .HasForeignKey(sm => sm.SaccoId);

        modelBuilder.Entity<SaccoShare>()
            .HasOne(ss => ss.Sacco)
            .WithMany(s => s.Shares)
            .HasForeignKey(ss => ss.SaccoId);

        modelBuilder.Entity<SaccoLoan>()
            .HasOne(sl => sl.Sacco)
            .WithMany(s => s.Loans)
            .HasForeignKey(sl => sl.SaccoId);

        modelBuilder.Entity<SaccoService>()
            .HasOne(ss => ss.Sacco)
            .WithMany(s => s.Services)
            .HasForeignKey(ss => ss.SaccoId);

        // One-to-one relationship
        modelBuilder.Entity<SaccoFinancial>()
            .HasOne(sf => sf.Sacco)
            .WithOne(s => s.Financial)
            .HasForeignKey<SaccoFinancial>(sf => sf.SaccoId);

        // Committee member relationships
        modelBuilder.Entity<SaccoCommitteeMember>()
            .HasOne(scm => scm.Committee)
            .WithMany(sc => sc.CommitteeMembers)
            .HasForeignKey(scm => scm.SaccoCommitteeId);

        modelBuilder.Entity<SaccoCommitteeMember>()
            .HasOne(scm => scm.Member)
            .WithMany()
            .HasForeignKey(scm => scm.SaccoMemberId);

        // Share and loan member relationships
        modelBuilder.Entity<SaccoShare>()
            .HasOne(ss => ss.Member)
            .WithMany()
            .HasForeignKey(ss => ss.SaccoMemberId);

        modelBuilder.Entity<SaccoLoan>()
            .HasOne(sl => sl.Member)
            .WithMany()
            .HasForeignKey(sl => sl.SaccoMemberId);

        // Meeting committee relationship (optional)
        modelBuilder.Entity<SaccoMeeting>()
            .HasOne(sm => sm.Committee)
            .WithMany()
            .HasForeignKey(sm => sm.SaccoCommitteeId)
            .IsRequired(false);
    }

    private void ConfigureProductModule(ModelBuilder modelBuilder)
    {
        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Name });
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Status);
        });

        // One-to-many relationships
        modelBuilder.Entity<ProductSpecification>()
            .HasOne(ps => ps.Product)
            .WithMany(p => p.Specifications)
            .HasForeignKey(ps => ps.ProductId);

        modelBuilder.Entity<ProductDocument>()
            .HasOne(pd => pd.Product)
            .WithMany(p => p.Documents)
            .HasForeignKey(pd => pd.ProductId);

        modelBuilder.Entity<ProductPricing>()
            .HasOne(pp => pp.Product)
            .WithMany(p => p.Pricing)
            .HasForeignKey(pp => pp.ProductId);
    }

    private void ConfigureRouteModule(ModelBuilder modelBuilder)
    {
        // Route configuration
        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Name });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RouteType);
        });

        // One-to-many relationships
        modelBuilder.Entity<RouteWaypoint>()
            .HasOne(rw => rw.Route)
            .WithMany(r => r.Waypoints)
            .HasForeignKey(rw => rw.RouteId);

        modelBuilder.Entity<RouteSchedule>()
            .HasOne(rs => rs.Route)
            .WithMany(r => r.Schedules)
            .HasForeignKey(rs => rs.RouteId);

        modelBuilder.Entity<RouteHistory>()
            .HasOne(rh => rh.Route)
            .WithMany(r => r.History)
            .HasForeignKey(rh => rh.RouteId);

        // Waypoint ordering
        modelBuilder.Entity<RouteWaypoint>()
            .HasIndex(e => new { e.RouteId, e.SequenceOrder });
    }

    private void ConfigureWeighbridgeModule(ModelBuilder modelBuilder)
    {
        // Weighbridge configuration
        modelBuilder.Entity<Weighbridge>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Name });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Location);
        });

        // One-to-many relationships
        modelBuilder.Entity<WeighbridgeCalibration>()
            .HasOne(wc => wc.Weighbridge)
            .WithMany(w => w.Calibrations)
            .HasForeignKey(wc => wc.WeighbridgeId);

        modelBuilder.Entity<WeighbridgeMaintenance>()
            .HasOne(wm => wm.Weighbridge)
            .WithMany(w => w.MaintenanceRecords)
            .HasForeignKey(wm => wm.WeighbridgeId);

        modelBuilder.Entity<WeighbridgeTransaction>()
            .HasOne(wt => wt.Weighbridge)
            .WithMany(w => w.Transactions)
            .HasForeignKey(wt => wt.WeighbridgeId);

        modelBuilder.Entity<WeighbridgeDocument>()
            .HasOne(wd => wd.Weighbridge)
            .WithMany(w => w.Documents)
            .HasForeignKey(wd => wd.WeighbridgeId);

        // Additional indexes for performance
        modelBuilder.Entity<WeighbridgeTransaction>()
            .HasIndex(e => e.TicketNumber)
            .IsUnique();

        modelBuilder.Entity<WeighbridgeTransaction>()
            .HasIndex(e => e.TransactionDate);

        modelBuilder.Entity<WeighbridgeCalibration>()
            .HasIndex(e => e.CertificateNumber)
            .IsUnique();
    }

    private void ConfigureReportModule(ModelBuilder modelBuilder)
    {
        // Report configuration
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Name });
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ReportType);
        });

        // One-to-many relationships
        modelBuilder.Entity<ReportTemplate>()
            .HasOne(rt => rt.Report)
            .WithMany(r => r.Templates)
            .HasForeignKey(rt => rt.ReportId);

        modelBuilder.Entity<ReportSchedule>()
            .HasOne(rs => rs.Report)
            .WithMany(r => r.Schedules)
            .HasForeignKey(rs => rs.ReportId);

        modelBuilder.Entity<ReportExecution>()
            .HasOne(re => re.Report)
            .WithMany(r => r.Executions)
            .HasForeignKey(re => re.ReportId);

        modelBuilder.Entity<ReportExecution>()
            .HasOne(re => re.Schedule)
            .WithMany(rs => rs.Executions)
            .HasForeignKey(re => re.ScheduleId)
            .IsRequired(false);

        modelBuilder.Entity<ReportPermission>()
            .HasOne(rp => rp.Report)
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.ReportId);

        // Additional indexes for performance
        modelBuilder.Entity<ReportExecution>()
            .HasIndex(e => e.ExecutionId)
            .IsUnique();

        modelBuilder.Entity<ReportExecution>()
            .HasIndex(e => e.StartTime);

        modelBuilder.Entity<ReportSchedule>()
            .HasIndex(e => e.NextRunTime);

        modelBuilder.Entity<ReportPermission>()
            .HasIndex(e => new { e.UserId, e.ReportId })
            .IsUnique();
    }

    private void ConfigureCrossModuleRelationships(ModelBuilder modelBuilder)
    {
        // Driver-SACCO membership
        modelBuilder.Entity<DriverSaccoMembership>(entity =>
        {
            entity.HasIndex(e => new { e.DriverId, e.SaccoId }).IsUnique();
            entity.HasIndex(e => e.MembershipNumber).IsUnique();
            
            entity.HasOne(e => e.Driver)
                .WithMany()
                .HasForeignKey(e => e.DriverId);
                
            entity.HasOne(e => e.Sacco)
                .WithMany()
                .HasForeignKey(e => e.SaccoId);
        });

        // Vehicle-Transporter ownership
        modelBuilder.Entity<VehicleTransporterOwnership>(entity =>
        {
            entity.HasIndex(e => new { e.VehicleId, e.TransporterProfileId });
            
            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId);
                
            entity.HasOne(e => e.TransporterProfile)
                .WithMany()
                .HasForeignKey(e => e.TransporterProfileId);
        });

        // Driver-Vehicle assignment
        modelBuilder.Entity<DriverVehicleAssignment>(entity =>
        {
            entity.HasIndex(e => new { e.DriverId, e.VehicleId, e.IsActive });
            
            entity.HasOne(e => e.Driver)
                .WithMany()
                .HasForeignKey(e => e.DriverId);
                
            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId);
        });

        // Driver-Transporter employment
        modelBuilder.Entity<DriverTransporterEmployment>(entity =>
        {
            entity.HasIndex(e => new { e.DriverId, e.TransporterProfileId });
            
            entity.HasOne(e => e.Driver)
                .WithMany()
                .HasForeignKey(e => e.DriverId);
                
            entity.HasOne(e => e.TransporterProfile)
                .WithMany()
                .HasForeignKey(e => e.TransporterProfileId);
        });

        // Product-Supplier catalog
        modelBuilder.Entity<ProductSupplierCatalog>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.SupplierProfileId });
            
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId);
                
            entity.HasOne(e => e.SupplierProfile)
                .WithMany()
                .HasForeignKey(e => e.SupplierProfileId);
        });

        // Route-Weighbridge association
        modelBuilder.Entity<RouteWeighbridgeAssociation>(entity =>
        {
            entity.HasIndex(e => new { e.RouteId, e.WeighbridgeId });
            
            entity.HasOne(e => e.Route)
                .WithMany()
                .HasForeignKey(e => e.RouteId);
                
            entity.HasOne(e => e.Weighbridge)
                .WithMany()
                .HasForeignKey(e => e.WeighbridgeId);
        });

        // Organization-Weighbridge ownership
        modelBuilder.Entity<OrganizationWeighbridgeOwnership>(entity =>
        {
            entity.HasIndex(e => new { e.OrganizationId, e.WeighbridgeId });
            
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId);
                
            entity.HasOne(e => e.Weighbridge)
                .WithMany()
                .HasForeignKey(e => e.WeighbridgeId);
        });

        // Vehicle-SACCO registration
        modelBuilder.Entity<VehicleSaccoRegistration>(entity =>
        {
            entity.HasIndex(e => new { e.VehicleId, e.SaccoId });
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            
            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId);
                
            entity.HasOne(e => e.Sacco)
                .WithMany()
                .HasForeignKey(e => e.SaccoId);
        });

        // User-Organization-Role (User and Role entities not defined in this context)
        modelBuilder.Entity<UserOrganizationRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.OrganizationId, e.RoleId }).IsUnique();
            
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId);
        });
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Global performance indexes
        modelBuilder.Entity<Organization>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<BusinessEntity>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Driver>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Vehicle>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Sacco>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Product>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Route>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Weighbridge>().HasIndex(e => e.IsDeleted);
        modelBuilder.Entity<Report>().HasIndex(e => e.IsDeleted);

        // Timestamp indexes for common queries
        modelBuilder.Entity<Organization>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<BusinessEntity>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Driver>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Vehicle>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Sacco>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Product>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Route>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Weighbridge>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<Report>().HasIndex(e => e.CreatedAt);
    }

    private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
    {
        // Global soft delete filter for ALL entities that inherit BaseEntity
        // This prevents the EF Core warnings about orphaned relationships
        
        // Organization Module
        modelBuilder.Entity<Organization>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrganizationUser>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrganizationLocation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrganizationSettings>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrganizationSubscription>().HasQueryFilter(e => !e.IsDeleted);
        
        // BusinessEntity Module
        modelBuilder.Entity<BusinessEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CustomerProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SupplierProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransporterProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BusinessEntityContact>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BusinessEntityLocation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BusinessEntityDocument>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CustomerContract>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CustomerOrder>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SupplierContract>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SupplierPerformance>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransporterContract>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransporterPerformance>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TransporterInsurance>().HasQueryFilter(e => !e.IsDeleted);
        
        // Driver Module
        modelBuilder.Entity<Driver>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverLicense>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverDocument>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverTraining>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverMedical>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverViolation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverPerformance>().HasQueryFilter(e => !e.IsDeleted);
        
        // Vehicle Module
        modelBuilder.Entity<Vehicle>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleRegistration>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleSpecification>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleDocument>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleInspection>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleInsurance>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleMaintenance>().HasQueryFilter(e => !e.IsDeleted);
        
        // SACCO Module
        modelBuilder.Entity<Sacco>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SaccoMember>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SaccoCommittee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SaccoCommitteeMember>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SaccoMeeting>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SaccoFinancial>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SaccoShare>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SaccoLoan>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SaccoService>().HasQueryFilter(e => !e.IsDeleted);
        
        // Product Module
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductSpecification>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductDocument>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductPricing>().HasQueryFilter(e => !e.IsDeleted);
        
        // Route Module
        modelBuilder.Entity<Route>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteWaypoint>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteSchedule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteHistory>().HasQueryFilter(e => !e.IsDeleted);
        
        // Weighbridge Module
        modelBuilder.Entity<Weighbridge>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WeighbridgeCalibration>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WeighbridgeMaintenance>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WeighbridgeTransaction>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WeighbridgeDocument>().HasQueryFilter(e => !e.IsDeleted);
        
        // Report Module
        modelBuilder.Entity<Report>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportTemplate>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportSchedule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportExecution>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportPermission>().HasQueryFilter(e => !e.IsDeleted);

        // Cross-Module Relationships
        modelBuilder.Entity<DriverSaccoMembership>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleTransporterOwnership>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverVehicleAssignment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DriverTransporterEmployment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductSupplierCatalog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RouteWeighbridgeAssociation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrganizationWeighbridgeOwnership>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VehicleSaccoRegistration>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserOrganizationRole>().HasQueryFilter(e => !e.IsDeleted);
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