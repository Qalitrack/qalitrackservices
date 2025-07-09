# TestServiceV4

TestServiceV4 microservice for the QaliTrack platform.

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

The primary entity for this service is `Testentity`.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/TestServiceV4.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/TestServiceV4.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/TestServiceV4.Core/Services/`
3. **Configure Database**: Modify `src/TestServiceV4.Infrastructure/Data/TestServiceV4DbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/TestServiceV4.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for Testentity management:

- `GET /api/testentitys` - Get all testentitys
- `GET /api/testentitys/{id}` - Get testentity by ID
- `POST /api/testentitys` - Create new testentity
- `PUT /api/testentitys/{id}` - Update testentity
- `DELETE /api/testentitys/{id}` - Delete testentity

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
TestServiceV4/
├── src/
│   ├── TestServiceV4.Api/           # Web API layer
│   ├── TestServiceV4.Core/          # Business logic
│   └── TestServiceV4.Infrastructure/ # Data access
├── tests/
│   └── TestServiceV4.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── TestServiceV4.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TestServiceV4.Tests
```

## Docker

```bash
# Build Docker image
docker build -t testservicev4 .

# Run container
docker run -p 5000:8080 testservicev4
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
