# Local Development Deployment

This directory contains Docker Compose configuration for running QaliTrack services locally for development.

## Services Included

| Service | Port | Description |
|---------|------|-------------|
| Gateway | 7000 | API Gateway (Production Gateway) |
| User Service | 7001 | Authentication & User Management |
| Masterdata Service | 7002 | Consolidated Master Data APIs |
| Transaction Service | 7015 | Transaction Processing |
| Backup Service | 7020 | Database Backup & Restore |

## Prerequisites

- Docker Engine 20.10+
- Docker Compose 2.0+

## Quick Start

### 1. Start All Services

```bash
cd /home/cyber-alchemist/qalitrackservices/scripts/deployments
docker compose up -d
```

### 2. Start Specific Services Only

```bash
# Start only user service and gateway
docker compose up -d user-service gateway
```

### 3. View Logs

```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f [service-name]

# Last 100 lines
docker compose logs --tail=100 -f
```

### 4. Stop Services

```bash
# Stop all
docker compose down

# Stop but keep data volumes
docker compose stop

# Stop and remove volumes (DELETES ALL DATA)
docker compose down -v
```

## Configuration

### Environment Variables

All services use **Development** environment by default with SQLite databases.

For PostgreSQL (production-like setup), update `docker-compose.yml`:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - UsePostgreSQL=true
  - ConnectionStrings__DefaultConnection=Host=postgres;Database=mydb;Username=user;Password=pass
```

### Ports

Default ports are mapped in `docker-compose.yml`. To change:

```yaml
ports:
  - "8080:80"  # Map container port 80 to host port 8080
```

## Data Persistence

Data is stored in Docker volumes:

- `user-data` - User service SQLite database
- etc.

**To reset all data:**
```bash
docker compose down -v
```

## Troubleshooting

### Service won't start

```bash
# Check logs
docker compose logs [service-name]

# Rebuild image
docker compose build [service-name]
docker compose up -d [service-name]
```

### Port already in use

```bash
# Check what's using the port
sudo lsof -i :7001

# Change port in docker-compose.yml
ports:
  - "7101:80"  # Use 7101 instead of 7001
```

### Database connection errors

Ensure SQLite mode is enabled:
```yaml
environment:
  - UsePostgreSQL=false
  - ConnectionStrings__DefaultConnection=Data Source=/data/mydb.db
```

## Testing Services

### Gateway
```bash
curl http://localhost:7000/health
```

### User Service
```bash
curl http://localhost:7001/health
```

## Adding New Services

When a new service is created:

1. Add to `docker-compose.yml`:
```yaml
  my-new-service:
    build:
      context: ../../packages/microservices/path/to/service
      dockerfile: Dockerfile
    ports:
      - "7XXX:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - my-service-data:/data
    networks:
      - qalitrack-network
```

2. Add volume:
```yaml
volumes:
  my-service-data:
```

3. Start it:
```bash
docker compose up -d my-new-service
```

## Production Deployment

**DO NOT USE THIS FOR PRODUCTION!**

For production, use:
- Kubernetes Helm charts in `/kubernetes/helm-charts/`
- ArgoCD GitOps deployment
- Images from GHCR (GitHub Container Registry)

See `/kubernetes/README.md` for production deployment instructions.
