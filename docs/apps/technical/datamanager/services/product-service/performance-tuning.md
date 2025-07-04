# Product Service Performance Tuning Guide

## Table of Contents

- [Performance Overview](#performance-overview)
- [Database Optimization](#database-optimization)
- [Application Performance](#application-performance)
- [Memory Management](#memory-management)
- [Network Optimization](#network-optimization)
- [Caching Strategies](#caching-strategies)
- [Load Testing and Benchmarking](#load-testing-and-benchmarking)
- [Monitoring and Metrics](#monitoring-and-metrics)
- [Scaling Strategies](#scaling-strategies)
- [Performance Best Practices](#performance-best-practices)

## Performance Overview

### Performance Targets

The Product Service is designed to meet the following performance requirements:

| Metric | Target | Measurement Method |
|--------|--------|--------------------|
| Response Time (P95) | < 500ms | HTTP request duration |
| Response Time (P99) | < 1000ms | HTTP request duration |
| Throughput | > 1000 RPS | Requests per second |
| Database Query Time | < 100ms average | SQL execution time |
| Memory Usage | < 512MB steady state | Container memory |
| CPU Usage | < 70% average | Container CPU |
| Availability | 99.9% | Uptime monitoring |

### Performance Measurement

#### Key Performance Indicators (KPIs)
- **Latency**: Time to complete individual requests
- **Throughput**: Number of requests processed per second
- **Resource Utilization**: CPU, memory, disk, and network usage
- **Error Rate**: Percentage of failed requests
- **Availability**: Service uptime percentage

#### Monitoring Tools
- **Application Metrics**: .NET performance counters, custom metrics
- **Database Monitoring**: Query execution plans, index usage
- **Container Monitoring**: Docker stats, Kubernetes metrics
- **Load Testing**: NBomber, k6, Apache Bench

## Database Optimization

### Query Optimization

#### Index Strategy

**Primary Indexes**
```sql
-- Product search optimization
CREATE INDEX CONCURRENTLY idx_products_name_gin ON Products 
USING gin(to_tsvector('english', Name));

CREATE INDEX CONCURRENTLY idx_products_code ON Products(Code) 
WHERE IsDeleted = false;

CREATE INDEX CONCURRENTLY idx_products_category_status ON Products(CategoryId, Status) 
WHERE IsDeleted = false;

-- Category hierarchy optimization
CREATE INDEX CONCURRENTLY idx_categories_parent ON ProductCategories(ParentCategoryId) 
WHERE IsDeleted = false;

-- Specifications lookup
CREATE INDEX CONCURRENTLY idx_specifications_product ON ProductSpecifications(ProductId) 
WHERE IsDeleted = false;

-- Pricing queries
CREATE INDEX CONCURRENTLY idx_pricing_product_active ON ProductPricing(ProductId, IsActive) 
WHERE IsDeleted = false;

CREATE INDEX CONCURRENTLY idx_pricing_customer_group ON ProductPricing(CustomerGroup, Currency) 
WHERE IsDeleted = false AND IsActive = true;
```

**Composite Indexes for Complex Queries**
```sql
-- Multi-column search optimization
CREATE INDEX CONCURRENTLY idx_products_search_composite ON Products(CategoryId, IsHazardous, Status) 
WHERE IsDeleted = false;

-- Specification filtering
CREATE INDEX CONCURRENTLY idx_specifications_name_value ON ProductSpecifications(Name, Value) 
WHERE IsDeleted = false;

-- Pricing range queries
CREATE INDEX CONCURRENTLY idx_pricing_range ON ProductPricing(MinimumQuantity, MaximumQuantity, Currency) 
WHERE IsDeleted = false AND IsActive = true;
```

#### Query Analysis and Optimization

**Identify Slow Queries**
```sql
-- PostgreSQL query statistics
SELECT query, calls, total_time, mean_time, stddev_time, rows,
       100.0 * shared_blks_hit / nullif(shared_blks_hit + shared_blks_read, 0) AS hit_percent
FROM pg_stat_statements
WHERE calls > 100  -- Filter for frequently called queries
ORDER BY mean_time DESC
LIMIT 20;

-- Check for sequential scans
SELECT schemaname, tablename, seq_scan, seq_tup_read, idx_scan, idx_tup_fetch
FROM pg_stat_user_tables
WHERE seq_scan > idx_scan  -- Tables with more sequential scans than index scans
ORDER BY seq_scan DESC;
```

**Optimize Common Queries**
```csharp
// Before: Inefficient query with N+1 problem
public async Task<List<ProductDto>> GetProductsWithCategories()
{
    var products = await _context.Products.ToListAsync();
    var result = new List<ProductDto>();
    
    foreach (var product in products)
    {
        var category = await _context.ProductCategories.FindAsync(product.CategoryId); // N+1 problem
        result.Add(MapToDto(product, category));
    }
    
    return result;
}

// After: Optimized with proper includes
public async Task<List<ProductDto>> GetProductsWithCategories()
{
    return await _context.Products
        .Include(p => p.Category)
        .Where(p => !p.IsDeleted)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            // Other properties...
        })
        .ToListAsync();
}
```

### Connection Pooling Optimization

#### Connection Pool Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=productdb;Username=user;Password=pass;Pooling=true;MinPoolSize=10;MaxPoolSize=100;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;"
  }
}
```

**Connection Pool Monitoring**
```csharp
public class DatabasePerformanceService
{
    private readonly ILogger<DatabasePerformanceService> _logger;
    
    public void MonitorConnectionPool()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        using var connection = new NpgsqlConnection(connectionString);
        
        // Monitor active connections
        var activeConnections = connection.QuerySingle<int>(
            "SELECT count(*) FROM pg_stat_activity WHERE state = 'active'");
        
        // Log if connection count is high
        if (activeConnections > 80)
        {
            _logger.LogWarning("High connection count: {ActiveConnections}", activeConnections);
        }
    }
}
```

### Database Configuration Tuning

#### PostgreSQL Configuration
```conf
# postgresql.conf optimizations for Product Service

# Memory settings
shared_buffers = 256MB                # 25% of available RAM
effective_cache_size = 1GB            # Available memory for caching
work_mem = 16MB                       # Memory for sorts and joins
maintenance_work_mem = 128MB          # Memory for maintenance operations

# Query planner settings
random_page_cost = 1.1                # SSD optimization
effective_io_concurrency = 200        # Concurrent I/O operations

# Write-ahead logging
wal_buffers = 16MB
checkpoint_completion_target = 0.9
wal_compression = on

# Connection settings
max_connections = 200
shared_preload_libraries = 'pg_stat_statements'

# Query optimization
default_statistics_target = 100
constraint_exclusion = partition
```

#### Database Maintenance
```sql
-- Regular maintenance tasks

-- Update table statistics
ANALYZE Products;
ANALYZE ProductCategories;
ANALYZE ProductSpecifications;

-- Reindex for optimal performance
REINDEX INDEX CONCURRENTLY idx_products_name_gin;
REINDEX INDEX CONCURRENTLY idx_products_code;

-- Vacuum to reclaim space
VACUUM (ANALYZE, VERBOSE) Products;
VACUUM (ANALYZE, VERBOSE) ProductCategories;

-- Check for bloated indexes
SELECT schemaname, tablename, indexname, 
       pg_size_pretty(pg_relation_size(indexrelid)) as index_size,
       idx_scan, idx_tup_read, idx_tup_fetch
FROM pg_stat_user_indexes
WHERE idx_scan < 100  -- Rarely used indexes
ORDER BY pg_relation_size(indexrelid) DESC;
```

## Application Performance

### Entity Framework Optimization

#### Query Optimization Patterns
```csharp
// Use AsNoTracking for read-only queries
public async Task<List<ProductDto>> GetProductsForDisplay()
{
    return await _context.Products
        .AsNoTracking()  // Disable change tracking for better performance
        .Where(p => !p.IsDeleted)
        .Select(p => new ProductDto  // Project only needed fields
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            CategoryName = p.Category.Name
        })
        .ToListAsync();
}

// Use Split Queries for multiple includes
public async Task<Product> GetProductWithAllDetailsAsync(string id)
{
    return await _context.Products
        .AsSplitQuery()  // Split into multiple queries to avoid cartesian explosion
        .Include(p => p.Category)
        .Include(p => p.Specifications)
        .Include(p => p.Pricing)
        .Include(p => p.ComplianceRequirements)
        .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
}

// Use raw SQL for complex queries
public async Task<List<ProductSummary>> GetProductSummaryAsync()
{
    return await _context.Database
        .SqlQueryRaw<ProductSummary>(@"
            SELECT p.Id, p.Name, p.Code, c.Name as CategoryName,
                   COUNT(s.Id) as SpecificationCount,
                   AVG(pr.UnitPrice) as AveragePrice
            FROM Products p
            LEFT JOIN ProductCategories c ON p.CategoryId = c.Id
            LEFT JOIN ProductSpecifications s ON p.Id = s.ProductId
            LEFT JOIN ProductPricing pr ON p.Id = pr.ProductId
            WHERE p.IsDeleted = false
            GROUP BY p.Id, p.Name, p.Code, c.Name")
        .ToListAsync();
}
```

#### DbContext Optimization
```csharp
// Configure DbContext for performance
public class ProductDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .EnableSensitiveDataLogging(false)  // Disable in production
            .EnableServiceProviderCaching()     // Cache service provider
            .EnableDetailedErrors(false);       // Disable in production
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure query filters to be compiled once
        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => !p.IsDeleted);
        
        // Configure value converters for performance
        modelBuilder.Entity<Product>()
            .Property(e => e.Status)
            .HasConversion<int>();  // Store enum as int for better performance
    }
}

// Use compiled queries for frequently executed queries
public static class CompiledQueries
{
    public static readonly Func<ProductDbContext, string, Task<Product>> GetProductById =
        EF.CompileAsyncQuery((ProductDbContext context, string id) =>
            context.Products.FirstOrDefault(p => p.Id == id && !p.IsDeleted));
    
    public static readonly Func<ProductDbContext, string, Task<List<Product>>> GetProductsByCategory =
        EF.CompileAsyncQuery((ProductDbContext context, string categoryId) =>
            context.Products.Where(p => p.CategoryId == categoryId && !p.IsDeleted).ToList());
}
```

### API Performance Optimization

#### Response Compression
```csharp
// Enable response compression
public void ConfigureServices(IServiceCollection services)
{
    services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/json" });
    });
    
    services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });
}

