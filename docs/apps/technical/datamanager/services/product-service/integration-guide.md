# Product Service Integration Guide

## Table of Contents

- [Overview](#overview)
- [Integration Patterns](#integration-patterns)
- [API Integration](#api-integration)
- [Database Integration](#database-integration)
- [Event-Driven Integration](#event-driven-integration)
- [Third-Party Integrations](#third-party-integrations)
- [Client SDKs and Libraries](#client-sdks-and-libraries)
- [Testing Integration](#testing-integration)
- [Error Handling and Resilience](#error-handling-and-resilience)
- [Security Considerations](#security-considerations)
- [Performance Optimization](#performance-optimization)
- [Monitoring and Observability](#monitoring-and-observability)

## Overview

The Product Service is designed to integrate seamlessly with other services in the QaliTrack ecosystem and external systems. This guide provides comprehensive information on integration patterns, best practices, and implementation examples for various integration scenarios.

### Integration Principles

1. **API-First Design**: RESTful APIs with comprehensive documentation
2. **Loose Coupling**: Minimal dependencies between services
3. **Contract-Based**: Well-defined interfaces and data contracts
4. **Fault Tolerance**: Resilient integration patterns
5. **Security**: Authentication and authorization across service boundaries
6. **Observability**: Comprehensive logging and monitoring
7. **Backward Compatibility**: Versioned APIs and graceful evolution

### Supported Integration Methods

- **Synchronous**: REST API calls
- **Asynchronous**: Message queues and events (future)
- **Database**: Direct database integration (discouraged)
- **File-Based**: Bulk data import/export
- **Real-Time**: WebSocket connections (future)

## Integration Patterns

### Service-to-Service Integration

#### Direct API Calls
```csharp
// Example: Customer Service integrating with Product Service
public class CustomerService
{
    private readonly HttpClient _productServiceClient;
    
    public async Task<CustomerDto> CreateCustomerOrderAsync(CreateOrderRequest request)
    {
        // Validate products exist before creating order
        foreach (var item in request.Items)
        {
            var product = await GetProductAsync(item.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product {item.ProductId} not found");
            }
        }
        
        // Continue with order creation
        return await CreateOrderAsync(request);
    }
    
    private async Task<ProductDto> GetProductAsync(string productId)
    {
        var response = await _productServiceClient.GetAsync($"/api/products/{productId}");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(json);
            return apiResponse.Data;
        }
        return null;
    }
}
```

#### Service Registry Pattern
```csharp
// Service discovery integration
public class ProductServiceClient
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly HttpClient _httpClient;
    
    public async Task<ProductDto> GetProductAsync(string id)
    {
        var serviceUrl = await _serviceRegistry.GetServiceUrlAsync("product-service");
        var response = await _httpClient.GetAsync($"{serviceUrl}/api/products/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(content);
            return result.Data;
        }
        
        throw new ServiceException($"Failed to retrieve product {id}");
    }
}
```

### Gateway Integration

#### API Gateway Configuration
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/products",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "product-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/products",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"]
    }
  ]
}
```

#### Gateway Request Headers
```csharp
// Gateway forwards authentication context
public class BaseController : ControllerBase
{
    protected string GetOrganizationId()
    {
        return HttpContext.Request.Headers["X-Organization-Id"].FirstOrDefault() ?? "default-org";
    }
    
    protected string GetUserId()
    {
        return HttpContext.Request.Headers["X-User-ID"].FirstOrDefault() ?? "system";
    }
    
    protected List<string> GetUserRoles()
    {
        var rolesHeader = HttpContext.Request.Headers["X-User-Roles"].FirstOrDefault();
        return rolesHeader?.Split(',').ToList() ?? new List<string>();
    }
}
```

## API Integration

### Authentication and Authorization

#### JWT Token Handling
```csharp
public class ProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenProvider _tokenProvider;
    
    public async Task<ApiResponseDto<ProductDto>> GetProductAsync(string id)
    {
        var token = await _tokenProvider.GetTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
            
        var response = await _httpClient.GetAsync($"/api/products/{id}");
        var content = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(content);
    }
}
```

#### Organization Context
```csharp
public class MultiTenantProductClient
{
    public async Task<List<ProductDto>> GetOrganizationProductsAsync(string organizationId)
    {
        _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId);
        
        var response = await _httpClient.GetAsync("/api/products");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDto<List<ProductDto>>>(content);
            return result.Data;
        }
        
        throw new IntegrationException("Failed to retrieve organization products");
    }
}
```

### Request/Response Patterns

#### Standard Request Pattern
```csharp
public class ProductServiceClient
{
    public async Task<ProductDto> CreateProductAsync(RegisterProductRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/api/products", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(responseJson);
            return result.Data;
        }
        
        await HandleErrorResponse(response);
        return null;
    }
    
    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ApiResponseDto>(errorContent);
        
        throw new ProductServiceException(errorResponse.Message, errorResponse.Errors);
    }
}
```

#### Batch Operations
```csharp
public class BulkProductClient
{
    public async Task<List<ProductDto>> CreateProductsBatchAsync(List<RegisterProductRequest> requests)
    {
        var results = new List<ProductDto>();
        var semaphore = new SemaphoreSlim(5); // Limit concurrent requests
        
        var tasks = requests.Select(async request =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await CreateProductAsync(request);
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        var responses = await Task.WhenAll(tasks);
        return responses.Where(r => r != null).ToList();
    }
}
```

### Error Handling Patterns

#### Retry Logic
```csharp
public class ResilientProductClient
{
    private readonly RetryPolicy _retryPolicy;
    
    public ResilientProductClient()
    {
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Log.Warning("Retry {RetryCount} for {Operation} after {Delay}ms", 
                        retryCount, context.OperationKey, timespan.TotalMilliseconds);
                });
    }
    
    public async Task<ProductDto> GetProductWithRetryAsync(string id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"/api/products/{id}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(content);
            return result.Data;
        });
    }
}
```

#### Circuit Breaker Pattern
```csharp
public class CircuitBreakerProductClient
{
    private readonly CircuitBreakerPolicy _circuitBreaker;
    
