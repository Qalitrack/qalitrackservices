# Getting Started

## Overview

This guide will help you set up and start using the QaliTrack Master Data Service. Whether you're running it locally for development, deploying it to production, or integrating with existing systems, this guide covers everything you need to get up and running quickly.

## Prerequisites

Before you begin, ensure you have the following installed:

### Required Software
- **.NET 8 SDK**: Download from [Microsoft .NET](https://dotnet.microsoft.com/download)
- **Docker**: For containerized deployment ([Docker Desktop](https://www.docker.com/products/docker-desktop/))
- **Git**: For source code management
- **Database**: One of the following:
  - PostgreSQL 13+ (recommended for production)
  - SQL Server 2019+ (enterprise environments)
  - SQLite (development and testing only)

### Development Tools (Optional)
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **Postman** or **Insomnia** for API testing
- **pgAdmin** or **SQL Server Management Studio** for database management

## Quick Start with Docker

The fastest way to get started is using Docker Compose:

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/qalitrackservices.git
cd qalitrackservices/packages/QaliTrack.MasterData
```

### 2. Start with Docker Compose
```bash
# Start all services (API + PostgreSQL database)
docker-compose up -d

# Check that services are running
docker-compose ps

# View logs
docker-compose logs -f qalitrack-masterdata
```

### 3. Verify Installation
```bash
# Check API health
curl http://localhost:8080/health

# Access Swagger UI
# Open browser: http://localhost:8080/swagger

# Access documentation
# Open browser: http://localhost:8080/docs
```

The service will be available at:
- **API**: http://localhost:8080
- **Documentation**: http://localhost:8080/docs
- **Health Checks**: http://localhost:8080/health
- **Swagger UI**: http://localhost:8080/swagger

## Local Development Setup

For development and customization, set up the service locally:

### 1. Clone and Setup
```bash
git clone https://github.com/yourusername/qalitrackservices.git
cd qalitrackservices/packages/QaliTrack.MasterData
```

### 2. Configure Database Connection
Create a `appsettings.Development.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QaliTrackMasterData;User Id=postgres;Password=yourpassword;"
  },
  "DatabaseProvider": "PostgreSQL"
}
```

For SQLite (development only):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=masterdata.db"
  },
  "DatabaseProvider": "Sqlite"
}
```

### 3. Install Dependencies and Run Migrations
```bash
# Restore NuGet packages
dotnet restore

# Create and run database migrations
dotnet ef database update --project src/QaliTrack.MasterData.Infrastructure --startup-project src/QaliTrack.MasterData.Api

# Build the solution
dotnet build
```

### 4. Run the Application
```bash
# Run the API service
dotnet run --project src/QaliTrack.MasterData.Api

# Or use the development profile
dotnet run --project src/QaliTrack.MasterData.Api --launch-profile Development
```

The service will start on `https://localhost:7001` and `http://localhost:5001`.

## Your First API Call

Let's test the service with some basic API calls:

### 1. Check Service Health
```bash
curl -X GET "http://localhost:8080/health" \
  -H "accept: application/json"
```

Expected response:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.123456",
  "entries": {
    "database": {
      "data": {},
      "duration": "00:00:00.123456",
      "status": "Healthy"
    }
  }
}
```

### 2. Create Your First Organization
```bash
curl -X POST "http://localhost:8080/api/organizations" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Demo Transport Company",
    "description": "A demo organization for getting started",
    "industry": "Transportation",
    "timeZone": "UTC",
    "currency": "USD"
  }'
```

### 3. List Organizations
```bash
curl -X GET "http://localhost:8080/api/organizations" \
  -H "accept: application/json"
```

### 4. Create a Vehicle
```bash
curl -X POST "http://localhost:8080/api/vehicles" \
  -H "Content-Type: application/json" \
  -d '{
    "licensePlate": "ABC-123",
    "make": "Toyota",
    "model": "Hilux",
    "year": 2023,
    "vehicleType": "Pickup",
    "organizationId": "your-org-id-from-step-2"
  }'
```

### 5. Create a Driver
```bash
curl -X POST "http://localhost:8080/api/drivers" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@demo.com",
    "phone": "+1234567890",
    "licenseNumber": "DL123456789",
    "organizationId": "your-org-id-from-step-2"
  }'
```

## Exploring the API

### Interactive Documentation
Visit the Swagger UI at `http://localhost:8080/swagger` to:
- Browse all available endpoints
- Test API calls directly in the browser
- View request/response schemas
- Download API specifications

### Using Postman
Import the OpenAPI specification:
1. Open Postman
2. Click "Import" → "Link"
3. Enter: `http://localhost:8080/swagger/v1/swagger.json`
4. Start testing endpoints

### Key API Patterns

#### Pagination
Most list endpoints support pagination:
```bash
GET /api/vehicles?page=1&pageSize=10
```

#### Filtering
Use query parameters for filtering:
```bash
GET /api/vehicles?make=Toyota&year=2023
```

