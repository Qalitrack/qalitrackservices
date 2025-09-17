# QTruck API Development Environment

Development environment for QTruck API using Docker with PostgreSQL database.

## Quick Start

```bash
# Start the services
cd apps/v0/qtruck_api/dev
docker compose up -d

# Check services are running
docker compose ps

# View logs
docker compose logs -f qtruck_api
```

## Accessing the API

Since no ports are exposed to the host, access the API via the container's internal IP:

### Get Container IP
```bash
docker inspect qtruck-api-dev --format='{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}'
```

### API Endpoints
- **Base URL**: `http://<container-ip>:8000`
- **API Documentation**: `http://<container-ip>:8000/api/schema/swagger-ui/`
- **Admin Panel**: `http://<container-ip>:8000/admin/`

### Example API Calls
```bash
# Get container IP first
CONTAINER_IP=$(docker inspect qtruck-api-dev --format='{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}')

# Health check
curl http://$CONTAINER_IP:8000/api/health/

# List trips (requires authentication)
curl http://$CONTAINER_IP:8000/api/trips/

# Create user account
curl -X POST http://$CONTAINER_IP:8000/api/auth/register/ \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "testpass123", "email": "test@example.com"}'
```

## Database Management

### Run Migrations
```bash
docker compose exec qtruck_api python manage.py migrate
```

### Create Superuser
```bash
docker compose exec qtruck_api python manage.py createsuperuser
```

### Access Database
```bash
docker compose exec postgresql psql -U qtruck_user -d qtruck_db_dev
```

## Environment Variables

Key configuration in `.env`:
- `DB_NAME`: PostgreSQL database name
- `DB_USER`: PostgreSQL username  
- `DB_PASSWORD`: PostgreSQL password
- `DEBUG`: Django debug mode
- `ALLOWED_HOSTS`: Allowed hostnames for Django

## Network Details

- **Network**: `qtruck_dev_qs_network`
- **API Container**: `qtruck-api-dev` (port 8000)
- **DB Container**: `qtruck-postgresql-dev` (port 5432)
- **No host port exposure**: Access via container IP only

## Troubleshooting

### Check Container Status
```bash
docker compose ps
docker compose logs qtruck_api
```

### Database Connection Issues
```bash
# Test database connectivity
docker compose exec qtruck_api python manage.py dbshell
```

### Reset Environment
```bash
docker compose down -v  # Removes volumes (data loss!)
docker compose up -d
```