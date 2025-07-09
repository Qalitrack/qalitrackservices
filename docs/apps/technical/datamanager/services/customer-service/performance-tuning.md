# Customer Service Performance Tuning Guide

## Overview

This guide provides comprehensive strategies for optimizing the performance of the Customer Service. It covers database optimization, API performance tuning, caching strategies, and system-level optimizations to ensure optimal throughput and response times.

## Performance Targets

### Service Level Objectives (SLOs)

**API Response Times:**
- Customer lookup (by ID): <100ms (p95)
- Customer search: <200ms (p95)
- Customer registration: <500ms (p95)
- Bulk operations: <2000ms per 100 records (p95)

**Throughput Targets:**
- Customer lookups: >1000 RPS
- Customer registrations: >100 RPS
- Search operations: >500 RPS

**Resource Utilization:**
- CPU usage: <70% average
- Memory usage: <80% of available
- Database connections: <80% of pool size

**Availability:**
- Service uptime: >99.9%
- Health check response: <50ms

## Database Performance Optimization

### Index Optimization

#### Essential Indexes

**Customer Table Indexes:**
```sql
-- Primary key (automatic)
CREATE UNIQUE INDEX pk_customer ON customer(id);

-- Email lookup (frequent operation)
CREATE UNIQUE INDEX idx_customer_email ON customer(contactemail);

-- Tax number lookup
CREATE UNIQUE INDEX idx_customer_taxnumber ON customer(taxnumber) 
WHERE taxnumber IS NOT NULL;

-- Registration number lookup
CREATE UNIQUE INDEX idx_customer_regnumber ON customer(registrationnumber) 
WHERE registrationnumber IS NOT NULL;

-- Status filtering
CREATE INDEX idx_customer_status ON customer(status);

-- Customer type filtering
CREATE INDEX idx_customer_type ON customer(customertype);

-- Search optimization - composite index
CREATE INDEX idx_customer_search ON customer(name, contactemail, taxnumber);

-- Date-based queries
CREATE INDEX idx_customer_created ON customer(createdat);
CREATE INDEX idx_customer_updated ON customer(updatedat);
```

**Contact Table Indexes:**
```sql
-- Foreign key performance
CREATE INDEX idx_contact_customer ON customercontact(customerid);

-- Contact type queries
CREATE INDEX idx_contact_type ON customercontact(contacttype);

-- Primary contact lookup
CREATE INDEX idx_contact_primary ON customercontact(customerid, contacttype, isprimary)
WHERE isprimary = true;

-- Active contacts
CREATE INDEX idx_contact_active ON customercontact(customerid, isactive)
WHERE isactive = true;

-- Email lookup
CREATE INDEX idx_contact_email ON customercontact(email)
WHERE email IS NOT NULL;
```

**Contract Table Indexes:**
```sql
-- Customer contracts
CREATE INDEX idx_contract_customer ON customercontract(customerid);

-- Contract number lookup
CREATE UNIQUE INDEX idx_contract_number ON customercontract(contractnumber);

-- Status queries
CREATE INDEX idx_contract_status ON customercontract(status);

-- Expiration monitoring
CREATE INDEX idx_contract_expiry ON customercontract(enddate, status)
WHERE status IN ('Active', 'Draft');

-- Contract type filtering
CREATE INDEX idx_contract_type ON customercontract(contracttype);

-- Date range queries
CREATE INDEX idx_contract_dates ON customercontract(startdate, enddate);
```

#### Index Monitoring and Maintenance

**Index Usage Analysis:**
```sql
-- PostgreSQL: Check index usage
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_tup_read,
    idx_tup_fetch,
    idx_scan
FROM pg_stat_user_indexes
ORDER BY idx_scan DESC;

-- Find unused indexes
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan,
    pg_size_pretty(pg_relation_size(i.indexrelid)) as size
FROM pg_stat_user_indexes ui
JOIN pg_index i ON ui.indexrelid = i.indexrelid
WHERE idx_scan = 0 
AND NOT indisunique;
```

**Index Maintenance:**
```sql
-- Rebuild indexes (PostgreSQL)
REINDEX TABLE customer;
REINDEX TABLE customercontract;

-- Update statistics
ANALYZE customer;
ANALYZE customercontract;
ANALYZE customercontact;
```

