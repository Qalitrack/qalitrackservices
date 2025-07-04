using CustomerService.Core.Entities;

namespace CustomerService.Tests.Helpers;

public static class TestDataFactory
{
    public static Customer CreateTestCustomer(
        string? id = null,
        string? name = null,
        string? email = null,
        CustomerType customerType = CustomerType.Corporate,
        CustomerStatus status = CustomerStatus.Active,
        decimal creditLimit = 10000)
    {
        return new Customer
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Customer Corp",
            TaxNumber = GenerateTaxNumber(),
            RegistrationNumber = GenerateRegistrationNumber(),
            ContactEmail = email ?? $"test{Guid.NewGuid().ToString()[..8]}@testcorp.com",
            ContactPhone = "+1-555-0123",
            BillingAddress = "123 Test Street, Test City, TC 12345",
            CustomerType = customerType,
            CreditLimit = creditLimit,
            Status = status,
            Notes = "Test customer created for unit testing",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            IsDeleted = false
        };
    }

    public static CustomerDto CreateTestCustomerDto(
        string? id = null,
        string? name = null,
        string? email = null,
        CustomerType customerType = CustomerType.Corporate,
        CustomerStatus status = CustomerStatus.Active)
    {
        return new CustomerDto
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Customer Corp",
            TaxNumber = GenerateTaxNumber(),
            RegistrationNumber = GenerateRegistrationNumber(),
            ContactEmail = email ?? $"test{Guid.NewGuid().ToString()[..8]}@testcorp.com",
            ContactPhone = "+1-555-0123",
            BillingAddress = "123 Test Street, Test City, TC 12345",
            CustomerType = customerType,
            CreditLimit = 10000,
            Status = status,
            Notes = "Test customer created for unit testing",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    public static RegisterCustomerRequest CreateRegisterCustomerRequest(
        string? name = null,
        string? email = null,
        CustomerType customerType = CustomerType.Corporate,
        decimal creditLimit = 10000)
    {
        return new RegisterCustomerRequest
        {
            Name = name ?? "New Customer Corp",
            TaxNumber = GenerateTaxNumber(),
            RegistrationNumber = GenerateRegistrationNumber(),
            ContactEmail = email ?? $"new{Guid.NewGuid().ToString()[..8]}@newcorp.com",
            ContactPhone = "+1-555-0456",
            BillingAddress = "456 New Street, New City, NC 67890",
            CustomerType = customerType,
            CreditLimit = creditLimit,
            Notes = "New customer registration request"
        };
    }

    public static UpdateCustomerRequest CreateUpdateCustomerRequest(
        string? name = null,
        string? email = null,
        CustomerStatus status = CustomerStatus.Active)
    {
        return new UpdateCustomerRequest
        {
            Name = name ?? "Updated Customer Corp",
            TaxNumber = GenerateTaxNumber(),
            RegistrationNumber = GenerateRegistrationNumber(),
            ContactEmail = email ?? $"updated{Guid.NewGuid().ToString()[..8]}@updatedcorp.com",
            ContactPhone = "+1-555-0789",
            BillingAddress = "789 Updated Street, Updated City, UC 11111",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 15000,
            Status = status,
            Notes = "Updated customer information"
        };
    }

    public static CustomerContact CreateTestCustomerContact(
        string? customerId = null,
        string? id = null,
        Core.Entities.ContactType contactType = Core.Entities.ContactType.Primary)
    {
        return new CustomerContact
        {
            Id = id ?? Guid.NewGuid().ToString(),
            CustomerId = customerId ?? Guid.NewGuid().ToString(),
            FirstName = "John",
            LastName = "Smith",
            Email = $"john.smith{Guid.NewGuid().ToString()[..8]}@testcorp.com",
            Phone = "+1-555-0100",
            Position = "Manager",
            Department = "Operations",
            ContactType = contactType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            IsDeleted = false
        };
    }

    public static CustomerContract CreateTestCustomerContract(
        string? customerId = null,
        string? id = null,
        Core.Entities.ContractStatus status = Core.Entities.ContractStatus.Active)
    {
        var contractNumber = $"CON-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        
        return new CustomerContract
        {
            Id = id ?? Guid.NewGuid().ToString(),
            CustomerId = customerId ?? Guid.NewGuid().ToString(),
            ContractNumber = contractNumber,
            Title = "Test Service Contract",
            Description = "Test contract description",
            ContractType = Core.Entities.ContractType.Service,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(335),
            ContractValue = 50000,
            Status = status,
            Terms = "Standard service contract terms and conditions",
            AutoRenew = true,
            RenewalPeriodMonths = 12,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            IsDeleted = false
        };
    }

    public static CustomerBilling CreateTestCustomerBilling(string? customerId = null)
    {
        return new CustomerBilling
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = customerId ?? Guid.NewGuid().ToString(),
            BillingAddress = "123 Billing Street, Billing City, BC 54321",
            BillingContactName = "Jane Doe",
            BillingEmail = $"billing{Guid.NewGuid().ToString()[..8]}@testcorp.com",
            BillingPhone = "+1-555-0200",
            BillingCity = "Billing City",
            BillingState = "BC",
            BillingCountry = "USA",
            BillingPostalCode = "54321",
            PaymentTerms = Core.Entities.PaymentTerms.Net30,
            PreferredPaymentMethod = "Bank Transfer",
            Currency = "USD",
            IsTaxExempt = false,
            TaxExemptNumber = null,
            AutomaticBilling = false,
            BillingCycleDay = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            IsDeleted = false
        };
    }

    public static CustomerCredit CreateTestCustomerCredit(
        string? customerId = null,
        decimal creditLimit = 10000,
        decimal usedCredit = 2500,
        Core.Entities.CreditStatus creditStatus = Core.Entities.CreditStatus.Good)
    {
        return new CustomerCredit
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = customerId ?? Guid.NewGuid().ToString(),
            CreditLimit = creditLimit,
            AvailableCredit = creditLimit - usedCredit,
            UsedCredit = usedCredit,
            CreditStatus = creditStatus,
            CreditScore = 750,
            LastCreditCheck = DateTime.UtcNow.AddDays(-7),
            NextCreditReview = DateTime.UtcNow.AddDays(90),
            CreditTerms = "Standard credit terms",
            RequiresApproval = creditLimit > 10000,
            SecurityDeposit = null,
            CreditReference1 = "Reference 1",
            CreditReference2 = "Reference 2",
            CreditReference3 = "Reference 3",
            Notes = "Test credit record",
            PaymentHistory = new Core.Entities.PaymentHistory
            {
                TotalInvoices = 25,
                OnTimePayments = 24,
                LatePayments = 1,
                TotalPaid = 25000,
                OutstandingBalance = 2500,
                LastPaymentDate = DateTime.UtcNow.AddDays(-15),
                LastPaymentAmount = 1000,
                AverageDaysToPayment = 28,
                OldestOutstandingInvoice = DateTime.UtcNow.AddDays(-30)
            },
            CreatedAt = DateTime.UtcNow.AddDays(-90),
            UpdatedAt = DateTime.UtcNow.AddDays(-7),
            IsDeleted = false
        };
    }

    public static List<Customer> CreateTestCustomerList(int count = 5)
    {
        var customers = new List<Customer>();
        var customerTypes = Enum.GetValues<CustomerType>();
        var statuses = Enum.GetValues<CustomerStatus>();

        for (int i = 0; i < count; i++)
        {
            customers.Add(CreateTestCustomer(
                name: $"Test Customer {i + 1}",
                email: $"customer{i + 1}@test{i + 1}.com",
                customerType: customerTypes[i % customerTypes.Length],
                status: statuses[i % statuses.Length]
            ));
        }

        return customers;
    }

    private static string GenerateTaxNumber()
    {
        return $"TAX{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(100, 999)}";
    }

    private static string GenerateRegistrationNumber()
    {
        return $"REG{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
    }
}