    public CircuitBreakerProductClient()
    {
        _circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    Log.Warning("Circuit breaker opened for {Duration}s due to {Exception}", 
                        duration.TotalSeconds, exception.Message);
                },
                onReset: () =>
                {
                    Log.Information("Circuit breaker reset");
                });
    }
    
    public async Task<ProductDto> GetProductAsync(string id)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await CallProductServiceAsync(id);
            });
        }
        catch (CircuitBreakerOpenException)
        {
            // Fallback to cached data or alternative service
            return await GetProductFromCacheAsync(id);
        }
    }
}
```

## Database Integration

### Direct Database Access (Discouraged)

While direct database access is possible, it's not recommended due to coupling concerns. However, for specific scenarios like reporting or data migration:

```csharp
public class ProductReportingService
{
    private readonly string _productDbConnectionString;
    
    public async Task<ProductSummaryReport> GenerateProductSummaryAsync()
    {
        using var connection = new SqlConnection(_productDbConnectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT 
                c.Name as CategoryName,
                COUNT(p.Id) as ProductCount,
                SUM(CASE WHEN p.IsHazardous = 1 THEN 1 ELSE 0 END) as HazardousCount
            FROM Products p
            INNER JOIN ProductCategories c ON p.CategoryId = c.Id
            WHERE p.IsDeleted = 0
            GROUP BY c.Name";
            
        var results = await connection.QueryAsync<ProductSummaryItem>(query);
        
        return new ProductSummaryReport
        {
            Categories = results.ToList(),
            GeneratedAt = DateTime.UtcNow
        };
    }
}
```

### Data Synchronization

#### Change Data Capture
```csharp
public class ProductChangeNotifier
{
    private readonly IEventPublisher _eventPublisher;
    
    public async Task OnProductUpdatedAsync(Product product)
    {
        var changeEvent = new ProductChangedEvent
        {
            ProductId = product.Id,
            ProductCode = product.Code,
            ChangeType = "Updated",
            ChangedAt = DateTime.UtcNow,
            ChangedFields = GetChangedFields(product)
        };
        
        await _eventPublisher.PublishAsync(changeEvent);
    }
}
```

## Event-Driven Integration

### Domain Events

#### Event Definition
```csharp
public class ProductRegisteredEvent
{
    public string ProductId { get; set; }
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public string CategoryId { get; set; }
    public bool IsHazardous { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string RegisteredBy { get; set; }
}

public class ProductStatusChangedEvent
{
    public string ProductId { get; set; }
    public string ProductCode { get; set; }
    public ProductStatus PreviousStatus { get; set; }
    public ProductStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; }
    public string Reason { get; set; }
}
```

#### Event Publishing
```csharp
public class ProductService : IProductService
{
    private readonly IEventBus _eventBus;
    