#### Organization Context
Include organization ID for multi-tenant operations:
```bash
GET /api/vehicles?organizationId=your-org-id
```

## Sample Data and Scenarios

### Transportation Company Setup
```bash
# 1. Create organization
curl -X POST "http://localhost:8080/api/organizations" \
  -H "Content-Type: application/json" \
  -d '{"name": "Acme Transport", "industry": "Transportation"}'

# 2. Add vehicles
curl -X POST "http://localhost:8080/api/vehicles" \
  -H "Content-Type: application/json" \
  -d '{"licensePlate": "TRK-001", "make": "Volvo", "model": "FH16", "organizationId": "org-id"}'

# 3. Add drivers
curl -X POST "http://localhost:8080/api/drivers" \
  -H "Content-Type: application/json" \
  -d '{"firstName": "Alice", "lastName": "Smith", "licenseNumber": "CDL123", "organizationId": "org-id"}'

# 4. Create driver-vehicle assignment
curl -X POST "http://localhost:8080/api/relationships/driver-vehicle-assignments" \
  -H "Content-Type: application/json" \
  -d '{"driverId": "driver-id", "vehicleId": "vehicle-id", "startDate": "2024-01-01"}'
```

### SACCO Management Setup
```bash
# 1. Create SACCO
curl -X POST "http://localhost:8080/api/saccos" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Transport Workers SACCO",
    "registrationNumber": "SACCO/001/2024",
    "saccoType": "Transport",
    "organizationId": "org-id"
  }'

# 2. Register members
curl -X POST "http://localhost:8080/api/saccos/{sacco-id}/members" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Driver",
    "membershipType": "Regular",
    "initialShares": 10
  }'
```

## Common Configuration

### Environment Variables
Key environment variables for configuration:

```bash
# Database
export DB_CONNECTION_STRING="Server=localhost;Database=QaliTrackMasterData;..."
export DB_PROVIDER="PostgreSQL"

# API Configuration
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://+:8080"

# Authentication (if enabled)
export JWT_SECRET="your-secret-key"
export JWT_ISSUER="qalitrack-masterdata"

# Logging
export SERILOG_MINIMUM_LEVEL="Information"
```

### Database Providers
Switch between database providers in configuration:

#### PostgreSQL (Recommended)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QaliTrackMasterData;User Id=postgres;Password=yourpassword;"
  },
  "DatabaseProvider": "PostgreSQL"
}
```

#### SQL Server
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QaliTrackMasterData;Trusted_Connection=true;"
  },
  "DatabaseProvider": "SqlServer"
}
```

#### SQLite (Development)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=masterdata.db"
  },
  "DatabaseProvider": "Sqlite"
}
```

## Development Workflow

### Making Changes
1. Make code changes in `src/` directory
2. Run tests: `dotnet test`
3. Update database schema: `dotnet ef migrations add YourMigrationName`
4. Apply migrations: `dotnet ef database update`
5. Test changes: `dotnet run`

### Documentation Updates
1. Update XML comments in C# code for API reference
2. Update markdown files in `docs/` for conceptual documentation
3. Regenerate documentation: `docfx`
4. View updated docs: `docfx serve _site`

## Troubleshooting

### Common Issues

#### Database Connection Failed
```bash
# Check database is running
docker ps
# or for local PostgreSQL
pg_isready -h localhost

# Verify connection string
dotnet ef database show
```

#### Port Already in Use
```bash
# Check what's using the port
netstat -tulpn | grep 8080
# or
lsof -i :8080

# Kill the process or use a different port
export ASPNETCORE_URLS="http://+:8081"
```

#### Migration Issues
```bash
# Reset database (development only)
dotnet ef database drop
dotnet ef database update

# Create new migration
dotnet ef migrations add FixIssue
```

### Getting Help

1. **Documentation**: Check the [full documentation](../docs/introduction.md)
2. **API Reference**: Browse the [API documentation](../api/)
3. **Logs**: Check application logs for detailed error messages
4. **Health Checks**: Visit `/health` endpoint for system status

## Next Steps

Now that you have the service running:

1. **[API Overview](api-overview.md)**: Learn about API patterns and conventions
2. **[Architecture](architecture.md)**: Understand the system design
3. **[Module Documentation](modules/business-entities.md)**: Explore specific business domains
4. **[Security](security.md)**: Configure authentication and authorization
5. **[Deployment](deployment.md)**: Deploy to production environments

## Production Considerations

Before deploying to production:

- [ ] Configure proper database with connection pooling
- [ ] Set up authentication and authorization
- [ ] Configure logging and monitoring
- [ ] Set up SSL/TLS certificates
- [ ] Configure backup and disaster recovery
- [ ] Review security settings
- [ ] Set up CI/CD pipelines
- [ ] Configure environment-specific settings

Welcome to QaliTrack Master Data Service! You're now ready to build powerful master data management solutions.