# Architecture

The QaliTrack Master Data Service follows a **modular Django-style architecture** with clean separation of concerns and modern .NET patterns.

## Overall Architecture

```
┌─────────────────────────────────────┐
│           API Gateway               │
│      /api/masterdata/*              │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│       QaliTrack MasterData          │
│         (Consolidated)              │
│                                     │
│  ┌─────────────────────────────┐    │
│  │      API Layer              │    │
│  │  - Controllers              │    │
│  │  - DTOs                     │    │
│  │  - Middleware               │    │
│  └─────────────┬───────────────┘    │
│                │                    │
│  ┌─────────────▼───────────────┐    │
│  │      Core Layer             │    │
│  │  - Entities                 │    │
│  │  - Business Logic           │    │
│  │  - Interfaces               │    │
│  │  - Modules                  │    │
│  └─────────────┬───────────────┘    │
│                │                    │
│  ┌─────────────▼───────────────┐    │
│  │   Infrastructure Layer      │    │
│  │  - Database Context         │    │
│  │  - Repositories             │    │
│  │  - External Services        │    │
│  └─────────────────────────────┘    │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│           Database                  │
│      (PostgreSQL/SQLite)           │
└─────────────────────────────────────┘
```

## Layer Responsibilities

### API Layer (`QaliTrack.MasterData.Api`)
- **Controllers**: Handle HTTP requests and responses
- **DTOs**: Data Transfer Objects for API contracts
- **Middleware**: Cross-cutting concerns (auth, logging, error handling)
- **Configuration**: Service registration and app configuration

### Core Layer (`QaliTrack.MasterData.Core`)
- **Entities**: Domain models and business objects
- **Modules**: Organized by business domain
- **Interfaces**: Contracts for repositories and services
- **Common**: Shared utilities and base classes

### Infrastructure Layer (`QaliTrack.MasterData.Infrastructure`)
- **Data Access**: Entity Framework DbContext and configurations
- **Repositories**: Data access implementations
- **External Services**: Third-party integrations
- **Migrations**: Database schema management

## Module Organization

Each business domain is organized as a self-contained module:

```
Core/Modules/
├── BusinessEntities/
│   ├── Entities/
│   │   ├── BusinessEntity.cs
│   │   ├── CustomerProfile.cs
│   │   └── SupplierProfile.cs
│   └── DTOs/
│       ├── BusinessEntityDto.cs
│       └── CreateBusinessEntityDto.cs
├── Vehicles/
│   ├── Entities/
│   │   ├── Vehicle.cs
│   │   ├── VehicleRegistration.cs
│   │   └── VehicleMaintenance.cs
│   └── DTOs/
└── [Other Modules...]
```

## Design Patterns

### Repository Pattern
Each module uses the repository pattern for data access:

```csharp
public interface IRepository<TEntity, TId> 
    where TEntity : BaseEntity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id);
    Task<PagedResult<TEntity>> GetPagedAsync(QueryParameters parameters);
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id);
}
```

### Django-Style URL Routing
Controllers follow Django-style conventions:

```csharp
[Route("api/masterdata/[controller]")]
[ApiController]
public class VehiclesController : ControllerBase
{
    // GET /api/masterdata/vehicles
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleSummaryDto>>>> GetVehicles()
    
    // GET /api/masterdata/vehicles/{id}/maintenance
    [HttpGet("{id}/maintenance")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleMaintenanceDto>>>> GetVehicleMaintenance(int id)
}
```

### AutoMapper Integration
All DTOs are mapped using AutoMapper profiles:

```csharp
public class VehicleProfile : Profile
{
    public VehicleProfile()
    {
        CreateMap<Vehicle, VehicleSummaryDto>();
        CreateMap<CreateVehicleDto, Vehicle>();
        CreateMap<Vehicle, VehicleDetailDto>();
    }
}
```

## Database Design

### Multi-Tenancy
All entities support multi-tenancy via `OrganizationId`:

```csharp
public abstract class BaseEntity<TId>
{
    public TId Id { get; set; }
    public int OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
```

### Relationships
Cross-module relationships are handled via dedicated entities:

```csharp
// Driver-Vehicle Assignment
public class DriverVehicleAssignment : BaseEntity<int>
{
    public int DriverId { get; set; }
    public int VehicleId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? UnassignedDate { get; set; }
    public AssignmentType AssignmentType { get; set; }
    
    // Navigation properties
    public Driver Driver { get; set; }
    public Vehicle Vehicle { get; set; }
}
```

## API Conventions

### Consistent Response Format
All endpoints return consistent response structure:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public object? Error { get; set; }
}
```

### HTTP Method Patterns
- `GET` - Retrieve (list/detail)
- `POST` - Create
- `PUT` - Full update
- `PATCH` - Partial update  
- `DELETE` - Remove
- `HEAD` - Check existence
- `OPTIONS` - Discover capabilities

### Query Parameters
Standardized query parameters across all endpoints:

```csharp
public class QueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public string? Search { get; set; }
    public string[]? SearchFields { get; set; }
}
```

## Configuration Management

### Environment-Based Configuration
```csharp
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=masterdata_dev;..."
  },
  "Logging": {
    "LogLevel": {
      "QaliTrack.MasterData": "Debug"
    }
  }
}
```

### Docker Configuration
Environment-specific settings via Docker Compose:

```yaml
services:
  qalitrack-masterdata:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=masterdata;...
```

## Error Handling

### Global Exception Middleware
Centralized error handling with consistent response format:

```csharp
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
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

## Performance Considerations

### Database Optimization
- **Indexing**: Strategic indexes on commonly queried fields
- **Pagination**: All list endpoints support pagination
- **Eager Loading**: Optimized Include() statements for related data

### Caching Strategy
- **Memory Caching**: Frequently accessed reference data
- **Response Caching**: Static/semi-static endpoints
- **Distributed Caching**: For multi-instance deployments

### Async/Await
All I/O operations use async patterns:

```csharp
public async Task<Vehicle?> GetVehicleAsync(int id)
{
    return await _context.Vehicles
        .Include(v => v.VehicleRegistrations)
        .FirstOrDefaultAsync(v => v.Id == id);
}
```