    public async Task<ProductDto> RegisterProductAsync(RegisterProductRequest request)
    {
        var product = await _productRepository.AddAsync(newProduct);
        
        // Publish domain event
        var productRegisteredEvent = new ProductRegisteredEvent
        {
            ProductId = product.Id,
            ProductCode = product.Code,
            ProductName = product.Name,
            CategoryId = product.CategoryId,
            IsHazardous = product.IsHazardous,
            RegisteredAt = product.CreatedAt,
            RegisteredBy = GetCurrentUserId()
        };
        
        await _eventBus.PublishAsync(productRegisteredEvent);
        
        return _mapper.Map<ProductDto>(product);
    }
}
```

#### Event Consumption
```csharp
public class InventoryService
{
    public async Task HandleAsync(ProductRegisteredEvent productRegistered)
    {
        // Create initial inventory record
        var inventory = new InventoryItem
        {
            ProductId = productRegistered.ProductId,
            ProductCode = productRegistered.ProductCode,
            CurrentStock = 0,
            MinimumStock = GetDefaultMinimumStock(productRegistered.CategoryId),
            CreatedAt = DateTime.UtcNow
        };
        
        await _inventoryRepository.AddAsync(inventory);
        
        Log.Information("Inventory record created for product {ProductCode}", 
            productRegistered.ProductCode);
    }
}
```

### Message Queue Integration

#### RabbitMQ Integration
```csharp
public class RabbitMQEventBus : IEventBus
{
    private readonly IConnection _connection;
    
    public async Task PublishAsync<T>(T @event) where T : class
    {
        using var channel = _connection.CreateModel();
        
        var exchangeName = typeof(T).Name;
        channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
        
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);
        
        channel.BasicPublish(
            exchange: exchangeName,
            routingKey: "",
            basicProperties: null,
            body: body);
            
        Log.Information("Published event {EventType} to exchange {Exchange}", 
            typeof(T).Name, exchangeName);
    }
}
```

#### Azure Service Bus Integration
```csharp
public class ServiceBusEventPublisher : IEventPublisher
{
    private readonly ServiceBusClient _serviceBusClient;
    
    public async Task PublishAsync<T>(T @event) where T : class
    {
        var topicName = typeof(T).Name.ToLowerInvariant();
        var sender = _serviceBusClient.CreateSender(topicName);
        
        var message = new ServiceBusMessage(JsonSerializer.Serialize(@event))
        {
            ContentType = "application/json",
            Subject = typeof(T).Name,
            MessageId = Guid.NewGuid().ToString()
        };
        
        await sender.SendMessageAsync(message);
        
        Log.Information("Published event {EventType} to topic {Topic}", 
            typeof(T).Name, topicName);
    }
}
```

## Third-Party Integrations

### ERP System Integration

#### SAP Integration
```csharp
public class SapProductSynchronizer
{
    private readonly ISapClient _sapClient;
    private readonly IProductService _productService;
    
    public async Task SynchronizeProductsAsync()
    {
        var sapProducts = await _sapClient.GetProductsAsync();
        
        foreach (var sapProduct in sapProducts)
        {
            var existingProduct = await _productService.GetProductByCodeAsync(sapProduct.MaterialNumber);
            
            if (existingProduct == null)
            {
                await CreateProductFromSap(sapProduct);
            }
            else
            {
                await UpdateProductFromSap(existingProduct, sapProduct);
            }
        }
    }
    
    private async Task CreateProductFromSap(SapMaterial sapProduct)
    {
        var request = new RegisterProductRequest
        {
            Name = sapProduct.Description,
            Code = sapProduct.MaterialNumber,
            CategoryId = await MapSapCategoryAsync(sapProduct.MaterialGroup),
            UnitOfMeasure = sapProduct.BaseUnitOfMeasure,
            Weight = sapProduct.GrossWeight,
            IsHazardous = sapProduct.HazardousMaterial,
            HazmatClass = sapProduct.HazmatClass
        };
        
        await _productService.RegisterProductAsync(request);
        
        Log.Information("Created product {ProductCode} from SAP", sapProduct.MaterialNumber);
    }
}
```

#### Oracle ERP Integration
```csharp
public class OracleProductImporter
{
    private readonly OracleConnection _oracleConnection;
    private readonly IProductService _productService;
    
