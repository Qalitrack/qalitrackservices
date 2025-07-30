# Docker Setup for User Service with PostgreSQL

This setup provides a complete Docker environment for the User Service with PostgreSQL database and Redis cache.

## Files Created

- `Dockerfile.postgres` - PostgreSQL database container
- `docker-compose.yml` - Orchestrates all services
- `scripts/init-db.sql` - Database initialization script

## Quick Start

1. **Start all services:**
   ```bash
   docker-compose up -d
   ```

2. **View logs:**
   ```bash
   # All services
   docker-compose logs -f
   
   # Specific service
   docker-compose logs -f user-service
   docker-compose logs -f postgres-db
   docker-compose logs -f redis-cache
   ```

3. **Stop all services:**
   ```bash
   docker-compose down
   ```

4. **Stop and remove all data:**
   ```bash
   docker-compose down -v
   ```

## Services

### PostgreSQL Database
- **Container:** `userservice-postgres`
- **Port:** `5432`
- **Database:** `userservicedb`
- **User:** `userservice`
- **Password:** `userservice123`
- **Readonly User:** `userservice_readonly` (password: `readonly123`)

### Redis Cache
- **Container:** `userservice-redis`
- **Port:** `6379`
- **Password:** `userservice123`

### User Service API
- **Container:** `userservice-api`
- **Port:** `8081`
- **Health Check:** `http://localhost:8081/health`
- **Swagger:** `http://localhost:8081/swagger` (in development)

## Environment Configuration

The User Service is configured to use:
- **PostgreSQL** as the primary database
- **Redis** for caching (suitable for 1000+ users)
- **Production** environment settings

### Connection Strings
- **PostgreSQL:** `Host=postgres-db;Port=5432;Database=userservicedb;Username=userservice;Password=userservice123;`
- **Redis:** `redis-cache:6379,password=userservice123`

## Database Management

### Connect to PostgreSQL
```bash
# Using docker exec
docker exec -it userservice-postgres psql -U userservice -d userservicedb

# Using local psql client
psql -h localhost -p 5432 -U userservice -d userservicedb
```

### Create Performance Indexes
After Entity Framework migrations have created tables:
```sql
SELECT create_user_service_indexes();
```

### Connect to Redis
```bash
# Using docker exec
docker exec -it userservice-redis redis-cli -a userservice123
```

## Development vs Production

### Development Mode
To run in development mode, modify the docker-compose.yml:
```yaml
user-service:
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
```

### Production Considerations
1. **Change default passwords** in all services
2. **Use Docker secrets** for sensitive data
3. **Configure proper JWT secret** via environment variables
4. **Set up SSL/TLS** certificates
5. **Configure backup strategy** for PostgreSQL
6. **Monitor resource usage** and adjust container limits

## Scaling

### For 1000+ Users
The current configuration is optimized for 1000+ users:
- PostgreSQL with 200 max connections
- Redis caching enabled
- Optimized PostgreSQL settings

### Horizontal Scaling
To scale the API:
```yaml
user-service:
  deploy:
    replicas: 3
```

Add a load balancer (nginx, traefik) in front of the API instances.

## Troubleshooting

### Check Service Health
```bash
docker-compose ps
```

### Database Connection Issues
1. Ensure PostgreSQL is healthy: `docker-compose logs postgres-db`
2. Check connection string in API logs
3. Verify network connectivity between containers

### Performance Issues
1. Monitor PostgreSQL performance: `docker exec -it userservice-postgres pg_stat_activity`
2. Check Redis cache hit rates: `docker exec -it userservice-redis redis-cli -a userservice123 info stats`
3. Review API logs for slow queries

## Backup and Restore

### Backup Database
```bash
docker exec userservice-postgres pg_dump -U userservice userservicedb > backup.sql
```

### Restore Database
```bash
docker exec -i userservice-postgres psql -U userservice userservicedb < backup.sql
```

## Monitoring

### Health Checks
All services include health checks:
- PostgreSQL: `pg_isready`
- Redis: `redis-cli ping`
- User Service: `/health` endpoint

### Logs Location
- API logs: `./logs` directory (mounted volume)
- Database logs: Container logs via `docker-compose logs`