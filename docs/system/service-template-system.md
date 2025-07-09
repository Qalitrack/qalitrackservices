# QaliTrack Service Template System

Complete guide for generating new microservices using the QaliTrack service template system.

## Table of Contents

- [Overview](#overview)
- [Template Architecture](#template-architecture)
- [Quick Start](#quick-start)
- [Service Generation](#service-generation)
- [Template Structure](#template-structure)
- [Customization Guide](#customization-guide)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)
- [Advanced Usage](#advanced-usage)
- [Contributing](#contributing)

## Overview

The QaliTrack Service Template System provides a standardized way to generate new microservices with consistent architecture, configuration, and best practices. It supports both **masterdata** and **datamanager** service types and automatically generates:

- **Clean Architecture** structure (API, Core, Infrastructure layers)
- **Entity Framework** database integration with SQLite
- **AutoMapper** configurations for DTOs
- **Swagger/OpenAPI** documentation
- **Docker** containerization
- **Unit tests** with xUnit and FluentAssertions
- **Make targets** for build automation
- **Executable scripts** for easy service execution

## Template Architecture

The template system uses a placeholder-based approach with the following key components:

### Service Types

| Type | Purpose | Example Services |
|------|---------|------------------|
| **masterdata** | Core business entities and reference data | user-service, product-service, customer-service |
| **datamanager** | Operational workflows and data processing | analytics-service, compliance-service, weight-data-service |

### Placeholder System

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `{{ServiceName}}` | PascalCase service name | `ReportService` |
| `{{service-name}}` | kebab-case service name | `report-service` |
| `{{EntityName}}` | PascalCase entity name | `Report` |
| `{{entityName}}` | camelCase entity name | `report` |
| `{{ServiceDescription}}` | Human-readable description | `Report Management Service` |
| `{{api-prefix}}` | API route prefix | `reports` |

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- Python 3.x
- Make (optional, for automation)

### Generate a New Service

```bash
# Basic generation
python3 scripts/generate-service.py masterdata inventory-service inventory "Inventory Management Service"

# Using Make targets
make generate-service TYPE=masterdata SERVICE=inventory-service ENTITY=inventory DESC="Inventory Management Service"

# Interactive generation
make generate-masterdata    # For masterdata services
make generate-datamanager   # For datamanager services
```

### Test the Generated Service

```bash
# Navigate to service directory
cd packages/microservices/masterdata/inventory-service

# Run the service
./run.sh

# Run tests
dotnet test

# Build and test using Make
make build-inventory-service
make test-inventory-service
```

## Service Generation

### Generation Script

The `scripts/generate-service.py` script handles all service generation:

```python
# Usage
python3 scripts/generate-service.py <service-type> <service-name> <entity-name> [description]

# Arguments
service-type    # masterdata or datamanager
service-name    # kebab-case service name
entity-name     # PascalCase entity name
description     # Optional service description
```

### Generation Process

1. **Template Copy**: Copies the service template directory
2. **Placeholder Replacement**: Replaces all placeholders with actual values
3. **File Renaming**: Renames files and directories with placeholders
4. **Permission Setup**: Makes shell scripts executable
5. **Make Integration**: Adds service targets to Makefile
6. **README Creation**: Generates service-specific documentation

### Auto-Generated Make Targets

Each service gets these targets automatically:

```makefile
build-{service}        # Build the service
run-{service}          # Run the service
test-{service}         # Run unit tests
docker-build-{service} # Build Docker image
docker-run-{service}   # Run Docker container
```

## Template Structure

### Directory Layout

```
packages/microservices/masterdata/service-template/
├── src/
│   ├── {{ServiceName}}.Api/           # Web API layer
│   │   ├── Controllers/
│   │   │   └── {{EntityName}}sController.cs
│   │   ├── Program.cs                 # Application startup
│   │   └── appsettings.json           # Configuration
│   ├── {{ServiceName}}.Core/          # Business logic layer
│   │   ├── DTOs/
│   │   │   └── {{EntityName}}Dto.cs   # Data transfer objects
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs          # Base entity class
│   │   │   └── MainEntity.cs          # Main entity ({{EntityName}})
│   │   ├── Interfaces/
│   │   │   ├── I{{EntityName}}Repository.cs
│   │   │   └── I{{EntityName}}Service.cs
│   │   ├── Mappings/
│   │   │   └── {{EntityName}}Profile.cs  # AutoMapper profiles
│   │   └── Services/
│   │       └── {{EntityName}}Service.cs   # Business logic
│   └── {{ServiceName}}.Infrastructure/   # Data access layer
│       ├── Data/
│       │   └── {{ServiceName}}DbContext.cs
│       └── Repositories/
│           ├── Repository.cs              # Base repository
│           └── {{EntityName}}Repository.cs
├── tests/
│   └── {{ServiceName}}.Tests/         # Unit tests
│       ├── BasicTests.cs              # Basic test examples
│       └── {{ServiceName}}.Tests.csproj
├── Dockerfile                         # Container configuration
├── {{ServiceName}}.sln               # Solution file
├── run.sh                            # Unix execution script
└── run.cmd                           # Windows execution script
```

### Key Template Files

#### Entity Definition (`MainEntity.cs`)
```csharp
namespace {{ServiceName}}.Core.Entities;

public class {{EntityName}} : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public {{EntityName}}Status Status { get; set; } = {{EntityName}}Status.Active;
    
    // TODO: Add domain-specific properties
}

public enum {{EntityName}}Status
{
    Active,
    Inactive,
    Suspended,
    Pending
}
```

#### API Controller (`{{EntityName}}sController.cs`)
```csharp
[Route("api/[controller]")]
public class {{EntityName}}sController : BaseController
{
    private readonly I{{EntityName}}Service _{{entityName}}Service;
    
    public {{EntityName}}sController(I{{EntityName}}Service {{entityName}}Service)
    {
        _{{entityName}}Service = {{entityName}}Service;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<{{EntityName}}ReadDto>>> GetAll()
    {
        var items = await _{{entityName}}Service.GetAllAsync();
        return Ok(items);
    }
    
    // Additional CRUD operations...
}
```

#### Repository Interface (`I{{EntityName}}Repository.cs`)
```csharp
public interface I{{EntityName}}Repository : IRepository<{{ServiceName}}.Core.Entities.{{EntityName}}>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<{{ServiceName}}.Core.Entities.{{EntityName}}?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods
}
```

#### Dependency Injection (`Program.cs`)
```csharp
// Add repositories
builder.Services.AddScoped<I{{EntityName}}Repository, {{EntityName}}Repository>();

// Add services
builder.Services.AddScoped<I{{EntityName}}Service, {{ServiceName}}.Core.Services.{{EntityName}}Service>();
```

## Customization Guide

### Adding Custom Properties

1. **Update Entity** (`src/{{ServiceName}}.Core/Entities/MainEntity.cs`):
```csharp
public class {{EntityName}} : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Add your custom properties
    public string Code { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}
```

2. **Update DTOs** (`src/{{ServiceName}}.Core/DTOs/{{EntityName}}Dto.cs`):
```csharp
public class {{EntityName}}ReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Add corresponding DTO properties
    public string Code { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
}
```

3. **Update AutoMapper Profile** (`src/{{ServiceName}}.Core/Mappings/{{EntityName}}Profile.cs`):
```csharp
public {{EntityName}}Profile()
{
    CreateMap<{{ServiceName}}.Core.Entities.{{EntityName}}, {{EntityName}}ReadDto>()
        .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
        .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.ValidFrom));
    
    CreateMap<Create{{EntityName}}Dto, {{ServiceName}}.Core.Entities.{{EntityName}}>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
}
```

### Adding Business Logic

1. **Custom Repository Methods** (`src/{{ServiceName}}.Core/Interfaces/I{{EntityName}}Repository.cs`):
```csharp
public interface I{{EntityName}}Repository : IRepository<{{ServiceName}}.Core.Entities.{{EntityName}}>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<{{ServiceName}}.Core.Entities.{{EntityName}}?> GetByNameAsync(string name);
    
    // Add domain-specific methods
    Task<IEnumerable<{{ServiceName}}.Core.Entities.{{EntityName}}>> GetActiveAsync();
    Task<IEnumerable<{{ServiceName}}.Core.Entities.{{EntityName}}>> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code);
}
```

2. **Custom Service Methods** (`src/{{ServiceName}}.Core/Interfaces/I{{EntityName}}Service.cs`):
```csharp
public interface I{{EntityName}}Service
{
    Task<IEnumerable<{{EntityName}}ReadDto>> GetAllAsync();
    Task<{{EntityName}}ReadDto?> GetByIdAsync(string id);
    Task<{{EntityName}}ReadDto> CreateAsync(Create{{EntityName}}Dto dto);
    Task<{{EntityName}}ReadDto?> UpdateAsync(string id, Update{{EntityName}}Dto dto);
    Task<bool> DeleteAsync(string id);
    
    // Add domain-specific methods
    Task<IEnumerable<{{EntityName}}ReadDto>> GetActiveAsync();
    Task<{{EntityName}}ReadDto?> GetByCodeAsync(string code);
    Task<bool> ValidateBusinessRulesAsync(Create{{EntityName}}Dto dto);
}
```

### Database Configuration

Update `src/{{ServiceName}}.Infrastructure/Data/{{ServiceName}}DbContext.cs`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configure {{EntityName}} entity
    modelBuilder.Entity<{{ServiceName}}.Core.Entities.{{EntityName}}>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Description).HasMaxLength(1000);
        
        // Add custom constraints
        entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
        entity.Property(e => e.ValidFrom).IsRequired();
        entity.Property(e => e.ValidTo).IsRequired();

        // Add indexes
        entity.HasIndex(e => e.Name).IsUnique();
        entity.HasIndex(e => e.Code).IsUnique();
        entity.HasIndex(e => new { e.ValidFrom, e.ValidTo });
    });
    
    // Add seed data
    modelBuilder.Entity<{{ServiceName}}.Core.Entities.{{EntityName}}>().HasData(
        new {{ServiceName}}.Core.Entities.{{EntityName}}
        {
            Id = "1",
            Name = "Default {{EntityName}}",
            Description = "Default {{EntityName}} Description",
            Code = "DEF001",
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    );
}
```

## Best Practices

### Naming Conventions

1. **Service Names**: Use kebab-case (e.g., `inventory-service`, `data-sync-service`)
2. **Entity Names**: Use PascalCase singular (e.g., `Product`, `Customer`, `WeightData`)
3. **Database Tables**: Auto-generated as pluralized entity names
4. **API Endpoints**: Follow RESTful conventions (`/api/products`, `/api/customers`)

### Entity Design

1. **Inherit from BaseEntity**: All entities should inherit from `BaseEntity`
2. **Use Descriptive Properties**: Name properties clearly and consistently
3. **Add Validation**: Use data annotations or FluentValidation
4. **Consider Relationships**: Plan entity relationships carefully

### Repository Pattern

1. **Generic Base**: Use the provided `Repository<T>` base class
2. **Specific Interfaces**: Create specific repository interfaces
3. **Business Logic**: Keep in service layer, not repository
4. **Async Operations**: Use async/await throughout

### Service Layer

1. **Separation of Concerns**: Keep business logic in services
2. **DTO Mapping**: Use AutoMapper for entity-DTO conversions
3. **Validation**: Validate input at service boundaries
4. **Exception Handling**: Handle exceptions gracefully

## Troubleshooting

### Common Issues

#### Namespace Conflicts
**Problem**: `CS0118: 'ServiceName' is a namespace but is used like a type`

**Solution**: The template handles this by using fully qualified type names:
```csharp
// Instead of
public interface IReportRepository : IRepository<Report>

// Use
public interface IReportRepository : IRepository<ServiceName.Core.Entities.Report>
```

#### Missing Dependencies
**Problem**: Package restore fails

**Solution**: Ensure all project references are correct:
```xml
<ProjectReference Include="../ServiceName.Core/ServiceName.Core.csproj" />
<ProjectReference Include="../ServiceName.Infrastructure/ServiceName.Infrastructure.csproj" />
```

#### Build Errors
**Problem**: Build fails after generation

**Solution**: Check that all placeholders were replaced correctly:
```bash
# Search for unreplaced placeholders
grep -r "{{" src/
```

#### Database Issues
**Problem**: Database connection or migration issues

**Solution**: Ensure Entity Framework is properly configured:
```csharp
// In Program.cs
builder.Services.AddDbContext<ServiceNameDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Service Won't Start

1. **Check Port Conflicts**: Ensure no other service is using the same port
2. **Verify Configuration**: Check `appsettings.json` for correct settings
3. **Database Initialization**: Ensure database can be created/connected
4. **Dependency Injection**: Verify all services are registered

### Tests Failing

1. **In-Memory Database**: Check that tests use `EntityFrameworkCore.InMemory`
2. **Mock Setup**: Ensure mocks are properly configured
3. **Async Tests**: Use proper async/await patterns
4. **Test Isolation**: Ensure tests don't interfere with each other

## Advanced Usage

### Service Removal

Remove a generated service and its make targets:

```bash
# Using Python script
python3 scripts/remove-service.py masterdata inventory-service

# The script will:
# 1. Remove the service directory
# 2. Clean up make targets from Makefile
# 3. Provide confirmation prompts
```

### Custom Templates

Create custom templates for specific use cases:

1. **Copy Template**: Copy `service-template` to `custom-template`
2. **Modify Structure**: Adjust files and directories as needed
3. **Update Generator**: Modify `generate-service.py` to use custom template
4. **Test Generation**: Generate services with custom template

### Integration with CI/CD

Add service generation to your CI/CD pipeline:

```yaml
# Example GitHub Actions workflow
name: Generate Service
on:
  workflow_dispatch:
    inputs:
      service_type:
        description: 'Service type (masterdata/datamanager)'
        required: true
        default: 'masterdata'
      service_name:
        description: 'Service name (kebab-case)'
        required: true
      entity_name:
        description: 'Entity name (PascalCase)'
        required: true

jobs:
  generate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Generate Service
        run: |
          python3 scripts/generate-service.py \
            ${{ github.event.inputs.service_type }} \
            ${{ github.event.inputs.service_name }} \
            ${{ github.event.inputs.entity_name }}
      - name: Test Service
        run: |
          cd packages/microservices/${{ github.event.inputs.service_type }}/${{ github.event.inputs.service_name }}
          dotnet test
```

### Service Discovery Integration

Generated services automatically integrate with the QaliTrack service discovery:

1. **Health Checks**: `/health` endpoint included
2. **Swagger Integration**: API documentation available
3. **Gateway Registration**: Services auto-register with gateway
4. **Monitoring**: Built-in logging and metrics

## Contributing

### Adding Template Features

1. **Update Template**: Modify files in `service-template/`
2. **Test Generation**: Generate test services to verify changes
3. **Update Documentation**: Update this guide with new features
4. **Test Edge Cases**: Ensure templates work with various naming scenarios

### Template Improvements

Consider these areas for improvement:

- **Validation**: Add more comprehensive input validation
- **Authentication**: Enhanced JWT and authorization templates
- **Monitoring**: Better logging and metrics integration
- **Testing**: More comprehensive test templates
- **Documentation**: Auto-generated API documentation

### Feedback and Issues

- Report issues with specific service generation scenarios
- Suggest improvements for template structure
- Share custom templates that might benefit others
- Contribute to documentation improvements

---

*The QaliTrack Service Template System provides a robust foundation for consistent microservice development, enabling rapid service creation while maintaining architectural standards and best practices.*