### Query Optimization

#### Efficient Query Patterns

**Customer Search Optimization:**
```csharp
// Optimized search with proper indexing
public async Task<IEnumerable<CustomerDto>> SearchCustomersOptimizedAsync(
    string searchTerm, 
    int page = 1, 
    int pageSize = 20)
{
    var query = _context.Customers.AsQueryable();

    if (!string.IsNullOrEmpty(searchTerm))
    {
        // Use indexed columns in optimal order
        if (searchTerm.Contains("@"))
        {
            // Email search - exact match using index
            query = query.Where(c => c.ContactEmail == searchTerm);
        }
        else if (Regex.IsMatch(searchTerm, @"^TAX\d+"))
        {
            // Tax number search - exact match using index
            query = query.Where(c => c.TaxNumber == searchTerm);
        }
        else
        {
            // Name search - use index for starts-with pattern
            query = query.Where(c => c.Name.StartsWith(searchTerm));
        }
    }

    // Apply pagination efficiently
    var customers = await query
        .OrderBy(c => c.Name) // Use indexed column for ordering
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new CustomerDto // Project only needed fields
        {
            Id = c.Id,
            Name = c.Name,
            ContactEmail = c.ContactEmail,
            CustomerType = c.CustomerType,
            Status = c.Status,
            CreditLimit = c.CreditLimit,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        })
        .ToListAsync();

    return customers;
}
```

**Efficient Customer Details Loading:**
```csharp
// Optimized detail loading with selective includes
public async Task<CustomerDetailDto?> GetCustomerDetailsOptimizedAsync(string id)
{
    var customer = await _context.Customers
        .Where(c => c.Id == id)
        .Include(c => c.Contacts.Where(contact => contact.IsActive))
        .Include(c => c.Contracts.Where(contract => 
            contract.Status == ContractStatus.Active || 
            contract.Status == ContractStatus.Draft))
        .Include(c => c.Billing)
        .Include(c => c.Credit)
        .Include(c => c.Locations.Where(location => location.IsActive))
        .AsSplitQuery() // Avoid cartesian explosion
        .FirstOrDefaultAsync();

    return customer != null ? _mapper.Map<CustomerDetailDto>(customer) : null;
}
```

**Bulk Operations Optimization:**
```csharp
// Efficient bulk customer creation
public async Task<BulkOperationResult> BulkCreateCustomersAsync(
    IEnumerable<RegisterCustomerRequest> requests)
{
    var customers = new List<Customer>();
    var errors = new List<string>();

    // Validate emails in batch
    var emails = requests.Select(r => r.ContactEmail).ToList();
    var existingEmails = await _context.Customers
        .Where(c => emails.Contains(c.ContactEmail))
        .Select(c => c.ContactEmail)
        .ToHashSetAsync();

    foreach (var request in requests)
    {
        if (existingEmails.Contains(request.ContactEmail))
        {
            errors.Add($"Email {request.ContactEmail} already exists");
            continue;
        }

        var customer = _mapper.Map<Customer>(request);
        customer.Id = Guid.NewGuid().ToString();
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;
        
        customers.Add(customer);
    }

    // Bulk insert
    _context.Customers.AddRange(customers);
    await _context.SaveChangesAsync();

    return new BulkOperationResult
    {
        SuccessCount = customers.Count,
        ErrorCount = errors.Count,
        Errors = errors
    };
}
```

#### Query Performance Analysis

**Entity Framework Query Logging:**
```csharp
// Enable query logging in development
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<CustomerDbContext>(options =>
    {
        options.UseSqlServer(connectionString);
        
        #if DEBUG
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
        #endif
    });
}
```

**Query Performance Testing:**
```csharp
[Fact]
public async Task CustomerSearch_PerformanceTest()
{
    // Arrange
    await SeedLargeDatasetAsync(10000); // 10k customers
    
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    var results = await _repository.SearchAsync("Test");
    
    // Assert
    stopwatch.Stop();
    Assert.True(stopwatch.ElapsedMilliseconds < 200, 
        $"Search took {stopwatch.ElapsedMilliseconds}ms, expected <200ms");
}
```

### Database Connection Optimization

#### Connection Pooling Configuration

