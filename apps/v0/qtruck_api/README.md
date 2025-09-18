# QTruck API Docker Deployment

Docker Compose configurations for QTruck API (Django) with Traefik reverse proxy and PostgreSQL database.

## Files Structure

```
apps/v0/qtruck_api/
├── dev/
│   ├── docker-compose.yml      # Development deployment
│   ├── .env.example           # Development environment template
│   └── README.md              # Development setup guide
├── prod/
│   ├── docker-compose.yml     # Production deployment  
│   └── .env.example          # Production environment template
└── README.md                 # This file
```

## Quick Start

### Development Deployment

1. **Navigate to dev directory:**
   ```bash
   cd apps/v0/qtruck_api/dev
   ```

2. **Create environment file:**
   ```bash
   cp .env.example .env
   # Edit .env with your development values
   ```

3. **Deploy:**
   ```bash
   docker-compose up -d
   ```

### Production Deployment

1. **Navigate to prod directory:**
   ```bash
   cd apps/v0/qtruck_api/prod
   ```

2. **Create environment file:**
   ```bash
   cp .env.example .env
   # Edit .env with your production values
   ```

3. **Deploy:**
   ```bash
   docker-compose up -d
   ```

## Environment Configuration

### Development (dev/.env)
```bash
# Database
DB_NAME=qtruck_db_dev
DB_USER=qtruck_user
DB_PASSWORD=your_secure_password

# Django
DEBUG=True
SECRET_KEY=your-secret-key-here
ALLOWED_HOSTS=localhost,127.0.0.1,qtruck-api-dev,qtruck-dev.qalibrated.co.ke
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://127.0.0.1:3000,https://qtruck-dev.qalibrated.co.ke
```

### Production (prod/.env)
```bash
# Database
DB_NAME=qtruck_prod
DB_USER=qtruck_user
DB_PASSWORD=secure_password_here

# Django
SECRET_KEY=your-super-secret-django-key-here
ALLOWED_HOSTS=qtruck.qalibrated.co.ke,api.qtruck.qalibrated.co.ke
CORS_ALLOWED_ORIGINS=https://qtruck.qalibrated.co.ke

# Security
SECURE_SSL_REDIRECT=True
SESSION_COOKIE_SECURE=True
CSRF_COOKIE_SECURE=True
```

## Traefik Configuration

The setup requires Traefik to be running with:
- `websecure` entrypoint on port 443
- `web` entrypoint on port 80  
- `letsencrypt` certificate resolver
- External `proxy-network` network

### Traefik Routes

**Production:**
- Main API: `https://qtruck.qalibrated.co.ke` → Django API (port 8000)
- Alt API: `https://api.qtruck.qalibrated.co.ke` → Django API (port 8000)
- HTTP redirect: Both domains redirect HTTP → HTTPS

**Development:**
- Dev API: `https://qtruck-dev.qalibrated.co.ke` → Django API (port 8000)
- HTTP redirect: `http://qtruck-dev.qalibrated.co.ke` → HTTPS

## Services Architecture

### Production
```
┌─────────────────┐
│     Traefik     │ ← Internet (qtruck.qalibrated.co.ke)
│  (Reverse Proxy)│
└─────────────────┘
         │
    proxy-network
         │
┌─────────────────┐    ┌─────────────────┐
│   qtruck-api    │────│   postgresql    │
│   (Django:8000) │    │   (port 5432)   │
└─────────────────┘    └─────────────────┘
         │                       │
    qtruck_prod_qs_network ──────┘
```

### Development
```
┌─────────────────┐
│     Traefik     │ ← Internet (qtruck-dev.qalibrated.co.ke)
│  (Reverse Proxy)│
└─────────────────┘
         │
    proxy-network
         │
┌─────────────────┐    ┌─────────────────┐
│   qtruck-api    │────│   postgresql    │
│   (Django:8000) │    │   (port 5432)   │
└─────────────────┘    └─────────────────┘
         │                       │
    qtruck_dev_qs_network ───────┘
```

## Health Checks

**Production Only:**
- **QTruck API:** `curl -f http://localhost:8000/health/` every 30s
- **Startup time:** 40s grace period
- **Retries:** 3 attempts with 10s timeout

## Database Management

### PostgreSQL Configuration
- **Image:** `postgres:15-alpine`
- **Data persistence:** Named volumes (`postgresql_data_dev`/`postgresql_data_prod`)
- **Network:** Internal network only (not exposed to Traefik)

### Database Access
```bash
# Development
docker exec -it qtruck-postgresql-dev psql -U qtruck_user -d qtruck_db_dev

# Production  
docker exec -it qtruck-postgresql-prod psql -U qtruck_user -d qtruck_prod
```

## Commands

### Development
```bash
cd apps/v0/qtruck_api/dev

# Start services
docker-compose up -d

# View logs
docker-compose logs -f qtruck_api

# Stop services
docker-compose down

# Database shell
docker-compose exec postgresql psql -U qtruck_user -d qtruck_db_dev

# Django shell
docker-compose exec qtruck_api python manage.py shell

# Run migrations
docker-compose exec qtruck_api python manage.py migrate
```

### Production
```bash
cd apps/v0/qtruck_api/prod

# Start services
docker-compose up -d

# View logs  
docker-compose logs -f qtruck_api

# Stop services
docker-compose down

# Update production image
docker-compose pull
docker-compose up -d

# Backup database
docker-compose exec postgresql pg_dump -U qtruck_user qtruck_prod > backup.sql
```

## Security Considerations

### Production Security
- SSL/TLS encryption via Traefik + Let's Encrypt
- Database only accessible via internal network
- Django security settings enabled (SSL redirect, secure cookies)
- Environment variables for sensitive data

### Network Security
- Internal `qtruck_*_qs_network` for service communication
- External `proxy-network` only for Traefik access
- PostgreSQL not exposed to public internet

## Troubleshooting

1. **Traefik not routing traffic:**
   - Ensure `proxy-network` exists: `docker network create proxy-network`
   - Check Traefik dashboard for registered services
   - Verify labels in docker-compose.yml

2. **Database connection issues:**
   - Check PostgreSQL container logs: `docker-compose logs postgresql`
   - Verify environment variables match in both services
   - Ensure internal network connectivity

3. **Django errors:**
   - Check application logs: `docker-compose logs qtruck_api`
   - Verify SECRET_KEY and ALLOWED_HOSTS are set
   - Run migrations if needed: `docker-compose exec qtruck_api python manage.py migrate`

4. **SSL/HTTPS issues:**
   - Verify Traefik has `letsencrypt` certificate resolver
   - Check domain DNS points to server
   - Ensure ALLOWED_HOSTS includes the domain

## Migration from nginx-proxy

This setup replaces nginx-proxy configuration. Key changes:
- Traefik labels instead of nginx virtual hosts
- Automatic SSL with Let's Encrypt
- Health checks for better monitoring
- Proper network separation