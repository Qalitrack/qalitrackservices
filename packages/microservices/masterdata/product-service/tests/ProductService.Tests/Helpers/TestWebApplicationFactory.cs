using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProductService.Infrastructure.Data;

namespace ProductService.Tests.Helpers;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private static readonly object _seedLock = new object();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all DbContext related registrations
            services.RemoveAll(typeof(DbContextOptions<ProductDbContext>));
            services.RemoveAll(typeof(ProductDbContext));
            services.RemoveAll<DbContextOptions<ProductDbContext>>();
            services.RemoveAll<ProductDbContext>();

            // Add in-memory database for testing with fresh options
            services.AddDbContext<ProductDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestProductServiceDb");
                options.EnableServiceProviderCaching(false);
                options.EnableSensitiveDataLogging();
            });
        });

        builder.UseEnvironment("Testing");
    }

    public void SeedData()
    {
        lock (_seedLock)
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            context.Database.EnsureCreated();
            
            // Always ensure fresh test data
            SeedTestData(context);
        }
    }

    private static void SeedTestData(ProductDbContext context)
    {
        // Clear existing data if any
        if (context.Products.Any())
        {
            context.Products.RemoveRange(context.Products);
        }
        if (context.ProductCategories.Any())
        {
            context.ProductCategories.RemoveRange(context.ProductCategories);
        }
        context.SaveChanges();

        // Seed test categories
        var categories = TestDataFactory.CreateTestCategoryHierarchy();
        foreach (var category in categories)
        {
            context.ProductCategories.Add(category);
        }
        context.SaveChanges();

        // Seed test products
        var products = TestDataFactory.CreateTestProductList();
        foreach (var product in products)
        {
            // Set category to one of the seeded categories
            product.CategoryId = categories.First().Id;
            context.Products.Add(product);
        }
        context.SaveChanges();

        // Seed additional test data for specific test scenarios
        SeedHazardousProducts(context, categories.First().Id);
        SeedProductSpecifications(context, products);
        SeedProductPricing(context, products);
        SeedProductCompliance(context, products);
        
        context.SaveChanges();
    }

    private static void SeedHazardousProducts(ProductDbContext context, string categoryId)
    {
        var hazardousProducts = new[]
        {
            TestDataFactory.CreateTestProduct(isHazardous: true),
            TestDataFactory.CreateTestProduct(isHazardous: true)
        };

        foreach (var product in hazardousProducts)
        {
            product.CategoryId = categoryId;
            product.Name = $"Hazardous {product.Name}";
            product.Code = $"HAZ{product.Code[3..]}";
            product.HazmatClass = "Class 3";
            product.RequiresSpecialHandling = true;
            context.Products.Add(product);
        }
    }

    private static void SeedProductSpecifications(ProductDbContext context, List<Core.Entities.Product> products)
    {
        foreach (var product in products.Take(3)) // Add specs for first 3 products
        {
            var specifications = new[]
            {
                TestDataFactory.CreateTestProductSpecification(product.Id),
                TestDataFactory.CreateTestProductSpecification(product.Id)
            };

            specifications[0].Name = "Viscosity";
            specifications[0].Value = "High";
            specifications[0].Unit = "cP";

            specifications[1].Name = "Flash Point";
            specifications[1].Value = "60";
            specifications[1].Unit = "°C";

            foreach (var spec in specifications)
            {
                context.ProductSpecifications.Add(spec);
            }
        }
    }

    private static void SeedProductPricing(ProductDbContext context, List<Core.Entities.Product> products)
    {
        foreach (var product in products.Take(3)) // Add pricing for first 3 products
        {
            var pricing = new[]
            {
                TestDataFactory.CreateTestProductPricing(product.Id),
                TestDataFactory.CreateTestProductPricing(product.Id)
            };

            pricing[0].PricingType = PricingType.Standard;
            pricing[0].UnitPrice = 99.99m;

            pricing[1].PricingType = PricingType.Volume;
            pricing[1].UnitPrice = 149.99m;

            foreach (var price in pricing)
            {
                context.ProductPricing.Add(price);
            }
        }
    }

    private static void SeedProductCompliance(ProductDbContext context, List<Core.Entities.Product> products)
    {
        foreach (var product in products.Take(2)) // Add compliance for first 2 products
        {
            var compliance = new[]
            {
                TestDataFactory.CreateTestProductCompliance(product.Id),
                TestDataFactory.CreateTestProductCompliance(product.Id)
            };

            compliance[0].ComplianceType = "DOT";
            compliance[0].Regulation = "Transportation Regulation";
            compliance[0].CertificationNumber = "UN1993";

            compliance[1].ComplianceType = "EPA";
            compliance[1].Regulation = "Environmental Regulation";
            compliance[1].CertificationNumber = "EPA-123";

            foreach (var comp in compliance)
            {
                context.ProductCompliance.Add(comp);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        // Cleanup code if needed
        base.Dispose(disposing);
    }
}