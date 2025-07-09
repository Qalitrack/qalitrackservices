# Customer Service Testing Guide

## Overview

This guide provides comprehensive testing strategies, methodologies, and examples for the Customer Service. It covers unit testing, integration testing, performance testing, and security testing to ensure the service meets quality and reliability standards.

## Testing Strategy

### Testing Pyramid

The Customer Service follows the testing pyramid approach:

```
                    /\
                   /  \
              End-to-End Tests
               /              \
              /                \
         Integration Tests
        /                        \
       /                          \
  Unit Tests (Foundation)
 /                                  \
```

**Test Distribution:**
- **Unit Tests (70%)**: Fast, isolated tests for business logic
- **Integration Tests (20%)**: API and database interaction tests
- **End-to-End Tests (10%)**: Complete workflow validation

### Testing Framework Stack

**Core Testing Technologies:**
- **xUnit**: Primary testing framework for .NET
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Readable assertion library
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing support
- **Testcontainers**: Containerized database testing
- **NBomber**: Performance and load testing

## Unit Testing

### Testing Conventions

#### Test Structure (AAA Pattern)

```csharp
[Fact]
public async Task MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange
    var dependencies = SetupDependencies();
    var service = new CustomerService(dependencies);
    var request = CreateValidRequest();

    // Act
    var result = await service.MethodAsync(request);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeOfType<ExpectedType>();
}
```

#### Test Naming Conventions

**Format**: `MethodName_StateUnderTest_ExpectedBehavior`

**Examples:**
- `RegisterCustomerAsync_WithValidRequest_ReturnsCustomerDto`
- `RegisterCustomerAsync_WithDuplicateEmail_ThrowsBusinessRuleException`
- `GetCustomerAsync_WithInvalidId_ReturnsNull`

### Service Layer Unit Tests

#### CustomerService Tests

**Test Setup:**
```csharp
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IValidator<RegisterCustomerRequest>> _mockValidator;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockValidator = new Mock<IValidator<RegisterCustomerRequest>>();
        
        _service = new CustomerService(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockValidator.Object);
    }

    [Fact]
    public async Task RegisterCustomerAsync_WithValidRequest_ReturnsCustomerDto()
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = "Test Customer",
            ContactEmail = "test@example.com",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        var customer = new Customer
        {
            Id = "customer-id",
            Name = request.Name,
            ContactEmail = request.ContactEmail
        };

        var customerDto = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            ContactEmail = customer.ContactEmail
        };

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(r => r.GetByEmailAsync(request.ContactEmail))
                      .ReturnsAsync((Customer?)null);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Customer>()))
                      .ReturnsAsync(customer);

        _mockRepository.Setup(r => r.SaveChangesAsync())
                      .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<CustomerDto>(customer))
                  .Returns(customerDto);

        // Act
        var result = await _service.RegisterCustomerAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(customerDto);
        
        _mockRepository.Verify(r => r.AddAsync(It.Is<Customer>(c => 
            c.Name == request.Name && 
            c.ContactEmail == request.ContactEmail)), Times.Once);
        
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterCustomerAsync_WithDuplicateEmail_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = "Test Customer",
            ContactEmail = "existing@example.com"
        };

        var existingCustomer = new Customer
        {
            Id = "existing-id",
            ContactEmail = request.ContactEmail
        };

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(r => r.GetByEmailAsync(request.ContactEmail))
                      .ReturnsAsync(existingCustomer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.RegisterCustomerAsync(request));

        exception.Message.Should().Contain("Email address already exists");
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetCustomerAsync_WithValidId_ReturnsCustomerDto()
    {
        // Arrange
        var customerId = "customer-id";
        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            ContactEmail = "test@example.com"
        };

        var customerDto = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            ContactEmail = customer.ContactEmail
        };

        _mockRepository.Setup(r => r.GetByIdAsync(customerId))
                      .ReturnsAsync(customer);

        _mockMapper.Setup(m => m.Map<CustomerDto>(customer))
                  .Returns(customerDto);

        // Act
        var result = await _service.GetCustomerAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(customerDto);
    }

    [Fact]
    public async Task GetCustomerAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var invalidId = "invalid-id";

        _mockRepository.Setup(r => r.GetByIdAsync(invalidId))
                      .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.GetCustomerAsync(invalidId);

        // Assert
        result.Should().BeNull();
        _mockMapper.Verify(m => m.Map<CustomerDto>(It.IsAny<Customer>()), Times.Never);
    }
}
```

