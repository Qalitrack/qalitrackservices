# QTruck Docker Deployment

Docker Compose configurations for QTruck application with Next.js frontend and Flask API backend using Traefik reverse proxy.

## Architecture

```
┌─────────────────┐
│     Traefik     │ ← Internet traffic (ports 80/443)
│  (Reverse Proxy)│
└─────────────────┘
         │
    proxy-network
         │
    ┌─────────┬─────────┐
    │         │         │
┌───▼───┐ ┌───▼────┐ ┌─▼──┐
│Frontend│ │   API  │ │Redis│
│(Next.js│ │(Flask) │ │     │
└───────┘ └────────┘ └────┘
```

## Files Structure

```
v0/qtruck/
├── docker-compose.yml        # Production deployment
├── docker-compose.dev.yml   # Development environment
├── .env.example             # Production environment template
├── .env.dev.example         # Development environment template
├── dev/
│   └── init-db.sql         # Database initialization
├── logs/
│   ├── frontend/           # Frontend logs
│   └── api/               # API logs
└── README.md              # This file
```

## Key Features

- **Combined Frontend & Backend**: Single domain with `/api` routing
- **Dynamic Pages**: Next.js App Router with server-side rendering
- **NEXT_PUBLIC_* Support**: Environment variables work in Docker
- **Development Environment**: Hot reload for both frontend and backend
- **Production Ready**: Traefik with SSL, health checks, logging

## Quick Start

### Production Deployment

1. **Create environment file:**
   ```bash
   cp .env.example .env
   # Edit .env with your production values
   ```

2. **Deploy:**
   ```bash
   docker-compose up -d
   ```

3. **View logs:**
   ```bash
   docker-compose logs -f qtruck-frontend
   docker-compose logs -f qtruck-api
   ```

### Development Environment

1. **Create environment file:**
   ```bash
   cp .env.dev.example .env.dev
   # Edit .env.dev with your development values
   ```

2. **Start development environment:**
   ```bash
   docker-compose -f docker-compose.dev.yml --env-file .env.dev up -d
   ```

3. **Access services:**
   - Frontend: http://localhost:3000
   - API: http://localhost:5000
   - Database: localhost:5432
   - Adminer: http://localhost:8080

## Environment Variables

### Production (`.env`)
```bash
DOMAIN=qtruck.yourdomain.com
QTRUCK_FRONTEND_IMAGE=ghcr.io/qalitrack/qalitrackservices/qtruck-frontend:latest
QTRUCK_API_IMAGE=ghcr.io/qalitrack/qalitrackservices/qtruck-api:latest
DATABASE_URL=postgresql://user:password@host:5432/qtruck_production
JWT_SECRET_KEY=your-super-secure-jwt-secret
REDIS_URL=redis://redis:6379/0
```

### Development (`.env.dev`)
```bash
FRONTEND_PORT=3000
API_PORT=5000
POSTGRES_PORT=5432
REDIS_PORT=6379
ADMINER_PORT=8080
DEV_DATABASE_URL=postgresql://qtruck_user:qtruck_password@postgres-dev:5432/qtruck_db
DEV_JWT_SECRET=dev-secret-change-in-production
```

## Routing Configuration

### Production Routes (via Traefik)
- `https://yourdomain.com/` → Next.js Frontend (port 3000)
- `https://yourdomain.com/api/*` → Flask API (port 5000)
- HTTP requests automatically redirect to HTTPS

### Development Routes (Direct Access)
- `http://localhost:3000/` → Next.js Frontend (dev server)
- `http://localhost:5000/api/*` → Flask API (dev server)
- `http://localhost:5432` → PostgreSQL Database
- `http://localhost:8080` → Adminer (Database UI)

## Next.js Environment Variables

The frontend supports both build-time and runtime environment variables:

### Build-time Variables (NEXT_PUBLIC_*)
```javascript
// Available in browser and server
process.env.NEXT_PUBLIC_API_URL      // API endpoint
process.env.NEXT_PUBLIC_SITE_URL     // Site URL
process.env.NEXT_PUBLIC_APP_NAME     // App name
```