    public async Task ImportProductsAsync()
    {
        var query = @"
            SELECT 
                INVENTORY_ITEM_ID,
                SEGMENT1 as ITEM_NUMBER,
                DESCRIPTION,
                PRIMARY_UOM_CODE,
                ITEM_TYPE,
                HAZARD_CLASS
            FROM MTL_SYSTEM_ITEMS_B 
            WHERE ENABLED_FLAG = 'Y'
            AND ORGANIZATION_ID = :orgId";
            
        var products = await _oracleConnection.QueryAsync(query, new { orgId = 101 });
        
        foreach (var product in products)
        {
            await ImportSingleProduct(product);
        }
    }
}
```

### Compliance System Integration

#### Regulatory Database Integration
```csharp
public class ComplianceDataSynchronizer
{
    private readonly IComplianceApiClient _complianceClient;
    private readonly IProductService _productService;
    
    public async Task UpdateComplianceDataAsync()
    {
        var hazardousProducts = await _productService.GetHazardousProductsAsync();
        
        foreach (var product in hazardousProducts)
        {
            var complianceInfo = await _complianceClient.GetComplianceInfoAsync(product.Code);
            
            if (complianceInfo != null)
            {
                await UpdateProductCompliance(product.Id, complianceInfo);
            }
        }
    }
    
    private async Task UpdateProductCompliance(string productId, ComplianceInfo info)
    {
        var compliance = new ProductComplianceDto
        {
            ProductId = productId,
            ComplianceType = "DOT",
            Regulation = info.Regulation,
            Authority = info.Authority,
            CertificationNumber = info.CertNumber,
            ExpirationDate = info.ExpirationDate
        };
        
        await _productService.UpdateProductComplianceAsync(productId, compliance);
    }
}
```

### Pricing System Integration

#### External Pricing Service
```csharp
public class ExternalPricingIntegration
{
    private readonly IPricingServiceClient _pricingClient;
    private readonly IProductService _productService;
    
    public async Task SynchronizePricingAsync()
    {
        var products = await _productService.GetAllProductsAsync();
        
        var pricingTasks = products.Select(async product =>
        {
            var pricing = await _pricingClient.GetPricingAsync(product.Code);
            if (pricing != null)
            {
                await UpdateProductPricing(product.Id, pricing);
            }
        });
        
        await Task.WhenAll(pricingTasks);
    }
    
