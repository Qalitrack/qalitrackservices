# Masterdata

MasterData

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

The primary entity for this service is `Base`.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/Masterdata.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/Masterdata.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/Masterdata.Core/Services/`
3. **Configure Database**: Modify `src/Masterdata.Infrastructure/Data/MasterdataDbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/Masterdata.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for Base management:

- `GET /api/bases` - Get all bases
- `GET /api/bases/{id}` - Get base by ID
- `POST /api/bases` - Create new base
- `PUT /api/bases/{id}` - Update base
- `DELETE /api/bases/{id}` - Delete base

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
Masterdata/
├── src/
│   ├── Masterdata.Api/           # Web API layer
│   ├── Masterdata.Core/          # Business logic
│   └── Masterdata.Infrastructure/ # Data access
├── tests/
│   └── Masterdata.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── Masterdata.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Masterdata.Tests
```

## Docker

```bash
# Build Docker image
docker build -t masterdata .

# Run container
docker run -p 5000:8080 masterdata
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
