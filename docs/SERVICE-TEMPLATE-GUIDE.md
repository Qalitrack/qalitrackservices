# QaliTrack Service Template System

A comprehensive service generation system for creating new microservices in the QaliTrack platform.

## Overview

The service template system enables rapid creation of new microservices with:
- **Clean Architecture** (API, Core, Infrastructure layers)
- **Entity Framework** with SQLite database
- **AutoMapper** for object mapping
- **Swagger/OpenAPI** documentation
- **Structured logging** with Serilog
- **Health checks** and monitoring
- **Gateway integration** ready

## Quick Start

### Interactive Mode (Recommended)

For **masterdata services** (users, customers, products, etc.):
```bash
make generate-masterdata
```

For **datamanager services** (analytics, compliance, transactions, etc.):
```bash
make generate-datamanager
```

### Command Line Mode

```bash
# Masterdata service example
make generate-service TYPE=masterdata SERVICE=inventory-service ENTITY=inventory DESC="Inventory Management"

# Datamanager service example  
make generate-service TYPE=datamanager SERVICE=analytics-service ENTITY=analytics DESC="Analytics Processing"
```

### Direct Script Usage

```bash
# From repository root
python3 scripts/generate-service.py masterdata inventory-service inventory "Inventory Management Service"
python3 scripts/generate-service.py datamanager analytics-service analytics "Analytics Processing Service"
```

## Service Types

### Masterdata Services
Located in `packages/microservices/masterdata/`

**Purpose**: Manage core business entities and relationships
**Examples**:
- `user-service` - User authentication and management
- `customer-service` - Customer relationship management
- `product-service` - Product catalog and specifications
- `driver-service` - Driver profiles and licenses
- `vehicle-service` - Vehicle fleet management
- `organization-service` - Multi-tenant organization management

### Datamanager Services  
Located in `packages/microservices/datamanager/`

**Purpose**: Handle data processing, analytics, and workflows
**Examples**:
- `analytics-service` - Data analytics and reporting
- `transaction-service` - Transaction processing and history
- `compliance-service` - Regulatory compliance checking
- `weight-data-service` - Weighbridge data management
- `data-sync-service` - Data synchronization workflows
- `archive-service` - Data archival and retrieval

## Generated Structure

```
<service-name>/
├── src/
│   ├── <ServiceName>.Api/           # Web API layer
│   │   ├── Controllers/             # REST API controllers
│   │   ├── Program.cs              # Application entry point
│   │   └── appsettings.json        # Configuration
│   ├── <ServiceName>.Core/          # Business logic layer
│   │   ├── DTOs/                   # Data transfer objects
│   │   ├── Entities/               # Domain entities
│   │   ├── Interfaces/             # Service/repository interfaces
│   │   ├── Mappings/               # AutoMapper profiles
│   │   └── Services/               # Business logic services
│   └── <ServiceName>.Infrastructure/ # Data access layer
│       ├── Data/                   # Database context
│       └── Repositories/           # Data repositories
├── tests/
│   └── <ServiceName>.Tests/        # Unit & integration tests
├── Dockerfile                      # Container configuration
├── README.md                       # Service documentation
└── <ServiceName>.sln              # Solution file
```

## Template Placeholders

The template uses these placeholders for customization:

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `{{ServiceName}}` | PascalCase service name | `InventoryService` |
| `{{service-name}}` | kebab-case service name | `inventory-service` |
| `{{EntityName}}` | PascalCase entity name | `Inventory` |
| `{{entityName}}` | camelCase entity name | `inventory` |
| `{{ServiceDescription}}` | Service description | `Inventory Management Service` |
| `{{api-prefix}}` | API route prefix | `inventories` |

## Naming Conventions

### Service Names
- Use **kebab-case**: `inventory-service`, `customer-service`
- End with `-service`: `analytics-service`, `compliance-service`
- Be descriptive but concise: `weight-data-service` not `weighbridge-data-management-service`

### Entity Names
- Use **PascalCase**: `Inventory`, `Customer`, `WeightRecord`
- Singular form: `Product` not `Products`
- Clear and domain-specific: `Driver` not `Person`

## Customization Guide

### 1. Update Domain Entities

Edit `src/<ServiceName>.Core/Entities/MainEntity.cs`:
```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public bool IsHazmat { get; set; }
    
    // Navigation properties
    public virtual ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
}
```

### 2. Configure Database Relationships

