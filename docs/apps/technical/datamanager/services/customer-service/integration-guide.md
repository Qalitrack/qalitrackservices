# Customer Service Integration Guide

## Overview

The Customer Service provides comprehensive customer relationship management capabilities within the QaliTrack ecosystem. This guide covers integration patterns, API usage, authentication methods, and best practices for consuming the Customer Service from other services and external applications.

## Integration Architecture

### Service Communication Patterns

#### Synchronous Communication

**HTTP REST API:**
- Direct API calls for real-time data access
- Request-response pattern with immediate results
- Suitable for CRUD operations and data queries
- Timeout handling and retry mechanisms required

**Use Cases:**
- Customer lookup during weighbridge operations
- Real-time credit limit verification
- Contact information retrieval
- Contract status validation

#### Asynchronous Communication

**Event-Driven Integration:**
- Message bus for event notifications
- Eventual consistency for data synchronization
- Decoupled service communication
- Scalable for high-volume operations

**Use Cases:**
- Customer creation notifications
- Contract expiration alerts
- Credit limit change notifications
- Payment status updates

### Integration Patterns

#### API Gateway Pattern

**Centralized Access Point:**
```
External System → API Gateway → Customer Service
                              ↓
                          Authentication
                          Rate Limiting
                          Load Balancing
```

**Gateway Configuration Example:**
```yaml
routes:
  - match: /api/customers/**
    backend: customer-service:7003
    policies:
      - authentication
      - rate-limiting
      - circuit-breaker
```

#### Service Mesh Pattern

**Direct Service-to-Service Communication:**
```
Service A ←→ Service Mesh ←→ Customer Service
           ↓
       Security
       Observability
       Traffic Management
```

## Authentication and Authorization

### Header-Based Authentication

#### Required Headers

All API requests must include organization and user context:

```http
X-Organization-Id: {organization-uuid}
X-User-Id: {user-uuid}
Content-Type: application/json
Accept: application/json
```

**Header Validation:**
- Organization ID validates data access scope
- User ID provides audit trail for operations
- Missing headers result in 401 Unauthorized response

#### Implementation Example

**C# Client Implementation:**
```csharp
public class CustomerServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _organizationId;
    private readonly string _userId;

    public CustomerServiceClient(
        HttpClient httpClient,
        string organizationId,
        string userId)
    {
        _httpClient = httpClient;
        _organizationId = organizationId;
        _userId = userId;
        
        ConfigureHeaders();
    }

    private void ConfigureHeaders()
    {
        _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", _organizationId);
        _httpClient.DefaultRequestHeaders.Add("X-User-Id", _userId);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<CustomerDto?> GetCustomerAsync(string customerId)
    {
        var response = await _httpClient.GetAsync($"api/customers/{customerId}");
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<CustomerDto>>(json);
            return apiResponse?.Data;
        }
        
        return null;
    }
}
```

**JavaScript Client Implementation:**
```javascript
class CustomerServiceClient {
    constructor(baseUrl, organizationId, userId) {
        this.baseUrl = baseUrl;
        this.organizationId = organizationId;
        this.userId = userId;
    }

    async getCustomer(customerId) {
        const response = await fetch(`${this.baseUrl}/api/customers/${customerId}`, {
            headers: {
                'X-Organization-Id': this.organizationId,
                'X-User-Id': this.userId,
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            }
        });

        if (response.ok) {
            const apiResponse = await response.json();
            return apiResponse.data;
        }

        throw new Error(`Failed to get customer: ${response.status} ${response.statusText}`);
    }
}
```

### Service-to-Service Authentication

#### JWT Token Authentication (Future Enhancement)

**Token-Based Authentication:**
```csharp
public class JwtAuthenticationHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
```

#### Mutual TLS (mTLS) Authentication

**Certificate-Based Authentication:**
```csharp
public class CustomerServiceClient
{
    private readonly HttpClient _httpClient;

    public CustomerServiceClient(X509Certificate2 clientCertificate)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(clientCertificate);
        
        _httpClient = new HttpClient(handler);
    }
}
```

## API Integration Patterns

### CRUD Operations

#### Customer Management

