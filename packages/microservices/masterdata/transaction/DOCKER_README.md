# Transaction Microservice - Docker Setup

This document explains how to run the Transaction microservice using Docker and Docker Compose with PostgreSQL.

## Prerequisites

- Docker installed (version 20.10+)
- Docker Compose installed (version 2.0+)

## Quick Start

### 1. Start PostgreSQL Database Only

To run only the PostgreSQL database:

```bash
docker-compose up -d postgres
```

This will:
- Start PostgreSQL 17 (Alpine) on port 5432
- Create database: `qalitrack_transactions`
- Username: `postgres`
- Password: `postgres`

### 2. Start Everything (Database + API)

To run both PostgreSQL and the Transaction API:

```bash
docker-compose up -d
```

This will:
- Start PostgreSQL database
- Build and start the Transaction API on port 5112
- Automatically run EF Core migrations on startup

### 3. Access the Services

Once running, you can access:

- **Transaction API**: http://localhost:5112
- **Swagger UI**: http://localhost:5112/swagger
- **Health Check**: http://localhost:5112/health
- **PostgreSQL**: localhost:5432

## Database Migrations

### Automatic Migrations (Recommended for Docker)

Migrations are automatically applied when the application starts. This is configured in `Program.cs`:

```csharp
dbContext.Database.Migrate();
```

No manual intervention needed when running in Docker!

### Manual Migrations (Development)

If you're running locally without Docker:

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project src/Transaction.Infrastructure --startup-project src/Transaction.Api

# Apply migrations
dotnet ef database update --project src/Transaction.Infrastructure --startup-project src/Transaction.Api

# Remove last migration
dotnet ef migrations remove --project src/Transaction.Infrastructure --startup-project src/Transaction.Api
```

## Environment Variables

### PostgreSQL

| Variable | Default Value | Description |
|----------|--------------|-------------|
| `POSTGRES_DB` | `qalitrack_transactions` | Database name |
| `POSTGRES_USER` | `postgres` | Database username |
| `POSTGRES_PASSWORD` | `postgres` | Database password |
| `PGDATA` | `/var/lib/postgresql/data/pgdata` | Data directory |

### Transaction API

| Variable | Default Value | Description |
|----------|--------------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Environment (Development/Production) |
| `ASPNETCORE_URLS` | `http://+:8080` | URLs to listen on |
| `ConnectionStrings__DefaultConnection` | See docker-compose.yml | PostgreSQL connection string |

## Docker Commands

### View Logs

```bash
# All services
docker-compose logs -f

# Only API
docker-compose logs -f transaction_api

# Only Database
docker-compose logs -f postgres
```

### Stop Services

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (⚠️ deletes database data)
docker-compose down -v
```

### Rebuild and Restart

```bash
# Rebuild API image
docker-compose build transaction_api

# Restart with rebuild
docker-compose up -d --build
```

### Execute Commands in Containers

```bash
# Access PostgreSQL
docker-compose exec postgres psql -U postgres -d qalitrack_transactions

# Access API container bash
docker-compose exec transaction_api /bin/bash
```

## Database Connection Strings

### From Docker Container (API → PostgreSQL)

```
Host=postgres;Port=5432;Database=qalitrack_transactions;Username=postgres;Password=postgres
```

### From Host Machine (Local Development)

```
Host=localhost;Port=5432;Database=qalitrack_transactions;Username=postgres;Password=postgres
```

Update in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=qalitrack_transactions;Username=postgres;Password=postgres"
  }
}
```

## Production Considerations

### Security

⚠️ **DO NOT use default passwords in production!**

1. **Change PostgreSQL password**:

```yaml
environment:
  POSTGRES_PASSWORD: ${DB_PASSWORD}  # Use environment variable
```

2. **Use secrets management**:
   - Docker secrets
   - Kubernetes secrets
   - Azure Key Vault
   - AWS Secrets Manager

3. **Update connection string**:

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=qalitrack_transactions;Username=postgres;Password=${DB_PASSWORD}
```

### Volume Persistence

Database data is persisted in a Docker volume:

```yaml
volumes:
  postgres_data:
    driver: local
```

To backup:

```bash
# Backup database
docker-compose exec postgres pg_dump -U postgres qalitrack_transactions > backup.sql

# Restore database
docker-compose exec -T postgres psql -U postgres qalitrack_transactions < backup.sql
```

### Health Checks

Both services have health checks configured:

**PostgreSQL**:
```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U postgres"]
  interval: 10s
  timeout: 5s
  retries: 5
```

**Transaction API**:
```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1
```

## Troubleshooting

### Database Connection Errors

If the API can't connect to PostgreSQL:

1. Check PostgreSQL is running:
   ```bash
   docker-compose ps postgres
   ```

2. Check PostgreSQL logs:
   ```bash
   docker-compose logs postgres
   ```

3. Verify network connectivity:
   ```bash
   docker-compose exec transaction_api ping postgres
   ```

### Migration Errors

If migrations fail:

1. Check API logs:
   ```bash
   docker-compose logs transaction_api
   ```

2. Manually run migrations:
   ```bash
   docker-compose exec transaction_api dotnet ef database update
   ```

3. Reset database (⚠️ deletes all data):
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

### Port Conflicts

If port 5432 or 5112 is already in use:

```yaml
ports:
  - "5433:5432"  # Change host port
```

## Development Workflow

### Local Development with Docker Database

1. Start only PostgreSQL:
   ```bash
   docker-compose up -d postgres
   ```

2. Run API locally:
   ```bash
   dotnet run --project src/Transaction.Api
   ```

3. Access Swagger:
   - http://localhost:5112/swagger

### Full Docker Development

1. Make code changes
2. Rebuild and restart:
   ```bash
   docker-compose up -d --build
   ```

3. View logs:
   ```bash
   docker-compose logs -f transaction_api
   ```

## Files

| File | Purpose |
|------|---------|
| `docker-compose.yml` | Docker Compose configuration |
| `Dockerfile` | API container build instructions |
| `appsettings.json` | Application configuration |
| `src/Transaction.Infrastructure/Migrations/` | EF Core migrations |

## Additional Commands

### Database Management

```bash
# List databases
docker-compose exec postgres psql -U postgres -c '\l'

# List tables
docker-compose exec postgres psql -U postgres -d qalitrack_transactions -c '\dt'

# Run SQL query
docker-compose exec postgres psql -U postgres -d qalitrack_transactions -c 'SELECT * FROM "WeighbridgeTransactions" LIMIT 5;'
```

### Clean Up

```bash
# Remove stopped containers
docker-compose rm

# Remove all unused Docker resources
docker system prune -a --volumes
```

## Summary

✅ **PostgreSQL 17** running on port 5432
✅ **Transaction API** running on port 5112
✅ **Automatic migrations** on container startup
✅ **Health checks** for both services
✅ **Persistent volumes** for database data
✅ **Swagger UI** available at /swagger

**Ready for development and deployment!** 🚀