### Docker Environment Variables
- **Production**: Set via docker-compose.yml environment section
- **Development**: Loaded from mounted source code .env.local files

## API Configuration

The Flask API is configured to:
- Handle all routes under `/api` prefix
- Maintain `/api` in the path (no stripping)
- Support CORS for frontend domain
- Use Redis for caching and sessions

## Database

### Production
- External PostgreSQL database (configure via DATABASE_URL)
- Migrations handled by API application

### Development
- Local PostgreSQL container
- Automatic database creation
- Adminer for database management
- Volume persistence across restarts

## Health Checks

### Frontend (Next.js)
- **Endpoint**: `GET /`
- **Interval**: 30s
- **Timeout**: 10s
- **Grace Period**: 40s

### API (Flask)
- **Endpoint**: `GET /api/health`
- **Interval**: 30s
- **Timeout**: 10s
- **Grace Period**: 60s

### Database & Redis
- Standard health checks with automatic retries

## Logging

- **Frontend Logs**: `./logs/frontend/`
- **API Logs**: `./logs/api/`
- **Docker Logs**: `docker-compose logs -f [service-name]`

## Volumes

### Production
- `qtruck-uploads`: API file uploads
- `qtruck-redis-data`: Redis persistence

### Development
- `qtruck-frontend-node-modules`: Node.js dependencies
- `qtruck-postgres-dev-data`: Database persistence
- `qtruck-redis-dev-data`: Redis persistence
- Source code mounted for hot reload

## Traefik Configuration

### Required Setup
- Traefik running with:
  - `websecure` entrypoint (port 443)
  - `web` entrypoint (port 80)
  - `letsencrypt` certificate resolver
  - External `proxy-network` network

### Create Proxy Network
```bash
docker network create proxy-network
```

## Commands

```bash
# Production
docker-compose up -d                    # Start services
docker-compose logs -f qtruck-frontend  # View frontend logs
docker-compose logs -f qtruck-api      # View API logs
docker-compose down                     # Stop services
docker-compose pull && docker-compose up -d  # Update images

# Development
docker-compose -f docker-compose.dev.yml --env-file .env.dev up -d    # Start dev
docker-compose -f docker-compose.dev.yml logs -f                      # View logs
docker-compose -f docker-compose.dev.yml down                         # Stop dev

# Database management (development)
docker-compose -f docker-compose.dev.yml exec postgres-dev psql -U qtruck_user -d qtruck_db
```

## Building Images

### Frontend (qtruck-webapp)
```bash
cd ../../apps/qtruck/qtruck-webapp
docker build -t ghcr.io/qalitrack/qalitrackservices/qtruck-frontend:latest .
```

### Backend (qtruck_api)
```bash
cd ../../apps/qtruck/qtruck_api
docker build -t ghcr.io/qalitrack/qalitrackservices/qtruck-api:latest .
```

## Migration from Separate Services

This setup combines frontend and backend under a single domain:

### Before
- Frontend: `https://app.yourdomain.com`
- API: `https://api.yourdomain.com`

### After
- Frontend: `https://yourdomain.com`
- API: `https://yourdomain.com/api`

### Frontend Code Changes Required
Update API calls to use relative paths:
```javascript
// Before
const API_URL = 'https://api.yourdomain.com'

// After
const API_URL = '/api'  // or use NEXT_PUBLIC_API_URL
```

## Troubleshooting

### 1. Frontend Build Issues
- Ensure `output: 'standalone'` in next.config.js
- Check NEXT_PUBLIC_* variables are set correctly
- Verify Node.js version compatibility

### 2. API Routing Issues
- Confirm API handles `/api` prefix correctly
- Check CORS settings for frontend domain
- Verify database connectivity

### 3. Traefik Issues
- Ensure `proxy-network` exists and is external
- Check domain DNS configuration
- Verify SSL certificate generation

### 4. Database Connection Issues
- Confirm DATABASE_URL format and credentials
- Check network connectivity between services
- Verify PostgreSQL extensions are installed

### 5. Development Hot Reload Issues
- Enable WATCHPACK_POLLING for file watching
- Check volume mounts for source code
- Verify port forwarding configuration