**Optimal Connection Pool Settings:**
```csharp
// Program.cs - Connection pool optimization
builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
}, ServiceLifetime.Scoped);

// For high-load scenarios, consider these connection string parameters:
// "...;Max Pool Size=100;Min Pool Size=10;Connection Idle Timeout=30;Connection Timeout=30"
```

**Connection Pool Monitoring:**
```csharp
public class DatabaseMetrics
{
    private readonly Counter<long> _activeConnections;
    private readonly Histogram<double> _connectionTime;

    public void RecordConnectionCreated()
    {
        _activeConnections.Add(1);
    }

    public void RecordConnectionClosed(TimeSpan duration)
    {
        _activeConnections.Add(-1);
        _connectionTime.Record(duration.TotalMilliseconds);
    }
}
```

## API Performance Optimization

### Response Caching

#### HTTP Response Caching

**Cache Headers Configuration:**
```csharp
// Cache static customer data
[HttpGet("{id}")]
[ResponseCache(Duration = 300, VaryByHeader = "X-Organization-Id")] // 5 minutes
public async Task<IActionResult> GetCustomer(string id)
{
    var customer = await _customerService.GetCustomerAsync(id);
    if (customer == null)
        return NotFound();

    // Add ETag for conditional requests
    var etag = $"\"{customer.UpdatedAt.Ticks}\"";
    Response.Headers.ETag = etag;

    // Check if client has current version
    if (Request.Headers.IfNoneMatch == etag)
        return StatusCode(304); // Not Modified

    return Ok(Success(customer));
}

// Cache customer search results
[HttpGet]
[ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "search", "page", "pageSize" })]
public async Task<IActionResult> GetCustomers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null)
{
    var customers = await _customerService.GetCustomersPagedAsync(page, pageSize, search);
    return Ok(Success(customers));
}
```

#### Memory Caching

**In-Memory Cache Implementation:**
```csharp
public class CachedCustomerService : ICustomerService
{
    private readonly ICustomerService _customerService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedCustomerService> _logger;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);

    public async Task<CustomerDto?> GetCustomerAsync(string id)
    {
        var cacheKey = $"customer:{id}";
        
        if (_cache.TryGetValue(cacheKey, out CustomerDto? cachedCustomer))
        {
            _logger.LogDebug("Cache hit for customer {CustomerId}", id);
            return cachedCustomer;
        }

        var customer = await _customerService.GetCustomerAsync(id);
        if (customer != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, customer, cacheOptions);
            _logger.LogDebug("Cached customer {CustomerId}", id);
        }

        return customer;
    }

    public async Task<CustomerDto> UpdateCustomerAsync(string id, UpdateCustomerRequest request)
    {
        var result = await _customerService.UpdateCustomerAsync(id, request);
        
        // Invalidate cache after update
        _cache.Remove($"customer:{id}");
        _cache.Remove($"customer:details:{id}");
        
        return result;
    }
}
```

#### Distributed Caching with Redis

**Redis Cache Implementation:**
```csharp
public class DistributedCachedCustomerService : ICustomerService
{
    private readonly ICustomerService _customerService;
    private readonly IDistributedCache _distributedCache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public async Task<CustomerDto?> GetCustomerAsync(string id)
    {
        var cacheKey = $"customer:{id}";
        var cachedJson = await _distributedCache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedJson))
        {
            return JsonSerializer.Deserialize<CustomerDto>(cachedJson);
        }

        var customer = await _customerService.GetCustomerAsync(id);
        if (customer != null)
        {
            var json = JsonSerializer.Serialize(customer);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };

            await _distributedCache.SetStringAsync(cacheKey, json, options);
        }

        return customer;
    }
}

// Redis configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "CustomerService";
});
```

### Serialization Optimization

#### JSON Serialization Performance

**Optimized JSON Configuration:**
```csharp
// Use System.Text.Json with optimized settings
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    
    // Performance optimizations
    options.SerializerOptions.WriteIndented = false; // Reduce payload size
    options.SerializerOptions.MaxDepth = 32; // Prevent deep object graphs
});

// Custom JSON source generators for better performance
[JsonSerializable(typeof(CustomerDto))]
[JsonSerializable(typeof(CustomerDetailDto))]
[JsonSerializable(typeof(ApiResponseDto<CustomerDto>))]
public partial class CustomerJsonContext : JsonSerializerContext
{
}

// Use in controllers
[HttpGet("{id}")]
public async Task<IActionResult> GetCustomer(string id)
{
    var customer = await _customerService.GetCustomerAsync(id);
    return Ok(Success(customer), CustomerJsonContext.Default.ApiResponseDtoCustomerDto);
}
```

