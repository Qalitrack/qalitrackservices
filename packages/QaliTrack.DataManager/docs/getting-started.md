# Getting Started

This guide will help you set up and run the QaliTrack Data Manager Service locally.

## Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- SQL Server, PostgreSQL, or SQLite (for development)

## Quick Setup

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd qalitrackservices/packages/QaliTrack.DataManager
```

### 2. Environment Configuration

Create a `.env` file based on `.env.sample`:

```bash
cp .env.sample .env
```

Edit the `.env` file with your database connection strings and configuration.

### 3. Database Setup

Run database migrations:

```bash
dotnet ef database update --project src/QaliTrack.DataManager.Infrastructure --startup-project src/QaliTrack.DataManager.Api
```

### 4. Run the Service

#### Using .NET CLI

```bash
dotnet run --project src/QaliTrack.DataManager.Api
```

#### Using Docker

```bash
docker-compose up -d
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000` (root path)

## Configuration

### Database Providers

The service supports multiple database providers:

- **SQLite** (development): Minimal setup, file-based
- **PostgreSQL** (recommended): Production-ready with full feature support
- **SQL Server**: Enterprise environments

Configure via `ConnectionStrings` in `appsettings.json` or environment variables.

### Master Data Integration

Configure the Master Data service connection:

```json
{
  "MasterDataService": {
    "BaseUrl": "http://localhost:3000",
    "Timeout": "00:00:30"
  }
}
```

### Multi-Tenancy

Organizations are automatically isolated using the `OrganizationId` from JWT claims or request headers.

## Testing the API

### Using Swagger UI

1. Navigate to `http://localhost:5000`
2. Explore the available endpoints
3. Test with the interactive interface

### Using curl

Get weight measurements:

```bash
curl -X GET "http://localhost:5000/weightdata/measurements?page=1&page_size=10" \
     -H "accept: application/json" \
     -H "X-Organization-Id: your-org-id"
```

Create a transaction:

```bash
curl -X POST "http://localhost:5000/transactions" \
     -H "Content-Type: application/json" \
     -H "X-Organization-Id: your-org-id" \
     -d '{
       "transactionNumber": "TXN-001",
       "transactionType": "Inbound",
       "vehicleId": "vehicle-uuid",
       "driverId": "driver-uuid",
       "productId": "product-uuid"
     }'
```

## Development Workflow

### 1. Database Migrations

Add a new migration:

```bash
dotnet ef migrations add MigrationName --project src/QaliTrack.DataManager.Infrastructure --startup-project src/QaliTrack.DataManager.Api
```

### 2. Code Generation

The service uses Entity Framework Core with code-first migrations. Models are defined in the `Core` project under `Modules/[ModuleName]/Entities/`.

### 3. Testing

Run unit tests:

```bash
dotnet test
```

### 4. API Documentation

Update XML documentation comments in controllers for automatic Swagger generation.

## Troubleshooting

### Common Issues

**Database Connection Failed**
- Verify connection string in configuration
- Ensure database server is running
- Check firewall settings

**Master Data Service Unavailable**
- Verify Master Data service is running
- Check network connectivity
- Review service discovery configuration

**JWT Token Issues**
- Ensure proper Authorization header format
- Verify token signature and expiration
- Check issuer and audience configuration

### Logging

Check application logs for detailed error information:

```bash
docker-compose logs -f datamanager
```

## Next Steps

- Review the [Architecture Guide](architecture.md) to understand the system design
- Explore the [API Guidelines](api-guidelines.md) for development standards
- Check the [Database Schema](database-schema.md) for data modeling details