public void Configure(IApplicationBuilder app)
{
    app.UseResponseCompression();
    // Other middleware...
}
```

#### API Response Optimization
```csharp
// Implement pagination for large result sets
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
}

public async Task<PagedResult<ProductDto>> GetProductsPagedAsync(
    int pageNumber = 1, 
    int pageSize = 50,
    string searchTerm = null,
    string categoryId = null)
{
    var query = _context.Products.AsNoTracking().Where(p => !p.IsDeleted);
    
    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(p => p.Name.Contains(searchTerm) || p.Code.Contains(searchTerm));
    }
    
    if (!string.IsNullOrEmpty(categoryId))
    {
        query = query.Where(p => p.CategoryId == categoryId);
    }
    
    var totalCount = await query.CountAsync();
    
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            CategoryName = p.Category.Name
        })
        .ToListAsync();
    
    return new PagedResult<ProductDto>
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}
```

#### Async/Await Optimization
```csharp
// Optimize async operations
public class ProductService : IProductService
{
    // Use ValueTask for frequently called methods that may return synchronously
    public ValueTask<ProductDto> GetCachedProductAsync(string id)
    {
        if (_cache.TryGetValue(id, out ProductDto cached))
        {
            return ValueTask.FromResult(cached);  // Synchronous completion
        }
        
        return GetProductFromDatabaseAsync(id);   // Asynchronous completion
    }
    