#### Response Compression

**Compression Configuration:**
```csharp
// Enable response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// Use compression middleware
app.UseResponseCompression();
```

### Async/Await Optimization

#### Efficient Async Patterns

**Optimized Async Operations:**
```csharp
// Good: Parallel execution where possible
public async Task<CustomerDetailDto?> GetCustomerDetailsAsync(string id)
{
    var customerTask = _customerRepository.GetByIdAsync(id);
    var contactsTask = _contactRepository.GetByCustomerIdAsync(id);
    var contractsTask = _contractRepository.GetActiveByCustomerIdAsync(id);
    var billingTask = _billingRepository.GetByCustomerIdAsync(id);

    await Task.WhenAll(customerTask, contactsTask, contractsTask, billingTask);

    var customer = await customerTask;
    if (customer == null) return null;

    return new CustomerDetailDto
    {
        // Map customer properties
        Id = customer.Id,
        Name = customer.Name,
        Contacts = await contactsTask,
        Contracts = await contractsTask,
        Billing = await billingTask
    };
}

// Good: ConfigureAwait(false) for library code
public async Task<CustomerDto> RegisterCustomerAsync(RegisterCustomerRequest request)
{
    var validation = await _validator.ValidateAsync(request).ConfigureAwait(false);
    if (!validation.IsValid)
        throw new ValidationException(validation.Errors);

    var customer = _mapper.Map<Customer>(request);
    await _repository.AddAsync(customer).ConfigureAwait(false);
    await _repository.SaveChangesAsync().ConfigureAwait(false);

    return _mapper.Map<CustomerDto>(customer);
}
```

**Avoid Common Anti-Patterns:**
```csharp
// Bad: Sync over async
public CustomerDto GetCustomer(string id)
{
    return GetCustomerAsync(id).Result; // Deadlock risk
}

// Bad: Async void (except for event handlers)
public async void ProcessCustomer(string id) // Should return Task
{
    await ProcessCustomerAsync(id);
}

// Bad: Unnecessary async/await
public async Task<CustomerDto> GetCustomerWrapper(string id)
{
    return await GetCustomerAsync(id); // Just return the Task directly
}

// Good: Return Task directly when no additional processing
public Task<CustomerDto> GetCustomerWrapper(string id)
{
    return GetCustomerAsync(id);
}
```

## Memory Optimization

### Object Allocation Reduction

#### Pooling and Reuse Patterns

**Object Pooling for High-Frequency Operations:**
```csharp
public class CustomerSearchService
{
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;
    private readonly ObjectPool<List<Customer>> _listPool;

    public CustomerSearchService(ObjectPool<StringBuilder> stringBuilderPool)
    {
        _stringBuilderPool = stringBuilderPool;
        _listPool = new DefaultObjectPool<List<Customer>>(
            new ListPooledObjectPolicy<Customer>());
    }

    public async Task<string> BuildSearchQueryAsync(SearchCriteria criteria)
    {
        var sb = _stringBuilderPool.Get();
        try
        {
            sb.Clear();
            sb.Append("SELECT * FROM customers WHERE 1=1");
            
            if (!string.IsNullOrEmpty(criteria.Name))
                sb.Append($" AND name LIKE '{criteria.Name}%'");
            
            if (criteria.Status.HasValue)
                sb.Append($" AND status = {(int)criteria.Status}");

            return sb.ToString();
        }
        finally
        {
            _stringBuilderPool.Return(sb);
        }
    }
}

// Configure object pools
builder.Services.AddSingleton<ObjectPool<StringBuilder>>(provider =>
{
    var policy = new StringBuilderPooledObjectPolicy();
    return new DefaultObjectPool<StringBuilder>(policy);
});
```

#### Span<T> and Memory<T> for Buffer Management

