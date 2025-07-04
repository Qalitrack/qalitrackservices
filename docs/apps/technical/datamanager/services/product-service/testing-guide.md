# Product Service Testing Guide

## Table of Contents

- [Testing Strategy](#testing-strategy)
- [Test Architecture](#test-architecture)
- [Unit Testing](#unit-testing)
- [Integration Testing](#integration-testing)
- [API Testing](#api-testing)
- [Database Testing](#database-testing)
- [Performance Testing](#performance-testing)
- [Security Testing](#security-testing)
- [Contract Testing](#contract-testing)
- [End-to-End Testing](#end-to-end-testing)
- [Test Automation](#test-automation)
- [Test Data Management](#test-data-management)
- [Continuous Testing](#continuous-testing)

## Testing Strategy

### Testing Pyramid

The Product Service follows the testing pyramid approach with emphasis on fast, reliable unit tests and comprehensive integration coverage.

```
           ┌─────────────────┐
          /     E2E Tests     \    ← Few, high-value scenarios
         /                   \
        ┌─────────────────────┐
       /   Integration Tests   \   ← Service boundaries, API contracts
      /                       \
     ┌─────────────────────────┐
    /      Unit Tests          \    ← Business logic, domain rules
   /                           \
  └─────────────────────────────┘
```

### Testing Principles

1. **Fast Feedback**: Unit tests run in milliseconds
2. **Reliable**: Tests are deterministic and repeatable
3. **Independent**: Tests don't depend on each other
4. **Comprehensive**: High code coverage with meaningful tests
5. **Maintainable**: Clear, readable test code
6. **Realistic**: Test data reflects real-world scenarios

### Test Categories

| Test Type | Purpose | Scope | Tools |
|-----------|---------|-------|-------|
| Unit | Business logic validation | Single class/method | xUnit, Moq, FluentAssertions |
| Integration | Service interaction | Multiple components | xUnit, TestContainers, WebApplicationFactory |
| API | HTTP endpoint behavior | Controller layer | xUnit, HttpClient, Postman |
| Database | Data persistence | Repository layer | xUnit, EF InMemory, SQLite |
| Performance | Response times, throughput | Full service | NBomber, k6, JMeter |
| Security | Authentication, authorization | Security boundaries | xUnit, OWASP ZAP |
| Contract | API compatibility | Service contracts | Pact, OpenAPI |
| E2E | Complete workflows | Full system | Playwright, Selenium |

## Test Architecture

### Project Structure

```
ProductService.Tests/
├── Unit/
│   ├── Services/
│   │   ├── ProductServiceTests.cs
│   │   └── CategoryServiceTests.cs
│   ├── Validators/
│   │   ├── RegisterProductValidatorTests.cs
│   │   └── UpdateProductValidatorTests.cs
│   └── Entities/
│       └── ProductTests.cs
├── Integration/
│   ├── Repositories/
│   │   ├── ProductRepositoryTests.cs
│   │   └── CategoryRepositoryTests.cs
│   ├── Controllers/
│   │   ├── ProductsControllerTests.cs
│   │   └── CategoriesControllerTests.cs
│   └── Database/
│       └── DatabaseIntegrationTests.cs
├── Api/
│   ├── ProductApiTests.cs
│   ├── CategoryApiTests.cs
│   └── SecurityApiTests.cs
├── Performance/
│   ├── LoadTests.cs
│   └── StressTests.cs
├── Security/
│   ├── AuthenticationTests.cs
│   ├── AuthorizationTests.cs
│   └── InputValidationTests.cs
├── Helpers/
│   ├── TestDataBuilder.cs
│   ├── DatabaseFixture.cs
│   └── ApiTestFixture.cs
└── GlobalUsings.cs
```

### Test Configuration

#### GlobalUsings.cs
```csharp
global using Xunit;
global using FluentAssertions;
global using Moq;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.EntityFrameworkCore;
global using ProductService.Core.Entities;
global using ProductService.Core.DTOs;
global using ProductService.Core.Interfaces;
global using ProductService.Infrastructure.Data;
global using ProductService.Tests.Helpers;
```

#### Test Project Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Moq" Version="4.20.69" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    <PackageReference Include="NBomber" Version="5.5.3" />
    <PackageReference Include="Testcontainers" Version="3.6.0" />
    <PackageReference Include="Bogus" Version="35.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../src/ProductService.Api/ProductService.Api.csproj" />
    <ProjectReference Include="../src/ProductService.Core/ProductService.Core.csproj" />
    <ProjectReference Include="../src/ProductService.Infrastructure/ProductService.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

## Unit Testing

### Service Layer Testing

#### ProductService Tests
```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IProductCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProductService.Core.Services.ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<IProductCategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        
        _productService = new ProductService.Core.Services.ProductService(
            _productRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            Mock.Of<IProductComplianceRepository>(),
            Mock.Of<IRepository<ProductSpecification>>(),
            Mock.Of<IRepository<ProductPricing>>(),
            Mock.Of<IRepository<ProductHazmat>>(),
            _mapperMock.Object);
    }

    [Fact]
    public async Task RegisterProductAsync_ValidRequest_ReturnsProductDto()
    {
        // Arrange
        var request = TestDataBuilder.CreateRegisterProductRequest();
        var product = TestDataBuilder.CreateProduct();
        var productDto = TestDataBuilder.CreateProductDto();

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(productDto);

        // Act
        var result = await _productService.RegisterProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Code.Should().Be(request.Code);
        
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);
        _mapperMock.Verify(x => x.Map<ProductDto>(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ProductNotFound_ThrowsArgumentException()
    {
        // Arrange
        var productId = "non-existent-id";
        var request = TestDataBuilder.CreateUpdateProductRequest();

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _productService.UpdateProductAsync(productId, request));
        
        exception.Message.Should().Be("Product not found");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GetProductByIdAsync_InvalidId_ReturnsNull(string invalidId)
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetProductByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }
}
```

### Validator Testing

#### RegisterProductValidator Tests
```csharp
public class RegisterProductValidatorTests
{
    private readonly RegisterProductValidator _validator;

    public RegisterProductValidatorTests()
    {
        _validator = new RegisterProductValidator();
    }

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        // Arrange
        var request = new RegisterProductRequest
        {
            Name = "Valid Product Name",
            Code = "VALID-001",
            CategoryId = "valid-category-id",
            UnitOfMeasure = "Liters"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_InvalidName_FailsValidation(string invalidName)
    {
        // Arrange
        var request = TestDataBuilder.CreateRegisterProductRequest();
        request.Name = invalidName;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Name));
    }

    [Fact]
    public void Validate_NameTooLong_FailsValidation()
    {
        // Arrange
        var request = TestDataBuilder.CreateRegisterProductRequest();
        request.Name = new string('a', 101); // Exceeds 100 character limit

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(request.Name) && 
            e.ErrorMessage.Contains("100"));
    }

    [Theory]
    [InlineData("VALID-CODE")]
    [InlineData("PROD-123")]
    [InlineData("A1B2C3")]
    public void Validate_ValidProductCode_PassesValidation(string validCode)
    {
        // Arrange
        var request = TestDataBuilder.CreateRegisterProductRequest();
        request.Code = validCode;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid code")]
    [InlineData("INVALID@CODE")]
    [InlineData("INVALID#CODE")]
    public void Validate_InvalidProductCode_FailsValidation(string invalidCode)
    {
        // Arrange
        var request = TestDataBuilder.CreateRegisterProductRequest();
        request.Code = invalidCode;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Code));
    }
}
```

### Entity Testing

#### Product Entity Tests
```csharp
public class ProductTests
{
    [Fact]
    public void Product_Creation_SetsDefaultValues()
    {
        // Act
        var product = new Product();

        // Assert
        product.Id.Should().NotBeNullOrEmpty();
        product.Status.Should().Be(ProductStatus.Active);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        product.IsDeleted.Should().BeFalse();
        product.Specifications.Should().NotBeNull();
        product.Pricing.Should().NotBeNull();
        product.ComplianceRequirements.Should().NotBeNull();
        product.Variants.Should().NotBeNull();
    }

    [Theory]
    [InlineData(ProductStatus.Active, true)]
    [InlineData(ProductStatus.Inactive, true)]
    [InlineData(ProductStatus.Discontinued, false)]
    [InlineData(ProductStatus.Pending, true)]
    public void CanBeModified_DifferentStatuses_ReturnsExpectedResult(
        ProductStatus status, bool expectedResult)
    {
        // Arrange
        var product = new Product { Status = status };

        // Act
        var result = product.CanBeModified();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void UpdateStatus_ValidTransition_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.Pending };
        var originalUpdateTime = product.UpdatedAt;

        // Act
        product.UpdateStatus(ProductStatus.Active);

        // Assert
        product.Status.Should().Be(ProductStatus.Active);
        product.UpdatedAt.Should().BeAfter(originalUpdateTime);
    }
}
```

## Integration Testing

### Repository Testing

#### ProductRepository Integration Tests
```csharp
public class ProductRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ProductDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _context = _fixture.CreateContext();
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ValidProduct_AddsToDatabase()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct();

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);

        var dbProduct = await _context.Products.FindAsync(product.Id);
        dbProduct.Should().NotBeNull();
        dbProduct!.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsProduct()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct();
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCodeAsync(product.Code);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Code.Should().Be(product.Code);
    }

    [Fact]
    public async Task GetByCodeAsync_NonExistentCode_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByCodeAsync("NON-EXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetHazardousProductsAsync_HasHazardousProducts_ReturnsOnlyHazardous()
    {
        // Arrange
        var hazardousProduct = TestDataBuilder.CreateProduct();
        hazardousProduct.IsHazardous = true;
        hazardousProduct.HazmatClass = "3";

        var nonHazardousProduct = TestDataBuilder.CreateProduct();
        nonHazardousProduct.IsHazardous = false;

        await _context.Products.AddRangeAsync(hazardousProduct, nonHazardousProduct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetHazardousProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(hazardousProduct.Id);
        result.First().IsHazardous.Should().BeTrue();
    }

    [Fact]
    public async Task SearchProductsAsync_PartialMatch_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProduct();
        product1.Name = "Diesel Fuel Premium";
        product1.Code = "DIESEL-001";

        var product2 = TestDataBuilder.CreateProduct();
        product2.Name = "Gasoline Regular";
        product2.Code = "GAS-001";

        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchProductsAsync("diesel");

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(product1.Id);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Database Fixture

#### DatabaseFixture Implementation
```csharp
public class DatabaseFixture : IDisposable
{
    private readonly string _connectionString;
    
    public DatabaseFixture()
    {
        _connectionString = $"Data Source={Path.GetTempFileName()}";
        
        using var context = CreateContext();
        context.Database.EnsureCreated();
        SeedTestData(context);
    }

    public ProductDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        return new ProductDbContext(options);
    }

    private void SeedTestData(ProductDbContext context)
    {
        // Seed test categories
        var categories = new[]
        {
            new ProductCategory { Id = "cat-fuel", Name = "Fuels", Code = "FUEL" },
            new ProductCategory { Id = "cat-chem", Name = "Chemicals", Code = "CHEM" },
        };

        context.ProductCategories.AddRange(categories);
        context.SaveChanges();
    }

    public void Dispose()
    {
        // Cleanup is automatic with SQLite temp files
    }
}
```

## API Testing

### Controller Integration Tests

#### ProductsController API Tests
```csharp
public class ProductsControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public ProductsControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task GetProducts_NoFilters_ReturnsAllProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<List<ProductDto>>>(content);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeOfType<List<ProductDto>>();
    }

    [Fact]
    public async Task GetProduct_ExistingId_ReturnsProduct()
    {
        // Arrange
        var productId = await _fixture.CreateTestProductAsync();

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(content);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetProduct_NonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/non-existent-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(content);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsCreatedProduct()
    {
        // Arrange
        var categoryId = await _fixture.CreateTestCategoryAsync();
        var request = new RegisterProductRequest
        {
            Name = "Test Product",
            Code = "TEST-001",
            CategoryId = categoryId,
            UnitOfMeasure = "Units",
            IsHazardous = false
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(responseContent);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Name.Should().Be(request.Name);
        result.Data.Code.Should().Be(request.Code);

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/products/{result.Data.Id}");
    }

    [Fact]
    public async Task CreateProduct_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterProductRequest
        {
            // Missing required fields
            Name = "",
            Code = "",
            CategoryId = "",
            UnitOfMeasure = ""
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(responseContent);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SearchProducts_ValidTerm_ReturnsMatchingProducts()
    {
        // Arrange
        var productId = await _fixture.CreateTestProductAsync("Searchable Product", "SEARCH-001");

        // Act
        var response = await _client.GetAsync("/api/products/search?searchTerm=searchable");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<List<ProductDto>>>(content);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().Contain(p => p.Id == productId);
    }
}
```

### API Test Fixture

#### ApiTestFixture Implementation
```csharp
public class ApiTestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ProductDbContext _context;

    public ApiTestFixture()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<ProductDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                    });

                    // Build service provider and create database
                    var serviceProvider = services.BuildServiceProvider();
                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                    context.Database.EnsureCreated();
                    SeedTestData(context);
                });
            });

        using var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    }

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task<string> CreateTestCategoryAsync(string name = "Test Category", string code = "TEST")
    {
        var category = new ProductCategory
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Code = code,
            IsActive = true
        };

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        context.ProductCategories.Add(category);
        await context.SaveChangesAsync();

        return category.Id;
    }

    public async Task<string> CreateTestProductAsync(string name = "Test Product", string code = "TEST-001")
    {
        var categoryId = await CreateTestCategoryAsync();
        
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Code = code,
            CategoryId = categoryId,
            UnitOfMeasure = "Units",
            Status = ProductStatus.Active
        };

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product.Id;
    }

    private void SeedTestData(ProductDbContext context)
    {
        // Add initial test data if needed
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
```

## Database Testing

### Database Migration Tests

#### Migration Testing
```csharp
public class DatabaseMigrationTests
{
    [Fact]
    public void DatabaseMigrations_CanBeApplied()
    {
        // Arrange
        var connectionString = $"Data Source={Path.GetTempFileName()}";
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseSqlite(connectionString)
            .Options;

        // Act & Assert
        using var context = new ProductDbContext(options);
        var exception = Record.Exception(() => context.Database.EnsureCreated());
        exception.Should().BeNull();

        // Verify tables exist
        context.Model.GetEntityTypes().Should().NotBeEmpty();
    }

    [Fact]
    public async Task DatabaseSeeding_CreatesInitialData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase($"SeedTest_{Guid.NewGuid()}")
            .Options;

        // Act
        using var context = new ProductDbContext(options);
        await context.Database.EnsureCreatedAsync();
        await SeedInitialDataAsync(context);

        // Assert
        var categoriesCount = await context.ProductCategories.CountAsync();
        categoriesCount.Should().BeGreaterThan(0);
    }

    private async Task SeedInitialDataAsync(ProductDbContext context)
    {
        var categories = new[]
        {
            new ProductCategory { Name = "Fuels", Code = "FUEL", IsActive = true },
            new ProductCategory { Name = "Chemicals", Code = "CHEM", IsActive = true }
        };

        context.ProductCategories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}
```

### Data Consistency Tests

#### Referential Integrity Tests
```csharp
public class DataConsistencyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DataConsistencyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Product_DeleteWithSpecifications_CascadeDeletes()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var product = TestDataBuilder.CreateProduct();
        var specification = new ProductSpecification
        {
            ProductId = product.Id,
            Name = "Test Spec",
            Value = "Test Value"
        };

        context.Products.Add(product);
        context.ProductSpecifications.Add(specification);
        await context.SaveChangesAsync();

        // Act
        context.Products.Remove(product);
        await context.SaveChangesAsync();

        // Assert
        var remainingSpecs = await context.ProductSpecifications
            .Where(s => s.ProductId == product.Id)
            .ToListAsync();
        remainingSpecs.Should().BeEmpty();
    }

    [Fact]
    public async Task Product_RequiresValidCategory()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var product = TestDataBuilder.CreateProduct();
        product.CategoryId = "non-existent-category";

        // Act & Assert
        context.Products.Add(product);
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync());
        
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task ProductCode_MustBeUnique()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var product1 = TestDataBuilder.CreateProduct();
        var product2 = TestDataBuilder.CreateProduct();
        product2.Code = product1.Code; // Same code

        context.Products.AddRange(product1, product2);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.SaveChangesAsync());
        
        exception.Should().NotBeNull();
    }
}
```

## Performance Testing

### Load Testing with NBomber

#### Basic Load Tests
```csharp
public class ProductServiceLoadTests
{
    [Fact]
    public void GetProducts_LoadTest_PerformanceCheck()
    {
        var scenario = Scenario.Create("get_products", async context =>
        {
            using var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:7005/api/products");
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(2)),
            Simulation.KeepConstant(copies: 5, during: TimeSpan.FromMinutes(3))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert performance requirements
        var scnStats = stats.AllScenarios.First();
        scnStats.Ok.Request.Mean.Should().BeLessOrEqualTo(TimeSpan.FromMilliseconds(500));
        scnStats.Ok.Request.StdDev.Should().BeLessOrEqualTo(TimeSpan.FromMilliseconds(200));
        scnStats.Fail.Request.Count.Should().BeLessOrEqualTo(scnStats.Ok.Request.Count * 0.01); // < 1% failure rate
    }

    [Fact]
    public void CreateProduct_StressTest_HandlesConcurrentRequests()
    {
        var scenario = Scenario.Create("create_product", async context =>
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
            
            var request = new RegisterProductRequest
            {
                Name = $"Load Test Product {context.InvocationNumber}",
                Code = $"LOAD-{context.InvocationNumber:D6}",
                CategoryId = "test-category",
                UnitOfMeasure = "Units"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync("http://localhost:7005/api/products", content);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 20, during: TimeSpan.FromMinutes(1))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert stress test requirements
        var scnStats = stats.AllScenarios.First();
        scnStats.Ok.Request.Mean.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(2));
        scnStats.Fail.Request.Count.Should().BeLessOrEqualTo(scnStats.Ok.Request.Count * 0.05); // < 5% failure rate
    }
}
```

### Database Performance Tests

#### Query Performance Tests
```csharp
public class DatabasePerformanceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DatabasePerformanceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProductSearch_LargeDataset_CompletesWithinTimeout()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedLargeDatasetAsync(context);

