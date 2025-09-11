# Nginx Proxy Setup

Multi-domain nginx proxy with Docker network integration and simple SSL management.

## ✅ Simplified Architecture

```
Internet → nginx-proxy (Docker network) → App Containers
         (Port 80/443)                   (No port exposure needed!)
```

**Key Improvements:**
- ✅ **Docker Networks**: No host networking needed
- ✅ **Container Discovery**: Access apps by container name 
- ✅ **Simple SSL**: Just mount directories, no complex scripts
- ✅ **No Port Exposure**: Apps don't need host ports

## Usage

### 1. Start Nginx Proxy

```bash
cd apps/nginx-proxy
docker compose up -d
```

Starts with HTTP-only configuration by default.

### 2. Request SSL Certificate

```bash
# Request certificate for domain
./scripts/request-ssl.sh dev.qalibrated.co.ke

# Enable HTTPS (updates config and restarts nginx)
./scripts/enable-ssl.sh dev.qalibrated.co.ke
```

### 3. Add New Domains

```bash
# Create new domain config (manual for now)
cp conf.d/dev.qalibrated.co.ke.conf conf.d/staging.qalibrated.co.ke.conf

# Edit the file to use correct container names
# Then request SSL as above
```

## Directory Structure

```
apps/nginx-proxy/
├── docker-compose.yml          # Simple nginx + certbot
├── nginx.conf                  # Base configuration
├── conf.d/                     # Domain configurations
│   ├── qalibrated.co.ke.conf   # Production
│   └── dev.qalibrated.co.ke.conf # Development
├── ssl-certs/                  # SSL certificates (mounted)
├── certbot-webroot/           # Challenge files
├── scripts/
│   ├── request-ssl.sh         # Get SSL certificate
│   └── enable-ssl.sh          # Enable HTTPS
└── logs/                      # Nginx logs
```

## Container Mapping

| Domain | Frontend Container | Backend Container |
|--------|--------------------|-------------------|
| qalibrated.co.ke | qs-frontend-prod | qs-backend-prod |
| dev.qalibrated.co.ke | qs-frontend-dev | qs-backend-dev |

## SSL Workflow

1. **HTTP First**: Domain works over HTTP immediately
2. **Request Certificate**: `./scripts/request-ssl.sh domain.com`
3. **Enable HTTPS**: `./scripts/enable-ssl.sh domain.com`
4. **Automatic Redirect**: HTTP → HTTPS redirect added

## Why This Is Better

**Before (Complex):**
- Host networking required
- Complex entrypoint scripts
- Difficult certificate management
- Port exposure needed

**Now (Simple):**
- Docker networks only
- Simple volume mounting
- Two-step SSL process
- Container-to-container communication

## SSL Certificate Management

**Simple Directory Mounting:**
- `./ssl-certs` → `/etc/letsencrypt` (where certificates are stored)
- `./certbot-webroot` → `/var/www/certbot` (for challenges)

**No complex entrypoints needed!** Just mount the directories and certificates work.

## Troubleshooting

```bash
# Check nginx
docker compose logs nginx-proxy

# Check certificates
docker compose run --rm certbot certificates

# Restart nginx
docker compose restart nginx-proxy
```