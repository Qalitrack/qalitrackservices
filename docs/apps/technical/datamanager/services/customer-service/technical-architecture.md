# Customer Service Technical Architecture

## Overview

The Customer Service is a microservice built using .NET 8 and ASP.NET Core, following clean architecture principles and Domain-Driven Design (DDD) patterns. It provides comprehensive customer relationship management capabilities within the QaliTrack ecosystem, with a focus on maintainability, testability, and scalability.

## Architecture Patterns

### Clean Architecture

The service follows Uncle Bob's Clean Architecture pattern with clear separation of concerns:

```
CustomerService/
├── CustomerService.Api/          # Presentation Layer
├── CustomerService.Core/         # Business Logic Layer  
└── CustomerService.Infrastructure/ # Data Access Layer
```

**Dependency Flow:**
```
Api → Core ← Infrastructure
```

- **API Layer**: Controllers, DTOs, Configuration
- **Core Layer**: Entities, Services, Interfaces, Business Logic
- **Infrastructure Layer**: Repositories, Database Context, External Services

### Domain-Driven Design (DDD)

#### Bounded Context

The Customer Service represents the "Customer Management" bounded context within QaliTrack:

**Core Domain Concepts:**
- **Customer**: Central aggregate root
- **Contact**: Customer communication entities
- **Contract**: Legal and service agreements
- **Billing**: Financial and payment information
- **Credit**: Credit management and limits
- **Location**: Physical service locations
- **Document**: File and document management

#### Aggregate Design

**Customer Aggregate:**
```csharp
Customer (Aggregate Root)
├── CustomerContact (Entity)
├── CustomerContract (Entity)
├── CustomerBilling (Value Object)
├── CustomerCredit (Value Object)
├── CustomerLocation (Entity)
├── CustomerDocument (Entity)
└── CustomerPreference (Value Object)
```

**Invariants Enforced:**
- Customer email uniqueness
- Tax number uniqueness when provided
- Credit limit non-negative validation
- Primary contact per contact type constraint

## Technology Stack

### Core Technologies

**Runtime & Framework:**
- **.NET 8**: Latest LTS version with performance improvements
- **ASP.NET Core 8**: Web API framework with minimal APIs support
- **C# 12**: Latest language features and nullable reference types

**Data Access:**
- **Entity Framework Core 8**: ORM with advanced querying capabilities
- **SQLite**: Development database with file-based storage
- **SQL Server**: Production database (configurable)
- **PostgreSQL**: Alternative production database support

**Serialization & Mapping:**
- **System.Text.Json**: High-performance JSON serialization
- **AutoMapper**: Object-to-object mapping with configuration
- **FluentValidation**: Comprehensive validation framework

**Documentation & Testing:**
- **Swagger/OpenAPI**: API documentation and testing
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for unit tests
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing

### Dependencies and NuGet Packages

**Core Dependencies:**
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="FluentValidation" Version="11.8.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

**Testing Dependencies:**
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="xunit" Version="2.6.1" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
<PackageReference Include="Moq" Version="4.20.69" />
```

## Application Architecture

### Layered Architecture Breakdown

#### Presentation Layer (CustomerService.Api)

**Controllers:**
```csharp
CustomerService.Api/
├── Controllers/
│   ├── BaseController.cs          # Common controller functionality
│   ├── CustomersController.cs     # Customer CRUD operations
│   ├── ContactsController.cs      # Contact management
│   └── ContractsController.cs     # Contract management
├── Program.cs                     # Application configuration
└── appsettings.json              # Configuration settings
```

**BaseController Features:**
- Organization and user context extraction
- Standardized response handling
- Error response formatting
- Common result transformation

**Controller Responsibilities:**
- HTTP request/response handling
- Input validation coordination
- Business service orchestration
- Response DTO transformation

#### Business Logic Layer (CustomerService.Core)

**Domain Entities:**
```csharp
CustomerService.Core/
├── Entities/
│   ├── BaseEntity.cs             # Common entity properties
│   ├── Customer.cs               # Customer aggregate root
│   ├── CustomerContact.cs        # Customer contacts
│   ├── CustomerContract.cs       # Service contracts
│   ├── CustomerBilling.cs        # Billing information
│   ├── CustomerCredit.cs         # Credit management
│   ├── CustomerLocation.cs       # Service locations
│   ├── CustomerDocument.cs       # Document management
│   └── CustomerPreference.cs     # Customer preferences
```

**Data Transfer Objects (DTOs):**
```csharp
CustomerService.Core/
├── DTOs/
│   ├── ApiResponseDto.cs         # Standard API response wrapper
│   ├── CustomerDto.cs            # Customer data transfer
│   ├── CustomerContactDto.cs     # Contact information
│   ├── CustomerContractDto.cs    # Contract details
│   ├── CustomerBillingDto.cs     # Billing information
│   └── CustomerCreditDto.cs      # Credit information
```

**Business Services:**
```csharp
CustomerService.Core/
├── Services/
│   └── CustomerService.cs        # Core business logic
├── Interfaces/
│   ├── ICustomerService.cs       # Service contract
│   ├── ICustomerRepository.cs    # Repository contract
│   └── IRepository.cs            # Generic repository contract
```

**Validation Rules:**
```csharp
CustomerService.Core/
├── Validators/
│   ├── RegisterCustomerValidator.cs     # Customer registration validation
│   ├── UpdateCustomerValidator.cs       # Customer update validation
│   └── CreateCustomerContactValidator.cs # Contact validation
```

#### Data Access Layer (CustomerService.Infrastructure)

**Database Context:**
```csharp
CustomerService.Infrastructure/
├── Data/
│   └── CustomerDbContext.cs      # EF Core database context
└── Repositories/
    ├── Repository.cs             # Generic repository implementation
    └── CustomerRepository.cs     # Customer-specific repository