Update `src/<ServiceName>.Infrastructure/Data/<ServiceName>DbContext.cs`:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
        entity.HasIndex(e => e.SKU).IsUnique();
    });
}
```

### 3. Add Business Logic

Implement services in `src/<ServiceName>.Core/Services/`:
```csharp
public class ProductService : IProductService
{
    public async Task<bool> ValidateHazmatClassificationAsync(string productId)
    {
        // Custom business logic
    }
}
```

### 4. Extend API Controllers

Add endpoints in `src/<ServiceName>.Api/Controllers/`:
```csharp
[HttpGet("{id}/specifications")]
public async Task<IActionResult> GetProductSpecifications(string id)
{
    // Custom endpoints
}
```

## Best Practices

### Entity Design
1. **Inherit from BaseEntity** for audit fields
2. **Use proper data types** (decimal for money, DateTime for dates)
3. **Add navigation properties** for relationships
4. **Include domain-specific properties**

### Service Layer
1. **Keep services focused** on single responsibilities
2. **Use DTOs** for API boundaries
3. **Implement proper validation**
4. **Add comprehensive error handling**

### Database
1. **Configure relationships** in OnModelCreating
2. **Add proper indexes** for performance
3. **Use constraints** for data integrity
4. **Consider soft deletes** (included in BaseEntity)

### API Design
1. **Follow REST conventions**
2. **Use consistent response formats**
3. **Add proper HTTP status codes**
4. **Include comprehensive documentation**

## Integration Features

### Gateway Integration
Generated services automatically include:
- **Health checks** at `/health`
- **CORS configuration** for cross-origin requests
- **Consistent API response format**
- **Swagger documentation** accessible via gateway

### Authentication (Optional)
The template includes commented JWT authentication code:
```csharp
// Uncomment in Program.cs for authenticated services
// builder.Services.AddAuthentication(...)
// app.UseAuthentication();
// app.UseAuthorization();
```

### Database Migrations
For production databases, generate migrations:
```bash
dotnet ef migrations add InitialCreate --project src/<ServiceName>.Infrastructure
dotnet ef database update --project src/<ServiceName>.Infrastructure
```

## Testing

### Running Tests
```bash
# All tests
dotnet test

# Specific project
dotnet test tests/<ServiceName>.Tests/

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure
- **Unit tests** for business logic
- **Integration tests** for database operations  
- **API tests** for endpoint functionality
- **Mock services** for external dependencies

## Deployment

### Docker
```bash
# Build image
docker build -t <service-name> .

# Run container
docker run -p 5000:8080 <service-name>
```

### Production Considerations
1. **Update connection strings** for production databases
2. **Configure logging** levels and outputs
3. **Set up monitoring** and alerting
4. **Implement backup strategies**
5. **Configure scaling** policies

## Troubleshooting

### Common Issues

**Template not found**:
```bash
Error: Template directory not found at packages/microservices/masterdata/service-template
```
**Solution**: Ensure you're running from the repository root

**Python not found**:
```bash
python3: command not found
```
**Solution**: Install Python 3.7+ or use `python` instead of `python3`

**Service already exists**:
```bash
Error: Service directory already exists!
```
**Solution**: Choose a different service name or remove existing directory

**Invalid service type**:
```bash
Error: Service type must be 'masterdata' or 'datamanager'
```
**Solution**: Use correct service type parameter

### Getting Help

1. **Check the help**: `make help` or `python3 scripts/generate-service.py`
2. **Review examples** in this guide
3. **Examine existing services** for patterns
4. **Use interactive mode** for guidance

## Examples

### Complete Masterdata Service
```bash
# Interactive mode
make generate-masterdata
# Enter: inventory-service
# Enter: inventory  
# Enter: Inventory Management Service

# Result: packages/microservices/masterdata/inventory-service/
```

### Complete Datamanager Service
```bash
# Command line mode
make generate-service TYPE=datamanager SERVICE=compliance-service ENTITY=compliance DESC="Regulatory Compliance Processing"

# Result: packages/microservices/datamanager/compliance-service/
```

### Quick Development Workflow
```bash
# 1. Generate service
make generate-masterdata

# 2. Navigate to service
cd packages/microservices/masterdata/your-service/

# 3. Customize entities and business logic
# Edit src/YourService.Core/Entities/
# Edit src/YourService.Core/Services/

# 4. Build and test
dotnet build
dotnet test

# 5. Run service
dotnet run --project src/YourService.Api

# 6. Access API
# Swagger: http://localhost:5000
# Health: http://localhost:5000/health
```

## Conclusion

The QaliTrack Service Template System provides a solid foundation for rapid microservice development while maintaining consistency across the platform. Use it to accelerate development, ensure architectural compliance, and focus on business logic rather than boilerplate code.

For questions or improvements, review existing services or update the template itself in `packages/microservices/masterdata/service-template/`.

Happy coding! 🚀