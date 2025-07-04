using AutoMapper;
using CustomerService.Tests.Helpers;
using CustomerService.Core.Services;

namespace CustomerService.Tests.Services;

[Trait("Category", "Unit")]
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<IRepository<CustomerContact>> _mockContactRepository;
    private readonly Mock<IRepository<CustomerContract>> _mockContractRepository;
    private readonly Mock<IRepository<CustomerBilling>> _mockBillingRepository;
    private readonly Mock<IRepository<CustomerCredit>> _mockCreditRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CustomerService.Core.Services.CustomerService _customerService;
    private readonly Fixture _fixture;

    public CustomerServiceTests()
    {
        _mockCustomerRepository = MockFactories.CreateMockCustomerRepository();
        _mockContactRepository = MockFactories.CreateMockRepository<CustomerContact>();
        _mockContractRepository = MockFactories.CreateMockRepository<CustomerContract>();
        _mockBillingRepository = MockFactories.CreateMockRepository<CustomerBilling>();
        _mockCreditRepository = MockFactories.CreateMockRepository<CustomerCredit>();
        _mockMapper = MockFactories.CreateMockMapper();
        
        _customerService = new CustomerService.Core.Services.CustomerService(
            _mockCustomerRepository.Object,
            _mockContactRepository.Object,
            _mockContractRepository.Object,
            _mockBillingRepository.Object,
            _mockCreditRepository.Object,
            _mockMapper.Object);

        _fixture = new Fixture();
    }

    #region RegisterCustomerAsync Tests

    [Fact]
    public async Task RegisterCustomerAsync_ShouldReturnCustomerDto_WhenValidRequest()
    {
        // Arrange
        var request = TestDataFactory.CreateRegisterCustomerRequest();
        var customer = TestDataFactory.CreateTestCustomer(name: request.Name, email: request.ContactEmail);
        var expectedDto = TestDataFactory.CreateTestCustomerDto(customer.Id, customer.Name, customer.ContactEmail);

        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(request.ContactEmail))
            .ReturnsAsync((Customer?)null);
        _mockCustomerRepository.Setup(x => x.GetByTaxNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((Customer?)null);
        _mockCustomerRepository.Setup(x => x.GetByRegistrationNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((Customer?)null);
        _mockCustomerRepository.Setup(x => x.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        
        var customerCredit = TestDataFactory.CreateTestCustomerCredit(customer.Id);
        _mockCreditRepository.Setup(x => x.AddAsync(It.IsAny<CustomerCredit>()))
            .ReturnsAsync(customerCredit);

        _mockMapper.Setup(x => x.Map<Customer>(request))
            .Returns(customer);
        _mockMapper.Setup(x => x.Map<CustomerDto>(customer))
            .Returns(expectedDto);

        // Act
        var result = await _customerService.RegisterCustomerAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.ContactEmail.Should().Be(request.ContactEmail);
        
        _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Once);
        _mockCreditRepository.Verify(x => x.AddAsync(It.IsAny<CustomerCredit>()), Times.Once);
    }

    [Fact]
    public async Task RegisterCustomerAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = TestDataFactory.CreateRegisterCustomerRequest();
        var existingCustomer = TestDataFactory.CreateTestCustomer(email: request.ContactEmail);

        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(request.ContactEmail))
            .ReturnsAsync(existingCustomer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _customerService.RegisterCustomerAsync(request));
        
        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task RegisterCustomerAsync_ShouldThrowException_WhenTaxNumberAlreadyExists()
    {
        // Arrange
        var request = TestDataFactory.CreateRegisterCustomerRequest();
        var existingCustomer = TestDataFactory.CreateTestCustomer();

        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(request.ContactEmail))
            .ReturnsAsync((Customer?)null);
        _mockCustomerRepository.Setup(x => x.GetByTaxNumberAsync(request.TaxNumber!))
            .ReturnsAsync(existingCustomer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _customerService.RegisterCustomerAsync(request));
        
        exception.Message.Should().Contain("tax number");
        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task RegisterCustomerAsync_ShouldThrowException_WhenRegistrationNumberAlreadyExists()
    {
        // Arrange
        var request = TestDataFactory.CreateRegisterCustomerRequest();
        var existingCustomer = TestDataFactory.CreateTestCustomer();

        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(request.ContactEmail))
            .ReturnsAsync((Customer?)null);
        _mockCustomerRepository.Setup(x => x.GetByTaxNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((Customer?)null);
        _mockCustomerRepository.Setup(x => x.GetByRegistrationNumberAsync(request.RegistrationNumber!))
            .ReturnsAsync(existingCustomer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _customerService.RegisterCustomerAsync(request));
        
        exception.Message.Should().Contain("registration number");
        exception.Message.Should().Contain("already exists");
    }

    [Theory]
    [InlineData(5000, false)]
    [InlineData(10000, false)]
    [InlineData(15000, true)]
    [InlineData(50000, true)]
    public async Task RegisterCustomerAsync_ShouldSetCorrectApprovalRequirement_BasedOnCreditLimit(
        decimal creditLimit, bool expectedRequiresApproval)
    {
        // Arrange
        var request = TestDataFactory.CreateRegisterCustomerRequest();
        request.CreditLimit = creditLimit;
        
        var customer = TestDataFactory.CreateTestCustomer(creditLimit: creditLimit);
        var expectedDto = TestDataFactory.CreateTestCustomerDto();

        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Customer?)null);
        _mockCustomerRepository.Setup(x => x.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        
        _mockMapper.Setup(x => x.Map<Customer>(It.IsAny<RegisterCustomerRequest>()))
            .Returns(customer);
        _mockMapper.Setup(x => x.Map<CustomerDto>(It.IsAny<Customer>()))
            .Returns(expectedDto);

        CustomerCredit? capturedCredit = null;
        _mockCreditRepository.Setup(x => x.AddAsync(It.IsAny<CustomerCredit>()))
            .Callback<CustomerCredit>(credit => capturedCredit = credit)
            .ReturnsAsync((CustomerCredit credit) => credit);

        // Act
        await _customerService.RegisterCustomerAsync(request);

        // Assert
        capturedCredit.Should().NotBeNull();
        capturedCredit!.RequiresApproval.Should().Be(expectedRequiresApproval);
        capturedCredit.CreditLimit.Should().Be(creditLimit);
        capturedCredit.AvailableCredit.Should().Be(creditLimit);
        capturedCredit.UsedCredit.Should().Be(0);
    }

    #endregion

    #region GetCustomerAsync Tests

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnCustomerDto_WhenCustomerExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var customer = TestDataFactory.CreateTestCustomer(customerId);
        var expectedDto = TestDataFactory.CreateTestCustomerDto(customerId);

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockMapper.Setup(x => x.Map<CustomerDto>(customer))
            .Returns(expectedDto);

        // Act
        var result = await _customerService.GetCustomerAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        const string customerId = "non-existent-id";
        
        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.GetCustomerAsync(customerId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdateCustomerAsync Tests

    [Fact]
    public async Task UpdateCustomerAsync_ShouldReturnUpdatedCustomerDto_WhenValidRequest()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = TestDataFactory.CreateUpdateCustomerRequest();
        var existingCustomer = TestDataFactory.CreateTestCustomer(customerId);
        var expectedDto = TestDataFactory.CreateTestCustomerDto(customerId, request.Name, request.ContactEmail);

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(request.ContactEmail))
            .ReturnsAsync((Customer?)null);
        _mockCustomerRepository.Setup(x => x.UpdateAsync(existingCustomer))
            .ReturnsAsync(existingCustomer);
        
        _mockMapper.Setup(x => x.Map(request, existingCustomer));
        _mockMapper.Setup(x => x.Map<CustomerDto>(existingCustomer))
            .Returns(expectedDto);

        // Act
        var result = await _customerService.UpdateCustomerAsync(customerId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.ContactEmail.Should().Be(request.ContactEmail);
        
        _mockCustomerRepository.Verify(x => x.UpdateAsync(existingCustomer), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomerAsync_ShouldThrowException_WhenCustomerNotFound()
    {
        // Arrange
        const string customerId = "non-existent-id";
        var request = TestDataFactory.CreateUpdateCustomerRequest();

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _customerService.UpdateCustomerAsync(customerId, request));
        
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateCustomerAsync_ShouldThrowException_WhenEmailAlreadyExistsForDifferentCustomer()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = TestDataFactory.CreateUpdateCustomerRequest();
        var existingCustomer = TestDataFactory.CreateTestCustomer(customerId);
        var customerWithSameEmail = TestDataFactory.CreateTestCustomer("different-id", email: request.ContactEmail);

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(request.ContactEmail))
            .ReturnsAsync(customerWithSameEmail);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _customerService.UpdateCustomerAsync(customerId, request));
        
        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateCustomerAsync_ShouldAllowSameEmail_WhenUpdatingSameCustomer()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = TestDataFactory.CreateUpdateCustomerRequest();
        var existingCustomer = TestDataFactory.CreateTestCustomer(customerId, email: request.ContactEmail);
        var expectedDto = TestDataFactory.CreateTestCustomerDto(customerId);

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(request.ContactEmail))
            .ReturnsAsync(existingCustomer); // Same customer
        _mockCustomerRepository.Setup(x => x.UpdateAsync(existingCustomer))
            .ReturnsAsync(existingCustomer);
        
        _mockMapper.Setup(x => x.Map<CustomerDto>(existingCustomer))
            .Returns(expectedDto);

        // Act
        var result = await _customerService.UpdateCustomerAsync(customerId, request);

        // Assert
        result.Should().NotBeNull();
        _mockCustomerRepository.Verify(x => x.UpdateAsync(existingCustomer), Times.Once);
    }

    #endregion

    #region DeleteCustomerAsync Tests

    [Fact]
    public async Task DeleteCustomerAsync_ShouldReturnTrue_WhenCustomerExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        
        _mockCustomerRepository.Setup(x => x.DeleteByIdAsync(customerId))
            .ReturnsAsync(true);

        // Act
        var result = await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCustomerAsync_ShouldReturnFalse_WhenCustomerDoesNotExist()
    {
        // Arrange
        const string customerId = "non-existent-id";
        
        _mockCustomerRepository.Setup(x => x.DeleteByIdAsync(customerId))
            .ReturnsAsync(false);

        // Act
        var result = await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ActivateCustomerAsync Tests

    [Fact]
    public async Task ActivateCustomerAsync_ShouldReturnTrue_WhenCustomerExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var customer = TestDataFactory.CreateTestCustomer(customerId, status: CustomerStatus.Inactive);

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockCustomerRepository.Setup(x => x.UpdateAsync(customer))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.ActivateCustomerAsync(customerId);

        // Assert
        result.Should().BeTrue();
        customer.Status.Should().Be(CustomerStatus.Active);
    }

    [Fact]
    public async Task ActivateCustomerAsync_ShouldReturnFalse_WhenCustomerDoesNotExist()
    {
        // Arrange
        const string customerId = "non-existent-id";
        
        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.ActivateCustomerAsync(customerId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DeactivateCustomerAsync Tests

    [Fact]
    public async Task DeactivateCustomerAsync_ShouldReturnTrue_WhenCustomerExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var customer = TestDataFactory.CreateTestCustomer(customerId, status: CustomerStatus.Active);

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockCustomerRepository.Setup(x => x.UpdateAsync(customer))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.DeactivateCustomerAsync(customerId);

        // Assert
        result.Should().BeTrue();
        customer.Status.Should().Be(CustomerStatus.Inactive);
    }

    #endregion

    #region Contact Management Tests

    [Fact]
    public async Task AddContactAsync_ShouldReturnContactDto_WhenValidRequest()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new CreateCustomerContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            ContactType = Core.Entities.ContactType.Primary
        };
        
        var customer = TestDataFactory.CreateTestCustomer(customerId);
        var contact = TestDataFactory.CreateTestCustomerContact(customerId);
        var expectedDto = new CustomerContactDto
        {
            Id = contact.Id,
            CustomerId = customerId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email
        };

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockContactRepository.Setup(x => x.AddAsync(It.IsAny<CustomerContact>()))
            .ReturnsAsync(contact);
        
        _mockMapper.Setup(x => x.Map<CustomerContact>(request))
            .Returns(contact);
        _mockMapper.Setup(x => x.Map<CustomerContactDto>(contact))
            .Returns(expectedDto);

        // Act
        var result = await _customerService.AddContactAsync(customerId, request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task AddContactAsync_ShouldThrowException_WhenCustomerNotFound()
    {
        // Arrange
        const string customerId = "non-existent-id";
        var request = new CreateCustomerContactRequest
        {
            FirstName = "John",
            LastName = "Doe"
        };

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _customerService.AddContactAsync(customerId, request));
        
        exception.Message.Should().Contain("not found");
    }

    #endregion

    #region Contract Management Tests

    [Fact]
    public async Task CreateContractAsync_ShouldReturnContractDto_WhenValidRequest()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new CreateCustomerContractRequest
        {
            ContractNumber = "CON-001",
            Title = "Service Contract",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            ContractValue = 50000,
            ContractType = Core.Entities.ContractType.Service
        };
        
        var customer = TestDataFactory.CreateTestCustomer(customerId);
        var contract = TestDataFactory.CreateTestCustomerContract(customerId);
        var expectedDto = new CustomerContractDto
        {
            Id = contract.Id,
            CustomerId = customerId,
            ContractNumber = request.ContractNumber,
            Title = request.Title
        };

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockContractRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CustomerContract, bool>>>()))
            .ReturnsAsync((CustomerContract?)null);
        _mockContractRepository.Setup(x => x.AddAsync(It.IsAny<CustomerContract>()))
            .ReturnsAsync(contract);
        
        _mockMapper.Setup(x => x.Map<CustomerContract>(request))
            .Returns(contract);
        _mockMapper.Setup(x => x.Map<CustomerContractDto>(contract))
            .Returns(expectedDto);

        // Act
        var result = await _customerService.CreateContractAsync(customerId, request);

        // Assert
        result.Should().NotBeNull();
        result.ContractNumber.Should().Be(request.ContractNumber);
        result.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task CreateContractAsync_ShouldThrowException_WhenContractNumberAlreadyExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new CreateCustomerContractRequest
        {
            ContractNumber = "CON-001",
            Title = "Service Contract"
        };
        
        var customer = TestDataFactory.CreateTestCustomer(customerId);
        var existingContract = TestDataFactory.CreateTestCustomerContract();

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockContractRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CustomerContract, bool>>>()))
            .ReturnsAsync(existingContract);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _customerService.CreateContractAsync(customerId, request));
        
        exception.Message.Should().Contain("already exists");
    }

    #endregion

    #region Search and Paging Tests

    [Fact]
    public async Task SearchCustomersAsync_ShouldReturnMatchingCustomers_WhenSearchTermProvided()
    {
        // Arrange
        const string searchTerm = "test";
        var customers = TestDataFactory.CreateTestCustomerList(3);
        var expectedDtos = customers.Select(c => TestDataFactory.CreateTestCustomerDto(c.Id, c.Name, c.ContactEmail));

        _mockCustomerRepository.Setup(x => x.SearchAsync(searchTerm))
            .ReturnsAsync(customers);
        _mockMapper.Setup(x => x.Map<IEnumerable<CustomerDto>>(customers))
            .Returns(expectedDtos);

        // Act
        var result = await _customerService.SearchCustomersAsync(searchTerm);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetCustomersPagedAsync_ShouldReturnPagedResults_WhenValidPageAndSize()
    {
        // Arrange
        const int page = 2;
        const int pageSize = 5;
        var customers = TestDataFactory.CreateTestCustomerList(5);
        var expectedDtos = customers.Select(c => TestDataFactory.CreateTestCustomerDto(c.Id, c.Name, c.ContactEmail));

        _mockCustomerRepository.Setup(x => x.GetPagedAsync(page, pageSize, null))
            .ReturnsAsync(customers);
        _mockMapper.Setup(x => x.Map<IEnumerable<CustomerDto>>(customers))
            .Returns(expectedDtos);

        // Act
        var result = await _customerService.GetCustomersPagedAsync(page, pageSize);

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region Billing and Credit Tests

    [Fact]
    public async Task UpdateBillingAsync_ShouldCreateNewBilling_WhenBillingDoesNotExist()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new UpdateCustomerBillingRequest
        {
            BillingContactName = "Billing Contact",
            BillingEmail = "billing@example.com",
            BillingAddress = "123 Billing St"
        };
        
        var customer = TestDataFactory.CreateTestCustomer(customerId);
        var billing = TestDataFactory.CreateTestCustomerBilling(customerId);
        var expectedDto = new CustomerBillingDto
        {
            Id = billing.Id,
            CustomerId = customerId,
            BillingContactName = request.BillingContactName
        };

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockBillingRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CustomerBilling, bool>>>()))
            .ReturnsAsync((CustomerBilling?)null);
        _mockBillingRepository.Setup(x => x.AddAsync(It.IsAny<CustomerBilling>()))
            .ReturnsAsync(billing);
        
        _mockMapper.Setup(x => x.Map<CustomerBillingDto>(billing))
            .Returns(expectedDto);

        // Act
        var result = await _customerService.UpdateBillingAsync(customerId, request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        _mockBillingRepository.Verify(x => x.AddAsync(It.IsAny<CustomerBilling>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCreditAsync_ShouldUpdateExistingCredit_WhenCreditExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new UpdateCustomerCreditRequest
        {
            CreditLimit = 15000,
            CreditStatus = Core.Entities.CreditStatus.Good,
            CreditScore = 750
        };
        
        var customer = TestDataFactory.CreateTestCustomer(customerId);
        var existingCredit = TestDataFactory.CreateTestCustomerCredit(customerId, usedCredit: 3000);
        var expectedDto = new CustomerCreditDto
        {
            Id = existingCredit.Id,
            CustomerId = customerId,
            CreditLimit = request.CreditLimit
        };

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockCreditRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CustomerCredit, bool>>>()))
            .ReturnsAsync(existingCredit);
        _mockCreditRepository.Setup(x => x.UpdateAsync(existingCredit))
            .ReturnsAsync(existingCredit);
        
        _mockMapper.Setup(x => x.Map<CustomerCreditDto>(existingCredit))
            .Returns(expectedDto);

        // Act
        var result = await _customerService.UpdateCreditAsync(customerId, request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        
        // Verify available credit was calculated correctly
        existingCredit.AvailableCredit.Should().Be(existingCredit.CreditLimit - existingCredit.UsedCredit);
        existingCredit.LastCreditCheck.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateCreditAsync_ShouldThrowException_WhenCreditRecordNotFound()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new UpdateCustomerCreditRequest();
        var customer = TestDataFactory.CreateTestCustomer(customerId);

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockCreditRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CustomerCredit, bool>>>()))
            .ReturnsAsync((CustomerCredit?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _customerService.UpdateCreditAsync(customerId, request));
        
        exception.Message.Should().Contain("Credit record");
        exception.Message.Should().Contain("not found");
    }

    #endregion
}