```

**Repository Pattern Implementation:**
- Generic repository for common CRUD operations
- Specialized repositories for complex queries
- Unit of Work pattern for transaction management
- Async/await for database operations

### Dependency Injection Configuration

**Service Registration (Program.cs):**
```csharp
// Database Configuration
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlite(connectionString));

// Repository Registration
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Business Service Registration
builder.Services.AddScoped<ICustomerService, CustomerService>();

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(CustomerProfile));

// Validation Registration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterCustomerValidator>();
```

## Data Access Architecture

### Entity Framework Core Configuration

#### Database Context Design

**CustomerDbContext Features:**
- **Convention-based Configuration**: Follows EF Core conventions
- **Fluent API Configuration**: Explicit relationship and constraint definition
- **Change Tracking**: Automatic audit field population
- **Connection String Management**: Environment-specific configuration

**Model Configuration Example:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Customer>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
        entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(255);
        entity.HasIndex(e => e.ContactEmail).IsUnique();
        entity.HasIndex(e => e.TaxNumber).IsUnique()
              .HasFilter("[TaxNumber] IS NOT NULL");
    });
}
```

#### Repository Pattern Implementation

**Generic Repository:**
```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly CustomerDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(CustomerDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    // Additional CRUD operations...
}
```

**Specialized Repository:**
```csharp
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(CustomerDbContext context) : base(context) { }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.ContactEmail == email);
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(c => c.Name.Contains(searchTerm) ||
                       c.ContactEmail.Contains(searchTerm) ||
                       c.TaxNumber == searchTerm)
            .ToListAsync();
    }
}
```

### Database Migration Strategy

**Migration Management:**
- **Code-First Approach**: Database schema defined in C# entities
- **Automatic Migrations**: Development environment auto-migration
- **Explicit Migrations**: Production environment manual migration
- **Rollback Support**: Down migrations for rollback scenarios

**Migration Commands:**
```bash
# Create new migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script
```

## Business Logic Architecture

### Service Layer Design

#### Core Business Service

**CustomerService Implementation:**
```csharp
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterCustomerRequest> _registerValidator;

    public async Task<CustomerDto> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        // 1. Validation
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // 2. Business Rule Enforcement
        var existingCustomer = await _customerRepository.GetByEmailAsync(request.ContactEmail);
        if (existingCustomer != null)
            throw new BusinessRuleException("Email address already exists");

        // 3. Entity Creation
        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            ContactEmail = request.ContactEmail,
            // ... other properties
        };

        // 4. Persistence
        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();

        // 5. Response Mapping
        return _mapper.Map<CustomerDto>(customer);
    }
}
```

#### Validation Strategy

**FluentValidation Implementation:**
```csharp
public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerRequest>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(500).WithMessage("Name cannot exceed 500 characters");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be non-negative");
    }
}
```

### Error Handling Architecture

#### Exception Hierarchy

**Custom Exceptions:**
```csharp
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityType, string id) 
        : base($"{entityType} with ID {id} not found") { }
}

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }
    
    public ValidationException(IEnumerable<ValidationFailure> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}
```