**Create Customer:**
```csharp
public async Task<CustomerDto> CreateCustomerAsync(RegisterCustomerRequest request)
{
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync("api/customers", content);
    response.EnsureSuccessStatusCode();
    
    var responseJson = await response.Content.ReadAsStringAsync();
    var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<CustomerDto>>(responseJson);
    
    return apiResponse.Data;
}
```

**Update Customer:**
```csharp
public async Task<CustomerDto> UpdateCustomerAsync(string customerId, UpdateCustomerRequest request)
{
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PutAsync($"api/customers/{customerId}", content);
    response.EnsureSuccessStatusCode();
    
    var responseJson = await response.Content.ReadAsStringAsync();
    var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<CustomerDto>>(responseJson);
    
    return apiResponse.Data;
}
```

**Delete Customer:**
```csharp
public async Task<bool> DeleteCustomerAsync(string customerId)
{
    var response = await _httpClient.DeleteAsync($"api/customers/{customerId}");
    return response.IsSuccessStatusCode;
}
```

#### Contact Management

**Add Customer Contact:**
```csharp
public async Task<CustomerContactDto> AddContactAsync(
    string customerId, 
    CreateCustomerContactRequest request)
{
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync($"api/customers/{customerId}/contacts", content);
    response.EnsureSuccessStatusCode();
    
    var responseJson = await response.Content.ReadAsStringAsync();
    var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<CustomerContactDto>>(responseJson);
    
    return apiResponse.Data;
}
```

### Search and Query Operations

#### Customer Search

**Basic Customer Search:**
```csharp
public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(
    string searchTerm, 
    int page = 1, 
    int pageSize = 20)
{
    var queryString = $"?search={Uri.EscapeDataString(searchTerm)}&page={page}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync($"api/customers{queryString}");
    
    response.EnsureSuccessStatusCode();
    
    var responseJson = await response.Content.ReadAsStringAsync();
    var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<IEnumerable<CustomerDto>>>(responseJson);
    
    return apiResponse.Data ?? new List<CustomerDto>();
}
```

**Advanced Query Builder:**
```csharp
public class CustomerQueryBuilder
{
    private readonly List<string> _filters = new();
    private int _page = 1;
    private int _pageSize = 20;

    public CustomerQueryBuilder WithSearch(string searchTerm)
    {
        if (!string.IsNullOrEmpty(searchTerm))
            _filters.Add($"search={Uri.EscapeDataString(searchTerm)}");
        return this;
    }

    public CustomerQueryBuilder WithStatus(CustomerStatus status)
    {
        _filters.Add($"status={status}");
        return this;
    }

    public CustomerQueryBuilder WithType(CustomerType type)
    {
        _filters.Add($"type={type}");
        return this;
    }

    public CustomerQueryBuilder WithPagination(int page, int pageSize)
    {
        _page = page;
        _pageSize = pageSize;
        return this;
    }

    public string Build()
    {
        _filters.Add($"page={_page}");
        _filters.Add($"pageSize={_pageSize}");
        return $"?{string.Join("&", _filters)}";
    }
}

// Usage
var query = new CustomerQueryBuilder()
    .WithSearch("Acme")
    .WithStatus(CustomerStatus.Active)
    .WithType(CustomerType.Corporate)
    .WithPagination(1, 10)
    .Build();

var customers = await SearchCustomersAsync(query);
```

### Bulk Operations

#### Batch Customer Operations

**Bulk Customer Import:**
```csharp
public async Task<BulkOperationResult> BulkCreateCustomersAsync(
    IEnumerable<RegisterCustomerRequest> requests)
{
    var results = new List<CustomerDto>();
    var errors = new List<string>();

    foreach (var request in requests)
    {
        try
        {
            var customer = await CreateCustomerAsync(request);
            results.Add(customer);
        }
        catch (Exception ex)
        {
            errors.Add($"Failed to create customer {request.Name}: {ex.Message}");
        }
    }

    return new BulkOperationResult
    {
        SuccessCount = results.Count,
        ErrorCount = errors.Count,
        Results = results,
        Errors = errors
    };
}
```

