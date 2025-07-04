using Microsoft.AspNetCore.Http;
using CustomerService.Tests.Helpers;

namespace CustomerService.Tests.Controllers;

[Trait("Category", "Unit")]
public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly CustomersController _controller;
    private readonly Fixture _fixture;

    public CustomersControllerTests()
    {
        _mockCustomerService = MockFactories.CreateMockCustomerService();
        _controller = new CustomersController(_mockCustomerService.Object);
        _fixture = new Fixture();

        // Setup HttpContext for header access
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Organization-Id"] = "test-org";
        httpContext.Request.Headers["X-User-Id"] = "test-user";
        _controller.ControllerContext.HttpContext = httpContext;
    }

    #region GetCustomers Tests

    [Fact]
    public async Task GetCustomers_ShouldReturnOkWithCustomers_WhenCustomersExist()
    {
        // Arrange
        var testCustomers = TestDataFactory.CreateTestCustomerList(3)
            .Select(c => TestDataFactory.CreateTestCustomerDto(c.Id, c.Name, c.ContactEmail));
        
        _mockCustomerService.Setup(x => x.GetCustomersPagedAsync(1, 20))
            .ReturnsAsync(testCustomers);

        // Act
        var result = await _controller.GetCustomers(1, 20);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ApiResponseDto<IEnumerable<CustomerDto>>>();
        
        var response = okResult.Value as ApiResponseDto<IEnumerable<CustomerDto>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnOkWithSearchResults_WhenSearchTermProvided()
    {
        // Arrange
        const string searchTerm = "test";
        var searchResults = new List<CustomerDto>
        {
            TestDataFactory.CreateTestCustomerDto(name: "Test Customer")
        };

        _mockCustomerService.Setup(x => x.SearchCustomersAsync(searchTerm))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.GetCustomers(search: searchTerm);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<IEnumerable<CustomerDto>>;
        
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
        response.Data.First().Name.Should().Contain("Test");
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnBadRequest_WhenServiceThrowsException()
    {
        // Arrange
        _mockCustomerService.Setup(x => x.GetCustomersPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _controller.GetCustomers();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value as ApiResponseDto<IEnumerable<CustomerDto>>;
        
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("Database error");
    }

    #endregion

    #region RegisterCustomer Tests

    [Fact]
    public async Task RegisterCustomer_ShouldReturnOkWithCustomer_WhenValidRequest()
    {
        // Arrange
        var request = TestDataFactory.CreateRegisterCustomerRequest();
        var expectedCustomer = TestDataFactory.CreateTestCustomerDto(name: request.Name, email: request.ContactEmail);

        _mockCustomerService.Setup(x => x.RegisterCustomerAsync(request))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _controller.RegisterCustomer(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.Name.Should().Be(request.Name);
        response.Data.ContactEmail.Should().Be(request.ContactEmail);
    }

    [Fact]
    public async Task RegisterCustomer_ShouldReturnBadRequest_WhenCustomerAlreadyExists()
    {
        // Arrange
        var request = TestDataFactory.CreateRegisterCustomerRequest();
        
        _mockCustomerService.Setup(x => x.RegisterCustomerAsync(request))
            .ThrowsAsync(new InvalidOperationException("Customer with email already exists"));

        // Act
        var result = await _controller.RegisterCustomer(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value as ApiResponseDto<CustomerDto>;
        
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("already exists");
    }

    #endregion

    #region GetCustomer Tests

    [Fact]
    public async Task GetCustomer_ShouldReturnOkWithCustomer_WhenCustomerExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var expectedCustomer = TestDataFactory.CreateTestCustomerDto(customerId);

        _mockCustomerService.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _controller.GetCustomer(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.Id.Should().Be(customerId);
    }

    [Fact]
    public async Task GetCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        const string customerId = "non-existent-id";
        
        _mockCustomerService.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetCustomer(customerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        var response = notFoundResult!.Value as ApiResponseDto<CustomerDto>;
        
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("not found");
    }

    #endregion

    #region UpdateCustomer Tests

    [Fact]
    public async Task UpdateCustomer_ShouldReturnOkWithUpdatedCustomer_WhenValidRequest()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = TestDataFactory.CreateUpdateCustomerRequest();
        var expectedCustomer = TestDataFactory.CreateTestCustomerDto(customerId, request.Name, request.ContactEmail);

        _mockCustomerService.Setup(x => x.UpdateCustomerAsync(customerId, request))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _controller.UpdateCustomer(customerId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        const string customerId = "non-existent-id";
        var request = TestDataFactory.CreateUpdateCustomerRequest();
        
        _mockCustomerService.Setup(x => x.UpdateCustomerAsync(customerId, request))
            .ThrowsAsync(new ArgumentException("Customer not found"));

        // Act
        var result = await _controller.UpdateCustomer(customerId, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeleteCustomer Tests

    [Fact]
    public async Task DeleteCustomer_ShouldReturnOk_WhenCustomerExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        
        _mockCustomerService.Setup(x => x.DeleteCustomerAsync(customerId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto;
        
        response!.Success.Should().BeTrue();
        response.Message.Should().Contain("deleted successfully");
    }

    [Fact]
    public async Task DeleteCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        const string customerId = "non-existent-id";
        
        _mockCustomerService.Setup(x => x.DeleteCustomerAsync(customerId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region ActivateCustomer Tests

    [Fact]
    public async Task ActivateCustomer_ShouldReturnOk_WhenCustomerExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        
        _mockCustomerService.Setup(x => x.ActivateCustomerAsync(customerId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ActivateCustomer(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto;
        
        response!.Success.Should().BeTrue();
        response.Message.Should().Contain("activated successfully");
    }

    [Fact]
    public async Task ActivateCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        const string customerId = "non-existent-id";
        
        _mockCustomerService.Setup(x => x.ActivateCustomerAsync(customerId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ActivateCustomer(customerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeactivateCustomer Tests

    [Fact]
    public async Task DeactivateCustomer_ShouldReturnOk_WhenCustomerExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        
        _mockCustomerService.Setup(x => x.DeactivateCustomerAsync(customerId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeactivateCustomer(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto;
        
        response!.Success.Should().BeTrue();
        response.Message.Should().Contain("deactivated successfully");
    }

    #endregion

    #region GetCustomerDetails Tests

    [Fact]
    public async Task GetCustomerDetails_ShouldReturnOkWithDetails_WhenCustomerExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var expectedDetails = new CustomerDetailDto
        {
            Id = customerId,
            Name = "Test Customer",
            ContactEmail = "test@example.com",
            Contacts = new List<CustomerContactDto>(),
            Contracts = new List<CustomerContractDto>()
        };

        _mockCustomerService.Setup(x => x.GetCustomerDetailsAsync(customerId))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _controller.GetCustomerDetails(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerDetailDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.Id.Should().Be(customerId);
    }

    #endregion

    #region Contact Management Tests

    [Fact]
    public async Task GetCustomerContacts_ShouldReturnOkWithContacts_WhenCustomerHasContacts()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var expectedContacts = new List<CustomerContactDto>
        {
            new() { Id = "contact-1", CustomerId = customerId, FirstName = "John", LastName = "Doe" }
        };

        _mockCustomerService.Setup(x => x.GetCustomerContactsAsync(customerId))
            .ReturnsAsync(expectedContacts);

        // Act
        var result = await _controller.GetCustomerContacts(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<IEnumerable<CustomerContactDto>>;
        
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddContact_ShouldReturnOkWithContact_WhenValidRequest()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new CreateCustomerContactRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            ContactType = Core.Entities.ContactType.Primary
        };
        
        var expectedContact = new CustomerContactDto
        {
            Id = "new-contact-id",
            CustomerId = customerId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email
        };

        _mockCustomerService.Setup(x => x.AddContactAsync(customerId, request))
            .ReturnsAsync(expectedContact);

        // Act
        var result = await _controller.AddContact(customerId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerContactDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.FirstName.Should().Be(request.FirstName);
    }

    #endregion

    #region Contract Management Tests

    [Fact]
    public async Task GetCustomerContracts_ShouldReturnOkWithContracts_WhenCustomerHasContracts()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var expectedContracts = new List<CustomerContractDto>
        {
            new() { Id = "contract-1", CustomerId = customerId, ContractNumber = "CON-001" }
        };

        _mockCustomerService.Setup(x => x.GetCustomerContractsAsync(customerId))
            .ReturnsAsync(expectedContracts);

        // Act
        var result = await _controller.GetCustomerContracts(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<IEnumerable<CustomerContractDto>>;
        
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateContract_ShouldReturnOkWithContract_WhenValidRequest()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new CreateCustomerContractRequest
        {
            ContractNumber = "CON-002",
            Title = "Service Contract",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            ContractValue = 50000,
            ContractType = Core.Entities.ContractType.Service
        };
        
        var expectedContract = new CustomerContractDto
        {
            Id = "new-contract-id",
            CustomerId = customerId,
            ContractNumber = request.ContractNumber,
            Title = request.Title
        };

        _mockCustomerService.Setup(x => x.CreateContractAsync(customerId, request))
            .ReturnsAsync(expectedContract);

        // Act
        var result = await _controller.CreateContract(customerId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerContractDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.ContractNumber.Should().Be(request.ContractNumber);
    }

    #endregion

    #region Billing Tests

    [Fact]
    public async Task GetCustomerBilling_ShouldReturnOkWithBilling_WhenBillingExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var expectedBilling = new CustomerBillingDto
        {
            Id = "billing-1",
            CustomerId = customerId,
            BillingContactName = "Billing Contact",
            BillingEmail = "billing@example.com"
        };

        _mockCustomerService.Setup(x => x.GetCustomerBillingAsync(customerId))
            .ReturnsAsync(expectedBilling);

        // Act
        var result = await _controller.GetCustomerBilling(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerBillingDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task UpdateBilling_ShouldReturnOkWithUpdatedBilling_WhenValidRequest()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new UpdateCustomerBillingRequest
        {
            BillingContactName = "Updated Billing Contact",
            BillingEmail = "updated.billing@example.com",
            BillingAddress = "123 Billing St"
        };
        
        var expectedBilling = new CustomerBillingDto
        {
            Id = "billing-1",
            CustomerId = customerId,
            BillingContactName = request.BillingContactName,
            BillingEmail = request.BillingEmail
        };

        _mockCustomerService.Setup(x => x.UpdateBillingAsync(customerId, request))
            .ReturnsAsync(expectedBilling);

        // Act
        var result = await _controller.UpdateBilling(customerId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerBillingDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.BillingContactName.Should().Be(request.BillingContactName);
    }

    #endregion

    #region Credit Tests

    [Fact]
    public async Task GetCustomerCredit_ShouldReturnOkWithCredit_WhenCreditExists()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var expectedCredit = new CustomerCreditDto
        {
            Id = "credit-1",
            CustomerId = customerId,
            CreditLimit = 10000,
            AvailableCredit = 7500,
            UsedCredit = 2500
        };

        _mockCustomerService.Setup(x => x.GetCustomerCreditAsync(customerId))
            .ReturnsAsync(expectedCredit);

        // Act
        var result = await _controller.GetCustomerCredit(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerCreditDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task UpdateCredit_ShouldReturnOkWithUpdatedCredit_WhenValidRequest()
    {
        // Arrange
        const string customerId = "test-customer-id";
        var request = new UpdateCustomerCreditRequest
        {
            CreditLimit = 15000,
            CreditStatus = Core.Entities.CreditStatus.Good,
            CreditScore = 750
        };
        
        var expectedCredit = new CustomerCreditDto
        {
            Id = "credit-1",
            CustomerId = customerId,
            CreditLimit = request.CreditLimit,
            CreditStatus = request.CreditStatus,
            CreditScore = request.CreditScore
        };

        _mockCustomerService.Setup(x => x.UpdateCreditAsync(customerId, request))
            .ReturnsAsync(expectedCredit);

        // Act
        var result = await _controller.UpdateCredit(customerId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponseDto<CustomerCreditDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.CreditLimit.Should().Be(request.CreditLimit);
    }

    #endregion
}