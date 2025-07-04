# Product Service Troubleshooting Guide

## Table of Contents

- [Common Issues](#common-issues)
- [Service Startup Problems](#service-startup-problems)
- [Database Connection Issues](#database-connection-issues)
- [API Request Failures](#api-request-failures)
- [Performance Issues](#performance-issues)
- [Authentication Problems](#authentication-problems)
- [Data Consistency Issues](#data-consistency-issues)
- [Integration Failures](#integration-failures)
- [Deployment Issues](#deployment-issues)
- [Monitoring and Diagnostics](#monitoring-and-diagnostics)
- [Emergency Procedures](#emergency-procedures)

## Common Issues

### Service Health Check Failures

#### Symptoms
- Service returns unhealthy status on `/health` endpoint
- Load balancer removes service from rotation
- API Gateway reports service unavailable

#### Diagnostics
```bash
# Check service health directly
curl -v http://localhost:7005/health

# Check service logs
docker logs product-service

# Check service status in container
docker exec -it product-service ps aux
```

#### Common Causes & Solutions

**Database Connection Failure**
```bash
# Check database connectivity
telnet database-host 5432

# Verify connection string
docker exec -it product-service env | grep ConnectionStrings

# Test database connection manually
docker exec -it product-service dotnet ef database update --dry-run
```

**Memory Issues**
```bash
# Check memory usage
docker stats product-service

# Check for memory leaks in logs
docker logs product-service | grep -i "OutOfMemory\|GC\|memory"

# Increase memory limit if needed
docker update --memory=1g product-service
```

**Configuration Problems**
```bash
# Verify configuration files
docker exec -it product-service cat appsettings.json

# Check environment variables
docker exec -it product-service printenv

# Validate configuration with test endpoint
curl http://localhost:7005/api/config/validate
```

### High Response Times

#### Symptoms
- API responses taking longer than 500ms
- Timeout errors from clients
- Users reporting slow performance

#### Diagnostics
```bash
# Check response times
curl -w "@curl-format.txt" http://localhost:7005/api/products

# Monitor database queries
tail -f logs/product-service.log | grep -i "query\|sql"

# Check resource utilization
docker stats product-service
htop
```

#### Solutions

**Database Query Optimization**
```sql
-- Check for missing indexes
SELECT schemaname, tablename, attname, n_distinct, correlation 
FROM pg_stats 
WHERE tablename IN ('Products', 'ProductCategories', 'ProductSpecifications');

-- Analyze slow queries
EXPLAIN ANALYZE SELECT * FROM Products WHERE Name LIKE '%search%';

-- Add missing indexes
CREATE INDEX CONCURRENTLY idx_products_name_search ON Products USING gin(to_tsvector('english', Name));
```

**Connection Pool Issues**
```csharp
// Increase connection pool size in configuration
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=productdb;Username=user;Password=pass;Pooling=true;MinPoolSize=5;MaxPoolSize=100;"
}
```

**Memory Pressure**
```bash
# Check garbage collection
dotnet-counters monitor --process-id $(pgrep -f ProductService.Api) Microsoft.AspNetCore.Hosting

# Increase heap size if needed
docker run -e DOTNET_gcServer=1 -e DOTNET_GCHeapSize=800000000 product-service
```

### Validation Errors

#### Symptoms
- HTTP 400 Bad Request responses
- Validation error messages in API responses
- Data not being saved to database

#### Common Validation Issues

**Product Code Uniqueness**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": ["Product with code 'PROD-001' already exists"]
}
```

**Solution:**
```bash
# Check for existing product codes
curl "http://localhost:7005/api/products/search?searchTerm=PROD-001"

# Use different product code or update existing product
# Implement code generation strategy for uniqueness
```

**Invalid Category References**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": ["Category 'invalid-category-id' not found"]
}
```

**Solution:**
```bash
# List available categories
curl http://localhost:7005/api/products/categories

# Verify category exists before product creation
curl http://localhost:7005/api/products/categories/valid-category-id
```

**Hazmat Classification Errors**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": ["Invalid hazmat class. Must be between 1 and 9"]
}
```

**Solution:**
```csharp
// Ensure hazmat class is valid
var validHazmatClasses = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
if (product.IsHazardous && !validHazmatClasses.Contains(product.HazmatClass))
{
    // Handle invalid hazmat class
}
```

## Service Startup Problems

### Application Won't Start

#### Symptoms
- Container exits immediately after startup
- "Application failed to start" errors
- Port binding failures

#### Diagnostics
```bash
# Check container logs
docker logs product-service --tail=100

# Check if port is already in use
netstat -tulpn | grep :7005
lsof -i :7005

# Verify Docker image
docker inspect product-service:latest

# Check startup command
docker exec -it product-service cat /proc/1/cmdline
```

#### Common Causes & Solutions

**Port Already in Use**
```bash
# Find process using port
sudo lsof -i :7005

# Kill conflicting process
sudo kill -9 $(lsof -t -i:7005)

# Use different port
docker run -p 7006:80 product-service
```

**Missing Dependencies**
```dockerfile
# Ensure all dependencies are included
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY --from=build /app/publish .
RUN apt-get update && apt-get install -y curl
```

**Configuration File Missing**
```bash
# Check if configuration files exist
docker exec -it product-service ls -la /app/
docker exec -it product-service cat /app/appsettings.json
```

**Environment Variables**
```bash
# Verify required environment variables
docker exec -it product-service env | grep -E "(ASPNETCORE|ConnectionStrings)"

# Add missing environment variables
docker run -e ASPNETCORE_ENVIRONMENT=Development -e ConnectionStrings__DefaultConnection="..." product-service
```

### Database Migration Failures

#### Symptoms
- "Database migration failed" errors
- Service starts but database operations fail
- Entity Framework exceptions

#### Diagnostics
```bash
# Check migration status
docker exec -it product-service dotnet ef migrations list

# Test database connection
docker exec -it product-service dotnet ef database update --dry-run

# Check database logs
docker logs database-container
```

#### Solutions

**Apply Missing Migrations**
```bash
# Apply migrations manually
docker exec -it product-service dotnet ef database update

# Or enable automatic migrations
# In Program.cs:
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    dbContext.Database.Migrate();
}
```

**Database Connection Issues**
```bash
# Test database connectivity
docker exec -it database-container psql -U username -d database_name -c "SELECT 1;"

# Check connection string format
"Host=hostname;Database=dbname;Username=user;Password=pass;Port=5432;"
```

## Database Connection Issues

### Connection Pool Exhaustion

#### Symptoms
- "Timeout expired" errors
- Connection pool exhaustion exceptions
- Intermittent database errors

#### Diagnostics
```sql
-- Check active connections (PostgreSQL)
SELECT count(*) as active_connections FROM pg_stat_activity WHERE state = 'active';

-- Check connection pool metrics
SELECT * FROM pg_stat_database WHERE datname = 'productservice';
```

#### Solutions

**Increase Pool Size**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=productdb;Username=user;Password=pass;Pooling=true;MinPoolSize=10;MaxPoolSize=200;ConnectionIdleLifetime=30;"
  }
}
```

**Optimize Connection Usage**
```csharp
// Ensure proper disposal of DbContext
public class ProductRepository : IDisposable
{
    private readonly ProductDbContext _context;
    
    public async Task<Product> GetProductAsync(string id)
    {
        // Use async operations to avoid blocking connections
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

### Database Deadlocks

#### Symptoms
- Deadlock detection errors in logs
- Transactions timing out
- Data consistency issues

#### Diagnostics
```sql
-- Check for deadlocks (PostgreSQL)
SELECT * FROM pg_stat_database_conflicts WHERE datname = 'productservice';

-- Monitor lock waits
SELECT * FROM pg_locks WHERE NOT granted;
```

#### Solutions

**Implement Retry Logic**
```csharp
public async Task<Product> CreateProductWithRetryAsync(Product product)
{
    var maxRetries = 3;
    var delay = TimeSpan.FromMilliseconds(100);
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await _repository.AddAsync(product);
        }
        catch (DbUpdateConcurrencyException) when (i < maxRetries - 1)
        {
            await Task.Delay(delay);
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
        }
    }
    
    throw new InvalidOperationException("Failed to create product after multiple retries");
}
```

**Optimize Transaction Scope**
```csharp
// Keep transactions short and focused
public async Task<ProductDto> RegisterProductAsync(RegisterProductRequest request)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var product = await _productRepository.AddAsync(newProduct);
        await transaction.CommitAsync();
        return _mapper.Map<ProductDto>(product);
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

## API Request Failures

### HTTP 500 Internal Server Error

#### Symptoms
- Generic 500 errors returned to clients
- Exception details in server logs
- Service appears healthy but requests fail

#### Diagnostics
```bash
# Check application logs
tail -f logs/product-service.log | grep -i error

# Test with curl for detailed output
curl -v -X GET http://localhost:7005/api/products

# Check service status
curl http://localhost:7005/health
```

#### Common Causes & Solutions

**Unhandled Exceptions**
```csharp
// Implement global exception handling
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = exception switch
        {
            ArgumentException => 400,
            UnauthorizedAccessException => 401,
            _ => 500
        };
        
        var response = new ApiResponseDto
        {
            Success = false,
            Message = "An error occurred while processing your request",
            Errors = new[] { exception.Message }
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

**Null Reference Exceptions**
```csharp
// Add null checks and defensive programming
public async Task<ProductDto> GetProductByIdAsync(string id)
{
    if (string.IsNullOrWhiteSpace(id))
        throw new ArgumentException("Product ID cannot be null or empty", nameof(id));
    
    var product = await _productRepository.GetByIdAsync(id);
    if (product == null)
        return null; // Or throw NotFoundException
    
    return _mapper.Map<ProductDto>(product);
}
```

### HTTP 401 Unauthorized

#### Symptoms
- Authentication failures
- "Bearer token required" errors
- Valid requests rejected

#### Diagnostics
```bash
# Test without token
curl -v http://localhost:7005/api/products

# Test with invalid token
curl -v -H "Authorization: Bearer invalid-token" http://localhost:7005/api/products

# Check JWT token validity
echo "your-jwt-token" | base64 -d
```

#### Solutions

**Token Validation Issues**
```csharp
// Check JWT configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
```

**Gateway Authentication Issues**
```bash
# Check if gateway is forwarding auth headers
curl -v -H "Authorization: Bearer token" http://gateway:7000/api/products

# Verify gateway configuration
cat configs/gateway/ocelot.json | grep -A5 -B5 auth
```

## Performance Issues

### Slow Database Queries

#### Symptoms
- High response times for database operations
- CPU spikes during queries
- Connection timeouts

#### Diagnostics
```sql
-- Check slow queries (PostgreSQL)
SELECT query, mean_time, calls, total_time
FROM pg_stat_statements
ORDER BY mean_time DESC
LIMIT 10;

-- Analyze specific query
EXPLAIN (ANALYZE, BUFFERS) 
SELECT * FROM Products p 
JOIN ProductCategories c ON p.CategoryId = c.Id 
WHERE p.Name ILIKE '%search%';
```

#### Solutions

**Add Missing Indexes**
```sql
-- Product search optimization
CREATE INDEX CONCURRENTLY idx_products_name_gin ON Products USING gin(to_tsvector('english', Name));
CREATE INDEX CONCURRENTLY idx_products_code ON Products(Code);
CREATE INDEX CONCURRENTLY idx_products_category_status ON Products(CategoryId, Status) WHERE IsDeleted = false;

-- Category hierarchy optimization
CREATE INDEX CONCURRENTLY idx_categories_parent ON ProductCategories(ParentCategoryId) WHERE IsDeleted = false;
```

**Query Optimization**
```csharp
// Use specific projections instead of loading full entities
public async Task<List<ProductSummaryDto>> GetProductSummariesAsync()
{
    return await _context.Products
        .Where(p => !p.IsDeleted)
        .Select(p => new ProductSummaryDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            CategoryName = p.Category.Name
        })
        .ToListAsync();
}

// Use pagination for large result sets
public async Task<PagedResult<ProductDto>> GetProductsPagedAsync(int page, int pageSize)
{
    var query = _context.Products.Where(p => !p.IsDeleted);
    
    var totalCount = await query.CountAsync();
    var products = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<ProductDto>
    {
        Items = _mapper.Map<List<ProductDto>>(products),
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

### Memory Issues

#### Symptoms
- OutOfMemoryException errors
- High memory usage
- Frequent garbage collection

#### Diagnostics
```bash
# Monitor memory usage
docker stats product-service

# Check .NET memory counters
dotnet-counters monitor --process-id $(pgrep -f ProductService.Api)

# Analyze memory dumps
dotnet-dump collect -p $(pgrep -f ProductService.Api)
dotnet-dump analyze memory.dump
```

#### Solutions

**Optimize Entity Loading**
```csharp
// Use AsNoTracking for read-only queries
public async Task<List<ProductDto>> GetProductsForDisplayAsync()
{
    return await _context.Products
        .AsNoTracking()
        .Where(p => !p.IsDeleted)
        .Select(p => new ProductDto { /* properties */ })
        .ToListAsync();
}

// Dispose DbContext properly
public class ProductService : IProductService, IDisposable
{
    private readonly ProductDbContext _context;
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

**Implement Caching**
```csharp
public class CachedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly IMemoryCache _cache;
    
    public async Task<ProductDto> GetProductByIdAsync(string id)
    {
        var cacheKey = $"product:{id}";
        
        if (_cache.TryGetValue(cacheKey, out ProductDto cached))
            return cached;
        
        var product = await _inner.GetProductByIdAsync(id);
        
        if (product != null)
        {
            _cache.Set(cacheKey, product, TimeSpan.FromMinutes(15));
        }
        
        return product;
    }
}
```

## Authentication Problems

### JWT Token Issues

#### Symptoms
- Token validation failures
- "Invalid token" errors
- Authentication working intermittently

#### Diagnostics
```bash
# Decode JWT token
echo "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." | cut -d. -f2 | base64 -d | jq

# Check token expiration
date -d @$(echo "token-payload" | jq -r .exp)

# Verify token signature
# Use online JWT debugger or custom validation tool
```

#### Solutions

**Clock Skew Issues**
```csharp
// Allow for clock skew between services
options.TokenValidationParameters = new TokenValidationParameters
{
    // ... other settings
    ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes of clock skew
};
```

**Token Refresh Issues**
```csharp
// Implement token refresh logic
public class JwtTokenService
{
    public async Task<string> RefreshTokenAsync(string expiredToken)
    {
        // Validate refresh token
        var principal = GetPrincipalFromExpiredToken(expiredToken);
        
        // Generate new token
        return GenerateJwtToken(principal.Claims);
    }
}
```

### Gateway Authorization Problems

#### Symptoms
- Authorization headers not forwarded
- User context missing in service
- Inconsistent authorization behavior

#### Diagnostics
```bash
# Check gateway logs
docker logs gateway-service | grep -i auth

# Verify header forwarding
curl -v -H "Authorization: Bearer token" http://gateway:7000/api/products

# Test direct service access
curl -v -H "Authorization: Bearer token" http://product-service:7005/api/products
```

#### Solutions

**Header Forwarding Configuration**
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/products",
      "UpstreamPathTemplate": "/api/products",
      "AddHeadersToRequest": {
        "X-User-ID": "Claims[user_id]",
        "X-User-Roles": "Claims[roles]",
        "X-Organization-Id": "Claims[org_id]"
      }
    }
  ]
}
```

**Service-Side Header Processing**
```csharp
public class BaseController : ControllerBase
{
    protected string GetUserId()
    {
        return HttpContext.Request.Headers["X-User-ID"].FirstOrDefault() ?? "unknown";
    }
    
    protected bool IsAuthorized(string requiredRole)
    {
        var userRoles = HttpContext.Request.Headers["X-User-Roles"]
            .FirstOrDefault()?.Split(',') ?? Array.Empty<string>();
        
        return userRoles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase);
    }
}
```

## Data Consistency Issues

### Concurrent Update Conflicts

#### Symptoms
- "Concurrency conflict" errors
- Data overwritten by concurrent operations
- Optimistic concurrency failures

#### Diagnostics
```csharp
// Check for concurrency conflicts in logs
// Look for DbUpdateConcurrencyException entries

// Add logging to track concurrent operations
public async Task<Product> UpdateProductAsync(Product product)
{
    try
    {
        _logger.LogInformation("Updating product {ProductId} at {Timestamp}", 
            product.Id, DateTime.UtcNow);
        
        return await _context.SaveChangesAsync() > 0 ? product : null;
    }
    catch (DbUpdateConcurrencyException ex)
    {
        _logger.LogWarning("Concurrency conflict updating product {ProductId}: {Error}", 
            product.Id, ex.Message);
        throw;
    }
}
```

#### Solutions

**Implement Optimistic Concurrency**
```csharp
// Add RowVersion to entities
public class Product : BaseEntity
{
    [Timestamp]
    public byte[] RowVersion { get; set; }
    
    // Other properties...
}

// Handle concurrency conflicts
public async Task<ProductDto> UpdateProductAsync(string id, UpdateProductRequest request)
{
    var product = await _productRepository.GetByIdAsync(id);
    if (product == null)
        throw new NotFoundException("Product not found");
    
    try
    {
        // Update properties
        _mapper.Map(request, product);
        
        var updatedProduct = await _productRepository.UpdateAsync(product);
        return _mapper.Map<ProductDto>(updatedProduct);
    }
    catch (DbUpdateConcurrencyException)
    {
        // Reload entity and retry or return conflict error
        var currentProduct = await _productRepository.GetByIdAsync(id);
        throw new ConcurrencyConflictException("Product was modified by another user", currentProduct);
    }
}
```

**Use Distributed Locking**
```csharp
// For critical operations, use distributed locks
public class DistributedLockProductService : IProductService
{
    private readonly IDistributedLock _distributedLock;
    
    public async Task<ProductDto> UpdateProductAsync(string id, UpdateProductRequest request)
    {
        var lockKey = $"product:update:{id}";
        
        await using var @lock = await _distributedLock.AcquireAsync(lockKey, TimeSpan.FromSeconds(30));
        if (@lock == null)
            throw new InvalidOperationException("Could not acquire lock for product update");
        
        return await _inner.UpdateProductAsync(id, request);
    }
}
```

### Data Migration Issues

#### Symptoms
- Data inconsistencies after migrations
- Foreign key constraint violations
- Missing or corrupted data

#### Diagnostics
```sql
-- Check referential integrity
SELECT p.Id, p.CategoryId 
FROM Products p 
LEFT JOIN ProductCategories c ON p.CategoryId = c.Id 
WHERE c.Id IS NULL;

-- Check for orphaned records
SELECT COUNT(*) FROM ProductSpecifications ps 
LEFT JOIN Products p ON ps.ProductId = p.Id 
WHERE p.Id IS NULL;
```

#### Solutions

**Data Validation Scripts**
```sql
-- Create validation script for post-migration checks
WITH orphaned_products AS (
    SELECT p.Id, p.Code, p.CategoryId
    FROM Products p 
    LEFT JOIN ProductCategories c ON p.CategoryId = c.Id 
    WHERE c.Id IS NULL AND p.IsDeleted = false
)
SELECT 'Orphaned products found: ' || COUNT(*) as validation_result
FROM orphaned_products;
```

**Safe Migration Practices**
```csharp
// Implement data migration with rollback capability
public class ProductDataMigration
{
    public async Task MigrateAsync()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Backup critical data before migration
            await CreateDataBackupAsync();
            
            // Perform migration
            await ExecuteMigrationStepsAsync();
            
            // Validate data integrity
            await ValidateDataIntegrityAsync();
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            await RestoreDataBackupAsync();
            throw;
        }
    }
}
```

## Integration Failures

### Service Communication Issues

#### Symptoms
- Timeout errors when calling other services
- Intermittent service unavailability
- Circuit breaker activation

#### Diagnostics
```bash
# Test service connectivity
curl -v http://user-service:7001/health
curl -v http://customer-service:7003/health

# Check network connectivity
ping user-service
telnet user-service 7001

# Monitor service discovery
curl http://service-discovery:7019/services
```

#### Solutions

**Implement Retry Policies**
```csharp
public class ResilientServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    
    public ResilientServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} after {Delay}ms", retryCount, timespan.TotalMilliseconds);
                });
    }
    
    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content);
    }
}
```

**Circuit Breaker Implementation**
```csharp
public class CircuitBreakerService
{
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreaker;
    
    public CircuitBreakerService()
    {
        _circuitBreaker = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, duration) =>
                {
                    _logger.LogWarning("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset");
                });
    }
}
```

### Message Queue Issues

#### Symptoms
- Messages not being processed
- Dead letter queue accumulation
- Event processing delays

#### Diagnostics
```bash
# Check message queue status (RabbitMQ example)
rabbitmqctl list_queues name messages consumers

# Check dead letter queues
rabbitmqctl list_queues name messages | grep -i dead

# Monitor message rates
rabbitmqctl list_queues name message_stats.publish_details.rate
```

#### Solutions

**Message Processing Retry**
```csharp
public class EventHandler
{
    public async Task<bool> HandleAsync(ProductRegisteredEvent productEvent)
    {
        var maxRetries = 3;
        var currentRetry = 0;
        
        while (currentRetry < maxRetries)
        {
            try
            {
                await ProcessEventAsync(productEvent);
                return true;
            }
            catch (Exception ex) when (currentRetry < maxRetries - 1)
            {
                currentRetry++;
                _logger.LogWarning("Event processing failed, retry {Retry}/{MaxRetries}: {Error}", 
                    currentRetry, maxRetries, ex.Message);
                
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, currentRetry)));
            }
        }
        
        // Send to dead letter queue after max retries
        await SendToDeadLetterQueueAsync(productEvent);
        return false;
    }
}
```

## Deployment Issues

### Container Startup Failures

#### Symptoms
- Containers exit immediately
- Image pull failures
- Resource allocation errors

#### Diagnostics
```bash
# Check container status
docker ps -a | grep product-service

# Inspect container configuration
docker inspect product-service

# Check resource usage
docker stats

# Review startup logs
docker logs product-service --since=10m
```

#### Solutions

**Resource Limits**
```yaml
# docker-compose.yml
services:
  product-service:
    image: qalitrack/product-service:latest
    deploy:
      resources:
        limits:
          memory: 1G
          cpus: '0.5'
        reservations:
          memory: 512M
          cpus: '0.25'
```

**Health Check Configuration**
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:80/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

### Load Balancer Issues

#### Symptoms
- Uneven traffic distribution
- Services removed from load balancer
- Health check failures

#### Diagnostics
```bash
# Check load balancer configuration
curl http://load-balancer/api/health

# Test individual service instances
curl http://product-service-1:7005/health
curl http://product-service-2:7005/health

# Monitor traffic distribution
tail -f /var/log/nginx/access.log | grep product-service
```

#### Solutions

**Health Check Optimization**
```csharp
// Implement comprehensive health check
public class ProductServiceHealthCheck : IHealthCheck
{
    private readonly ProductDbContext _context;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database connectivity
            await _context.Database.CanConnectAsync(cancellationToken);
            
            // Check critical dependencies
            var categoryCount = await _context.ProductCategories.CountAsync(cancellationToken);
            
            return HealthCheckResult.Healthy($"Service is healthy. Categories: {categoryCount}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Service is unhealthy: {ex.Message}");
        }
    }
}
```

## Monitoring and Diagnostics

### Logging Configuration

#### Structured Logging Setup
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/product-service-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

#### Application Metrics
```csharp
public class ProductServiceMetrics
{
    private readonly Counter<int> _productsCreated;
    private readonly Histogram<double> _requestDuration;
    
    public ProductServiceMetrics()
    {
        var meter = new Meter("ProductService");
        
        _productsCreated = meter.CreateCounter<int>(
            "products_created_total",
            description: "Total number of products created");
            
        _requestDuration = meter.CreateHistogram<double>(
            "request_duration_seconds",
            description: "Request duration in seconds");
    }
    
    public void RecordProductCreated() => _productsCreated.Add(1);
    public void RecordRequestDuration(double duration) => _requestDuration.Record(duration);
}
```

### Performance Monitoring

#### Database Query Monitoring
```sql
-- Enable query logging (PostgreSQL)
ALTER SYSTEM SET log_statement = 'all';
ALTER SYSTEM SET log_min_duration_statement = 1000; -- Log queries > 1 second

-- Monitor slow queries
SELECT query, calls, total_time, mean_time, stddev_time
FROM pg_stat_statements
WHERE mean_time > 1000
ORDER BY mean_time DESC;
```

#### Application Performance Monitoring
```csharp
public class PerformanceMonitoringMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            var duration = stopwatch.ElapsedMilliseconds;
            if (duration > 1000) // Log slow requests
            {
                _logger.LogWarning("Slow request: {Method} {Path} took {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    duration);
            }
        }
    }
}
```

## Emergency Procedures

### Service Recovery

#### Immediate Actions
1. **Check Service Status**
   ```bash
   curl http://localhost:7005/health
   docker ps | grep product-service
   ```

2. **Review Recent Logs**
   ```bash
   docker logs product-service --tail=100
   tail -f logs/product-service.log | grep -i error
   ```

3. **Restart Service**
   ```bash
   docker restart product-service
   # Or for Kubernetes
   kubectl rollout restart deployment/product-service
   ```

#### Database Recovery

**Connection Issues**
```bash
# Check database status
docker exec -it database-container pg_isready

# Restart database if needed
docker restart database-container

# Verify connection from service
docker exec -it product-service dotnet ef database update --dry-run
```

**Data Corruption**
```bash
# Restore from backup
pg_restore -h localhost -U username -d productservice backup_file.sql

# Verify data integrity
psql -d productservice -c "SELECT COUNT(*) FROM Products WHERE IsDeleted = false;"
```

### Rollback Procedures

#### Application Rollback
```bash
# Rollback to previous version
docker tag product-service:current product-service:backup
docker pull product-service:previous-stable
docker stop product-service
docker run -d --name product-service product-service:previous-stable

# For Kubernetes
kubectl rollout undo deployment/product-service
```

#### Database Rollback
```bash
# Apply rollback migration
docker exec -it product-service dotnet ef migrations remove

# Or restore from backup
pg_restore -h localhost -U username -d productservice -c backup_before_migration.sql
```

### Escalation Procedures

1. **Level 1**: Development Team
   - Service restart and basic troubleshooting
   - Log analysis and configuration fixes

2. **Level 2**: Infrastructure Team
   - Database and network issues
   - Container and orchestration problems

3. **Level 3**: Architecture Team
   - Design and performance issues
   - Cross-service integration problems

#### Contact Information
- **Development Team**: dev-team@qalitrack.com
- **Infrastructure Team**: infra-team@qalitrack.com
- **On-Call Engineer**: +1-555-0199 (24/7)

---

*This troubleshooting guide should be kept up-to-date with new issues and solutions as they are discovered and resolved.*