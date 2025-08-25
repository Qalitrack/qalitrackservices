# Microservice Template

This template provides a clean architecture foundation for creating new microservices in the QaliTrack system.

## Template Structure

```
service-template/
├── src/
│   ├── {{ServiceName}}.Api/           # Web API layer
│   │   ├── Controllers/               # API controllers
│   │   ├── Program.cs                 # Application entry point
│   │   └── appsettings.json          # Configuration
│   ├── {{ServiceName}}.Core/          # Business logic layer
│   │   ├── DTOs/                      # Data transfer objects
│   │   ├── Entities/                  # Domain entities
│   │   ├── Interfaces/                # Service interfaces
│   │   ├── Mappings/                  # AutoMapper profiles
│   │   └── Services/                  # Business logic services
│   └── {{ServiceName}}.Infrastructure/ # Data access layer
│       ├── Data/                      # Database context
│       └── Repositories/              # Data repositories
├── tests/
│   └── {{ServiceName}}.Tests/         # Unit and integration tests
└── generate-service.py               # Service generator script
```

## Features

- **Clean Architecture**: Separation of concerns with API, Core, and Infrastructure layers
- **Entity Framework**: SQLite database with Entity Framework Core
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: Comprehensive API documentation
- **Logging**: Structured logging with Serilog
- **CORS**: Cross-origin resource sharing support
- **Health Checks**: Built-in health monitoring
- **Template Placeholders**: Easy customization through replaceable tokens

## Generating a New Service

Use the QaliTrack service generation system to create new services from this template.

### Quick Start (Recommended)

From the repository root:

```bash
# Interactive masterdata service generation
make generate-masterdata

# Interactive datamanager service generation  
make generate-datamanager

# Command line with parameters
make generate-service TYPE=masterdata SERVICE=inventory-service ENTITY=inventory DESC="Inventory Management"
```

### Direct Script Usage

```bash
# From repository root
python3 scripts/generate-service.py masterdata inventory-service inventory "Inventory Management Service"
python3 scripts/generate-service.py datamanager analytics-service analytics "Analytics Processing Service"
```

### Parameters

- **service-type**: Either `masterdata` or `datamanager`
- **service-name**: The name of your service (kebab-case recommended)
- **entity-name**: The main entity/domain object (single word)
- **description**: Optional description for the service

### Service Types

- **masterdata**: For core business entities (user, customer, product, etc.)
- **datamanager**: For data processing services (analytics, transaction, etc.)

## Template Placeholders

The template uses the following placeholders that get replaced during generation:

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `{{ServiceName}}` | PascalCase service name | `InventoryService` |
| `{{service-name}}` | kebab-case service name | `inventory-service` |
| `{{EntityName}}` | PascalCase entity name | `Inventory` |
| `{{entityName}}` | camelCase entity name | `inventory` |
| `{{ServiceDescription}}` | Service description | `Inventory Management Service` |
| `{{api-prefix}}` | API route prefix | `inventories` |

## After Generation

1. **Navigate to your new service**:
   ```bash
   cd your-service-name
   ```

2. **Review and customize**:
   - Update entities in `src/YourService.Core/Entities/`
   - Add domain-specific properties and methods
   - Configure database relationships in `DbContext`
   - Add business logic to services
   - Create validators if needed

3. **Build and run**:
   ```bash
   dotnet build
   dotnet run --project src/YourService.Api
   ```

4. **Access the API**:
   - Swagger UI: `http://localhost:5000` (or configured port)
   - Health check: `http://localhost:5000/health`

## Customization Guide

### Adding New Entities

1. Create entity class in `Core/Entities/`
2. Add corresponding DTOs in `Core/DTOs/`
3. Create repository interface and implementation
4. Add service interface and implementation
5. Create controller
6. Update `DbContext` with new `DbSet` and configuration
7. Add AutoMapper mappings

### Authentication (Optional)

The template includes commented-out JWT authentication code. To enable:

1. Uncomment authentication sections in `Program.cs`
2. Add authentication-related services
3. Configure JWT settings in `appsettings.json`
4. Add `[Authorize]` attributes to controllers as needed

### Database Configuration

The template uses SQLite by default. To use SQL Server or PostgreSQL:

1. Update the NuGet package references
2. Modify the connection string in `Program.cs`
3. Update `appsettings.json` with appropriate connection string

## Integration with Gateway

Generated services are automatically configured to work with the QaliTrack API Gateway:

- Health checks at `/health`
- CORS configured for gateway integration
- Standardized API response format

## Best Practices

1. **Follow Clean Architecture principles**
2. **Use meaningful entity and service names**
3. **Implement proper error handling**
4. **Add comprehensive logging**
5. **Write unit and integration tests**
6. **Use DTOs for API boundaries**
7. **Implement proper validation**
8. **Follow consistent naming conventions**

## Example Generated Service

After running:
```bash
python generate-service.py inventory-service inventory "Inventory Management"
```

You'll get a complete microservice with:
- `InventoryService.Api` with `InventoriesController`
- `InventoryService.Core` with `Inventory` entity and related DTOs
- `InventoryService.Infrastructure` with `InventoryRepository` and `InventoryDbContext`
- Full CRUD operations and database integration
- Swagger documentation at `/swagger`

Happy coding! 🚀