**Efficient String Operations:**
```csharp
public class CustomerValidator
{
    // Use Span<T> for efficient string parsing
    public bool IsValidTaxNumber(ReadOnlySpan<char> taxNumber)
    {
        if (taxNumber.Length != 9) return false;
        
        if (!taxNumber.StartsWith("TAX".AsSpan())) return false;
        
        var numberPart = taxNumber.Slice(3);
        foreach (var c in numberPart)
        {
            if (!char.IsDigit(c)) return false;
        }
        
        return true;
    }

    // Use Memory<T> for async operations with buffers
    public async Task<ValidationResult> ValidateCustomerDataAsync(
        Memory<byte> customerData)
    {
        using var reader = new MemoryStream(customerData.ToArray());
        // Process data without additional allocations
        return await ProcessCustomerDataAsync(reader);
    }
}
```

### Garbage Collection Optimization

#### GC-Friendly Patterns

**Minimize Allocations in Hot Paths:**
```csharp
public class HighPerformanceCustomerService
{
    // Cache delegates to avoid allocation
    private static readonly Func<Customer, bool> ActiveCustomerPredicate = 
        c => c.Status == CustomerStatus.Active;
    
    private static readonly Func<Customer, CustomerSummaryDto> ToSummaryMapper =
        c => new CustomerSummaryDto { Id = c.Id, Name = c.Name };

    // Use ArrayPool for temporary arrays
    private readonly ArrayPool<Customer> _customerArrayPool = ArrayPool<Customer>.Shared;

    public async Task<CustomerSummaryDto[]> GetActiveCustomerSummariesAsync()
    {
        var customers = await _repository.GetAllActiveCustomersAsync();
        
        // Use pooled array if we know approximate size
        var summaryArray = _customerArrayPool.Rent(customers.Count);
        try
        {
            var index = 0;
            foreach (var customer in customers.Where(ActiveCustomerPredicate))
            {
                summaryArray[index++] = ToSummaryMapper(customer);
            }
            
            // Create result array with exact size
            var result = new CustomerSummaryDto[index];
            Array.Copy(summaryArray, result, index);
            return result;
        }
        finally
        {
            _customerArrayPool.Return(summaryArray);
        }
    }
}
```

#### GC Monitoring and Analysis

**GC Performance Metrics:**
```csharp
public class GCMetrics
{
    private readonly Counter<long> _gen0Collections;
    private readonly Counter<long> _gen1Collections;
    private readonly Counter<long> _gen2Collections;
    private readonly Gauge<long> _totalMemory;

    public GCMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("CustomerService.GC");
        
        _gen0Collections = meter.CreateCounter<long>("gc_collections_gen0");
        _gen1Collections = meter.CreateCounter<long>("gc_collections_gen1");
        _gen2Collections = meter.CreateCounter<long>("gc_collections_gen2");
        _totalMemory = meter.CreateGauge<long>("gc_total_memory");
    }

    public void RecordGCMetrics()
    {
        _gen0Collections.Add(GC.CollectionCount(0));
        _gen1Collections.Add(GC.CollectionCount(1));
        _gen2Collections.Add(GC.CollectionCount(2));
        _totalMemory.Record(GC.GetTotalMemory(false));
    }
}
```

## Scaling and Load Balancing

### Horizontal Scaling

#### Stateless Service Design

**Session State Management:**
```csharp
// Store session data in distributed cache instead of memory
public class CustomerSessionService
{
    private readonly IDistributedCache _cache;

    public async Task SetUserSessionAsync(string userId, UserSession session)
    {
        var key = $"session:{userId}";
        var json = JsonSerializer.Serialize(session);
        
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        });
    }

    public async Task<UserSession?> GetUserSessionAsync(string userId)
    {
        var key = $"session:{userId}";
        var json = await _cache.GetStringAsync(key);
        
        return json != null ? JsonSerializer.Deserialize<UserSession>(json) : null;
    }
}
```

#### Load Balancer Configuration

**Nginx Load Balancer Setup:**
```nginx
upstream customer_service {
    least_conn;
    server customer-service-1:7003 max_fails=3 fail_timeout=30s;
    server customer-service-2:7003 max_fails=3 fail_timeout=30s;
    server customer-service-3:7003 max_fails=3 fail_timeout=30s;
    
    keepalive 32;
}

server {
    listen 80;
    server_name customer-api.qalitrack.com;
    
    location / {
        proxy_pass http://customer_service;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # Connection pooling
        proxy_set_header Connection "";
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
    
    # Health check endpoint
    location /health {
        proxy_pass http://customer_service/health;
        access_log off;
    }
}
```