    // Use ConfigureAwait(false) for library code
    private async ValueTask<ProductDto> GetProductFromDatabaseAsync(string id)
    {
        var product = await _repository.GetByIdAsync(id).ConfigureAwait(false);
        var dto = _mapper.Map<ProductDto>(product);
        
        _cache.Set(id, dto, TimeSpan.FromMinutes(5));
        return dto;
    }
    
    // Parallelize independent operations
    public async Task<ProductDetailsDto> GetProductDetailsAsync(string id)
    {
        var productTask = _repository.GetByIdAsync(id);
        var specificationsTask = _specificationRepository.GetByProductIdAsync(id);
        var pricingTask = _pricingRepository.GetByProductIdAsync(id);
        
        await Task.WhenAll(productTask, specificationsTask, pricingTask);
        
        return new ProductDetailsDto
        {
            Product = _mapper.Map<ProductDto>(productTask.Result),
            Specifications = _mapper.Map<List<ProductSpecificationDto>>(specificationsTask.Result),
            Pricing = _mapper.Map<List<ProductPricingDto>>(pricingTask.Result)
        };
    }
}
```

## Memory Management

### Garbage Collection Optimization

#### GC Configuration
```dockerfile
# Dockerfile GC optimizations
FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV DOTNET_gcServer=1
ENV DOTNET_GCHeapCount=2
ENV DOTNET_GCConserveMemory=5
ENV DOTNET_GCHighMemPercent=90

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
```

#### Memory Monitoring
```csharp
public class MemoryMonitoringService : BackgroundService
{
    private readonly ILogger<MemoryMonitoringService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var memoryBefore = GC.GetTotalMemory(false);
            
