# TestService

TestService microservice for the QaliTrack platform.

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

The primary entity for this service is `Item`.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/TestService.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/TestService.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/TestService.Core/Services/`
3. **Configure Database**: Modify `src/TestService.Infrastructure/Data/TestServiceDbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/TestService.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for Item management:

- `GET /api/items` - Get all items
- `GET /api/items/{id}` - Get item by ID
- `POST /api/items` - Create new item
- `PUT /api/items/{id}` - Update item
- `DELETE /api/items/{id}` - Delete item

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
TestService/
├── src/
│   ├── TestService.Api/           # Web API layer
│   ├── TestService.Core/          # Business logic
│   └── TestService.Infrastructure/ # Data access
├── tests/
│   └── TestService.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── TestService.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TestService.Tests
```

## Docker

```bash
# Build Docker image
docker build -t testservice .

# Run container
docker run -p 5000:8080 testservice
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
