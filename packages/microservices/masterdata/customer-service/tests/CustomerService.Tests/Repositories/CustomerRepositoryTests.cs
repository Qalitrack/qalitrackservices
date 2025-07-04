using CustomerService.Infrastructure.Repositories;
using CustomerService.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerService.Tests.Repositories;

[Trait("Category", "Unit")]
public class CustomerRepositoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly CustomerDbContext _context;
    private readonly CustomerRepository _repository;
    private readonly Fixture _fixture;

    public CustomerRepositoryTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _repository = new CustomerRepository(_context);
        _fixture = new Fixture();
    }

    #region GetByTaxNumberAsync Tests

    [Fact]
    public async Task GetByTaxNumberAsync_ShouldReturnCustomer_WhenTaxNumberExists()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTaxNumberAsync(customer.TaxNumber!);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.TaxNumber.Should().Be(customer.TaxNumber);
    }

    [Fact]
    public async Task GetByTaxNumberAsync_ShouldReturnNull_WhenTaxNumberDoesNotExist()
    {
        // Arrange
        const string nonExistentTaxNumber = "NOTFOUND123";

        // Act
        var result = await _repository.GetByTaxNumberAsync(nonExistentTaxNumber);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTaxNumberAsync_ShouldReturnNull_WhenCustomerIsDeleted()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        customer.IsDeleted = true;
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTaxNumberAsync(customer.TaxNumber!);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByRegistrationNumberAsync Tests

    [Fact]
    public async Task GetByRegistrationNumberAsync_ShouldReturnCustomer_WhenRegistrationNumberExists()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRegistrationNumberAsync(customer.RegistrationNumber!);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.RegistrationNumber.Should().Be(customer.RegistrationNumber);
    }

    [Fact]
    public async Task GetByRegistrationNumberAsync_ShouldReturnNull_WhenRegistrationNumberDoesNotExist()
    {
        // Arrange
        const string nonExistentRegNumber = "NOTFOUND123";

        // Act
        var result = await _repository.GetByRegistrationNumberAsync(nonExistentRegNumber);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnCustomer_WhenEmailExists()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(customer.ContactEmail);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.ContactEmail.Should().Be(customer.ContactEmail);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Arrange
        const string nonExistentEmail = "notfound@example.com";

        // Act
        var result = await _repository.GetByEmailAsync(nonExistentEmail);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetWithContactsAsync Tests

    [Fact]
    public async Task GetWithContactsAsync_ShouldReturnCustomerWithContacts_WhenCustomerHasContacts()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var contacts = new List<CustomerContact>
        {
            TestDataFactory.CreateTestCustomerContact(customer.Id),
            TestDataFactory.CreateTestCustomerContact(customer.Id)
        };
        _context.CustomerContacts.AddRange(contacts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithContactsAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Contacts.Should().HaveCount(2);
        result.Contacts.Should().AllSatisfy(c => c.CustomerId.Should().Be(customer.Id));
    }

    [Fact]
    public async Task GetWithContactsAsync_ShouldExcludeDeletedContacts()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString();
        var customer = TestDataFactory.CreateTestCustomer(id: uniqueId, name: $"DeleteTestCustomer_{uniqueId}");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var activeContact = TestDataFactory.CreateTestCustomerContact(customer.Id);
        activeContact.FirstName = $"ActiveContact_{uniqueId}";
        var deletedContact = TestDataFactory.CreateTestCustomerContact(customer.Id);
        deletedContact.FirstName = $"DeletedContact_{uniqueId}";
        deletedContact.IsDeleted = true;

        _context.CustomerContacts.AddRange(activeContact, deletedContact);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithContactsAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Contacts.Should().HaveCount(1, "because only non-deleted contacts should be included");
        result.Contacts.First().IsDeleted.Should().BeFalse();
        result.Contacts.First().FirstName.Should().Be($"ActiveContact_{uniqueId}");
    }

    #endregion

    #region GetWithContractsAsync Tests

    [Fact]
    public async Task GetWithContractsAsync_ShouldReturnCustomerWithContracts_WhenCustomerHasContracts()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var contracts = new List<CustomerContract>
        {
            TestDataFactory.CreateTestCustomerContract(customer.Id),
            TestDataFactory.CreateTestCustomerContract(customer.Id)
        };
        _context.CustomerContracts.AddRange(contracts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithContractsAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Contracts.Should().HaveCount(2);
        result.Contracts.Should().AllSatisfy(c => c.CustomerId.Should().Be(customer.Id));
    }

    #endregion

    #region GetWithBillingAsync Tests

    [Fact]
    public async Task GetWithBillingAsync_ShouldReturnCustomerWithBilling_WhenBillingExists()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var billing = TestDataFactory.CreateTestCustomerBilling(customer.Id);
        _context.CustomerBilling.Add(billing);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithBillingAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Billing.Should().NotBeNull();
        result.Billing!.CustomerId.Should().Be(customer.Id);
    }

    #endregion

    #region GetWithCreditAsync Tests

    [Fact]
    public async Task GetWithCreditAsync_ShouldReturnCustomerWithCredit_WhenCreditExists()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var credit = TestDataFactory.CreateTestCustomerCredit(customer.Id);
        _context.CustomerCredit.Add(credit);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithCreditAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Credit.Should().NotBeNull();
        result.Credit!.CustomerId.Should().Be(customer.Id);
    }

    #endregion

    #region GetWithAllDetailsAsync Tests

    [Fact]
    public async Task GetWithAllDetailsAsync_ShouldReturnCustomerWithAllRelatedData()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Add contacts
        var contacts = new List<CustomerContact>
        {
            TestDataFactory.CreateTestCustomerContact(customer.Id),
            TestDataFactory.CreateTestCustomerContact(customer.Id)
        };
        _context.CustomerContacts.AddRange(contacts);

        // Add contracts
        var contracts = new List<CustomerContract>
        {
            TestDataFactory.CreateTestCustomerContract(customer.Id)
        };
        _context.CustomerContracts.AddRange(contracts);

        // Add billing
        var billing = TestDataFactory.CreateTestCustomerBilling(customer.Id);
        _context.CustomerBilling.Add(billing);

        // Add credit
        var credit = TestDataFactory.CreateTestCustomerCredit(customer.Id);
        _context.CustomerCredit.Add(credit);

        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithAllDetailsAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Contacts.Should().HaveCount(2);
        result.Contracts.Should().HaveCount(1);
        result.Billing.Should().NotBeNull();
        result.Credit.Should().NotBeNull();
    }

    #endregion

    #region GetByStatusAsync Tests

    [Theory]
    [InlineData(CustomerStatus.Active)]
    [InlineData(CustomerStatus.Inactive)]
    [InlineData(CustomerStatus.Suspended)]
    [InlineData(CustomerStatus.Pending)]
    public async Task GetByStatusAsync_ShouldReturnCustomersWithSpecifiedStatus(CustomerStatus status)
    {
        // Arrange
        var customers = new List<Customer>
        {
            TestDataFactory.CreateTestCustomer(status: status),
            TestDataFactory.CreateTestCustomer(status: status),
            TestDataFactory.CreateTestCustomer(status: CustomerStatus.Active) // Different status
        };
        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStatusAsync(status);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCountGreaterOrEqualTo(2);
        resultList.Should().AllSatisfy(c => c.Status.Should().Be(status));
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnEmptyList_WhenNoCustomersWithStatus()
    {
        // Arrange - ensure no customers with Suspended status exist
        var customers = _context.Customers.Where(c => c.Status == CustomerStatus.Suspended).ToList();
        _context.Customers.RemoveRange(customers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStatusAsync(CustomerStatus.Suspended);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByTypeAsync Tests

    [Theory]
    [InlineData(CustomerType.Corporate)]
    [InlineData(CustomerType.Individual)]
    [InlineData(CustomerType.Government)]
    [InlineData(CustomerType.NonProfit)]
    public async Task GetByTypeAsync_ShouldReturnCustomersWithSpecifiedType(CustomerType customerType)
    {
        // Arrange
        var customers = new List<Customer>
        {
            TestDataFactory.CreateTestCustomer(customerType: customerType),
            TestDataFactory.CreateTestCustomer(customerType: customerType),
            TestDataFactory.CreateTestCustomer(customerType: CustomerType.Corporate) // Different type
        };
        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(customerType);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCountGreaterOrEqualTo(2);
        resultList.Should().AllSatisfy(c => c.CustomerType.Should().Be(customerType));
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingCustomers_WhenSearchingByName()
    {
        // Arrange
        const string searchTerm = "SearchTest";
        var matchingCustomer = TestDataFactory.CreateTestCustomer(name: $"{searchTerm} Company");
        var nonMatchingCustomer = TestDataFactory.CreateTestCustomer(name: "Other Company");
        
        _context.Customers.AddRange(matchingCustomer, nonMatchingCustomer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(searchTerm.ToLower());

        // Assert
        var resultList = result.ToList();
        resultList.Should().Contain(c => c.Id == matchingCustomer.Id);
        resultList.Should().NotContain(c => c.Id == nonMatchingCustomer.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingCustomers_WhenSearchingByEmail()
    {
        // Arrange
        const string searchTerm = "searchtest";
        var matchingCustomer = TestDataFactory.CreateTestCustomer(email: $"{searchTerm}@example.com");
        var nonMatchingCustomer = TestDataFactory.CreateTestCustomer(email: "other@example.com");
        
        _context.Customers.AddRange(matchingCustomer, nonMatchingCustomer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(searchTerm);

        // Assert
        var resultList = result.ToList();
        resultList.Should().Contain(c => c.Id == matchingCustomer.Id);
        resultList.Should().NotContain(c => c.Id == nonMatchingCustomer.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingCustomers_WhenSearchingByTaxNumber()
    {
        // Arrange
        const string searchTerm = "TAX123";
        var matchingCustomer = TestDataFactory.CreateTestCustomer();
        matchingCustomer.TaxNumber = $"{searchTerm}456";
        var nonMatchingCustomer = TestDataFactory.CreateTestCustomer();
        
        _context.Customers.AddRange(matchingCustomer, nonMatchingCustomer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(searchTerm.ToLower());

        // Assert
        var resultList = result.ToList();
        resultList.Should().Contain(c => c.Id == matchingCustomer.Id);
        resultList.Should().NotContain(c => c.Id == nonMatchingCustomer.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingCustomers_WhenSearchingByRegistrationNumber()
    {
        // Arrange
        const string searchTerm = "REG123";
        var matchingCustomer = TestDataFactory.CreateTestCustomer();
        matchingCustomer.RegistrationNumber = $"{searchTerm}789";
        var nonMatchingCustomer = TestDataFactory.CreateTestCustomer();
        
        _context.Customers.AddRange(matchingCustomer, nonMatchingCustomer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(searchTerm.ToLower());

        // Assert
        var resultList = result.ToList();
        resultList.Should().Contain(c => c.Id == matchingCustomer.Id);
        resultList.Should().NotContain(c => c.Id == nonMatchingCustomer.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingCustomers_WhenSearchingByPhone()
    {
        // Arrange
        var uniquePhone = $"555-{DateTime.Now:mmss}-{Guid.NewGuid().ToString()[..4]}";
        var matchingCustomer = TestDataFactory.CreateTestCustomer();
        matchingCustomer.ContactPhone = uniquePhone;
        var nonMatchingCustomer = TestDataFactory.CreateTestCustomer();
        
        _context.Customers.AddRange(matchingCustomer, nonMatchingCustomer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(uniquePhone);

        // Assert
        var resultList = result.ToList();
        resultList.Should().Contain(c => c.Id == matchingCustomer.Id);
        resultList.Should().NotContain(c => c.Id == nonMatchingCustomer.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldExcludeDeletedCustomers()
    {
        // Arrange
        const string searchTerm = "DeletedTest";
        var activeCustomer = TestDataFactory.CreateTestCustomer(name: $"{searchTerm} Active");
        var deletedCustomer = TestDataFactory.CreateTestCustomer(name: $"{searchTerm} Deleted");
        deletedCustomer.IsDeleted = true;
        
        _context.Customers.AddRange(activeCustomer, deletedCustomer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(searchTerm.ToLower());

        // Assert
        var resultList = result.ToList();
        resultList.Should().Contain(c => c.Id == activeCustomer.Id);
        resultList.Should().NotContain(c => c.Id == deletedCustomer.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmptyList_WhenNoMatches()
    {
        // Arrange
        const string searchTerm = "NoMatchesExpected12345";

        // Act
        var result = await _repository.SearchAsync(searchTerm);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        const string customerName = "CaseTestCompany";
        var customer = TestDataFactory.CreateTestCustomer(name: customerName);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result1 = await _repository.SearchAsync("casetest");
        var result2 = await _repository.SearchAsync("CASETEST");
        var result3 = await _repository.SearchAsync("CaseTest");

        // Assert
        result1.Should().Contain(c => c.Id == customer.Id);
        result2.Should().Contain(c => c.Id == customer.Id);
        result3.Should().Contain(c => c.Id == customer.Id);
    }

    #endregion

    #region Base Repository Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedCustomers()
    {
        // Arrange
        var activeCustomers = TestDataFactory.CreateTestCustomerList(3);
        var deletedCustomer = TestDataFactory.CreateTestCustomer();
        deletedCustomer.IsDeleted = true;
        
        _context.Customers.AddRange(activeCustomers);
        _context.Customers.Add(deletedCustomer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCountGreaterOrEqualTo(3);
        resultList.Should().NotContain(c => c.Id == deletedCustomer.Id);
        resultList.Should().AllSatisfy(c => c.IsDeleted.Should().BeFalse());
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        const int pageNumber = 1;
        const int pageSize = 2;
        var customers = TestDataFactory.CreateTestCustomerList(5);
        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(pageNumber, pageSize);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(pageSize);
    }

    [Fact]
    public async Task AddAsync_ShouldAddCustomerSuccessfully()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();

        // Act
        var result = await _repository.AddAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        
        var savedCustomer = await _context.Customers.FindAsync(result.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Name.Should().Be(customer.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCustomerSuccessfully()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        const string newName = "Updated Customer Name";
        customer.Name = newName;

        // Act
        var result = await _repository.UpdateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(newName);
        
        var updatedCustomer = await _context.Customers.FindAsync(customer.Id);
        updatedCustomer!.Name.Should().Be(newName);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldMarkCustomerAsDeleted()
    {
        // Arrange
        var customer = TestDataFactory.CreateTestCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteByIdAsync(customer.Id);

        // Assert
        result.Should().BeTrue();
        
        var deletedCustomer = await _context.Customers.FindAsync(customer.Id);
        deletedCustomer!.IsDeleted.Should().BeTrue();
    }

    #endregion
}