#### Global Exception Handling

**Exception Middleware:**
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            ValidationException ex => new ApiResponseDto
            {
                Success = false,
                Message = ex.Message,
                Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
            },
            EntityNotFoundException ex => new ApiResponseDto
            {
                Success = false,
                Message = ex.Message
            },
            BusinessRuleException ex => new ApiResponseDto
            {
                Success = false,
                Message = ex.Message
            },
            _ => new ApiResponseDto
            {
                Success = false,
                Message = "An error occurred while processing the request"
            }
        };

        response.StatusCode = exception switch
        {
            ValidationException => 400,
            EntityNotFoundException => 404,
            BusinessRuleException => 409,
            _ => 500
        };

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
```

## API Design Architecture

### RESTful API Design

#### Resource-Oriented URLs

**Customer Resources:**
```
GET    /api/customers              # Get customers with pagination
POST   /api/customers              # Create new customer
GET    /api/customers/{id}         # Get specific customer
PUT    /api/customers/{id}         # Update customer
DELETE /api/customers/{id}         # Delete customer
```

**Sub-Resource Management:**
```
GET    /api/customers/{id}/contacts     # Get customer contacts
POST   /api/customers/{id}/contacts     # Add contact to customer
PUT    /api/contacts/{id}               # Update specific contact
DELETE /api/contacts/{id}               # Delete specific contact
```

#### HTTP Status Code Usage

**Success Responses:**
- **200 OK**: Successful GET, PUT operations
- **201 Created**: Successful POST operations
- **204 No Content**: Successful DELETE operations

**Client Error Responses:**
- **400 Bad Request**: Validation errors, malformed requests
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Business rule violations

**Server Error Responses:**
- **500 Internal Server Error**: Unhandled server errors
- **503 Service Unavailable**: Service temporarily unavailable

### Response Format Standardization

#### Consistent Response Wrapper

**Success Response:**
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* Response data */ },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Name is required",
    "Email format is invalid"
  ],
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

#### Pagination Support

**Pagination Parameters:**
```csharp
public class PaginationParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
}
```

**Paginated Response:**
```json
{
  "success": true,
  "data": {
    "items": [ /* Array of items */ ],
    "totalCount": 150,
    "pageSize": 20,
    "currentPage": 1,
    "totalPages": 8
  }
}
```

## Security Architecture

### Authentication and Authorization

#### Header-Based Authentication

**Required Headers:**
```http
X-Organization-Id: {organization-id}
X-User-Id: {user-id}
```

**BaseController Security:**
```csharp
public class BaseController : ControllerBase
{
    protected string GetOrganizationId()
    {
        return HttpContext.Request.Headers["X-Organization-Id"].FirstOrDefault() 
               ?? "default-org";
    }

    protected string GetUserId()
    {
        return HttpContext.Request.Headers["X-User-Id"].FirstOrDefault() 
               ?? "system";
    }
}
```

#### Data Isolation

**Multi-Tenant Design:**
- Organization-based data isolation
- Row-level security through organization context
- Automatic filtering by organization ID
- Audit trail with user and organization tracking

### Input Validation and Sanitization

#### Validation Layers

**1. Model Validation:**
```csharp
[Required]
[EmailAddress]
[StringLength(255)]
public string ContactEmail { get; set; } = string.Empty;
```

**2. FluentValidation:**
```csharp
RuleFor(x => x.ContactEmail)
    .NotEmpty()
    .EmailAddress()
    .MaximumLength(255);
```

**3. Business Rule Validation:**
```csharp
var existingCustomer = await _repository.GetByEmailAsync(request.ContactEmail);
if (existingCustomer != null)
    throw new BusinessRuleException("Email already exists");
