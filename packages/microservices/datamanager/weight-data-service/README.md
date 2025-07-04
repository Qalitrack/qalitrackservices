# Weight Data Service

A comprehensive microservice for managing weight measurements and weighbridge operations in the Qalitrack system.

## Overview

The Weight Data Service is a .NET 8 Web API that provides:

- Weight measurement recording and management
- Weighbridge status monitoring
- Weight correction workflow
- Analytics and reporting
- Real-time weight data processing

## Features

### Core Functionality

- **Weight Measurements**: Record, validate, and manage weight measurements from multiple weighbridges
- **Weighbridge Management**: Monitor weighbridge status, capacity, and maintenance schedules
- **Weight Corrections**: Implement correction workflow with approval process
- **Analytics**: Generate usage statistics and weight summaries
- **Multi-tenancy**: Organization-based data isolation

### Technical Features

- RESTful API with OpenAPI/Swagger documentation
- Entity Framework Core with SQLite (development) / PostgreSQL (production)
- AutoMapper for object mapping
- FluentValidation for input validation
- Serilog for structured logging
- Comprehensive unit and integration tests
- Docker containerization

## API Endpoints

### Weight Measurements

- `GET /api/measurements` - Get paginated measurements with filtering
- `GET /api/measurements/{id}` - Get measurement by ID
- `GET /api/measurements/ticket/{ticketReference}` - Get measurement by ticket
- `POST /api/measurements` - Create new measurement
- `PUT /api/measurements/{id}` - Update measurement
- `DELETE /api/measurements/{id}` - Delete measurement
- `GET /api/measurements/pending` - Get pending measurements
- `GET /api/measurements/vehicle/{registration}` - Get measurements by vehicle
- `GET /api/measurements/weighbridge/{weighbridgeId}` - Get measurements by weighbridge

### Weighbridge Status

- `GET /api/weighbridges` - Get all weighbridges
- `GET /api/weighbridges/{id}/status` - Get weighbridge status
- `POST /api/weighbridges` - Create new weighbridge
- `PUT /api/weighbridges/{id}/status` - Update weighbridge status
- `GET /api/weighbridges/active` - Get active weighbridges
- `GET /api/weighbridges/calibration-due` - Get weighbridges requiring calibration
- `PUT /api/weighbridges/{id}/current-weight` - Update current weight

### Weight Corrections

- `POST /api/corrections` - Create weight correction
- `PUT /api/corrections/{id}/approve` - Approve/reject correction
- `GET /api/corrections/measurement/{measurementId}` - Get corrections for measurement
- `GET /api/corrections/pending` - Get pending corrections
- `GET /api/corrections/recent` - Get recent corrections

### Analytics

- `GET /api/analytics/summary` - Get analytics summary
- `GET /api/analytics/weighbridge-usage` - Get weighbridge usage statistics
- `GET /api/analytics/daily-weights` - Get daily weight summaries

### Health Checks

- `GET /api/health` - Basic health check
- `GET /api/health/database` - Database connectivity check

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker (optional, for containerized deployment)

### Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd apps/datamanager/weight-data-service
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database**
   ```bash
   cd src/WeightDataService.Api
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/WeightDataService.Api
   ```

5. **Access Swagger UI**
   ```
   http://localhost:5000
   ```

### Docker Deployment

1. **Build the image**
   ```bash
   docker build -f docker/Dockerfile -t weight-data-service .
   ```

2. **Run with Docker Compose**
   ```bash
   docker-compose -f docker/docker-compose.yml up -d
   ```

### Configuration

#### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=weightdata.db"
  }
}
```

#### Logging Configuration

The service uses Serilog for structured logging:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/weightdata-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## Testing

### Unit Tests

```bash
dotnet test tests/WeightDataService.Tests/
```

### Integration Tests

Integration tests use an in-memory database and test the full API pipeline:

```bash
dotnet test tests/WeightDataService.Tests/ --filter "Category=Integration"
```

### Test Coverage

The project includes comprehensive test coverage for:

- Service layer business logic
- Controller endpoints
- Repository operations
- Data validation
- Error handling

## Architecture

### Project Structure

```
src/
├── WeightDataService.Api/          # Web API layer
│   ├── Controllers/                # API controllers
│   ├── Middleware/                 # Custom middleware
│   └── Extensions/                 # Service extensions
├── WeightDataService.Core/         # Business logic
│   ├── Entities/                   # Domain entities
│   ├── DTOs/                       # Data transfer objects
│   ├── Services/                   # Business services
│   ├── Interfaces/                 # Service contracts
│   ├── Mappings/                   # AutoMapper profiles
│   └── Validators/                 # FluentValidation validators
└── WeightDataService.Infrastructure/ # Data access
    ├── Data/                       # DbContext
    ├── Repositories/               # Repository implementations
    └── Extensions/                 # Infrastructure extensions
```

### Design Patterns

- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Dependency Injection**: IoC container
- **CQRS-lite**: Separate read/write operations where beneficial
- **Clean Architecture**: Separation of concerns

## Data Models

### WeightMeasurement

```csharp
public class WeightMeasurement
{
    public Guid Id { get; set; }
    public string WeighbridgeId { get; set; }
    public string VehicleRegistration { get; set; }
    public decimal Weight { get; set; }
    public MeasurementType Type { get; set; }
    public MeasurementStatus Status { get; set; }
    public DateTime MeasurementDateTime { get; set; }
    // ... additional properties
}
```

### WeighbridgeStatus

```csharp
public class WeighbridgeStatus
{
    public Guid Id { get; set; }
    public string WeighbridgeId { get; set; }
    public string Name { get; set; }
    public MaintenanceStatus Status { get; set; }
    public decimal MaxCapacity { get; set; }
    public decimal CurrentWeight { get; set; }
    // ... additional properties
}
```

## Security

### Headers

The service expects the following headers for multi-tenancy and user context:

- `X-Organization-Id`: Organization identifier
- `X-User-Id`: User identifier

### Data Isolation

All data operations are scoped to the organization context, ensuring proper data isolation between tenants.

## Monitoring

### Health Checks

- Basic service health at `/api/health`
- Database connectivity at `/api/health/database`

### Logging

Structured logging with Serilog includes:

- Request/response logging
- Error tracking
- Performance metrics
- Business event logging

## Production Considerations

### Database

For production deployment, consider:

- Using PostgreSQL instead of SQLite
- Implementing connection pooling
- Setting up database backups
- Configuring read replicas for analytics

### Scaling

The service is designed to be stateless and can be scaled horizontally:

- Multiple instances behind a load balancer
- Shared database
- Redis for distributed caching (future enhancement)

### Performance

- Entity Framework query optimization
- Pagination for large datasets
- Caching for frequently accessed data
- Asynchronous operations throughout

## Contributing

1. Follow the existing code style and patterns
2. Add unit tests for new functionality
3. Update documentation for API changes
4. Ensure all tests pass before submitting

## License

[License information]

## Support

For issues and questions, please [contact information].