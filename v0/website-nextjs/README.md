# QS Website Next.js Docker Deployment

Docker Compose configurations for QS Website Next.js application with Traefik reverse proxy.

## Files Structure

```
v0/website-nextjs/
├── docker-compose.yml        # Production deployment
├── docker-compose.dev.yml   # Development environment
├── .env.example             # Environment variables template
├── config/
│   ├── runtime-config.js    # Production runtime config
│   └── dev-config.js        # Development runtime config
└── README.md               # This file
```

## Quick Start

### Production Deployment

1. **Create environment file:**
   ```bash
   cp .env.example .env
   # Edit .env with your production values
   ```

2. **Update runtime configuration:**
   ```bash
   # Edit config/runtime-config.js with your production API URLs
   ```

3. **Deploy:**
   ```bash
   docker-compose up -d
   ```

### Development Environment

1. **Create environment file:**
   ```bash
   cp .env.example .env.dev
   # Edit .env.dev with your development values
   ```

2. **Start development environment:**
   ```bash
   docker-compose -f docker-compose.dev.yml --env-file .env.dev up -d
   ```

## Environment Variables

### Production (`.env`)
```bash
DOMAIN=qalibrated.co.ke
QS_WEBSITE_NEXTJS_IMAGE=ghcr.io/qalitrack/qalitrackservices/qs-website-nextjs:latest
API_BASE_URL=https://api.qalibrated.co.ke
```

### Development (`.env.dev`)
```bash
DEV_API_BASE_URL=http://localhost:5000/api
DEV_DATABASE_URL=postgresql://qsuser:qspassword@postgres-dev:5432/qsdb
DEV_JWT_SECRET=your-dev-jwt-secret-here
NEXTJS_PORT=3001
API_PORT=5000
POSTGRES_PORT=5432
```

## Traefik Configuration

The production setup requires Traefik to be running with:
- `websecure` entrypoint on port 443
- `web` entrypoint on port 80  
- `letsencrypt` certificate resolver
- External `proxy-network` network

### Traefik Routes

**Production:**
- Main site: `https://qalibrated.co.ke` → Next.js app (port 3001)
- HTTP redirect: `http://qalibrated.co.ke` → HTTPS

**Development:**
- Site: `http://localhost:3001` → Next.js dev server
- API: `http://localhost:5000` → API dev server
- Database: `http://localhost:5432` → PostgreSQL

## Runtime Configuration

The application uses runtime configuration files that can be modified without rebuilding:

- **Production:** `config/runtime-config.js` → mounted to `/app/public/config.js`
- **Development:** `config/dev-config.js` → mounted to `/app/public/config.js`

This allows changing API endpoints, site URLs, and other settings after deployment.

## Network Architecture

```
┌─────────────────┐
│     Traefik     │ ← Internet traffic (ports 80/443)
│  (Reverse Proxy)│
└─────────────────┘
         │
    proxy-network
         │
┌─────────────────┐
│ qs-website-nextjs│ ← Next.js application (port 3001)
└─────────────────┘
         │
 qs-website-network
```

## Health Checks

- **Next.js App:** `wget http://127.0.0.1:3001/` every 30s
- **Startup time:** 40s grace period
- **Retries:** 3 attempts with 10s timeout

## Volumes

- `./config/runtime-config.js` → Runtime configuration (read-only)
- `./logs/app` → Application logs

## Commands

```bash
# Production
docker-compose up -d                    # Start services
docker-compose logs -f qs-website-nextjs # View logs
docker-compose down                     # Stop services

# Development  
docker-compose -f docker-compose.dev.yml up -d    # Start dev environment
docker-compose -f docker-compose.dev.yml logs -f  # View logs
docker-compose -f docker-compose.dev.yml down     # Stop dev environment

# Update production image
docker-compose pull
docker-compose up -d
```

## Migration from nginx-proxy

This setup replaces the previous nginx-proxy configuration. Key differences:
- Uses Traefik labels instead of nginx virtual hosts
- Automatic SSL with Let's Encrypt
- Runtime configuration support
- Health checks and better monitoring

## Troubleshooting

1. **Traefik not routing traffic:**
   - Ensure `proxy-network` exists: `docker network create proxy-network`
   - Check Traefik dashboard for registered services

2. **SSL issues:**
   - Verify Traefik has `letsencrypt` certificate resolver configured
   - Check domain DNS points to server

3. **Health check failures:**
   - Verify Next.js app starts correctly: `docker-compose logs qs-website-nextjs`
   - Check port 3001 is accessible inside container