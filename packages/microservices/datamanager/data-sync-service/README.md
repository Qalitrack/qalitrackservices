# Data Sync Service

A comprehensive multi-site data synchronization service built with .NET 8, designed to handle real-time data synchronization across multiple sites with advanced conflict resolution capabilities.

## Architecture

The service follows Clean Architecture principles with three main layers:

### Core Layer (`DataSyncService.Core`)
- **Entities**: Domain models for sync sessions, sites, conflicts, change records, and health checks
- **DTOs**: Data transfer objects for API communication
- **Interfaces**: Service and repository contracts
- **Services**: Business logic for synchronization, site management, and conflict resolution
- **Validators**: FluentValidation validators for request validation
- **Mappings**: AutoMapper profiles for object mapping

### Infrastructure Layer (`DataSyncService.Infrastructure`)
- **Data**: Entity Framework DbContext and database configuration
- **Repositories**: Data access layer implementations
- **Services**: Infrastructure-specific services (sync engine, conflict detection, health monitoring)
- **Migrations**: Database schema migrations

### API Layer (`DataSyncService.Api`)
- **Controllers**: REST API endpoints for sync operations, site management, conflict resolution, and health checks
- **Middleware**: Global exception handling and request processing
- **Extensions**: Dependency injection and service configuration

## Key Features

### Multi-Site Synchronization
- **Real-time sync**: Automatic synchronization across multiple sites
- **Sync modes**: Full, incremental, and delta synchronization
- **Site management**: Register, monitor, and manage multiple sync sites
- **Health monitoring**: Continuous health checks for all registered sites

### Conflict Resolution
- **Automatic detection**: Identifies data conflicts during synchronization
- **Multiple strategies**: Last write wins, first write wins, merge changes, manual resolution
- **Escalation support**: Conflicts can be escalated for manual review
- **Audit trail**: Complete history of conflict resolutions

### Data Management
- **Change tracking**: Comprehensive logging of all data changes
- **Sequence numbering**: Ordered change records for consistent synchronization
- **Retry mechanisms**: Automatic retry for failed synchronization attempts
- **Performance optimization**: Batching and parallel processing support

## API Endpoints

### Synchronization (`/api/sync`)
- `POST /api/sync/start` - Start a new sync session
- `POST /api/sync/{sessionId}/stop` - Stop an active sync session
- `GET /api/sync/{sessionId}/status` - Get sync session status
- `GET /api/sync/active` - Get all active sync sessions
- `POST /api/sync/table` - Sync a specific table
- `GET /api/sync/summary` - Get synchronization status summary

### Site Management (`/api/sites`)
- `POST /api/sites` - Register a new sync site
- `GET /api/sites` - Get all registered sites
- `GET /api/sites/{id}` - Get specific site details
- `PUT /api/sites/{id}` - Update site configuration
- `DELETE /api/sites/{id}` - Unregister a site
- `GET /api/sites/{siteId}/health` - Check site health

### Conflict Resolution (`/api/conflicts`)
- `GET /api/conflicts` - Get all conflicts (paginated)
- `GET /api/conflicts/unresolved` - Get unresolved conflicts
- `POST /api/conflicts/{id}/resolve` - Resolve a specific conflict
- `POST /api/conflicts/resolve-multiple` - Bulk conflict resolution
- `GET /api/conflicts/statistics` - Get conflict statistics

### Health Monitoring (`/api/health`)
- `GET /api/health` - Service health check
- `GET /api/health/ready` - Readiness probe
- `GET /api/health/live` - Liveness probe

## Configuration

### Database
The service uses SQLite for development and can be configured for PostgreSQL in production:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=datasync.db"
  }
}
```

### Synchronization Settings
```json
{
  "DataSync": {
    "DefaultSyncIntervalMinutes": 30,
    "MaxRetryAttempts": 3,
    "HealthCheckIntervalMinutes": 5,
    "ConflictRetentionDays": 30
  }
}
```

## Running the Service

### Development
```bash
cd src/DataSyncService.Api
dotnet run
```

The service will start on `http://localhost:5000` with Swagger UI available at the root URL.

### Database Migrations
```bash
dotnet ef migrations add <MigrationName> -p src/DataSyncService.Infrastructure -s src/DataSyncService.Api
dotnet ef database update -p src/DataSyncService.Infrastructure -s src/DataSyncService.Api
```

## Testing

Run all tests:
```bash
dotnet test
```

The test suite includes:
- Unit tests for core entities and business logic
- Integration tests for repositories and services
- API endpoint tests

## Key Technologies

- **.NET 8**: Modern framework with performance improvements
- **Entity Framework Core**: ORM with SQLite/PostgreSQL support
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Request validation
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Testing framework
- **FluentAssertions**: Fluent test assertions

## Architecture Benefits

1. **Scalability**: Supports multiple sites and high data volumes
2. **Reliability**: Comprehensive error handling and retry mechanisms
3. **Flexibility**: Multiple sync modes and conflict resolution strategies
4. **Monitoring**: Built-in health checks and performance metrics
5. **Maintainability**: Clean architecture with separation of concerns
6. **Extensibility**: Easy to add new sync strategies and conflict resolution methods

## Future Enhancements

- Message queue integration (NATS, RabbitMQ, or Azure Service Bus)
- Real-time notifications via SignalR
- Advanced monitoring and alerting
- Performance analytics and reporting
- Multi-tenant support
- Encryption for sensitive data synchronization