**Parallel Processing:**
```csharp
public async Task<BulkOperationResult> BulkCreateCustomersParallelAsync(
    IEnumerable<RegisterCustomerRequest> requests)
{
    var semaphore = new SemaphoreSlim(5); // Limit concurrent requests
    var tasks = requests.Select(async request =>
    {
        await semaphore.WaitAsync();
        try
        {
            return await CreateCustomerAsync(request);
        }
        finally
        {
            semaphore.Release();
        }
    });

    var results = await Task.WhenAll(tasks);
    return new BulkOperationResult { Results = results };
}
```

## Error Handling and Resilience

### HTTP Status Code Handling

#### Standard Response Processing

**Response Handler:**
```csharp
public async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
{
    var responseJson = await response.Content.ReadAsStringAsync();
    
    if (response.IsSuccessStatusCode)
    {
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<T>>(responseJson);
        return apiResponse.Data;
    }
    
    var errorResponse = JsonSerializer.Deserialize<ApiResponseDto>(responseJson);
    
    throw response.StatusCode switch
    {
        HttpStatusCode.BadRequest => new ValidationException(errorResponse.Message, errorResponse.Errors),
        HttpStatusCode.NotFound => new EntityNotFoundException(errorResponse.Message),
        HttpStatusCode.Conflict => new BusinessRuleException(errorResponse.Message),
        HttpStatusCode.Unauthorized => new UnauthorizedException(errorResponse.Message),
        HttpStatusCode.Forbidden => new ForbiddenException(errorResponse.Message),
        _ => new ServiceException($"Unexpected error: {response.StatusCode}")
    };
}
```

### Retry Policies

#### Exponential Backoff Retry

**Polly Integration:**
```csharp
public class ResilientCustomerServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public ResilientCustomerServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timespan}s");
                });
    }

    public async Task<CustomerDto?> GetCustomerWithRetryAsync(string customerId)
    {
        var response = await _retryPolicy.ExecuteAsync(async () =>
            await _httpClient.GetAsync($"api/customers/{customerId}"));

        return await HandleResponseAsync<CustomerDto>(response);
    }
}
```

#### Circuit Breaker Pattern

**Circuit Breaker Implementation:**
```csharp
public class CustomerServiceCircuitBreaker
{
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;

    public CustomerServiceCircuitBreaker()
    {
        _circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (result, duration) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {duration}");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset");
                });
    }

    public async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> operation)
    {
        return await _circuitBreakerPolicy.ExecuteAsync(operation);
    }
}
```

### Timeout Management

#### Configurable Timeouts

**Timeout Configuration:**
```csharp
public class CustomerServiceClient
{
    private readonly HttpClient _httpClient;

    public CustomerServiceClient(TimeSpan? timeout = null)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    public async Task<CustomerDto?> GetCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10)); // Operation-specific timeout

        try
        {
            var response = await _httpClient.GetAsync($"api/customers/{customerId}", cts.Token);
            return await HandleResponseAsync<CustomerDto>(response);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException("Customer lookup timed out");
        }
    }
}
```

## Caching Strategies

### Client-Side Caching

#### In-Memory Caching

**Memory Cache Implementation:**
```csharp
public class CachedCustomerServiceClient
{
    private readonly CustomerServiceClient _client;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);

    public async Task<CustomerDto?> GetCustomerAsync(string customerId)
    {
        var cacheKey = $"customer_{customerId}";
        
        if (_cache.TryGetValue(cacheKey, out CustomerDto cachedCustomer))
            return cachedCustomer;

        var customer = await _client.GetCustomerAsync(customerId);
        if (customer != null)
        {
            _cache.Set(cacheKey, customer, _cacheDuration);
        }

        return customer;
    }

    public async Task InvalidateCustomerCacheAsync(string customerId)
    {
        _cache.Remove($"customer_{customerId}");
    }
}
```

#### Distributed Caching

**Redis Cache Implementation:**
```csharp
public class DistributedCachedCustomerServiceClient
{
    private readonly CustomerServiceClient _client;
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public async Task<CustomerDto?> GetCustomerAsync(string customerId)
    {
        var cacheKey = $"customer_{customerId}";
        var cachedJson = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedJson))
        {
            return JsonSerializer.Deserialize<CustomerDto>(cachedJson);
        }

        var customer = await _client.GetCustomerAsync(customerId);
        if (customer != null)
        {
            var json = JsonSerializer.Serialize(customer);
            await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration
            });
        }

        return customer;
    }
}
```

