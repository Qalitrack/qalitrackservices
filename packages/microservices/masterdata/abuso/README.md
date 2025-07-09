# Abuso

Abuso microservice for the QaliTrack platform.

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

The primary entity for this service is `Abuso`.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/Abuso.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/Abuso.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/Abuso.Core/Services/`
3. **Configure Database**: Modify `src/Abuso.Infrastructure/Data/AbusoDbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/Abuso.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for Abuso management:

- `GET /api/abusos` - Get all abusos
- `GET /api/abusos/{id}` - Get abuso by ID
- `POST /api/abusos` - Create new abuso
- `PUT /api/abusos/{id}` - Update abuso
- `DELETE /api/abusos/{id}` - Delete abuso

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
Abuso/
├── src/
│   ├── Abuso.Api/           # Web API layer
│   ├── Abuso.Core/          # Business logic
│   └── Abuso.Infrastructure/ # Data access
├── tests/
│   └── Abuso.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── Abuso.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Abuso.Tests
```

## Docker

```bash
# Build Docker image
docker build -t abuso .

# Run container
docker run -p 5000:8080 abuso
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
