using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CustomerService.Infrastructure.Data;

namespace CustomerService.Tests.Helpers;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> 
    where TStartup : class
{
    private readonly string _databaseName = $"TestCustomerServiceDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all existing DbContext-related registrations
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<CustomerDbContext>) ||
                d.ServiceType == typeof(CustomerDbContext) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                d.ServiceType.Name.Contains("DbContext") ||
                d.ServiceType.Name.Contains("EntityFramework") ||
                d.ServiceType.Name.Contains("Sqlite") ||
                d.ServiceType.Name.Contains("InMemory")).ToList();
            
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Clear the services collection and rebuild only what we need for testing
            services.RemoveAll<DbContextOptions<CustomerDbContext>>();
            services.RemoveAll<CustomerDbContext>();
            
            // Add in-memory database for testing with a unique name per factory instance
            services.AddDbContext<CustomerDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
            });
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        builder.UseEnvironment("Testing");
    }

    public void SeedTestData()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        context.Database.EnsureCreated();
        SeedTestData(context);
    }

    public void ClearDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        
        // Clear existing data
        context.Customers.RemoveRange(context.Customers);
        context.CustomerContacts.RemoveRange(context.CustomerContacts);
        context.CustomerContracts.RemoveRange(context.CustomerContracts);
        context.CustomerBilling.RemoveRange(context.CustomerBilling);
        context.CustomerCredit.RemoveRange(context.CustomerCredit);
        context.SaveChanges();
    }

    private static void SeedTestData(CustomerDbContext context)
    {
        // Clear existing data
        context.Customers.RemoveRange(context.Customers);
        context.CustomerContacts.RemoveRange(context.CustomerContacts);
        context.CustomerContracts.RemoveRange(context.CustomerContracts);
        context.CustomerBilling.RemoveRange(context.CustomerBilling);
        context.CustomerCredit.RemoveRange(context.CustomerCredit);
        context.SaveChanges();

        // Seed test customers
        var customers = TestDataFactory.CreateTestCustomerList(10);
        context.Customers.AddRange(customers);
        context.SaveChanges();

        // Seed related data for first few customers
        SeedCustomerContacts(context, customers.Take(3).ToList());
        SeedCustomerContracts(context, customers.Take(3).ToList());
        SeedCustomerBilling(context, customers.Take(3).ToList());
        SeedCustomerCredit(context, customers.Take(5).ToList());

        context.SaveChanges();
    }

    private static void SeedCustomerContacts(CustomerDbContext context, List<Customer> customers)
    {
        foreach (var customer in customers)
        {
            var contacts = new List<CustomerContact>
            {
                TestDataFactory.CreateTestCustomerContact(customer.Id, contactType: ContactType.Primary),
                TestDataFactory.CreateTestCustomerContact(customer.Id, contactType: ContactType.Technical)
            };
            context.CustomerContacts.AddRange(contacts);
        }
    }

    private static void SeedCustomerContracts(CustomerDbContext context, List<Customer> customers)
    {
        foreach (var customer in customers)
        {
            var contracts = new List<CustomerContract>
            {
                TestDataFactory.CreateTestCustomerContract(customer.Id, status: ContractStatus.Active),
                TestDataFactory.CreateTestCustomerContract(customer.Id, status: ContractStatus.Draft)
            };
            context.CustomerContracts.AddRange(contracts);
        }
    }

    private static void SeedCustomerBilling(CustomerDbContext context, List<Customer> customers)
    {
        foreach (var customer in customers)
        {
            var billing = TestDataFactory.CreateTestCustomerBilling(customer.Id);
            context.CustomerBilling.Add(billing);
        }
    }

    private static void SeedCustomerCredit(CustomerDbContext context, List<Customer> customers)
    {
        foreach (var customer in customers)
        {
            var credit = TestDataFactory.CreateTestCustomerCredit(
                customer.Id, 
                creditLimit: customer.CreditLimit,
                usedCredit: Random.Shared.Next(0, (int)(customer.CreditLimit * 0.8m)),
                creditStatus: CreditStatus.Good);
            
            context.CustomerCredit.Add(credit);
        }
    }
}

public class TestDatabaseFixture : IDisposable
{
    public CustomerDbContext Context { get; private set; }

    public TestDatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase($"TestCustomerDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        Context = new CustomerDbContext(options);
        Context.Database.EnsureCreated();
        SeedTestData();
    }

    private void SeedTestData()
    {
        var customers = TestDataFactory.CreateTestCustomerList(5);
        Context.Customers.AddRange(customers);

        foreach (var customer in customers.Take(3))
        {
            // Add contacts
            var contacts = new List<CustomerContact>
            {
                TestDataFactory.CreateTestCustomerContact(customer.Id),
                TestDataFactory.CreateTestCustomerContact(customer.Id)
            };
            Context.CustomerContacts.AddRange(contacts);

            // Add contracts
            var contracts = new List<CustomerContract>
            {
                TestDataFactory.CreateTestCustomerContract(customer.Id)
            };
            Context.CustomerContracts.AddRange(contracts);

            // Add billing
            var billing = TestDataFactory.CreateTestCustomerBilling(customer.Id);
            Context.CustomerBilling.Add(billing);

            // Add credit
            var credit = TestDataFactory.CreateTestCustomerCredit(customer.Id);
            Context.CustomerCredit.Add(credit);
        }

        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}