            // Trigger garbage collection if memory usage is high
            if (memoryBefore > 400_000_000) // 400MB threshold
            {
                _logger.LogWarning("High memory usage detected: {MemoryMB}MB", memoryBefore / 1024 / 1024);
                
                GC.Collect(2, GCCollectionMode.Optimized);
                GC.WaitForPendingFinalizers();
                
                var memoryAfter = GC.GetTotalMemory(true);
                _logger.LogInformation("GC completed. Memory reduced from {BeforeMB}MB to {AfterMB}MB",
                    memoryBefore / 1024 / 1024, memoryAfter / 1024 / 1024);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

### Object Pooling

#### ArrayPool Usage
```csharp
public class OptimizedProductService
{
    private static readonly ArrayPool<char> CharPool = ArrayPool<char>.Shared;
    
    public string ProcessProductCodes(List<string> codes)
    {
        var totalLength = codes.Sum(c => c.Length) + codes.Count - 1; // Include separators
        var buffer = CharPool.Rent(totalLength);
        
        try
        {
            var span = buffer.AsSpan(0, totalLength);
            var position = 0;
            
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 0)
                {
                    span[position++] = ',';
                }
                
                codes[i].AsSpan().CopyTo(span.Slice(position));
                position += codes[i].Length;
            }
            
            return new string(span);
        }
        finally
        {
            CharPool.Return(buffer);
        }
    }
}
```

#### Object Pool for DTOs
```csharp
public class ProductDtoPool : ObjectPool<ProductDto>
{
    private readonly ConcurrentQueue<ProductDto> _objects = new();
    
    public override ProductDto Get()
    {
        if (_objects.TryDequeue(out var item))
        {
            return item;
        }
        
        return new ProductDto();
    }
    
    public override void Return(ProductDto obj)
    {
        // Reset object state
        obj.Id = null;
        obj.Name = null;
        obj.Code = null;
        // ... reset other properties
        
        _objects.Enqueue(obj);
    }
}
```

## Network Optimization

### HTTP Client Optimization

#### Connection Pooling
```csharp
public class HttpClientConfiguration
{
    public static void ConfigureHttpClients(IServiceCollection services)
    {
        services.AddHttpClient<IExternalService, ExternalService>(client =>
        {
            client.BaseAddress = new Uri("https://external-api.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            MaxConnectionsPerServer = 50,
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5)
        });
    }
}
```

#### Request/Response Optimization
```csharp
public class OptimizedApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public async Task<T> GetAsync<T>(string endpoint)
    {
        using var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        
        // Use stream for large responses
        using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
    }
    
    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        using var content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");
        
        using var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        
        using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions);
    }
}
```

### Response Optimization

#### ETags and Conditional Requests
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetProduct(string id)
{
    var product = await _productService.GetProductByIdAsync(id);
    if (product == null)
        return NotFound();
    
    // Generate ETag based on product data
    var etag = GenerateETag(product);
    
    // Check If-None-Match header
    if (Request.Headers.IfNoneMatch.Contains(etag))
    {
        return StatusCode(304); // Not Modified
    }
    
    Response.Headers.ETag = etag;
    Response.Headers.CacheControl = "public, max-age=300"; // 5 minutes
    
    return Ok(ApiResponseDto<ProductDto>.SuccessResponse(product));
}

private string GenerateETag(ProductDto product)
{
    var data = $"{product.Id}-{product.UpdatedAt.Ticks}";
    using var sha256 = SHA256.Create();
    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
    return Convert.ToBase64String(hash);
}
```

## Caching Strategies

### In-Memory Caching

#### Memory Cache Implementation
```csharp
public class CachedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _defaultOptions;
    
    public CachedProductService(IProductService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
        _defaultOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(15),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            Priority = CacheItemPriority.Normal,
            Size = 1
        };
    }
    