    private async Task UpdateProductPricing(string productId, ExternalPricing pricing)
    {
        var pricingRequest = new UpdateProductPricingRequest
        {
            PricingRules = pricing.Rules.Select(rule => new ProductPricingDto
            {
                UnitPrice = rule.Price,
                Currency = rule.Currency,
                MinimumQuantity = rule.MinQty,
                MaximumQuantity = rule.MaxQty,
                CustomerGroup = rule.CustomerTier,
                EffectiveDate = rule.EffectiveDate
            }).ToList()
        };
        
        await _productService.UpdateProductPricingAsync(productId, pricingRequest);
    }
}
```

## Client SDKs and Libraries

### .NET Client SDK

#### Product Service Client
```csharp
public interface IProductServiceClient
{
    Task<ProductDto> GetProductAsync(string id);
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto> CreateProductAsync(RegisterProductRequest request);
    Task<ProductDto> UpdateProductAsync(string id, UpdateProductRequest request);
    Task DeleteProductAsync(string id);
    Task<List<ProductDto>> SearchProductsAsync(string searchTerm);
}

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ProductServiceClientOptions _options;
    
    public ProductServiceClient(HttpClient httpClient, ProductServiceClientOptions options)
    {
        _httpClient = httpClient;
        _options = options;
        
        _httpClient.BaseAddress = new Uri(options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ProductServiceClient/1.0");
    }
    
    public async Task<ProductDto> GetProductAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/api/products/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDto<ProductDto>>(content);
            return result.Data;
        }
        
        throw await CreateExceptionFromResponse(response);
    }
    
    private async Task<ProductServiceException> CreateExceptionFromResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ApiResponseDto>(content);
        
        return new ProductServiceException(response.StatusCode, errorResponse.Message, errorResponse.Errors);
    }
}
```

#### Client Configuration
```csharp
public class ProductServiceClientOptions
{
    public string BaseUrl { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetryAttempts { get; set; } = 3;
    public bool EnableCircuitBreaker { get; set; } = true;
}

// DI Registration
services.AddHttpClient<IProductServiceClient, ProductServiceClient>()
    .ConfigureHttpClient((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<ProductServiceClientOptions>();
        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = options.Timeout;
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

### JavaScript/TypeScript Client

#### TypeScript Client
```typescript
interface ProductDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  categoryId: string;
  categoryName?: string;
  unitOfMeasure: string;
  weight?: number;
  density?: number;
  isHazardous: boolean;
  hazmatClass?: string;
  requiresSpecialHandling: boolean;
  status: string;
  notes?: string;
  createdAt: Date;
  updatedAt: Date;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

class ProductServiceClient {
  private baseUrl: string;
  private defaultHeaders: Record<string, string>;
  
  constructor(baseUrl: string, authToken?: string) {
    this.baseUrl = baseUrl.replace(/\/$/, '');
    this.defaultHeaders = {
      'Content-Type': 'application/json',
      ...(authToken && { 'Authorization': `Bearer ${authToken}` })
    };
  }
  
  async getProduct(id: string): Promise<ProductDto> {
    const response = await fetch(`${this.baseUrl}/api/products/${id}`, {
      headers: this.defaultHeaders
    });
    
    if (!response.ok) {
      throw new Error(`Failed to get product: ${response.statusText}`);
    }
    
    const result: ApiResponse<ProductDto> = await response.json();
    if (!result.success) {
      throw new Error(result.message);
    }
    
    return result.data;
  }
  
  async createProduct(request: RegisterProductRequest): Promise<ProductDto> {
    const response = await fetch(`${this.baseUrl}/api/products`, {
      method: 'POST',
      headers: this.defaultHeaders,
      body: JSON.stringify(request)
    });
    
    if (!response.ok) {
      const errorResult: ApiResponse<any> = await response.json();
      throw new Error(errorResult.message || 'Failed to create product');
    }
    
    const result: ApiResponse<ProductDto> = await response.json();
    return result.data;
  }
  
  async searchProducts(searchTerm: string): Promise<ProductDto[]> {
    const url = `${this.baseUrl}/api/products/search?searchTerm=${encodeURIComponent(searchTerm)}`;
    const response = await fetch(url, {
      headers: this.defaultHeaders
    });
    
    if (!response.ok) {
      throw new Error(`Search failed: ${response.statusText}`);
    }
    
    const result: ApiResponse<ProductDto[]> = await response.json();
    return result.data;
  }
}
```

#### React Hook Integration
```typescript
import { useState, useEffect } from 'react';

export function useProduct(productId: string) {
  const [product, setProduct] = useState<ProductDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  useEffect(() => {
    let cancelled = false;
    
    async function fetchProduct() {
      try {
        setLoading(true);
        setError(null);
        
        const client = new ProductServiceClient(process.env.REACT_APP_API_BASE_URL!);
        const productData = await client.getProduct(productId);
        
        if (!cancelled) {
          setProduct(productData);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Failed to fetch product');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }
    
    fetchProduct();
    
    return () => {
      cancelled = true;
    };
  }, [productId]);
  
  return { product, loading, error };
}
```

## Testing Integration

### Contract Testing

#### Pact Consumer Test
```csharp
[Fact]
public async Task GetProduct_ReturnsValidProduct()
{
    // Arrange
    var productId = "test-product-123";
    var expectedProduct = new ProductDto
    {
        Id = productId,
        Name = "Test Product",
        Code = "TEST-001",
        CategoryId = "cat-001"
    };
    
    _pact
        .UponReceiving("a request for a product")
        .Given($"product {productId} exists")
        .WithRequest(HttpMethod.Get, $"/api/products/{productId}")
        .WithHeader("Authorization", Match.Regex("Bearer .*"))
        .WillRespondWith()
        .WithStatus(HttpStatusCode.OK)
        .WithHeader("Content-Type", "application/json")
        .WithJsonBody(Match.Type(new
        {
            success = true,
            data = Match.Type(expectedProduct)
        }));
    
    // Act
    var result = await _productClient.GetProductAsync(productId);
    
    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(productId);
    result.Name.Should().Be("Test Product");
}
```

#### Provider Verification
```csharp
[Fact]
public void VerifyProductServiceContracts()
{
    var config = new PactVerifierConfig
    {
        Outputters = new List<IOutput> { new ConsoleOutput() },
        LogLevel = PactLogLevel.Information
    };
    
    IPactVerifier pactVerifier = new PactVerifier(config);
    
    pactVerifier
        .ServiceProvider("ProductService", new Uri("http://localhost:7005"))
        .HonoursPactWith("CustomerService")
        .PactUri("path/to/customer-service-product-service.json")
        .Verify();
}
```

### Integration Testing

#### API Integration Tests
```csharp
public class ProductApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public ProductApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsCreatedProduct()
    {
        // Arrange
        var request = new RegisterProductRequest
        {
            Name = "Integration Test Product",
            Code = "INT-TEST-001",
            CategoryId = "test-category",
            UnitOfMeasure = "Units"
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
        result.Success.Should().BeTrue();
        result.Data.Name.Should().Be(request.Name);
        result.Data.Code.Should().Be(request.Code);
    }
}
```

### Load Testing

#### Performance Testing with NBomber
```csharp
public class ProductServiceLoadTests
{
    [Fact]
    public void LoadTest_GetProducts_PerformanceCheck()
    {
        var scenario = Scenario.Create("get_products", async context =>
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:7005/api/products");
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(5))
        );
        
        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}
```

## Error Handling and Resilience

### Retry Strategies

#### Exponential Backoff
```csharp
public class RetryableProductClient
{
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    
    public RetryableProductClient()
    {
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Log.Warning("Retry {RetryCount} after {Delay}ms", 
                        retryCount, timespan.TotalMilliseconds);
                });
    }
}
```

### Circuit Breaker

#### Implementation with Polly
```csharp
public class CircuitBreakerConfiguration
{
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, duration) =>
                {
                    Log.Warning("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
                },
                onReset: () =>
                {
                    Log.Information("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    Log.Information("Circuit breaker half-open");
                });
    }
}
```

### Fallback Mechanisms

#### Cache Fallback
```csharp
public class FallbackProductService
{
    private readonly IProductServiceClient _primaryClient;
    private readonly IMemoryCache _cache;
    
    public async Task<ProductDto> GetProductWithFallbackAsync(string id)
    {
        try
        {
            var product = await _primaryClient.GetProductAsync(id);
            _cache.Set($"product:{id}", product, TimeSpan.FromMinutes(15));
            return product;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Primary service failed, attempting fallback for product {ProductId}", id);
            
            if (_cache.TryGetValue($"product:{id}", out ProductDto cachedProduct))
            {
                Log.Information("Returned cached product {ProductId}", id);
                return cachedProduct;
            }
            
            throw;
        }
    }
}
```

## Security Considerations

### API Security

#### Input Validation
```csharp
public class SecureProductController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] RegisterProductRequest request)
    {
        // Validate input format
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        // Check authorization
        if (!IsOperatorOrHigher())
        {
            return Forbidden("Insufficient permissions to create products");
        }
        
        // Sanitize input
        request.Name = SanitizeInput(request.Name);
        request.Code = SanitizeProductCode(request.Code);
        
        var result = await _productService.RegisterProductAsync(request);
        return Ok(ApiResponseDto<ProductDto>.SuccessResponse(result));
    }
    
    private string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        // Remove dangerous characters
        return Regex.Replace(input, @"[<>""'%;()&+]", "");
    }
    
    private string SanitizeProductCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return code;
        
        // Allow only alphanumeric characters and hyphens
        return Regex.Replace(code, @"[^A-Z0-9\-]", "");
    }
}
```

#### Rate Limiting
```csharp
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var requestKey = $"rate_limit:{clientId}";
        
        if (_cache.TryGetValue(requestKey, out int requestCount))
        {
            if (requestCount >= 100) // 100 requests per minute
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Rate limit exceeded");
                return;
            }
            
            _cache.Set(requestKey, requestCount + 1, TimeSpan.FromMinutes(1));
        }
        else
        {
            _cache.Set(requestKey, 1, TimeSpan.FromMinutes(1));
        }
        
        await _next(context);
    }
}
```

### Data Protection

#### Sensitive Data Handling
```csharp
public class DataProtectionService
{
    private readonly IDataProtector _protector;
    
    public DataProtectionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("ProductService.SensitiveData");
    }
    
    public string ProtectSensitiveInfo(string sensitiveData)
    {
        return _protector.Protect(sensitiveData);
    }
    
    public string UnprotectSensitiveInfo(string protectedData)
    {
        return _protector.Unprotect(protectedData);
    }
}
```

## Performance Optimization

### Caching Strategies

#### Distributed Caching
```csharp
public class CachedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly IDistributedCache _cache;
    
    public async Task<ProductDto> GetProductByIdAsync(string id)
    {
        var cacheKey = $"product:{id}";
        var cachedProduct = await _cache.GetStringAsync(cacheKey);
        
        if (cachedProduct != null)
        {
            return JsonSerializer.Deserialize<ProductDto>(cachedProduct);
        }
        
        var product = await _inner.GetProductByIdAsync(id);
        if (product != null)
        {
            var serializedProduct = JsonSerializer.Serialize(product);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            };
            
            await _cache.SetStringAsync(cacheKey, serializedProduct, options);
        }
        
        return product;
    }
}
```

### Connection Pooling

#### HTTP Client Pool
```csharp
public class HttpClientConfiguration
{
    public static void ConfigureHttpClients(IServiceCollection services)
    {
        services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
        {
            client.BaseAddress = new Uri("http://product-service");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            MaxConnectionsPerServer = 20
        });
    }
}
```

### Batch Processing

#### Bulk Operations
```csharp
public class BulkProductProcessor
{
    private readonly IProductService _productService;
    private readonly SemaphoreSlim _semaphore;
    
    public BulkProductProcessor(IProductService productService)
    {
        _productService = productService;
        _semaphore = new SemaphoreSlim(10); // Limit concurrent operations
    }
    
    public async Task<List<ProductDto>> ProcessProductsBatchAsync(
        List<RegisterProductRequest> requests)
    {
        var tasks = requests.Select(async request =>
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _productService.RegisterProductAsync(request);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process product {ProductCode}", request.Code);
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        });
        
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).ToList();
    }
}
```

## Monitoring and Observability

### Distributed Tracing

#### OpenTelemetry Integration
```csharp
public class TracingConfiguration
{
    public static void ConfigureTracing(IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddJaegerExporter();
            });
    }
}
```

#### Custom Tracing
```csharp
public class TracedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly ActivitySource _activitySource;
    
    public TracedProductService(IProductService inner)
    {
        _inner = inner;
        _activitySource = new ActivitySource("ProductService");
    }
    
    public async Task<ProductDto> GetProductByIdAsync(string id)
    {
        using var activity = _activitySource.StartActivity("GetProductById");
        activity?.SetTag("product.id", id);
        
        try
        {
            var result = await _inner.GetProductByIdAsync(id);
            activity?.SetTag("product.found", result != null);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

### Metrics Collection

#### Custom Metrics
```csharp
public class ProductServiceMetrics
{
    private readonly Counter<int> _productCreatedCounter;
    private readonly Histogram<double> _requestDuration;
    
    public ProductServiceMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("ProductService");
        
        _productCreatedCounter = meter.CreateCounter<int>(
            "products_created_total",
            description: "Total number of products created");
            
        _requestDuration = meter.CreateHistogram<double>(
            "request_duration_ms",
            description: "Request duration in milliseconds");
    }
    
    public void RecordProductCreated(string category)
    {
        _productCreatedCounter.Add(1, new TagList { { "category", category } });
    }
    
    public void RecordRequestDuration(double durationMs, string endpoint)
    {
        _requestDuration.Record(durationMs, new TagList { { "endpoint", endpoint } });
    }
}
```

---

*This integration guide provides comprehensive patterns and examples for integrating with the Product Service. It should be updated as new integration scenarios and technologies are adopted.*