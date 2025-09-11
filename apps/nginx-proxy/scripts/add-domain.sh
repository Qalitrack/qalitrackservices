#!/bin/bash

# Add new domain configuration script
set -e

DOMAIN=$1
PORT_FRONTEND=$2
PORT_BACKEND=$3

if [ -z "$DOMAIN" ] || [ -z "$PORT_FRONTEND" ] || [ -z "$PORT_BACKEND" ]; then
    echo "Usage: $0 <domain> <frontend-port> <backend-port>"
    echo "Example: $0 staging.qalibrated.co.ke 3003 5003"
    exit 1
fi

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1"
}

NGINX_DIR="/opt/qalitrack/qalitrackservices/apps/nginx-proxy"

# Create HTTP-only config
log "Creating HTTP-only configuration for $DOMAIN..."
cat > "$NGINX_DIR/conf.d/http-only/$DOMAIN.conf" << EOF
# $DOMAIN - HTTP only (for certificate generation)
server {
    listen 80;
    server_name $DOMAIN;

    # Let's Encrypt challenge location
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    # Proxy all other requests to backend
    location / {
        proxy_pass http://host.docker.internal:$PORT_FRONTEND;
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
        proxy_pass http://host.docker.internal:$PORT_BACKEND;
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

# Create SSL config
log "Creating SSL configuration for $DOMAIN..."
cat > "$NGINX_DIR/conf.d/ssl/$DOMAIN.conf" << EOF
# $DOMAIN - HTTPS with SSL
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

    # Proxy to container
    location / {
        proxy_pass http://host.docker.internal:$PORT_FRONTEND;
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
        proxy_pass http://host.docker.internal:$PORT_BACKEND;
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

log "Domain configuration created for $DOMAIN"
log "Frontend: localhost:$PORT_FRONTEND"
log "Backend: localhost:$PORT_BACKEND"
log ""
log "Next steps:"
log "1. Start your application on ports $PORT_FRONTEND and $PORT_BACKEND"
log "2. Run: ./request-ssl.sh $DOMAIN"