# QaliTrack Chained Traefik Setup

This directory contains the chained Traefik configuration for QaliTrack services with separate dev and prod environments.

## Architecture

```
[Internet]
    ↓
[Main Traefik] (proxy-network) - SSL termination, domain routing
    ↓ 
[QaliTrack Traefik] (proxy-network + qalitrack) - Service routing
    ↓
[User Service] (/api/users, /api/auth)
[Other Services] (future services)
```

## Directory Structure

```
qalitrack/
├── README.md
├── .env.example               # Environment configuration template
├── .env                       # Environment configuration  
├── docker-compose.yml         # Single QaliTrack Traefik (handles both dev/prod)
├── start-dev.sh              # Start development environment
├── start-prod.sh             # Start production environment  
├── stop-dev.sh               # Stop development environment
├── stop-prod.sh              # Stop production environment
└── services/
    └── user-service/
        ├── dev/
        │   └── docker-compose.yml    # Dev User Service
        └── prod/
            └── docker-compose.yml    # Prod User Service
```

## Networks

- **proxy-network**: External network connecting to main Traefik
- **qalitrack-proxy**: Internal network for QaliTrack services (shared by dev/prod)

## Key Features

✅ **NO EXPOSED PORTS** - All routing handled by Traefik labels  
✅ **Single Traefik** handles both dev and prod routing  
✅ **Separate dev/prod services** in isolated directories  
✅ **Path-based routing** (/api/users, /api/auth)  
✅ **Dashboard at /traefik path**  
✅ **Environment-based domains** (configurable via .env files)  

## Quick Start

### Development
```bash
./start-dev.sh
```

Access at: http://qalitrack.localhost/api/users

### Production
```bash
./start-prod.sh
```

Access at: https://qalitrack.cseco.co.ke/api/users

### Stop Services
```bash
./stop-dev.sh    # Stop dev
./stop-prod.sh   # Stop prod
```

## Manual Commands

### Development
```bash
# Start QaliTrack Traefik (single instance)
docker compose up -d

# Start Dev User Service
cd services/user-service/dev && docker compose up -d
```

### Production
```bash
# Start QaliTrack Traefik (single instance)
docker compose up -d

# Start Prod User Service
cd services/user-service/prod && docker compose up -d
```

## Adding New Services

1. Create directories for dev and prod:
   ```bash
   mkdir -p services/your-service/dev
   mkdir -p services/your-service/prod
   ```

2. Add `docker-compose.yml` in each with:
   - Network: `qalitrack-proxy`
   - **NO host ports**
   - Traefik labels for routing

Example Traefik labels:
```yaml
# Development labels
labels:
  - "traefik.enable=true"
  - "traefik.http.routers.your-service-dev.rule=PathPrefix(`/api/your-service`)"
  - "traefik.http.routers.your-service-dev.entrypoints=qalitrack-http"
  - "traefik.http.services.your-service-dev.loadbalancer.server.port=80"

# Production labels  
labels:
  - "traefik.enable=true"
  - "traefik.http.routers.your-service-prod.rule=PathPrefix(`/api/your-service`)"
  - "traefik.http.routers.your-service-prod.entrypoints=qalitrack-https"
  - "traefik.http.services.your-service-prod.loadbalancer.server.port=80"
```

## Troubleshooting

### Check Service Status
```bash
docker compose ps                                          # QaliTrack Traefik status
cd services/user-service/dev && docker compose ps         # Dev user service status  
cd services/user-service/prod && docker compose ps        # Prod user service status
```

### View Logs
```bash
docker compose logs -f qalitrack-traefik                   # Traefik logs
cd services/user-service/dev && docker compose logs -f    # Dev service logs
cd services/user-service/prod && docker compose logs -f   # Prod service logs
```

### Access Points
- **Dev**: http://qalitrack.localhost/traefik
- **Prod**: https://qalitrack.cseco.co.ke/traefik

## Domain Configuration

Domains are configured via `.env` files in each environment:

### Development (`dev/.env`):
```bash
QALITRACK_DOMAIN=qalitrack.localhost
QALITRACK_ALT_DOMAIN=qalitrack-dev.localhost
ENVIRONMENT=development
```

### Production (`prod/.env`):
```bash
QALITRACK_DOMAIN=qalitrack.cseco.co.ke
ENVIRONMENT=production
```

### Custom Domain Setup:
1. Copy `.env.example` to `dev/.env` or `prod/.env`
2. Set your domain: `QALITRACK_DOMAIN=your.domain.com`
3. Restart services for changes to take effect