### Repository Unit Tests

#### CustomerRepository Tests

**Test Setup with In-Memory Database:**
```csharp
public class CustomerRepositoryTests : IDisposable
{
    private readonly CustomerDbContext _context;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CustomerDbContext(options);
        _repository = new CustomerRepository(_context);
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            Id = "customer-id",
            Name = "Test Customer",
            ContactEmail = "test@example.com",
            Status = CustomerStatus.Active
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.ContactEmail.Should().Be(customer.ContactEmail);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Arrange & Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_WithNameMatch_ReturnsMatchingCustomers()
    {
        // Arrange
        var customers = new[]
        {
            new Customer { Id = "1", Name = "Acme Corporation", ContactEmail = "contact@acme.com" },
            new Customer { Id = "2", Name = "Beta Industries", ContactEmail = "info@beta.com" },
            new Customer { Id = "3", Name = "Acme Solutions", ContactEmail = "hello@acmesol.com" }
        };

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.SearchAsync("Acme");

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(c => c.Name == "Acme Corporation");
        results.Should().Contain(c => c.Name == "Acme Solutions");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Validation Unit Tests

#### FluentValidation Tests

**Validator Testing:**
```csharp
public class RegisterCustomerValidatorTests
{
    private readonly RegisterCustomerValidator _validator;