### Cache Invalidation Strategies

#### Event-Based Invalidation

**Event Subscription:**
```csharp
public class EventDrivenCacheInvalidation
{
    private readonly IMemoryCache _cache;
    private readonly IEventBus _eventBus;

    public EventDrivenCacheInvalidation(IMemoryCache cache, IEventBus eventBus)
    {
        _cache = cache;
        _eventBus = eventBus;
        
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<CustomerUpdatedEvent>(OnCustomerUpdated);
        _eventBus.Subscribe<CustomerDeletedEvent>(OnCustomerDeleted);
        _eventBus.Subscribe<ContactUpdatedEvent>(OnContactUpdated);
    }

    private void OnCustomerUpdated(CustomerUpdatedEvent eventArgs)
    {
        _cache.Remove($"customer_{eventArgs.CustomerId}");
        _cache.Remove($"customer_details_{eventArgs.CustomerId}");
    }

    private void OnCustomerDeleted(CustomerDeletedEvent eventArgs)
    {
        _cache.Remove($"customer_{eventArgs.CustomerId}");
    }

    private void OnContactUpdated(ContactUpdatedEvent eventArgs)
    {
        _cache.Remove($"customer_details_{eventArgs.CustomerId}");
        _cache.Remove($"customer_contacts_{eventArgs.CustomerId}");
    }
}
```

## Event-Driven Integration

### Event Publishing

#### Customer Events

**Event Definitions:**
```csharp
public class CustomerRegisteredEvent
{
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string ContactEmail { get; set; }
    public CustomerType CustomerType { get; set; }
    public string OrganizationId { get; set; }
    public DateTime RegisteredAt { get; set; }
}

public class CustomerUpdatedEvent
{
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public CustomerStatus PreviousStatus { get; set; }
    public CustomerStatus CurrentStatus { get; set; }
    public string OrganizationId { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ContractCreatedEvent
{
    public string ContractId { get; set; }
    public string CustomerId { get; set; }
    public string ContractNumber { get; set; }
    public ContractType ContractType { get; set; }
    public decimal ContractValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Event Consumption

#### Event Handlers

**Customer Event Handler:**
```csharp
public class CustomerEventHandler
{
    private readonly ILogger<CustomerEventHandler> _logger;
    private readonly IUserNotificationService _notificationService;

    public async Task Handle(CustomerRegisteredEvent eventArgs)
    {
        _logger.LogInformation("Processing customer registration event for {CustomerId}", 
                              eventArgs.CustomerId);

        // Send welcome notification
        await _notificationService.SendWelcomeEmailAsync(
            eventArgs.ContactEmail, 
            eventArgs.CustomerName);

        // Update analytics
        await UpdateCustomerAnalyticsAsync(eventArgs);
    }

    public async Task Handle(ContractCreatedEvent eventArgs)
    {
        _logger.LogInformation("Processing contract creation event for {ContractId}", 
                              eventArgs.ContractId);

        // Update financial projections
        await UpdateRevenueProjectionsAsync(eventArgs);

        // Schedule contract reviews
        await ScheduleContractReviewsAsync(eventArgs);
    }
}
```

#### Message Bus Integration

**RabbitMQ Integration:**
```csharp
public class RabbitMQEventConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly CustomerEventHandler _eventHandler;

    public RabbitMQEventConsumer(IConnection connection, CustomerEventHandler eventHandler)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        _eventHandler = eventHandler;
        
        SetupQueues();
    }

    private void SetupQueues()
    {
        _channel.QueueDeclare(queue: "customer.events", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: "customer.events", exchange: "qalitrack.events", routingKey: "customer.*");
    }

    public void StartConsuming()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                await ProcessMessageAsync(ea.RoutingKey, message);
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message: {Message}", message);
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: "customer.events", autoAck: false, consumer: consumer);
    }

    private async Task ProcessMessageAsync(string routingKey, string message)
    {
        switch (routingKey)
        {
            case "customer.registered":
                var registeredEvent = JsonSerializer.Deserialize<CustomerRegisteredEvent>(message);
                await _eventHandler.Handle(registeredEvent);
                break;
                
            case "customer.updated":
                var updatedEvent = JsonSerializer.Deserialize<CustomerUpdatedEvent>(message);
                await _eventHandler.Handle(updatedEvent);
                break;
        }
    }
}
```

## Integration Examples by Use Case

### Weighbridge Operations Integration

#### Customer Verification

**Real-Time Customer Lookup:**
```csharp
public class WeighbridgeCustomerService
{
    private readonly CustomerServiceClient _customerClient;
    private readonly ILogger<WeighbridgeCustomerService> _logger;