        var repository = new ProductRepository(context);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = await repository.SearchProductsAsync("test");
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(1000); // < 1 second
        results.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProductsByCategory_WithIncludes_OptimizedQuery()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedTestDataWithRelationsAsync(context);

        var stopwatch = Stopwatch.StartNew();

        // Act
        var products = await context.Products
            .Include(p => p.Category)
            .Include(p => p.Specifications)
            .Where(p => p.CategoryId == "test-category")
            .ToListAsync();

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(500); // < 500ms
        products.Should().NotBeEmpty();
        products.All(p => p.Category != null).Should().BeTrue();
    }

    private async Task SeedLargeDatasetAsync(ProductDbContext context)
    {
        var faker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Code, f => f.Commerce.Ean13())
            .RuleFor(p => p.CategoryId, "test-category")
            .RuleFor(p => p.UnitOfMeasure, f => f.PickRandom("Liters", "Kilograms", "Units"))
            .RuleFor(p => p.Status, f => f.PickRandom<ProductStatus>());

        var products = faker.Generate(1000);
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}
```

## Security Testing

### Authentication Tests

#### JWT Token Validation Tests
```csharp
public class AuthenticationTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public AuthenticationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task GetProducts_NoToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProducts_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProducts_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var token = TestHelpers.GenerateValidJwtToken();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Authorization Tests

