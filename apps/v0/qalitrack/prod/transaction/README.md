# Transaction Service - Production Deployment

Production deployment configuration for the Transaction microservice.

## Architecture

This deployment follows the same pattern as the Backup Service with 4 services:

1. **transaction-service-prod** - Main ASP.NET Core API service
2. **postgres-transaction-prod** - PostgreSQL 15 database
3. **redis-transaction-prod** - Redis 7 cache layer
4. **rabbitmq-transaction-prod** - RabbitMQ 3 message broker

## Directory Structure

```
transaction/
├── docker-compose.yml      # Main deployment configuration
├── .env                    # Environment variables (gitignored)
├── .env.example           # Environment template
├── data/                  # Application data
├── logs/                  # Application logs
├── metadata/              # Transaction metadata
├── init-scripts/          # Database initialization scripts
└── README.md             # This file
```

## Prerequisites

1. **External Docker Volumes** must be created before deployment:
   ```bash
   docker volume create postgres_transaction_prod_data
   docker volume create redis_transaction_prod_data
   docker volume create rabbitmq_transaction_prod_data
   ```

2. **External Networks** must exist:
   - `qalitrack-proxy` - Internal service network
   - `proxy-network` - Traefik proxy network

3. **Traefik** must be running with Let's Encrypt configuration

4. **JWT Auth Service** must be running at `qalitrack-jwt-auth-service:3000`

## Configuration

1. Copy the environment template:
   ```bash
   cp .env.example .env
   ```

2. Update `.env` with your values:
   - `QALITRACK_PROD_DOMAIN` - Your production domain
   - Database passwords
   - Redis password
   - RabbitMQ credentials
   - JWT secret key

## Deployment

### Start Services

```bash
docker compose up -d
```

### View Logs

```bash
docker compose logs -f transaction-service-prod
```

### Stop Services

```bash
docker compose down
```

### Update Service

```bash
docker compose pull
docker compose up -d
```

## Traefik Routing

### Protected Endpoints (JWT Required)

**Route:** `https://qalitrack.cseco.co.ke/api/Transaction/*`

- Requires JWT authentication via ForwardAuth
- User ID and email passed in headers: `X-User-Id`, `X-User-Email`
- Path `/api` is stripped before forwarding to service
- Priority: 100

### Public Endpoints (No JWT)

**Route:** `https://qalitrack.cseco.co.ke/api/Microservice/*`

- No authentication required
- Read-only microservice information
- Path `/api` is stripped before forwarding to service
- Priority: 100

## Middleware

### Custom Middleware

- **transaction-jwt-forwardauth** - JWT authentication via ForwardAuth
  - Validates token with `qalitrack-jwt-auth-service`
  - Passes user headers to service

### Shared Middleware

- **strip-api-prefix** - Removes `/api` from request path

## Service Endpoints

- Transaction API: `https://qalitrack.cseco.co.ke/api/Transaction`
- Microservice Info: `https://qalitrack.cseco.co.ke/api/Microservice`

## Resource Limits

- **Transaction Service**: 1GB RAM, 1.0 CPU
- **PostgreSQL**: 2GB RAM, 2.0 CPUs
- **Redis**: 512MB RAM, 0.5 CPU
- **RabbitMQ**: 512MB RAM, 0.5 CPU

## Database Configuration

- **Database**: transactiondb
- **User**: transaction
- **Password**: transaction123 (change in .env)
- **Max Connections**: 600
- **Shared Buffers**: 512MB

## Redis Configuration

- **Port**: 6379 (internal only)
- **Password**: transaction123 (change in .env)
- **Max Memory**: 256MB
- **Eviction Policy**: allkeys-lru

## RabbitMQ Configuration

- **Port**: 5672 (internal only)
- **Management Port**: 15672 (internal only)
- **User**: admin
- **Password**: securepassword (change in .env)

## Security

- ✅ No ports exposed to host
- ✅ All traffic through Traefik reverse proxy
- ✅ JWT authentication on API endpoints
- ✅ SSL/TLS with Let's Encrypt
- ✅ Resource limits on all containers
- ✅ Isolated network

## Troubleshooting

### Check Service Health

```bash
docker compose ps
docker compose logs transaction-service-prod
```

### Check Database Connection

```bash
docker compose exec postgres-transaction-prod psql -U transaction -d transactiondb
```

### Check Redis Connection

```bash
docker compose exec redis-transaction-prod redis-cli -a transaction123 ping
```

### Check RabbitMQ

```bash
docker compose exec rabbitmq-transaction-prod rabbitmqctl status
```

## Backup & Restore

### Backup Database

```bash
docker compose exec postgres-transaction-prod pg_dump -U transaction transactiondb > backup.sql
```

### Restore Database

```bash
docker compose exec -T postgres-transaction-prod psql -U transaction transactiondb < backup.sql
```

## Monitoring

Logs are stored in `./logs/` directory and can be accessed on the host machine.

## Notes

- Based on Backup Service deployment pattern
- Follows QaliTrack production standards
- External volumes ensure data persistence
- All services restart automatically unless stopped