```

#### SQL Injection Prevention

**Parameterized Queries:**
- Entity Framework Core automatic parameterization
- No direct SQL execution
- LINQ-to-SQL query translation
- Stored procedure support with parameters

**Example Safe Query:**
```csharp
public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
{
    return await _context.Customers
        .Where(c => c.Name.Contains(searchTerm) ||  // Parameterized
                   c.ContactEmail.Contains(searchTerm))
        .ToListAsync();
}
```

## Performance Architecture

### Caching Strategy

#### In-Memory Caching

**Cache Configuration:**
```csharp
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
    options.CompactionPercentage = 0.25;
});
```

**Caching Implementation:**
```csharp
public async Task<CustomerDto?> GetCustomerAsync(string id)
{
    var cacheKey = $"customer_{id}";
    
    if (_cache.TryGetValue(cacheKey, out CustomerDto cachedCustomer))
        return cachedCustomer;

    var customer = await _repository.GetByIdAsync(id);
    if (customer == null) return null;

    var customerDto = _mapper.Map<CustomerDto>(customer);
    
    _cache.Set(cacheKey, customerDto, TimeSpan.FromMinutes(15));
    
    return customerDto;
}
```

#### Cache Invalidation

**Cache Invalidation Strategy:**
- Time-based expiration (15 minutes default)
- Event-based invalidation on data changes
- Cache warming for frequently accessed data
- Distributed cache for multi-instance deployments

### Database Performance

#### Query Optimization

**Efficient Queries:**
```csharp
// Optimized customer search with includes
public async Task<Customer?> GetCustomerDetailsAsync(string id)
{
    return await _context.Customers
        .Include(c => c.Contacts.Where(contact => contact.IsActive))
        .Include(c => c.Contracts.Where(contract => contract.Status == ContractStatus.Active))
        .Include(c => c.Billing)
        .Include(c => c.Credit)
        .Include(c => c.Locations.Where(location => location.IsActive))
        .FirstOrDefaultAsync(c => c.Id == id);
}
```

**Pagination Implementation:**
```csharp
public async Task<(IEnumerable<Customer> customers, int totalCount)> GetPagedAsync(
    int page, int pageSize, string? search = null)
{
    var query = _context.Customers.AsQueryable();
    
    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(c => c.Name.Contains(search) ||
                                c.ContactEmail.Contains(search));
    }
    
    var totalCount = await query.CountAsync();
    
    var customers = await query
        .OrderBy(c => c.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return (customers, totalCount);
}
```

#### Indexing Strategy

**Database Indexes:**
```sql
-- Customer table indexes
CREATE INDEX IX_Customer_Name ON Customer(Name);
CREATE INDEX IX_Customer_Status ON Customer(Status);
CREATE UNIQUE INDEX IX_Customer_ContactEmail ON Customer(ContactEmail);
CREATE UNIQUE INDEX IX_Customer_TaxNumber ON Customer(TaxNumber) 
    WHERE TaxNumber IS NOT NULL;

-- Contact table indexes
CREATE INDEX IX_CustomerContact_CustomerId ON CustomerContact(CustomerId);
CREATE INDEX IX_CustomerContact_ContactType ON CustomerContact(ContactType);
```

### Asynchronous Processing

#### Async/Await Pattern

**Service Layer Async Implementation:**
```csharp
public async Task<CustomerDto> RegisterCustomerAsync(RegisterCustomerRequest request)
{
    // Validation
    var validationResult = await _validator.ValidateAsync(request);
    
    // Business logic
    var customer = await CreateCustomerEntityAsync(request);
    
    // Persistence
    await _repository.AddAsync(customer);
    await _repository.SaveChangesAsync();
    
    // Response
    return _mapper.Map<CustomerDto>(customer);
}
```

**Controller Async Implementation:**
```csharp
[HttpPost]
public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request)
{
    try
    {
        var customer = await _customerService.RegisterCustomerAsync(request);
        return HandleResult(Success(customer, "Customer registered successfully"));
    }
    catch (Exception ex)
    {
        return HandleResult(Error<CustomerDto>(ex.Message));
    }
}
```

## Monitoring and Observability

### Health Checks

#### Health Check Configuration

**Health Check Registration:**
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CustomerDbContext>("database")
    .AddCheck("self", () => HealthCheckResult.Healthy("Service is running"));
```

**Health Check Endpoint:**
```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

#### Custom Health Checks

**Database Connectivity Check:**
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly CustomerDbContext _context;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
```

### Logging Architecture

#### Structured Logging

**Logging Configuration:**
```csharp
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddEventLog();
});
```

**Structured Logging Implementation:**
```csharp
public class CustomerService : ICustomerService
{
    private readonly ILogger<CustomerService> _logger;

    public async Task<CustomerDto> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        _logger.LogInformation("Starting customer registration for {CustomerName}", 
                              request.Name);
        
        try
        {
            var customer = await ProcessRegistrationAsync(request);
            
            _logger.LogInformation("Customer registration completed successfully. " +
                                 "CustomerId: {CustomerId}, Name: {CustomerName}", 
                                 customer.Id, customer.Name);
            
            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Customer registration failed for {CustomerName}", 
                           request.Name);
            throw;
        }
    }
}
```