    public async Task<CustomerValidationResult> ValidateCustomerAsync(string identifier)
    {
        try
        {
            // Try multiple lookup methods
            var customer = await TryGetCustomerByEmailAsync(identifier) ??
                          await TryGetCustomerByTaxNumberAsync(identifier) ??
                          await TryGetCustomerByIdAsync(identifier);

            if (customer == null)
                return CustomerValidationResult.NotFound();

            if (customer.Status != CustomerStatus.Active)
                return CustomerValidationResult.Inactive(customer.Status);

            // Verify credit status
            var creditStatus = await VerifyCreditStatusAsync(customer.Id);
            if (!creditStatus.IsValid)
                return CustomerValidationResult.CreditIssue(creditStatus.Reason);

            return CustomerValidationResult.Valid(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Customer validation failed for identifier: {Identifier}", identifier);
            return CustomerValidationResult.Error("Customer validation service unavailable");
        }
    }

    private async Task<CustomerDto?> TryGetCustomerByEmailAsync(string email)
    {
        if (!email.Contains("@")) return null;
        return await _customerClient.GetCustomerByEmailAsync(email);
    }

    private async Task<CreditStatus> VerifyCreditStatusAsync(string customerId)
    {
        var credit = await _customerClient.GetCustomerCreditAsync(customerId);
        if (credit == null)
            return CreditStatus.Invalid("No credit information available");

        if (credit.AvailableCredit <= 0)
            return CreditStatus.Invalid("Credit limit exceeded");

        return CreditStatus.Valid();
    }
}
```

### Billing System Integration

#### Invoice Generation

**Customer Billing Integration:**
```csharp
public class BillingIntegrationService
{
    private readonly CustomerServiceClient _customerClient;
    private readonly IInvoiceGenerator _invoiceGenerator;

    public async Task<Invoice> CreateInvoiceAsync(string customerId, IEnumerable<LineItem> lineItems)
    {
        // Get customer details
        var customer = await _customerClient.GetCustomerAsync(customerId);
        if (customer == null)
            throw new CustomerNotFoundException(customerId);

        // Get billing information
        var billing = await _customerClient.GetCustomerBillingAsync(customerId);
        if (billing == null)
            throw new BillingInformationNotFoundException(customerId);

        // Get customer credit info for payment terms
        var credit = await _customerClient.GetCustomerCreditAsync(customerId);

        // Create invoice
        var invoice = new Invoice
        {
            CustomerId = customerId,
            CustomerName = customer.Name,
            BillingAddress = FormatBillingAddress(billing),
            BillingEmail = billing.BillingEmail,
            Currency = billing.Currency,
            PaymentTerms = billing.PaymentTermsDays,
            DiscountPercentage = billing.DiscountPercentage,
            LineItems = lineItems.ToList()
        };

        // Generate invoice
        return await _invoiceGenerator.GenerateAsync(invoice);
    }

    private string FormatBillingAddress(CustomerBillingDto billing)
    {
        return $"{billing.BillingAddress}\n" +
               $"{billing.BillingCity}, {billing.BillingState} {billing.BillingPostalCode}\n" +
               $"{billing.BillingCountry}";
    }
}
```

### CRM Integration

#### Lead Conversion

**Lead to Customer Conversion:**
```csharp
public class LeadConversionService
{
    private readonly CustomerServiceClient _customerClient;
    private readonly ICrmService _crmService;

