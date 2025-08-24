# Driver Service

A comprehensive Driver Management microservice for the QaliTrack platform, providing driver information management, license tracking, performance monitoring, and compliance features.

## Service Type: Masterdata

This service was generated from the QaliTrack service template and follows clean architecture principles.

## Features

- **Clean Architecture**: API, Core, and Infrastructure layers
- **Entity Framework**: SQLite database with EF Core
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation
- **Logging**: Structured logging capabilities
- **Health Checks**: Built-in monitoring

## Main Entities

The primary entities for this service are:
- `Driver` - Core driver information and profile
- `DriverLicense` - Driver license information and validation
- `DriverDocument` - Document management for drivers
- `DriverPerformance` - Performance tracking and metrics
- `DriverViolation` - Traffic violations and incident tracking

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/DriverService.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/DriverService.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/DriverService.Core/Services/`
3. **Configure Database**: Modify `src/DriverService.Infrastructure/Data/DriverDbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/DriverService.Api/Controllers/`

### API Endpoints

The service provides comprehensive RESTful endpoints for Driver management:

#### Driver Management
- `GET /api/drivers` - Get all drivers with pagination
- `GET /api/drivers/{id}` - Get driver by ID
- `POST /api/drivers` - Create new driver
- `PUT /api/drivers/{id}` - Update driver
- `DELETE /api/drivers/{id}` - Delete driver

#### License Management
- `GET /api/driver-licenses` - Get all driver licenses
- `GET /api/driver-licenses/{id}` - Get license by ID
- `POST /api/driver-licenses` - Create new license
- `PUT /api/driver-licenses/{id}` - Update license
- `DELETE /api/driver-licenses/{id}` - Delete license

#### Violation Management
- `GET /api/driver-violations` - Get all violations
- `GET /api/driver-violations/{id}` - Get violation by ID
- `POST /api/driver-violations` - Create new violation
- `PUT /api/driver-violations/{id}` - Update violation
- `DELETE /api/driver-violations/{id}` - Delete violation

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
DriverService/
├── src/
│   ├── DriverService.Api/           # Web API layer
│   ├── DriverService.Core/          # Business logic
│   └── DriverService.Infrastructure/ # Data access
├── tests/
│   └── DriverService.Tests/         # Unit & integration tests
├── Dockerfile                       # Container configuration
└── DriverService.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/DriverService.Tests
```

## Docker

```bash
# Build Docker image
docker build -t driverservice .

# Run container
docker run -p 5000:8080 driverservice
```

## Key Features

### Driver Profile Management
- Complete driver information storage
- Contact details and emergency contacts
- Driver classification and categorization
- Photo and document management

### License Management
- License validation and verification
- Expiration tracking and alerts
- License class and endorsement tracking
- Renewal workflow management

### Performance Monitoring
- Driver performance metrics
- Safety scores and ratings
- Incident tracking and reporting
- Performance trend analysis

### Compliance Management
- Regulatory compliance tracking
- Training record management
- Medical examination tracking
- Violation and penalty management

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