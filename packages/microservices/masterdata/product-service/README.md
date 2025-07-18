# ProductService

Product Catalog Management

## Service Type: Masterdata

This service was generated from the QaliTrack service template and follows clean architecture principles.

## Features

- **Clean Architecture**: API, Core, and Infrastructure layers
- **Entity Framework**: SQLite database with EF Core
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation
- **Logging**: Structured logging with Serilog
- **Health Checks**: Built-in monitoring

## Main Entity

The primary entity for this service is `Product`.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/ProductService.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/ProductService.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/ProductService.Core/Services/`
3. **Configure Database**: Modify `src/ProductService.Infrastructure/Data/ProductServiceDbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/ProductService.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for Product management:

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
ProductService/
├── src/
│   ├── ProductService.Api/           # Web API layer
│   ├── ProductService.Core/          # Business logic
│   └── ProductService.Infrastructure/ # Data access
├── tests/
│   └── ProductService.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── ProductService.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/ProductService.Tests
```

## Docker

```bash
# Build Docker image
docker build -t productservice .

# Run container
docker run -p 5000:8080 productservice
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