    public async Task<CustomerDto> ConvertLeadToCustomerAsync(string leadId)
    {
        // Get lead information from CRM
        var lead = await _crmService.GetLeadAsync(leadId);
        if (lead == null)
            throw new LeadNotFoundException(leadId);

        // Map lead to customer registration request
        var customerRequest = new RegisterCustomerRequest
        {
            Name = lead.CompanyName,
            ContactEmail = lead.Email,
            ContactPhone = lead.Phone,
            BillingAddress = lead.Address,
            CustomerType = DetermineCustomerType(lead),
            CreditLimit = CalculateInitialCreditLimit(lead),
            Notes = $"Converted from lead {leadId} on {DateTime.UtcNow:yyyy-MM-dd}"
        };

        // Create customer
        var customer = await _customerClient.RegisterCustomerAsync(customerRequest);

        // Add primary contact
        if (!string.IsNullOrEmpty(lead.ContactName))
        {
            var contactRequest = new CreateCustomerContactRequest
            {
                FirstName = lead.ContactFirstName,
                LastName = lead.ContactLastName,
                Email = lead.Email,
                Phone = lead.Phone,
                Position = lead.Title,
                ContactType = ContactType.Business,
                IsPrimary = true
            };

            await _customerClient.AddContactAsync(customer.Id, contactRequest);
        }

        // Update lead status in CRM
        await _crmService.MarkLeadAsConvertedAsync(leadId, customer.Id);

        return customer;
    }
}
```

### Analytics Integration

#### Customer Data Analytics

**Analytics Data Export:**
```csharp
public class CustomerAnalyticsExporter
{
    private readonly CustomerServiceClient _customerClient;
    private readonly IAnalyticsService _analyticsService;

    public async Task ExportCustomerDataAsync()
    {
        var customers = await GetAllCustomersAsync();
        var analyticsData = new List<CustomerAnalyticsRecord>();

        foreach (var customer in customers)
        {
            var details = await _customerClient.GetCustomerDetailsAsync(customer.Id);
            var analyticsRecord = MapToAnalyticsRecord(details);
            analyticsData.Add(analyticsRecord);
        }

        await _analyticsService.BulkInsertAsync(analyticsData);
    }

    private async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var allCustomers = new List<CustomerDto>();
        var page = 1;
        const int pageSize = 100;

        while (true)
        {
            var customers = await _customerClient.GetCustomersAsync(page, pageSize);
            if (!customers.Any()) break;

            allCustomers.AddRange(customers);
            page++;
        }

        return allCustomers;
    }

    private CustomerAnalyticsRecord MapToAnalyticsRecord(CustomerDetailDto customer)
    {
        return new CustomerAnalyticsRecord
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerType = customer.CustomerType.ToString(),
            Status = customer.Status.ToString(),
            RegistrationDate = customer.CreatedAt,
            CreditLimit = customer.Credit?.CreditLimit ?? 0,
            ContactCount = customer.Contacts.Count,
            ActiveContractCount = customer.Contracts.Count(c => c.Status == ContractStatus.Active),
            TotalContractValue = customer.Contracts.Where(c => c.Status == ContractStatus.Active)
                                                 .Sum(c => c.ContractValue),
            LastUpdateDate = customer.UpdatedAt
        };
    }
}
```

## Performance Optimization

### Connection Pooling

#### HttpClient Best Practices

**HttpClient Factory Pattern:**
```csharp
public class CustomerServiceClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CustomerServiceOptions _options;

    public CustomerServiceClientFactory(
        IHttpClientFactory httpClientFactory, 
        IOptions<CustomerServiceOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public CustomerServiceClient CreateClient(string organizationId, string userId)
    {
        var httpClient = _httpClientFactory.CreateClient("CustomerService");
        return new CustomerServiceClient(httpClient, organizationId, userId);
    }
}

