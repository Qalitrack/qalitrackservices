# TechnicianApi

Technician API service

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

The primary entity for this service is `Technician`.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/TechnicianApi.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/TechnicianApi.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/TechnicianApi.Core/Services/`
3. **Configure Database**: Modify `src/TechnicianApi.Infrastructure/Data/TechnicianApiDbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/TechnicianApi.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for Technician management:

- `GET /api/technicians` - Get all technicians
- `GET /api/technicians/{id}` - Get technician by ID
- `POST /api/technicians` - Create new technician
- `PUT /api/technicians/{id}` - Update technician
- `DELETE /api/technicians/{id}` - Delete technician

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
TechnicianApi/
├── src/
│   ├── TechnicianApi.Api/           # Web API layer
│   ├── TechnicianApi.Core/          # Business logic
│   └── TechnicianApi.Infrastructure/ # Data access
├── tests/
│   └── TechnicianApi.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── TechnicianApi.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TechnicianApi.Tests
```

## Docker

```bash
# Build Docker image
docker build -t technicianapi .

# Run container
docker run -p 5000:8080 technicianapi
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