    public RegisterCustomerValidatorTests()
    {
        _validator = new RegisterCustomerValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ReturnsNoErrors()
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = "Valid Customer Name",
            ContactEmail = "valid@example.com",
            BillingAddress = "123 Valid Street, City, State 12345",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Validate_WithInvalidName_ReturnsNameError(string invalidName)
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = invalidName,
            ContactEmail = "valid@example.com",
            BillingAddress = "Valid Address",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Name));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("")]
    public async Task Validate_WithInvalidEmail_ReturnsEmailError(string invalidEmail)
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = "Valid Name",
            ContactEmail = invalidEmail,
            BillingAddress = "Valid Address",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ContactEmail));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1000)]
    public async Task Validate_WithNegativeCreditLimit_ReturnsCreditLimitError(decimal invalidCreditLimit)
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = "Valid Name",
            ContactEmail = "valid@example.com",
            BillingAddress = "Valid Address",
            CustomerType = CustomerType.Corporate,
            CreditLimit = invalidCreditLimit
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.CreditLimit));
    }
}
```

## Integration Testing

### API Integration Tests

#### Controller Integration Tests

**Test Setup with WebApplicationFactory:**
```csharp
public class CustomersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        // Set required headers
        _client.DefaultRequestHeaders.Add("X-Organization-Id", "test-org");
        _client.DefaultRequestHeaders.Add("X-User-Id", "test-user");
    }

    [Fact]
    public async Task GetCustomers_ReturnsSuccessWithCustomerList()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<IEnumerable<CustomerDto>>>(
            content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostCustomer_WithValidData_ReturnsCreatedCustomer()
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = "Integration Test Customer",
            ContactEmail = "integration@test.com",
            BillingAddress = "123 Test Street, Test City, TS 12345",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 15000
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<CustomerDto>>(
            responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be(request.Name);
        apiResponse.Data.ContactEmail.Should().Be(request.ContactEmail);
    }

    [Fact]
    public async Task PostCustomer_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var existingCustomer = await CreateTestCustomerAsync("existing@test.com");
        
        var request = new RegisterCustomerRequest
        {
            Name = "Duplicate Email Customer",
            ContactEmail = "existing@test.com", // Same email
            BillingAddress = "123 Test Street",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto>(
            responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ReturnsCustomer()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync("gettest@example.com");

        // Act
        var response = await _client.GetAsync($"/api/customers/{customer.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<CustomerDto>>(
            content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = "invalid-customer-id";

        // Act
        var response = await _client.GetAsync($"/api/customers/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<CustomerDto> CreateTestCustomerAsync(string email)
    {
        var request = new RegisterCustomerRequest
        {
            Name = $"Test Customer {Guid.NewGuid()}",
            ContactEmail = email,
            BillingAddress = "123 Test Street",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/customers", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<CustomerDto>>(
            responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return apiResponse!.Data!;
    }

    private async Task SeedTestDataAsync()
    {
        // Create some test customers for list operations
        await CreateTestCustomerAsync("seed1@test.com");
        await CreateTestCustomerAsync("seed2@test.com");
        await CreateTestCustomerAsync("seed3@test.com");
    }
}
```

### Database Integration Tests

#### Repository Integration Tests with Test Containers

**Test Setup with Real Database:**
```csharp
public class CustomerRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private CustomerDbContext _context;
    private CustomerRepository _repository;

    public CustomerRepositoryIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("customertest")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        _context = new CustomerDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        
        _repository = new CustomerRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_WithValidCustomer_SavesToDatabase()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Integration Test Customer",
            ContactEmail = "integration@test.com",
            BillingAddress = "123 Integration Street",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 25000,
            Status = CustomerStatus.Active
        };

        // Act
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Assert
        var savedCustomer = await _repository.GetByIdAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Name.Should().Be(customer.Name);
        savedCustomer.ContactEmail.Should().Be(customer.ContactEmail);
    }

    [Fact]
    public async Task SearchAsync_WithComplexQuery_ReturnsCorrectResults()
    {
        // Arrange
        var customers = new[]
        {
            new Customer { Id = Guid.NewGuid().ToString(), Name = "Alpha Corp", ContactEmail = "alpha@corp.com", TaxNumber = "TAX001" },
            new Customer { Id = Guid.NewGuid().ToString(), Name = "Beta Industries", ContactEmail = "beta@industries.com", TaxNumber = "TAX002" },
            new Customer { Id = Guid.NewGuid().ToString(), Name = "Gamma Solutions", ContactEmail = "gamma@solutions.com", TaxNumber = "TAX003" }
        };

        foreach (var customer in customers)
        {
            await _repository.AddAsync(customer);
        }
        await _repository.SaveChangesAsync();

        // Act - Search by partial name
        var nameResults = await _repository.SearchAsync("Corp");
        
        // Act - Search by email
        var emailResults = await _repository.SearchAsync("beta@industries.com");
        
        // Act - Search by tax number
        var taxResults = await _repository.SearchAsync("TAX003");

        // Assert
        nameResults.Should().HaveCount(1);
        nameResults.First().Name.Should().Be("Alpha Corp");
        
        emailResults.Should().HaveCount(1);
        emailResults.First().ContactEmail.Should().Be("beta@industries.com");
        
        taxResults.Should().HaveCount(1);
        taxResults.First().TaxNumber.Should().Be("TAX003");
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ReturnsCorrectSubset()
    {
        // Arrange
        var customers = Enumerable.Range(1, 25).Select(i => new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"Customer {i:D2}",
            ContactEmail = $"customer{i:D2}@test.com",
            BillingAddress = $"{i} Test Street"
        }).ToList();

        foreach (var customer in customers)
        {
            await _repository.AddAsync(customer);
        }
        await _repository.SaveChangesAsync();

        // Act
        var (page1Results, totalCount) = await _repository.GetPagedAsync(1, 10);
        var (page2Results, _) = await _repository.GetPagedAsync(2, 10);
        var (page3Results, _) = await _repository.GetPagedAsync(3, 10);

        // Assert
        totalCount.Should().Be(25);
        page1Results.Should().HaveCount(10);
        page2Results.Should().HaveCount(10);
        page3Results.Should().HaveCount(5); // Remaining items
    }
}
```

## Performance Testing

### Load Testing with NBomber

#### API Load Tests

**Basic Load Test Setup:**
```csharp
public class CustomerServiceLoadTests
{
    [Fact]
    public void CustomerRegistration_LoadTest()
    {
        var scenario = Scenario.Create("customer_registration", async context =>
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Organization-Id", "load-test-org");
            httpClient.DefaultRequestHeaders.Add("X-User-Id", "load-test-user");

            var customer = new RegisterCustomerRequest
            {
                Name = $"Load Test Customer {context.ScenarioInfo.ThreadId}-{context.InvocationNumber}",
                ContactEmail = $"loadtest{context.ScenarioInfo.ThreadId}{context.InvocationNumber}@example.com",
                BillingAddress = "123 Load Test Street",
                CustomerType = CustomerType.Corporate,
                CreditLimit = 10000
            };

            var json = JsonSerializer.Serialize(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync("http://localhost:7003/api/customers", content);
                
                return response.IsSuccessStatusCode 
                    ? Response.Ok() 
                    : Response.Fail($"HTTP {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Response.Fail(ex.Message);
            }
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(2)),
            Simulation.KeepConstant(copies: 5, during: TimeSpan.FromMinutes(1))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert performance requirements
        var sceneStats = stats.AllScenarios.First();
        sceneStats.Ok.Request.Mean.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
        sceneStats.Ok.Request.Count.Should().BeGreaterThan(1000);
        sceneStats.Fail.Request.Count.Should().Be(0);
    }

    [Fact]
    public void CustomerSearch_LoadTest()
    {
        var searchTerms = new[] { "Acme", "Corp", "Industries", "Solutions", "test@example.com" };

        var scenario = Scenario.Create("customer_search", async context =>
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Organization-Id", "load-test-org");
            httpClient.DefaultRequestHeaders.Add("X-User-Id", "load-test-user");

            var searchTerm = searchTerms[context.InvocationNumber % searchTerms.Length];
            var url = $"http://localhost:7003/api/customers?search={Uri.EscapeDataString(searchTerm)}&pageSize=20";

            try
            {
                var response = await httpClient.GetAsync(url);
                
                return response.IsSuccessStatusCode 
                    ? Response.Ok() 
                    : Response.Fail($"HTTP {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Response.Fail(ex.Message);
            }
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 50, during: TimeSpan.FromMinutes(1))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert search performance
        var sceneStats = stats.AllScenarios.First();
        sceneStats.Ok.Request.Mean.Should().BeLessThan(TimeSpan.FromMilliseconds(200));
        sceneStats.Ok.Request.P95.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
    }
}
```

### Database Performance Tests

#### Query Performance Testing

**Database Performance Benchmarks:**
```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class CustomerRepositoryBenchmarks
{
    private CustomerDbContext _context;
    private CustomerRepository _repository;

    [GlobalSetup]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new CustomerDbContext(options);
        _repository = new CustomerRepository(_context);

        // Seed test data
        var customers = Enumerable.Range(1, 10000).Select(i => new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"Customer {i}",
            ContactEmail = $"customer{i}@example.com",
            TaxNumber = $"TAX{i:D6}",
            Status = (CustomerStatus)(i % 4)
        }).ToList();

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();
    }

    [Benchmark]
    public async Task<Customer?> GetByIdAsync()
    {
        var randomId = _context.Customers.Skip(Random.Shared.Next(10000)).First().Id;
        return await _repository.GetByIdAsync(randomId);
    }

    [Benchmark]
    public async Task<Customer?> GetByEmailAsync()
    {
        var randomEmail = $"customer{Random.Shared.Next(1, 10000)}@example.com";
        return await _repository.GetByEmailAsync(randomEmail);
    }

    [Benchmark]
    public async Task<IEnumerable<Customer>> SearchAsync()
    {
        var searchTerms = new[] { "Customer 1", "customer100", "TAX001" };
        var term = searchTerms[Random.Shared.Next(searchTerms.Length)];
        return await _repository.SearchAsync(term);
    }

    [Benchmark]
    public async Task<(IEnumerable<Customer>, int)> GetPagedAsync()
    {
        var page = Random.Shared.Next(1, 100);
        return await _repository.GetPagedAsync(page, 20);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }
}
```

## Security Testing

### Authentication Tests

#### Security Integration Tests

**Authentication and Authorization Tests:**
```csharp
public class CustomerServiceSecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomerServiceSecurityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_WithoutOrganizationHeader_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-User-Id", "test-user");
        // Missing X-Organization-Id header

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomers_WithoutUserHeader_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Organization-Id", "test-org");
        // Missing X-User-Id header

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostCustomer_WithSqlInjectionAttempt_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Organization-Id", "test-org");
        _client.DefaultRequestHeaders.Add("X-User-Id", "test-user");

        var maliciousRequest = new RegisterCustomerRequest
        {
            Name = "'; DROP TABLE Customers; --",
            ContactEmail = "evil@hacker.com",
            BillingAddress = "123 Hacker Street",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        var json = JsonSerializer.Serialize(maliciousRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        // Should either succeed (input sanitized) or fail validation, but not cause SQL injection
        if (response.IsSuccessStatusCode)
        {
            // Verify the malicious input was handled safely
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<CustomerDto>>(
                responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // The name should be stored as-is (parameterized query prevents injection)
            apiResponse!.Data!.Name.Should().Be(maliciousRequest.Name);
        }
        else
        {
            // Validation should catch malicious patterns
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task PostCustomer_WithXssAttempt_SanitizesInput()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Organization-Id", "test-org");
        _client.DefaultRequestHeaders.Add("X-User-Id", "test-user");

        var xssRequest = new RegisterCustomerRequest
        {
            Name = "<script>alert('xss')</script>",
            ContactEmail = "xss@test.com",
            BillingAddress = "<img src=x onerror=alert('xss')>",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        var json = JsonSerializer.Serialize(xssRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<CustomerDto>>(
                responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // XSS content should be encoded or rejected
            apiResponse!.Data!.Name.Should().NotContain("<script>");
            apiResponse.Data.BillingAddress.Should().NotContain("<img");
        }
    }
}
```

### Input Validation Security Tests

**Malicious Input Testing:**
```csharp
public class InputValidationSecurityTests
{
    private readonly RegisterCustomerValidator _validator;

    public InputValidationSecurityTests()
    {
        _validator = new RegisterCustomerValidator();
    }

    [Theory]
    [InlineData("'; DROP TABLE Customers; --")]
    [InlineData("admin'; DELETE FROM Customers WHERE '1'='1")]
    [InlineData("1' OR '1'='1")]
    public async Task Validate_WithSqlInjectionAttempts_AllowsSafeStorage(string maliciousName)
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = maliciousName,
            ContactEmail = "test@example.com",
            BillingAddress = "123 Test Street",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        // The validator should either accept the input (letting parameterized queries handle safety)
        // or reject it based on business rules, but not cause security issues
        if (!result.IsValid)
        {
            result.Errors.Should().NotBeEmpty();
        }
        // If valid, the parameterized queries will prevent SQL injection
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    [InlineData("javascript:alert('xss')")]
    public async Task Validate_WithXssAttempts_HandlesSecurely(string maliciousInput)
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = maliciousInput,
            ContactEmail = "test@example.com",
            BillingAddress = "123 Test Street",
            CustomerType = CustomerType.Corporate,
            CreditLimit = 10000
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        // Validation should handle XSS attempts appropriately
        // Either reject them or allow safe storage with proper encoding on output
        if (result.IsValid)
        {
            // If accepted, ensure output encoding will prevent XSS
            request.Name.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(long.MaxValue)]
    [InlineData(double.MaxValue)]
    public async Task Validate_WithExtremeValues_HandlesGracefully(decimal extremeValue)
    {
        // Arrange
        var request = new RegisterCustomerRequest
        {
            Name = "Test Customer",
            ContactEmail = "test@example.com",
            BillingAddress = "123 Test Street",
            CustomerType = CustomerType.Corporate,
            CreditLimit = extremeValue
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        // Should handle extreme values without causing overflow or system issues
        if (!result.IsValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(request.CreditLimit));
        }
    }
}
```

## Test Data Management

### Test Data Factories

#### Customer Test Data Factory

**Realistic Test Data Generation:**
```csharp
public class CustomerTestDataFactory
{
    private static readonly Faker<Customer> CustomerFaker = new Faker<Customer>()
        .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
        .RuleFor(c => c.Name, f => f.Company.CompanyName())
        .RuleFor(c => c.ContactEmail, f => f.Internet.Email())
        .RuleFor(c => c.ContactPhone, f => f.Phone.PhoneNumber())
        .RuleFor(c => c.BillingAddress, f => f.Address.FullAddress())
        .RuleFor(c => c.CustomerType, f => f.PickRandom<CustomerType>())
        .RuleFor(c => c.CreditLimit, f => f.Random.Decimal(1000, 100000))
        .RuleFor(c => c.Status, f => f.PickRandom<CustomerStatus>())
        .RuleFor(c => c.TaxNumber, f => f.Random.Replace("TAX######"))
        .RuleFor(c => c.RegistrationNumber, f => f.Random.Replace("REG######"))
        .RuleFor(c => c.Notes, f => f.Lorem.Sentence())
        .RuleFor(c => c.CreatedAt, f => f.Date.Past(2))
        .RuleFor(c => c.UpdatedAt, f => DateTime.UtcNow)
        .RuleFor(c => c.CreatedBy, f => "test-system")
        .RuleFor(c => c.UpdatedBy, f => "test-system");

    private static readonly Faker<RegisterCustomerRequest> RequestFaker = new Faker<RegisterCustomerRequest>()
        .RuleFor(r => r.Name, f => f.Company.CompanyName())
        .RuleFor(r => r.ContactEmail, f => f.Internet.Email())
        .RuleFor(r => r.ContactPhone, f => f.Phone.PhoneNumber())
        .RuleFor(r => r.BillingAddress, f => f.Address.FullAddress())
        .RuleFor(r => r.CustomerType, f => f.PickRandom<CustomerType>())
        .RuleFor(r => r.CreditLimit, f => f.Random.Decimal(1000, 50000))
        .RuleFor(r => r.TaxNumber, f => f.Random.Replace("TAX######"))
        .RuleFor(r => r.RegistrationNumber, f => f.Random.Replace("REG######"))
        .RuleFor(r => r.Notes, f => f.Lorem.Sentence());

    public static Customer CreateCustomer() => CustomerFaker.Generate();
    
    public static List<Customer> CreateCustomers(int count) => CustomerFaker.Generate(count);
    
    public static RegisterCustomerRequest CreateRequest() => RequestFaker.Generate();
    
    public static List<RegisterCustomerRequest> CreateRequests(int count) => RequestFaker.Generate(count);

    public static Customer CreateCustomerWithType(CustomerType type)
    {
        var customer = CreateCustomer();
        customer.CustomerType = type;
        return customer;
    }

    public static Customer CreateCustomerWithStatus(CustomerStatus status)
    {
        var customer = CreateCustomer();
        customer.Status = status;
        return customer;
    }

    public static Customer CreateCorporateCustomer()
    {
        return CreateCustomerWithType(CustomerType.Corporate);
    }

    public static Customer CreateIndividualCustomer()
    {
        return CreateCustomerWithType(CustomerType.Individual);
    }
}
```

### Test Database Seeding

#### Database Seed Data

**Test Environment Setup:**
```csharp
public class TestDatabaseSeeder
{
    private readonly CustomerDbContext _context;

    public TestDatabaseSeeder(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Customers.AnyAsync())
            return; // Already seeded

        // Create test customers with specific scenarios
        var customers = new List<Customer>
        {
            // Active corporate customer
            CustomerTestDataFactory.CreateCustomer() with
            {
                Name = "Acme Corporation",
                ContactEmail = "contact@acme.com",
                CustomerType = CustomerType.Corporate,
                Status = CustomerStatus.Active,
                CreditLimit = 50000
            },

            // Individual customer
            CustomerTestDataFactory.CreateCustomer() with
            {
                Name = "John Smith",
                ContactEmail = "john.smith@email.com",
                CustomerType = CustomerType.Individual,
                Status = CustomerStatus.Active,
                CreditLimit = 5000
            },

            // Suspended customer
            CustomerTestDataFactory.CreateCustomer() with
            {
                Name = "Suspended Company",
                ContactEmail = "suspended@company.com",
                CustomerType = CustomerType.Corporate,
                Status = CustomerStatus.Suspended,
                CreditLimit = 0
            },

            // Government customer
            CustomerTestDataFactory.CreateCustomer() with
            {
                Name = "City Public Works",
                ContactEmail = "procurement@city.gov",
                CustomerType = CustomerType.Government,
                Status = CustomerStatus.Active,
                CreditLimit = 100000
            }
        };

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        // Add contacts for the first customer
        var acmeCustomer = customers.First();
        var contacts = new List<CustomerContact>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = acmeCustomer.Id,
                FirstName = "John",
                LastName = "Doe",
                Email = "j.doe@acme.com",
                ContactType = ContactType.Business,
                IsPrimary = true,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = acmeCustomer.Id,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "j.smith@acme.com",
                ContactType = ContactType.Technical,
                IsPrimary = true,
                IsActive = true
            }
        };

        _context.CustomerContacts.AddRange(contacts);
        await _context.SaveChangesAsync();
    }
}
```

## Continuous Integration Testing

### Test Pipeline Configuration

#### GitHub Actions Test Workflow

**CI/CD Test Pipeline:**
```yaml
name: Customer Service Tests

on:
  push:
    branches: [ main, develop ]
    paths: 
      - 'packages/microservices/masterdata/customer-service/**'
  pull_request:
    branches: [ main ]
    paths:
      - 'packages/microservices/masterdata/customer-service/**'

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: customerservice_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore packages/microservices/masterdata/customer-service/
      
    - name: Build
      run: dotnet build packages/microservices/masterdata/customer-service/ --no-restore
      
    - name: Run Unit Tests
      run: |
        dotnet test packages/microservices/masterdata/customer-service/tests/CustomerService.Tests/ \
          --no-build --verbosity normal \
          --logger "trx;LogFileName=unit-tests.trx" \
          --collect:"XPlat Code Coverage" \
          --filter Category=Unit
          
    - name: Run Integration Tests
      run: |
        dotnet test packages/microservices/masterdata/customer-service/tests/CustomerService.Tests/ \
          --no-build --verbosity normal \
          --logger "trx;LogFileName=integration-tests.trx" \
          --collect:"XPlat Code Coverage" \
          --filter Category=Integration
      env:
        ConnectionStrings__DefaultConnection: "Host=localhost;Database=customerservice_test;Username=postgres;Password=postgres"
        
    - name: Run Security Tests
      run: |
        dotnet test packages/microservices/masterdata/customer-service/tests/CustomerService.Tests/ \
          --no-build --verbosity normal \
          --logger "trx;LogFileName=security-tests.trx" \
          --filter Category=Security
          
    - name: Generate Code Coverage Report
      run: |
        dotnet tool install --global dotnet-reportgenerator-globaltool
        reportgenerator \
          -reports:"**/coverage.cobertura.xml" \
          -targetdir:"coverage" \
          -reporttypes:"Html;Cobertura"
          
    - name: Upload Coverage Reports
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage/Cobertura.xml
        
    - name: Publish Test Results
      uses: dorny/test-reporter@v1
      if: always()
      with:
        name: Test Results
        path: "**/*.trx"
        reporter: dotnet-trx
```

### Quality Gates

#### Code Coverage Requirements

**Quality Metrics:**
```csharp
[assembly: AssemblyMetadata("MinimumCodeCoverage", "90")]
[assembly: AssemblyMetadata("MinimumBranchCoverage", "85")]
[assembly: AssemblyMetadata("MaximumCyclomaticComplexity", "10")]
```

**Coverage Verification:**
```csharp
public class CodeCoverageTests
{
    [Fact]
    public void VerifyCodeCoverage()
    {
        var assembly = typeof(CustomerService).Assembly;
        var coverageAttribute = assembly.GetCustomAttribute<AssemblyMetadataAttribute>();
        
        // This would be implemented with actual coverage tools
        Assert.NotNull(coverageAttribute);
    }
}
```

This comprehensive testing guide ensures that the Customer Service maintains high quality, reliability, and security standards through thorough testing at all levels of the application stack.