#### Role-Based Access Tests
```csharp
public class AuthorizationTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public AuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Theory]
    [InlineData("Admin", HttpStatusCode.Created)]
    [InlineData("SiteManager", HttpStatusCode.Created)]
    [InlineData("Operator", HttpStatusCode.Created)]
    [InlineData("Viewer", HttpStatusCode.Forbidden)]
    public async Task CreateProduct_DifferentRoles_ReturnsExpectedStatusCode(
        string role, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var token = TestHelpers.GenerateJwtTokenWithRole(role);
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        var request = new RegisterProductRequest
        {
            Name = "Test Product",
            Code = "TEST-001",
            CategoryId = "test-category",
            UnitOfMeasure = "Units"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task DeleteProduct_InsufficientPermissions_ReturnsForbidden()
    {
        // Arrange
        var token = TestHelpers.GenerateJwtTokenWithRole("Viewer");
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        var productId = await _fixture.CreateTestProductAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
```

### Input Validation Security Tests

#### SQL Injection Prevention Tests
```csharp
public class InputValidationSecurityTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public InputValidationSecurityTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        
        var token = TestHelpers.GenerateValidJwtToken();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Theory]
    [InlineData("'; DROP TABLE Products; --")]
    [InlineData("1' OR '1'='1")]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("../../etc/passwd")]
    public async Task SearchProducts_MaliciousInput_RejectsSafely(string maliciousInput)
    {
        // Act
        var response = await _client.GetAsync($"/api/products/search?searchTerm={Uri.EscapeDataString(maliciousInput)}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("error", "Error occurred while processing");
        }
    }

    [Fact]
    public async Task CreateProduct_ExcessivelyLongInput_RejectsSafely()
    {
        // Arrange
        var request = new RegisterProductRequest
        {
            Name = new string('A', 10000), // Excessively long name
            Code = "TEST-001",
            CategoryId = "test-category",
            UnitOfMeasure = "Units"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

## Contract Testing

### API Contract Tests with Pact

#### Consumer Contract Tests
```csharp
public class ProductServiceConsumerTests : IClassFixture<PactFixture>
{
    private readonly PactFixture _fixture;
    private readonly IPactBuilderV3 _pact;