    public async Task<ProductDto> GetProductByIdAsync(string id)
    {
        var cacheKey = $"product:{id}";
        
        if (_cache.TryGetValue(cacheKey, out ProductDto cached))
        {
            return cached;
        }
        
        var product = await _inner.GetProductByIdAsync(id);
        if (product != null)
        {
            _cache.Set(cacheKey, product, _defaultOptions);
        }
        
        return product;
    }
    
    public async Task<ProductDto> UpdateProductAsync(string id, UpdateProductRequest request)
    {
        var result = await _inner.UpdateProductAsync(id, request);
        
        // Invalidate cache entries
        _cache.Remove($"product:{id}");
        _cache.Remove($"products:category:{result.CategoryId}");
        
        return result;
    }
}
```

### Distributed Caching

#### Redis Integration
```csharp
public class RedisProductCache : IProductCache
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisProductCache> _logger;
    
    public async Task<ProductDto> GetProductAsync(string id)
    {
        var cacheKey = $"product:{id}";
        var cached = await _cache.GetStringAsync(cacheKey);
        
        if (cached != null)
        {
            return JsonSerializer.Deserialize<ProductDto>(cached);
        }
        
        return null;
    }
    
    public async Task SetProductAsync(string id, ProductDto product, TimeSpan? expiry = null)
    {
        var cacheKey = $"product:{id}";
        var serialized = JsonSerializer.Serialize(product);
        
        var options = new DistributedCacheEntryOptions();
        if (expiry.HasValue)
        {
            options.SetAbsoluteExpiration(expiry.Value);
        }
        else
        {
            options.SetSlidingExpiration(TimeSpan.FromMinutes(15));
        }
        
        await _cache.SetStringAsync(cacheKey, serialized, options);
    }
    
    public async Task InvalidateProductAsync(string id)
    {
        var cacheKey = $"product:{id}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

### Cache Warming

#### Background Cache Warming
```csharp
public class CacheWarmupService : BackgroundService
{
    private readonly IProductService _productService;
    private readonly IProductCache _cache;
    private readonly ILogger<CacheWarmupService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial warmup
        await WarmupCacheAsync();
        
        // Periodic refresh
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RefreshPopularProductsAsync();
        }
    }
    
    private async Task WarmupCacheAsync()
    {
        _logger.LogInformation("Starting cache warmup");
        
        // Load frequently accessed products
        var popularProducts = await _productService.GetPopularProductsAsync(100);
        
        var tasks = popularProducts.Select(async product =>
        {
            await _cache.SetProductAsync(product.Id, product, TimeSpan.FromHours(2));
        });
        
        await Task.WhenAll(tasks);
        
        _logger.LogInformation("Cache warmup completed for {Count} products", popularProducts.Count);
    }
}
```

## Load Testing and Benchmarking

### NBomber Load Tests

#### Basic Load Test
```csharp
public class ProductServiceLoadTests
{
    [Fact]
    public void GetProducts_LoadTest()
    {
        var scenario = Scenario.Create("get_products", async context =>
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetTestToken());
            
            var response = await client.GetAsync("http://localhost:7005/api/products");
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 50, during: TimeSpan.FromMinutes(5))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
    
    [Fact]
    public void SearchProducts_StressTest()
    {
        var searchTerms = new[] { "diesel", "fuel", "chemical", "oil", "gas" };
        var random = new Random();
        
        var scenario = Scenario.Create("search_products", async context =>
        {
            var searchTerm = searchTerms[random.Next(searchTerms.Length)];
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetTestToken());
            
            var response = await client.GetAsync($"http://localhost:7005/api/products/search?searchTerm={searchTerm}");
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.RampPerSec(from: 10, to: 100, during: TimeSpan.FromMinutes(2)),
            Simulation.KeepConstant(copies: 100, during: TimeSpan.FromMinutes(5))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
        
        // Assert performance requirements
        var scnStats = stats.AllScenarios.First();
        Assert.True(scnStats.Ok.Request.Mean <= TimeSpan.FromMilliseconds(500));
        Assert.True(scnStats.Fail.Request.Count <= scnStats.Ok.Request.Count * 0.01); // < 1% failure rate
    }
}
```

### Database Benchmarking

#### Query Performance Tests
```csharp
public class DatabaseBenchmarkTests
{
    [Fact]
    public async Task ProductSearch_Performance_Benchmark()
    {
        using var context = CreateTestContext();
        await SeedTestDataAsync(context, productCount: 10000);
        
        var repository = new ProductRepository(context);
        var stopwatch = Stopwatch.StartNew();
        
        // Test search performance
        var results = await repository.SearchProductsAsync("test");
        
        stopwatch.Stop();
        
        // Assert performance requirements
        Assert.True(stopwatch.ElapsedMilliseconds < 100); // < 100ms
        Assert.True(results.Count > 0);
    }
    
    [Fact]
    public async Task CategoryHierarchy_Performance_Benchmark()
    {
        using var context = CreateTestContext();
        await SeedCategoryHierarchyAsync(context, depth: 5, childrenPerLevel: 10);
        
        var repository = new ProductCategoryRepository(context);
        var stopwatch = Stopwatch.StartNew();
        
        // Test hierarchy traversal
        var categories = await repository.GetCategoryHierarchyAsync("root-category");
        
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 50); // < 50ms
        Assert.True(categories.Count > 0);
    }
}
```

## Monitoring and Metrics

### Application Metrics

#### Custom Metrics Collection
```csharp
public class ProductServiceMetrics
{
    private readonly Meter _meter;
    private readonly Counter<int> _productsCreated;
    private readonly Counter<int> _productsUpdated;
    private readonly Histogram<double> _requestDuration;
    private readonly UpDownCounter<int> _activeRequests;
    
    public ProductServiceMetrics()
    {
        _meter = new Meter("ProductService", "1.0.0");
        
        _productsCreated = _meter.CreateCounter<int>(
            "products_created_total",
            description: "Total number of products created");
            
        _productsUpdated = _meter.CreateCounter<int>(
            "products_updated_total",
            description: "Total number of products updated");
            
        _requestDuration = _meter.CreateHistogram<double>(
            "request_duration_seconds",
            unit: "s",
            description: "Request duration in seconds");
            
        _activeRequests = _meter.CreateUpDownCounter<int>(
            "active_requests",
            description: "Number of active requests");
    }
    
    public void RecordProductCreated(string category) =>
        _productsCreated.Add(1, new("category", category));
    
    public void RecordProductUpdated(string category) =>
        _productsUpdated.Add(1, new("category", category));
    
    public void RecordRequestDuration(double duration, string endpoint, int statusCode) =>
        _requestDuration.Record(duration, 
            new("endpoint", endpoint), 
            new("status_code", statusCode.ToString()));
    
    public IDisposable TrackActiveRequest()
    {
        _activeRequests.Add(1);
        return new ActiveRequestTracker(() => _activeRequests.Add(-1));
    }
}

public class ActiveRequestTracker : IDisposable
{
    private readonly Action _onDispose;
    private bool _disposed;
    
    public ActiveRequestTracker(Action onDispose) => _onDispose = onDispose;
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _onDispose();
            _disposed = true;
        }
    }
}
```

#### Performance Monitoring Middleware
```csharp
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ProductServiceMetrics _metrics;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        using var activeRequestTracker = _metrics.TrackActiveRequest();
        
        var stopwatch = Stopwatch.StartNew();
        var endpoint = context.Request.Path.Value ?? "unknown";
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalSeconds;
            
            _metrics.RecordRequestDuration(duration, endpoint, context.Response.StatusCode);
            
            // Log slow requests
            if (duration > 1.0) // > 1 second
            {
                _logger.LogWarning("Slow request: {Method} {Path} took {Duration:F2}s",
                    context.Request.Method,
                    context.Request.Path,
                    duration);
            }
        }
    }
}
```

### Database Performance Monitoring

#### SQL Performance Tracking
```csharp
public class DatabasePerformanceInterceptor : DbCommandInterceptor
{
    private readonly ILogger<DatabasePerformanceInterceptor> _logger;
    private readonly ProductServiceMetrics _metrics;
    
    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        var duration = eventData.Duration.TotalMilliseconds;
        
        // Record query performance
        _metrics.RecordQueryDuration(duration, GetQueryType(command.CommandText));
        
        // Log slow queries
        if (duration > 100) // > 100ms
        {
            _logger.LogWarning("Slow query detected: {Duration}ms - {CommandText}",
                duration, command.CommandText);
        }
        
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }
    
    private string GetQueryType(string commandText)
    {
        var trimmed = commandText.TrimStart().ToUpper();
        return trimmed switch
        {
            var text when text.StartsWith("SELECT") => "SELECT",
            var text when text.StartsWith("INSERT") => "INSERT",
            var text when text.StartsWith("UPDATE") => "UPDATE",
            var text when text.StartsWith("DELETE") => "DELETE",
            _ => "OTHER"
        };
    }
}
```

## Scaling Strategies

### Horizontal Scaling

#### Load Balancer Configuration
```yaml
# nginx.conf for load balancing
upstream product-service {
    least_conn;
    server product-service-1:7005 max_fails=3 fail_timeout=30s;
    server product-service-2:7005 max_fails=3 fail_timeout=30s;
    server product-service-3:7005 max_fails=3 fail_timeout=30s;
}

server {
    listen 80;
    
    location /api/products {
        proxy_pass http://product-service;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        
        # Connection pooling
        proxy_http_version 1.1;
        proxy_set_header Connection "";
        
        # Timeouts
        proxy_connect_timeout 5s;
        proxy_send_timeout 30s;
        proxy_read_timeout 30s;
    }
}
```

#### Kubernetes Scaling
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: product-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: product-service
  template:
    metadata:
      labels:
        app: product-service
    spec:
      containers:
      - name: product-service
        image: qalitrack/product-service:latest
        ports:
        - containerPort: 80
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 60
          periodSeconds: 30

---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: product-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: product-service
  minReplicas: 3
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
```

### Database Scaling

#### Read Replica Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=primary-db;Database=productdb;Username=user;Password=pass;",
    "ReadOnlyConnection": "Host=readonly-db;Database=productdb;Username=reader;Password=pass;"
  }
}
```

```csharp
public class ScalableProductRepository : IProductRepository
{
    private readonly ProductDbContext _writeContext;
    private readonly ProductDbContext _readContext;
    
    public ScalableProductRepository(
        [FromKeyedServices("write")] ProductDbContext writeContext,
        [FromKeyedServices("read")] ProductDbContext readContext)
    {
        _writeContext = writeContext;
        _readContext = readContext;
    }
    
    // Use read replica for queries
    public async Task<Product> GetByIdAsync(string id)
    {
        return await _readContext.Products.FirstOrDefaultAsync(p => p.Id == id);
    }
    
    // Use primary database for writes
    public async Task<Product> AddAsync(Product product)
    {
        _writeContext.Products.Add(product);
        await _writeContext.SaveChangesAsync();
        return product;
    }
}
```

## Performance Best Practices

### Code-Level Optimizations

#### String Operations
```csharp
// Use StringBuilder for multiple concatenations
public string BuildProductDescription(Product product)
{
    var sb = new StringBuilder();
    sb.Append(product.Name);
    
    if (!string.IsNullOrEmpty(product.Description))
    {
        sb.Append(" - ").Append(product.Description);
    }
    
    if (product.IsHazardous)
    {
        sb.Append(" (Hazmat Class ").Append(product.HazmatClass).Append(")");
    }
    
    return sb.ToString();
}

// Use string interpolation for simple cases
public string FormatProductCode(string prefix, int number)
{
    return $"{prefix}-{number:D6}";  // More efficient than string.Format
}

// Use spans for substring operations
public bool IsValidProductCode(ReadOnlySpan<char> code)
{
    return code.Length >= 5 && 
           code.Slice(0, 4).SequenceEqual("PROD".AsSpan()) &&
           code[4] == '-';
}
```

#### Collection Operations
```csharp
// Use appropriate collection types
public class ProductLookup
{
    private readonly Dictionary<string, Product> _productsByCode;
    private readonly Dictionary<string, List<Product>> _productsByCategory;
    
    public ProductLookup(IEnumerable<Product> products)
    {
        // Use ToDictionary for single key lookups
        _productsByCode = products.ToDictionary(p => p.Code, p => p);
        
        // Use ToLookup for multiple values per key
        _productsByCategory = products
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
    
    public Product FindByCode(string code) =>
        _productsByCode.TryGetValue(code, out var product) ? product : null;
    
    public List<Product> FindByCategory(string categoryId) =>
        _productsByCategory.TryGetValue(categoryId, out var products) ? products : new List<Product>();
}

// Use LINQ efficiently
public List<ProductDto> GetActiveProductsByCategory(string categoryId)
{
    return _products
        .Where(p => p.CategoryId == categoryId && p.Status == ProductStatus.Active)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code
        })
        .ToList();
}
```

### Configuration Optimizations

#### Startup Performance
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Use singleton for expensive-to-create services
        services.AddSingleton<IProductCodeGenerator, ProductCodeGenerator>();
        
        // Configure Entity Framework for performance
        services.AddDbContext<ProductDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.EnableRetryOnFailure(3);
            })
            .EnableServiceProviderCaching()  // Cache service provider
            .EnableSensitiveDataLogging(false); // Disable in production
        });
        
        // Configure AutoMapper for performance
        services.AddAutoMapper(config =>
        {
            config.AllowNullCollections = true;
            config.DisableConstructorMapping();
        }, typeof(ProductProfile));
    }
}
```

#### Runtime Configuration
```json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxConcurrentUpgradedConnections": 100,
      "MaxRequestBodySize": 10485760,
      "KeepAliveTimeout": "00:02:00",
      "RequestHeadersTimeout": "00:00:30"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  }
}
```

---

*This performance tuning guide provides comprehensive strategies for optimizing the Product Service. Regular performance testing and monitoring should be used to validate improvements and identify new optimization opportunities.*