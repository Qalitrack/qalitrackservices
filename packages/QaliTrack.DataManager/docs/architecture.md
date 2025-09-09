# Architecture Overview

The QaliTrack Data Manager Service implements a Django-style modular architecture that consolidates seven previously separate microservices into a unified, scalable solution.

## System Architecture

### High-Level Design

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Client Apps   │────│  Load Balancer   │────│   API Gateway   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                                         │
┌────────────────────────────────────────────────────────────────────┐
│                    QaliTrack Data Manager Service                   │
├────────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐  │
│  │ WeightData  │ │Transactions │ │ Compliance  │ │  Analytics  │  │
│  │   Module    │ │   Module    │ │   Module    │ │   Module    │  │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘  │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                  │
│  │ Operations  │ │  DataSync   │ │   Archive   │                  │
│  │   Module    │ │   Module    │ │   Module    │                  │
│  └─────────────┘ └─────────────┘ └─────────────┘                  │
├────────────────────────────────────────────────────────────────────┤
│                     Shared Infrastructure                          │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐  │
│  │ Repository  │ │    Auth     │ │  Caching    │ │  Messaging  │  │
│  │   Pattern   │ │  & Security │ │   Layer     │ │    Bus      │  │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘  │
└────────────────────────────────────────────────────────────────────┘
                                │
                    ┌───────────────────────┐
                    │    Data Layer         │
                    │ ┌─────────────────┐   │
                    │ │ Entity Framework│   │
                    │ │      Core       │   │
                    │ └─────────────────┘   │
                    │ ┌─────────────────┐   │
                    │ │   Database      │   │
                    │ │(SQLite/PG/MSSQL)│   │
                    │ └─────────────────┘   │
                    └───────────────────────┘
```

## Modular Architecture

### Module Structure

Each module follows a consistent structure:

```
Module/
├── Entities/           # Domain entities and DTOs
├── Services/           # Business logic services
├── Controllers/        # API controllers
└── Specifications/     # Query specifications
```

### 1. WeightData Module
- **Purpose**: Real-time weight measurement processing
- **Key Features**: Calibration workflows, quality control, real-time streaming
- **Entities**: WeightMeasurement, WeighbridgeCalibration, QualityCheck

### 2. Transactions Module
- **Purpose**: Business transaction lifecycle management
- **Key Features**: Approval workflows, audit trails, transaction states
- **Entities**: Transaction, TransactionWorkflow, TransactionApproval

### 3. Compliance Module
- **Purpose**: Regulatory compliance and violation management
- **Key Features**: Automated violation detection, regulatory reporting
- **Entities**: ComplianceViolation, RegulatoryStandard, ComplianceReport

### 4. Analytics Module
- **Purpose**: Business intelligence and reporting
- **Key Features**: KPIs, dashboards, trend analysis
- **Entities**: AnalyticsKPI, Dashboard, TrendAnalysis

### 5. Operations Module
- **Purpose**: Operational monitoring and maintenance
- **Key Features**: Alert systems, maintenance scheduling
- **Entities**: OperationalAlert, MaintenanceSchedule

### 6. DataSync Module
- **Purpose**: Multi-site data synchronization
- **Key Features**: Conflict resolution, automated reconciliation
- **Entities**: SyncSession, SiteConfiguration, SyncConflict

### 7. Archive Module
- **Purpose**: Data lifecycle management
- **Key Features**: Automated archiving, retention policies
- **Entities**: ArchivePolicy, ArchivedData, ArchiveJob

## Design Patterns

### Repository Pattern

All data access follows the Repository pattern with two main interfaces:

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(...);
}

public interface ITenantRepository<T> : IRepository<T> where T : TenantBaseEntity
{
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedByOrganizationAsync(Guid orgId, ...);
}
```

### Multi-Tenancy Pattern

Data isolation is achieved through:

1. **Organization-based Partitioning**: All tenant data includes `OrganizationId`
2. **Global Query Filters**: Automatic filtering by organization context
3. **Security Context**: JWT claims provide organization identity

### Entity Base Classes

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class TenantBaseEntity : BaseEntity
{
    public Guid OrganizationId { get; set; }
}
```

## Data Flow Architecture

### Request Processing Flow

1. **Authentication & Authorization**: JWT token validation
2. **Organization Context**: Extract organization ID from claims/headers
3. **Input Validation**: Model binding and validation
4. **Business Logic**: Service layer processing
5. **Data Access**: Repository pattern with EF Core
6. **Response Formatting**: Standardized API responses

### Cross-Module Communication

Modules communicate through:

1. **Master Data Integration**: HTTP clients for reference data validation
2. **Event-Driven Architecture**: Domain events for loose coupling
3. **Shared Services**: Common infrastructure services

## Database Design

### Connection Strategy

The service supports multiple database providers through Entity Framework Core:

- **Development**: SQLite for quick local setup
- **Production**: PostgreSQL or SQL Server for scalability
- **Cloud**: Azure SQL, Amazon RDS, Google Cloud SQL

### Migration Strategy

- **Code-First**: Entity definitions drive database schema
- **Versioned Migrations**: All schema changes are versioned and tracked
- **Environment-Specific**: Different configurations for dev/test/prod

## Security Architecture

### Authentication
- **JWT Bearer Tokens**: Stateless authentication
- **Claims-Based**: Role and organization information in claims
- **Token Validation**: Signature verification and expiration checks

### Authorization
- **Organization Isolation**: Automatic data filtering by organization
- **Role-Based Access Control**: Action-level authorization
- **Resource-Based**: Entity-level permissions

### Data Protection
- **Encryption at Rest**: Database-level encryption
- **Encryption in Transit**: HTTPS/TLS for all communications
- **Sensitive Data Handling**: No logging of PII or secrets

## Performance Considerations

### Caching Strategy
- **Memory Caching**: Frequently accessed reference data
- **Distributed Caching**: Redis for multi-instance deployments
- **Query Result Caching**: EF Core query result caching

### Database Optimization
- **Indexed Queries**: Strategic indexing for common query patterns
- **Paginated Results**: Consistent pagination to limit memory usage
- **Query Optimization**: LINQ query optimization and profiling

### Scalability
- **Horizontal Scaling**: Stateless design enables load balancing
- **Database Sharding**: Organization-based partitioning ready
- **Microservice Migration**: Module boundaries support service extraction

## Monitoring & Observability

### Health Checks
- **Database Connectivity**: EF Core health checks
- **External Dependencies**: Master Data service health
- **Application Health**: Memory, CPU, and performance metrics

### Logging
- **Structured Logging**: JSON-formatted logs with correlation IDs
- **Log Levels**: Appropriate logging levels for different environments
- **Security Logging**: Authentication and authorization events

### Metrics
- **Application Metrics**: Request rates, response times, error rates
- **Business Metrics**: Transaction volumes, compliance rates
- **Infrastructure Metrics**: Database performance, memory usage

## Deployment Architecture

### Containerization
- **Docker**: Multi-stage builds for optimized images
- **Health Checks**: Container health monitoring
- **Environment Configuration**: 12-factor app principles

### Orchestration
- **Docker Compose**: Local development and testing
- **Kubernetes**: Production orchestration (optional)
- **Service Discovery**: Built-in service registration

### CI/CD Integration
- **Automated Testing**: Unit and integration test execution
- **Database Migrations**: Automated schema deployment
- **Rolling Deployments**: Zero-downtime updates