    public ProductServiceConsumerTests(PactFixture fixture)
    {
        _fixture = fixture;
        _pact = Pact.V3("CustomerService", "ProductService", fixture.PactConfig);
    }

    [Fact]
    public async Task GetProduct_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        var productId = "test-product-123";
        var expectedProduct = new
        {
            id = productId,
            name = "Test Product",
            code = "TEST-001",
            categoryId = "test-category",
            isHazardous = false
        };

        _pact
            .UponReceiving("a request for an existing product")
            .Given($"product {productId} exists")
            .WithRequest(HttpMethod.Get, $"/api/products/{productId}")
            .WithHeader("Authorization", Match.Regex(@"Bearer .+"))
            .WillRespondWith()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(new
            {
                success = true,
                data = Match.Type(expectedProduct)
            });

        // Act
        var mockServerUri = _pact.MockServer();
        var client = new ProductServiceClient(mockServerUri.ToString());
        var result = await client.GetProductAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetProduct_NonExistentProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = "non-existent-product";

        _pact
            .UponReceiving("a request for a non-existent product")
            .Given($"product {productId} does not exist")
            .WithRequest(HttpMethod.Get, $"/api/products/{productId}")
            .WithHeader("Authorization", Match.Regex(@"Bearer .+"))
            .WillRespondWith()
            .WithStatus(HttpStatusCode.NotFound)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(new
            {
                success = false,
                message = "Product not found"
            });