// Registration in DI container
services.AddHttpClient("CustomerService", client =>
{
    client.BaseAddress = new Uri("https://customer-service.qalitrack.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

services.AddScoped<CustomerServiceClientFactory>();
```

### Batch Operations

#### Efficient Bulk Processing

**Batch Customer Updates:**
```csharp
public class BatchCustomerProcessor
{
    private readonly CustomerServiceClient _client;
    private readonly SemaphoreSlim _semaphore;

    public BatchCustomerProcessor(CustomerServiceClient client, int maxConcurrency = 5)
    {
        _client = client;
        _semaphore = new SemaphoreSlim(maxConcurrency);
    }

    public async Task<BatchResult> ProcessCustomerUpdatesAsync(
        IEnumerable<CustomerUpdateOperation> operations)
    {
        var tasks = operations.Select(ProcessSingleOperationAsync);
        var results = await Task.WhenAll(tasks);

        return new BatchResult
        {
            TotalOperations = operations.Count(),
            SuccessfulOperations = results.Count(r => r.IsSuccess),
            FailedOperations = results.Count(r => !r.IsSuccess),
            Results = results
        };
    }

    private async Task<OperationResult> ProcessSingleOperationAsync(CustomerUpdateOperation operation)
    {
        await _semaphore.WaitAsync();
        try
        {
            switch (operation.Type)
            {
                case OperationType.Update:
                    await _client.UpdateCustomerAsync(operation.CustomerId, operation.UpdateRequest);
                    return OperationResult.Success(operation.CustomerId);

                case OperationType.StatusChange:
                    await _client.UpdateCustomerStatusAsync(operation.CustomerId, operation.NewStatus);
                    return OperationResult.Success(operation.CustomerId);

                default:
                    return OperationResult.Failure(operation.CustomerId, "Unknown operation type");
            }
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(operation.CustomerId, ex.Message);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

## Integration Testing

### API Contract Testing

#### Contract Test Examples

**Customer API Contract Tests:**
```csharp
[Fact]
public async Task GetCustomer_WithValidId_ReturnsCustomerData()
{
    // Arrange
    var customerId = "test-customer-id";
    var expectedCustomer = new CustomerDto
    {
        Id = customerId,
        Name = "Test Customer",
        ContactEmail = "test@example.com"
    };

    // Act
    var customer = await _customerClient.GetCustomerAsync(customerId);

    // Assert
    Assert.NotNull(customer);
    Assert.Equal(expectedCustomer.Id, customer.Id);
    Assert.Equal(expectedCustomer.Name, customer.Name);
    Assert.Equal(expectedCustomer.ContactEmail, customer.ContactEmail);
}

[Fact]
public async Task CreateCustomer_WithValidData_ReturnsCreatedCustomer()
{
    // Arrange
    var request = new RegisterCustomerRequest
    {
        Name = "New Test Customer",
        ContactEmail = "newtest@example.com",
        CustomerType = CustomerType.Corporate,
        CreditLimit = 10000
    };

    // Act
    var customer = await _customerClient.RegisterCustomerAsync(request);

    // Assert
    Assert.NotNull(customer);
    Assert.Equal(request.Name, customer.Name);
    Assert.Equal(request.ContactEmail, customer.ContactEmail);
    Assert.True(Guid.TryParse(customer.Id, out _));
}
```

### Integration Test Helpers

#### Test Data Management

**Test Customer Factory:**
```csharp
public class TestCustomerFactory
{
    private readonly CustomerServiceClient _client;
    private static int _customerCounter = 0;

    public async Task<CustomerDto> CreateTestCustomerAsync(
        string namePrefix = "Test Customer",
        CustomerType type = CustomerType.Corporate)
    {
        var counter = Interlocked.Increment(ref _customerCounter);
        var request = new RegisterCustomerRequest
        {
            Name = $"{namePrefix} {counter}",
            ContactEmail = $"test{counter}@example.com",
            ContactPhone = $"+1-555-{counter:D4}",
            BillingAddress = $"{counter} Test Street, Test City, TS 12345",
            CustomerType = type,
            CreditLimit = 10000,
            Notes = $"Test customer created at {DateTime.UtcNow}"
        };

        return await _client.RegisterCustomerAsync(request);
    }

    public async Task CleanupTestCustomerAsync(string customerId)
    {
        try
        {
            await _client.DeleteCustomerAsync(customerId);
        }
        catch (Exception)
        {
            // Ignore cleanup errors in tests
        }
    }
}
```

This comprehensive integration guide provides all the necessary information for successfully integrating with the Customer Service, ensuring robust, reliable, and efficient service-to-service communication within the QaliTrack ecosystem.