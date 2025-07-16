using Microsoft.EntityFrameworkCore;
using CustomerService.Core.Entities;
using System.Text.Json;

namespace CustomerService.Infrastructure.Data;

public class CustomerServiceDbContext : DbContext
{
    public CustomerServiceDbContext(DbContextOptions<CustomerServiceDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerService.Core.Entities.Customer> Customers { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractRenewal> ContractRenewals { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }
    public DbSet<ContactCommunication> ContactCommunications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Customer entity
        modelBuilder.Entity<CustomerService.Core.Entities.Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.BillingAddress).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);

            entity.HasIndex(e => e.TaxNumber).IsUnique().HasFilter("[TaxNumber] IS NOT NULL");
            entity.HasIndex(e => e.RegistrationNumber).IsUnique().HasFilter("[RegistrationNumber] IS NOT NULL");
            entity.HasIndex(e => e.ContactEmail).IsUnique();
        });

        // Configure Contact entity
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Mobile).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.PreferredContactTime).HasMaxLength(100);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10);
            entity.Property(e => e.LastContactMethod).HasMaxLength(50);
            entity.Property(e => e.LastContactNotes).HasMaxLength(1000);
            entity.Property(e => e.Address).HasMaxLength(1000);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.LinkedInProfile).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Contacts)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Email);
        });

        // Configure Contract entity
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ContractValue).HasPrecision(18, 2);
            entity.Property(e => e.PricingTerms).HasMaxLength(2000);
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.PricingModel).HasMaxLength(100);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.PaymentTerms).HasMaxLength(1000);
            entity.Property(e => e.ComplianceRequirements).HasMaxLength(2000);
            entity.Property(e => e.RegulatoryStandards).HasMaxLength(1000);
            entity.Property(e => e.QualityStandards).HasMaxLength(1000);
            entity.Property(e => e.SafetyRequirements).HasMaxLength(1000);
            entity.Property(e => e.EnvironmentalRequirements).HasMaxLength(1000);
            entity.Property(e => e.SignedByCustomer).HasMaxLength(200);
            entity.Property(e => e.SignedByCompany).HasMaxLength(200);
            entity.Property(e => e.RenewalNotificationEmail).HasMaxLength(255);
            entity.Property(e => e.PerformanceMetrics).HasMaxLength(2000);
            entity.Property(e => e.PenaltyClause).HasMaxLength(2000);
            entity.Property(e => e.LatePaymentPenalty).HasPrecision(18, 2);
            entity.Property(e => e.TerminationClause).HasMaxLength(2000);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Contracts)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ContractNumber).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.EndDate);
        });

        // Configure ContractRenewal entity
        modelBuilder.Entity<ContractRenewal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NewContractValue).HasPrecision(18, 2);
            entity.Property(e => e.RenewalTerms).HasMaxLength(2000);
            entity.Property(e => e.RenewedBy).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne(e => e.Contract)
                  .WithMany(c => c.Renewals)
                  .HasForeignKey(e => e.ContractId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ContractId);
            entity.HasIndex(e => e.RenewalDate);
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.UnitOfMeasure).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 4);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.OriginLocation).HasMaxLength(500);
            entity.Property(e => e.DestinationLocation).HasMaxLength(500);
            entity.Property(e => e.QualitySpecifications).HasMaxLength(2000);
            entity.Property(e => e.SpecialInstructions).HasMaxLength(2000);
            entity.Property(e => e.TolerancePercentage).HasPrecision(5, 2);
            entity.Property(e => e.CustomerOrderReference).HasMaxLength(100);
            entity.Property(e => e.SupplierOrderReference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.CancellationReason).HasMaxLength(1000);
            entity.Property(e => e.CancelledBy).HasMaxLength(100);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.CustomerOrders)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                  .WithMany(c => c.SupplierOrders)
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OrderDate);
        });

        // Configure OrderStatusHistory entity
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.StatusHistory)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ChangedAt);
        });

        // Configure ContactCommunication entity
        modelBuilder.Entity<ContactCommunication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).HasMaxLength(4000);
            entity.Property(e => e.InitiatedBy).HasMaxLength(100);
            entity.Property(e => e.ResponseRequired).HasMaxLength(500);
            entity.Property(e => e.Resolution).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Contact)
                  .WithMany(c => c.Communications)
                  .HasForeignKey(e => e.ContactId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ContactId);
            entity.HasIndex(e => e.CommunicationDate);
            entity.HasIndex(e => e.Type);
        });

        // Add global query filter for soft deletes
        modelBuilder.Entity<CustomerService.Core.Entities.Customer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Contact>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Contract>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ContractRenewal>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrderStatusHistory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ContactCommunication>().HasQueryFilter(e => !e.IsDeleted);
    }
}