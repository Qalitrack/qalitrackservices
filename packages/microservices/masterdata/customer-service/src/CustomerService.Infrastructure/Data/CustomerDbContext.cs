using Microsoft.EntityFrameworkCore;
using CustomerService.Core.Entities;
using System.Text.Json;

namespace CustomerService.Infrastructure.Data;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerContact> CustomerContacts { get; set; }
    public DbSet<CustomerContract> CustomerContracts { get; set; }
    public DbSet<CustomerBilling> CustomerBilling { get; set; }
    public DbSet<CustomerLocation> CustomerLocations { get; set; }
    public DbSet<CustomerDocument> CustomerDocuments { get; set; }
    public DbSet<CustomerPreference> CustomerPreferences { get; set; }
    public DbSet<CustomerCredit> CustomerCredit { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.BillingAddress).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);

            entity.HasIndex(e => e.TaxNumber).IsUnique().HasFilter("[TaxNumber] IS NOT NULL");
            entity.HasIndex(e => e.RegistrationNumber).IsUnique().HasFilter("[RegistrationNumber] IS NOT NULL");
            entity.HasIndex(e => e.ContactEmail).IsUnique();
        });

        modelBuilder.Entity<CustomerContact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Mobile).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Contacts)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerContract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ContractValue).HasPrecision(18, 2);
            entity.Property(e => e.SignedByCustomer).HasMaxLength(200);
            entity.Property(e => e.SignedByCompany).HasMaxLength(200);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Contracts)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ContractNumber).IsUnique();
        });

        modelBuilder.Entity<CustomerBilling>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BillingContactName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BillingEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.BillingPhone).HasMaxLength(20);
            entity.Property(e => e.BillingAddress).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.BillingCity).HasMaxLength(100);
            entity.Property(e => e.BillingState).HasMaxLength(100);
            entity.Property(e => e.BillingCountry).HasMaxLength(100);
            entity.Property(e => e.BillingPostalCode).HasMaxLength(20);
            entity.Property(e => e.PreferredPaymentMethod).HasMaxLength(100);
            entity.Property(e => e.TaxExemptNumber).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);

            entity.HasOne(e => e.Customer)
                  .WithOne(c => c.Billing)
                  .HasForeignKey<CustomerBilling>(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.ContactPerson).HasMaxLength(200);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.AccessInstructions).HasMaxLength(1000);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Locations)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OriginalFileName).HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.FileUrl).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.UploadedBy).HasMaxLength(100);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Documents)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10);
            entity.Property(e => e.PreferredCurrency).HasMaxLength(3);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.PreferredContactTime).HasMaxLength(100);

            entity.Property(e => e.CustomSettings)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null));

            entity.OwnsOne(e => e.NotificationSettings);
            entity.OwnsOne(e => e.CommunicationPreferences);

            entity.HasOne(e => e.Customer)
                  .WithOne(c => c.Preferences)
                  .HasForeignKey<CustomerPreference>(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerCredit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.AvailableCredit).HasPrecision(18, 2);
            entity.Property(e => e.UsedCredit).HasPrecision(18, 2);
            entity.Property(e => e.CreditTerms).HasMaxLength(1000);
            entity.Property(e => e.SecurityDeposit).HasPrecision(18, 2);
            entity.Property(e => e.CreditReference1).HasMaxLength(500);
            entity.Property(e => e.CreditReference2).HasMaxLength(500);
            entity.Property(e => e.CreditReference3).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.OwnsOne(e => e.PaymentHistory);

            entity.HasOne(e => e.Customer)
                  .WithOne(c => c.Credit)
                  .HasForeignKey<CustomerCredit>(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}