### Performance Monitoring

#### Metrics Collection

**Performance Counters:**
- Request duration tracking
- Database query execution time
- Memory usage monitoring
- Cache hit/miss ratios

**Custom Metrics:**
```csharp
public class CustomerMetrics
{
    private readonly IMetricsCollector _metrics;
    
    public void TrackCustomerRegistration(string customerType)
    {
        _metrics.Increment("customer_registrations_total", 
                          new Dictionary<string, string> 
                          { 
                              ["customer_type"] = customerType 
                          });
    }
    
    public void TrackQueryDuration(string operation, TimeSpan duration)
    {
        _metrics.RecordValue("query_duration_ms", duration.TotalMilliseconds,
                           new Dictionary<string, string> 
                           { 
                               ["operation"] = operation 
                           });
    }
}
```

## Deployment Architecture

### Containerization

#### Docker Configuration

**Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CustomerService.Api/CustomerService.Api.csproj", "CustomerService.Api/"]
COPY ["CustomerService.Core/CustomerService.Core.csproj", "CustomerService.Core/"]
COPY ["CustomerService.Infrastructure/CustomerService.Infrastructure.csproj", "CustomerService.Infrastructure/"]
RUN dotnet restore "CustomerService.Api/CustomerService.Api.csproj"

COPY . .
WORKDIR "/src/CustomerService.Api"
RUN dotnet build "CustomerService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomerService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerService.Api.dll"]
```

**Docker Compose:**
```yaml
version: '3.8'
services:
  customer-service:
    build: .
    ports:
      - "7003:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Data Source=/data/customerservice.db
    volumes:
      - customer-data:/data
    depends_on:
      - database

  database:
    image: postgres:15
    environment:
      POSTGRES_DB: customerservice
      POSTGRES_USER: qalitrack
      POSTGRES_PASSWORD: password
    volumes:
      - postgres-data:/var/lib/postgresql/data

volumes:
  customer-data:
  postgres-data:
```

### Scalability Considerations

#### Horizontal Scaling

**Stateless Design:**
- No in-memory session state
- Database-backed data storage
- External cache for shared data
- Load balancer compatibility

**Load Balancing Configuration:**
```nginx
upstream customer-service {
    server customer-service-1:8080;
    server customer-service-2:8080;
    server customer-service-3:8080;
}

server {
    listen 80;
    server_name customer-api.qalitrack.com;
    
    location / {
        proxy_pass http://customer-service;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

#### Database Scaling

**Read Replicas:**
- Master-slave database configuration
- Read operations routed to replicas
- Write operations to master database
- Automatic failover configuration

**Connection Pooling:**
```csharp
builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
}, ServiceLifetime.Scoped);
```

## Integration Architecture

### Message Bus Integration

#### Event Publishing

**Domain Events:**
```csharp
public class CustomerRegisteredEvent
{
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string OrganizationId { get; set; }
    public DateTime RegisteredAt { get; set; }
}

public class CustomerEventPublisher
{
    private readonly IMessageBus _messageBus;
    
    public async Task PublishCustomerRegisteredAsync(Customer customer)
    {
        var eventMessage = new CustomerRegisteredEvent
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            OrganizationId = customer.OrganizationId,
            RegisteredAt = customer.CreatedAt
        };
        
        await _messageBus.PublishAsync("customer.registered", eventMessage);
    }
}
```

#### External Service Integration

**HTTP Client Configuration:**
```csharp
builder.Services.AddHttpClient<UserServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://user-service.qalitrack.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

public class UserServiceClient
{
    private readonly HttpClient _httpClient;
    
    public async Task<UserDto?> GetUserAsync(string userId)
    {
        var response = await _httpClient.GetAsync($"api/users/{userId}");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDto>(json);
        }
        return null;
    }
}
```

### API Versioning

#### Version Strategy

**URL-based Versioning:**
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CustomersController : BaseController
{
    // v1.0 implementation
}

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class CustomersV2Controller : BaseController
{
    // v2.0 implementation with breaking changes
}
```

**API Versioning Configuration:**
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version")
    );
});
```

This technical architecture provides a comprehensive foundation for understanding the Customer Service implementation, enabling developers to maintain, extend, and integrate the service effectively within the QaliTrack ecosystem.