### Auto-Scaling Configuration

#### Kubernetes Horizontal Pod Autoscaler

**HPA Configuration:**
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: customer-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: customer-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  - type: Pods
    pods:
      metric:
        name: http_requests_per_second
      target:
        type: AverageValue
        averageValue: "100"
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
      - type: Percent
        value: 50
        periodSeconds: 30
```

## Monitoring and Performance Analysis

### Application Performance Monitoring

#### Custom Performance Metrics

**Performance Tracking Middleware:**
```csharp
public class PerformanceTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceTrackingMiddleware> _logger;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _requestCount;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = context.Request.Path.Value ?? "unknown";
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalMilliseconds;
            
            _requestDuration.Record(duration, 
                new KeyValuePair<string, object?>("endpoint", endpoint),
                new KeyValuePair<string, object?>("method", context.Request.Method),
                new KeyValuePair<string, object?>("status", context.Response.StatusCode));
            
            _requestCount.Add(1,
                new KeyValuePair<string, object?>("endpoint", endpoint),
                new KeyValuePair<string, object?>("method", context.Request.Method));

            if (duration > 1000) // Log slow requests
            {
                _logger.LogWarning("Slow request: {Method} {Path} took {Duration}ms",
                    context.Request.Method, context.Request.Path, duration);
            }
        }
    }
}
```

#### Database Performance Monitoring

**EF Core Interceptors:**
```csharp
public class PerformanceInterceptor : DbCommandInterceptor
{
    private readonly ILogger<PerformanceInterceptor> _logger;
    private readonly Histogram<double> _queryDuration;

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        eventData.Context?.Database.BeginTransaction();
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        var duration = eventData.Duration.TotalMilliseconds;
        
        _queryDuration.Record(duration,
            new KeyValuePair<string, object?>("command_type", eventData.ExecuteMethod),
            new KeyValuePair<string, object?>("is_async", eventData.IsAsync));

        if (duration > 500) // Log slow queries
        {
            _logger.LogWarning("Slow query: {CommandText} took {Duration}ms",
                command.CommandText, duration);
        }

        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }
}
```

### Load Testing

#### Performance Benchmarking

**NBomber Load Test Scenarios:**
```csharp
public class CustomerServiceLoadTests
{
    [Fact]
    public void CustomerService_LoadTest_MeetsSLO()
    {
        var scenario = Scenario.Create("customer_operations", async context =>
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Organization-Id", "load-test");
            httpClient.DefaultRequestHeaders.Add("X-User-Id", "load-test-user");

            var operation = context.InvocationNumber % 4;
            
            try
            {
                switch (operation)
                {
                    case 0: // Get customer
                        var getResponse = await httpClient.GetAsync(
                            $"http://localhost:7003/api/customers/test-customer-{context.InvocationNumber % 1000}");
                        return getResponse.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                        
                    case 1: // Search customers
                        var searchResponse = await httpClient.GetAsync(
                            $"http://localhost:7003/api/customers?search=test&page={context.InvocationNumber % 10 + 1}");
                        return searchResponse.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                        
                    case 2: // Create customer
                        var customer = CreateTestCustomer(context.InvocationNumber);
                        var json = JsonSerializer.Serialize(customer);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var createResponse = await httpClient.PostAsync("http://localhost:7003/api/customers", content);
                        return createResponse.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                        
                    default: // Health check
                        var healthResponse = await httpClient.GetAsync("http://localhost:7003/health");
                        return healthResponse.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                }
            }
            catch (Exception ex)
            {
                return Response.Fail(ex.Message);
            }
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(2)),
            Simulation.KeepConstant(copies: 50, during: TimeSpan.FromMinutes(1))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert SLO compliance
        var sceneStats = stats.AllScenarios.First();
        Assert.True(sceneStats.Ok.Request.Mean < TimeSpan.FromMilliseconds(500));
        Assert.True(sceneStats.Ok.Request.P95 < TimeSpan.FromMilliseconds(1000));
        Assert.True(sceneStats.Fail.Request.Count == 0);
    }
}
```

This comprehensive performance tuning guide provides the foundation for optimizing the Customer Service to meet demanding performance requirements while maintaining reliability and scalability.