        // Act
        var mockServerUri = _pact.MockServer();
        var client = new ProductServiceClient(mockServerUri.ToString());
        
        // Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => client.GetProductAsync(productId));
        exception.Message.Should().Contain("not found");
    }
}
```

### Provider Contract Verification

#### Provider Verification Tests
```csharp
public class ProductServiceProviderTests
{
    [Fact]
    public void ProductService_HonoursConsumerContracts()
    {
        var config = new PactVerifierConfig
        {
            Outputters = new List<IOutput> { new ConsoleOutput() },
            LogLevel = PactLogLevel.Information
        };

        var verifier = new PactVerifier(config);

        verifier
            .ServiceProvider("ProductService", new Uri("http://localhost:7005"))
            .HonoursPactWith("CustomerService")
            .PactUri(@"../pacts/customerservice-productservice.json")
            .ProviderState("product test-product-123 exists", SetupProductExists)
            .ProviderState("product non-existent-product does not exist", SetupProductNotExists)
            .Verify();
    }

    private void SetupProductExists()
    {
        // Setup test data for the provider state
        // This would typically involve seeding the test database
        TestDataSeeder.SeedProductWithId("test-product-123");
    }

    private void SetupProductNotExists()
    {
        // Ensure the product doesn't exist
        // This might involve cleaning up test data
        TestDataSeeder.CleanupProduct("non-existent-product");
    }
}
```

## End-to-End Testing

### Full Workflow Tests

#### Product Lifecycle E2E Tests
```csharp
public class ProductLifecycleE2ETests : IClassFixture<E2ETestFixture>
{
    private readonly E2ETestFixture _fixture;

