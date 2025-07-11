# ReportService

Report Management Service

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

The primary entity for this service is `Report`.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/ReportService.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/ReportService.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/ReportService.Core/Services/`
3. **Configure Database**: Modify `src/ReportService.Infrastructure/Data/ReportServiceDbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/ReportService.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for Report management:

- `GET /api/reports` - Get all reports
- `GET /api/reports/{id}` - Get report by ID
- `POST /api/reports` - Create new report
- `PUT /api/reports/{id}` - Update report
- `DELETE /api/reports/{id}` - Delete report

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
ReportService/
├── src/
│   ├── ReportService.Api/           # Web API layer
│   ├── ReportService.Core/          # Business logic
│   └── ReportService.Infrastructure/ # Data access
├── tests/
│   └── ReportService.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── ReportService.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/ReportService.Tests
```

## Docker

```bash
# Build Docker image
docker build -t reportservice .

# Run container
docker run -p 5000:8080 reportservice
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
