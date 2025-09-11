#!/bin/bash

# Simple SSL enablement script
DOMAIN=$1

if [ -z "$DOMAIN" ]; then
    echo "Usage: $0 <domain>"
    echo "Example: $0 dev.qalibrated.co.ke"
    exit 1
fi

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1"
}

CONFIG_DIR="/opt/qalitrack/qalitrackservices/apps/nginx-proxy/conf.d"
CONFIG_FILE="${CONFIG_DIR}/${DOMAIN}.conf"

if [ ! -f "$CONFIG_FILE" ]; then
    log "Error: Configuration file $CONFIG_FILE not found"
    exit 1
fi

# Check if SSL certificate exists
CERT_DIR="/opt/qalitrack/qalitrackservices/apps/nginx-proxy/ssl-certs/live/${DOMAIN}"
if [ ! -d "$CERT_DIR" ]; then
    log "Error: SSL certificate not found at $CERT_DIR"
    log "Run certificate request first: ./request-ssl.sh $DOMAIN"
    exit 1
fi

# Determine container names based on domain
case "$DOMAIN" in
    "qalibrated.co.ke"|"www.qalibrated.co.ke")
        FRONTEND_CONTAINER="qs-frontend-prod"
        BACKEND_CONTAINER="qs-backend-prod"
        ;;
    "dev.qalibrated.co.ke")
        FRONTEND_CONTAINER="qs-frontend-dev"
        BACKEND_CONTAINER="qs-backend-dev"
        ;;
    *)
        # Default naming pattern
        ENV_NAME=$(echo "$DOMAIN" | cut -d'.' -f1)
        FRONTEND_CONTAINER="qs-frontend-${ENV_NAME}"
        BACKEND_CONTAINER="qs-backend-${ENV_NAME}"
        ;;
esac

log "Updating configuration for $DOMAIN to use HTTPS..."

# Replace the existing config with HTTPS version
cat > "$CONFIG_FILE" << EOF
# $DOMAIN with SSL
server {
    listen 80;
    server_name $DOMAIN;

    # Let's Encrypt challenge location
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    # Redirect HTTP to HTTPS
    location / {
        return 301 https://\$server_name\$request_uri;
    }
}

server {
    listen 443 ssl http2;
    server_name $DOMAIN;

    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/$DOMAIN/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/$DOMAIN/privkey.pem;
    
    # Security Headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

    # Proxy to containers
    location / {
        proxy_pass http://$FRONTEND_CONTAINER;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
    }

    # API endpoint
    location /api {
        proxy_pass http://$BACKEND_CONTAINER:5000;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
    }
}
EOF

log "SSL enabled for $DOMAIN"
log "Restarting nginx..."

cd /opt/qalitrack/qalitrackservices/apps/nginx-proxy
docker compose restart nginx-proxy

log "Done! $DOMAIN now redirects HTTP to HTTPS"