    public ProductLifecycleE2ETests(E2ETestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CompleteProductWorkflow_Success()
    {
        // Arrange
        var client = _fixture.CreateAuthenticatedClient();

        // 1. Create category
        var categoryRequest = new CreateProductCategoryRequest
        {
            Name = "E2E Test Category",
            Code = "E2E-CAT",
            Description = "Category for E2E testing"
        };

        var categoryResponse = await client.PostAsJsonAsync("/api/products/categories", categoryRequest);
        categoryResponse.EnsureSuccessStatusCode();
        
        var categoryResult = await categoryResponse.Content.ReadFromJsonAsync<ApiResponseDto<ProductCategoryDto>>();
        var categoryId = categoryResult!.Data.Id;

        // 2. Create product
        var productRequest = new RegisterProductRequest
        {
            Name = "E2E Test Product",
            Code = "E2E-PROD-001",
            CategoryId = categoryId,
            UnitOfMeasure = "Units",
            IsHazardous = true,
            HazmatClass = "3",
            RequiresSpecialHandling = true
        };

        var productResponse = await client.PostAsJsonAsync("/api/products", productRequest);
        productResponse.EnsureSuccessStatusCode();
        
        var productResult = await productResponse.Content.ReadFromJsonAsync<ApiResponseDto<ProductDto>>();
        var productId = productResult!.Data.Id;

        // 3. Add specifications
        var specsRequest = new UpdateProductSpecificationsRequest
        {
            Specifications = new List<ProductSpecificationDto>
            {
                new() { Name = "Density", Value = "0.85", Unit = "g/cm³" },
                new() { Name = "Flash Point", Value = "60", Unit = "°C" }
            }
        };

        var specsResponse = await client.PutAsJsonAsync($"/api/products/{productId}/specifications", specsRequest);
        specsResponse.EnsureSuccessStatusCode();

        // 4. Add pricing
        var pricingRequest = new UpdateProductPricingRequest
        {
            PricingRules = new List<ProductPricingDto>
            {
                new()
                {
                    UnitPrice = 2.50m,
                    Currency = "USD",
                    MinimumQuantity = 1000,
                    CustomerGroup = "Retail",
                    EffectiveDate = DateTime.UtcNow
                }
            }
        };

        var pricingResponse = await client.PutAsJsonAsync($"/api/products/{productId}/pricing", pricingRequest);
        pricingResponse.EnsureSuccessStatusCode();

        // 5. Verify complete product
        var getResponse = await client.GetAsync($"/api/products/{productId}");
        getResponse.EnsureSuccessStatusCode();
        
        var getResult = await getResponse.Content.ReadFromJsonAsync<ApiResponseDto<ProductDto>>();
        var finalProduct = getResult!.Data;

        // Assert complete workflow
        finalProduct.Should().NotBeNull();
        finalProduct.Name.Should().Be(productRequest.Name);
        finalProduct.Code.Should().Be(productRequest.Code);
        finalProduct.CategoryId.Should().Be(categoryId);
        finalProduct.IsHazardous.Should().BeTrue();
        finalProduct.HazmatClass.Should().Be("3");

        // 6. Verify specifications
        var specsGetResponse = await client.GetAsync($"/api/products/{productId}/specifications");
        specsGetResponse.EnsureSuccessStatusCode();
        
        var specsGetResult = await specsGetResponse.Content.ReadFromJsonAsync<ApiResponseDto<List<ProductSpecificationDto>>>();
        specsGetResult!.Data.Should().HaveCount(2);

        // 7. Verify pricing
        var pricingGetResponse = await client.GetAsync($"/api/products/{productId}/pricing");
        pricingGetResponse.EnsureSuccessStatusCode();
        
        var pricingGetResult = await pricingGetResponse.Content.ReadFromJsonAsync<ApiResponseDto<List<ProductPricingDto>>>();
        pricingGetResult!.Data.Should().HaveCount(1);
        pricingGetResult.Data.First().UnitPrice.Should().Be(2.50m);
    }
}
```

## Test Automation

### CI/CD Pipeline Integration

#### GitHub Actions Test Workflow
```yaml
name: Test Product Service

on:
  push:
    branches: [ main, develop ]
    paths: [ 'packages/microservices/masterdata/product-service/**' ]
  pull_request:
    branches: [ main ]
    paths: [ 'packages/microservices/masterdata/product-service/**' ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: productservice_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore packages/microservices/masterdata/product-service/
      
    - name: Build
      run: dotnet build packages/microservices/masterdata/product-service/ --no-restore
      
    - name: Run unit tests
      run: |
        dotnet test packages/microservices/masterdata/product-service/tests/ProductService.Tests/ \
          --no-build --verbosity normal --logger trx --results-directory TestResults \
          --collect:"XPlat Code Coverage" --filter Category=Unit
          
    - name: Run integration tests
      run: |
        dotnet test packages/microservices/masterdata/product-service/tests/ProductService.Tests/ \
          --no-build --verbosity normal --logger trx --results-directory TestResults \
          --collect:"XPlat Code Coverage" --filter Category=Integration
      env:
        ConnectionStrings__DefaultConnection: Host=localhost;Database=productservice_test;Username=postgres;Password=postgres
        
    - name: Upload test results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: test-results
        path: TestResults/
        
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        directory: TestResults/
        flags: product-service
```

### Test Data Management

#### Test Data Builder
```csharp
public static class TestDataBuilder
{
    private static readonly Faker _faker = new();

    public static Product CreateProduct(string? categoryId = null)
    {
        return new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = _faker.Commerce.ProductName(),
            Code = GenerateProductCode(),
            Description = _faker.Commerce.ProductDescription(),
            CategoryId = categoryId ?? "default-category",
            UnitOfMeasure = _faker.PickRandom("Liters", "Kilograms", "Units", "Gallons"),
            Weight = _faker.Random.Decimal(0.1m, 100m),
            Density = _faker.Random.Decimal(0.5m, 2.0m),
            IsHazardous = _faker.Random.Bool(0.3f), // 30% chance of being hazardous
            HazmatClass = _faker.Random.Bool(0.3f) ? _faker.PickRandom("1", "2", "3", "4", "5", "6", "7", "8", "9") : null,
            RequiresSpecialHandling = _faker.Random.Bool(0.2f),
            Status = _faker.PickRandom<ProductStatus>(),
            Notes = _faker.Lorem.Sentence()
        };
    }

    public static ProductCategory CreateProductCategory(string? parentId = null)
    {
        return new ProductCategory
        {
            Id = Guid.NewGuid().ToString(),
            Name = _faker.Commerce.Categories(1).First(),
            Code = GenerateCategoryCode(),
            Description = _faker.Lorem.Sentence(),
            ParentCategoryId = parentId,
            SortOrder = _faker.Random.Int(1, 100),
            IsActive = true
        };
    }

    public static RegisterProductRequest CreateRegisterProductRequest(string? categoryId = null)
    {
        return new RegisterProductRequest
        {
            Name = _faker.Commerce.ProductName(),
            Code = GenerateProductCode(),
            Description = _faker.Commerce.ProductDescription(),
            CategoryId = categoryId ?? "default-category",
            UnitOfMeasure = _faker.PickRandom("Liters", "Kilograms", "Units"),
            Weight = _faker.Random.Decimal(0.1m, 100m),
            Density = _faker.Random.Decimal(0.5m, 2.0m),
            IsHazardous = false,
            RequiresSpecialHandling = false,
            Notes = _faker.Lorem.Sentence()
        };
    }

    public static ProductDto CreateProductDto()
    {
        return new ProductDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = _faker.Commerce.ProductName(),
            Code = GenerateProductCode(),
            Description = _faker.Commerce.ProductDescription(),
            CategoryId = "default-category",
            CategoryName = _faker.Commerce.Categories(1).First(),
            UnitOfMeasure = _faker.PickRandom("Liters", "Kilograms", "Units"),
            Weight = _faker.Random.Decimal(0.1m, 100m),
            IsHazardous = false,
            Status = ProductStatus.Active.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string GenerateProductCode()
    {
        return $"PROD-{_faker.Random.AlphaNumeric(6).ToUpper()}";
    }

    private static string GenerateCategoryCode()
    {
        return $"CAT-{_faker.Random.AlphaNumeric(4).ToUpper()}";
    }
}
```

## Continuous Testing

### Test Execution Strategy

#### Test Categories and Execution
```bash
# Unit tests (fast, run on every commit)
dotnet test --filter Category=Unit

# Integration tests (medium speed, run on PR)
dotnet test --filter Category=Integration

# API tests (slower, run on PR and before deployment)
dotnet test --filter Category=API

# E2E tests (slowest, run on deployment pipeline)
dotnet test --filter Category=E2E

# Performance tests (run nightly)
dotnet test --filter Category=Performance

# Security tests (run weekly)
dotnet test --filter Category=Security
```

### Quality Gates

#### Test Coverage Requirements
```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <!-- Minimum test coverage thresholds -->
    <CoverageThreshold>80</CoverageThreshold>
    <BranchCoverageThreshold>70</BranchCoverageThreshold>
  </PropertyGroup>
</Project>
```

#### Automated Quality Checks
```csharp
public class QualityGateTests
{
    [Fact]
    public void CodeCoverage_MeetsMinimumThreshold()
    {
        // This test would integrate with coverage tools
        // to verify minimum coverage requirements
        var coverageReport = LoadCoverageReport();
        coverageReport.LineCoverage.Should().BeGreaterOrEqualTo(80);
        coverageReport.BranchCoverage.Should().BeGreaterOrEqualTo(70);
    }

    [Fact]
    public void PerformanceTests_MeetSLARequirements()
    {
        // Verify that performance tests meet SLA requirements
        var performanceResults = LoadPerformanceResults();
        performanceResults.AverageResponseTime.Should().BeLessOrEqualTo(TimeSpan.FromMilliseconds(500));
        performanceResults.PercentileP95.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(2));
        performanceResults.ErrorRate.Should().BeLessOrEqualTo(0.01); // < 1%
    }
}
```

---

*This testing guide provides comprehensive coverage of testing strategies, implementation patterns, and best practices for the Product Service. It should be updated as new testing requirements and technologies are adopted.*