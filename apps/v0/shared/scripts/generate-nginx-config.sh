#!/bin/bash

# Nginx Configuration Generator for QaliTrack
# Generates environment-specific nginx configurations with SSL support

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$(dirname "$SCRIPT_DIR")")"

# Colors
RED='\033[31m'
GREEN='\033[32m'
YELLOW='\033[33m'
BLUE='\033[34m'
RESET='\033[0m'

usage() {
    echo -e "${BLUE}🌐 Nginx Configuration Generator${RESET}"
    echo ""
    echo "Usage: $0 <environment>"
    echo ""
    echo "Environments: development, production"
    echo ""
    echo "Examples:"
    echo "  $0 development    # Generate HTTP-only nginx config"
    echo "  $0 production     # Generate HTTPS nginx config with SSL"
}

generate_nginx_config() {
    local env=$1
    local env_dir="$ROOT_DIR/environments/$env"
    local nginx_dir="$env_dir/nginx-configs"
    
    if [ ! -f "$env_dir/.env" ]; then
        echo -e "${RED}❌ .env file not found: $env_dir/.env${RESET}"
        echo -e "${YELLOW}Run 'make generate' first to create environment configuration${RESET}"
        exit 1
    fi
    
    # Load environment variables
    source "$env_dir/.env"
    
    echo -e "${BLUE}🌐 Generating nginx configuration for $env environment...${RESET}"
    
    # Determine SSL settings based on environment
    if [ "$env" = "production" ] && [ "${SSL_ENABLED}" = "true" ]; then
        generate_production_ssl_config "$nginx_dir"
    else
        generate_development_config "$nginx_dir"
    fi
    
    echo -e "${GREEN}✅ Nginx configuration generated successfully!${RESET}"
}

generate_development_config() {
    local nginx_dir=$1
    local primary_domain=${DOMAIN_NAME:-localhost}
    
    echo -e "  ${GREEN}✓${RESET} Generating HTTP-only configuration for development"
    
    # Development-specific blocks
    local ssl_redirect_block="# server {
#     listen 80 default_server;
#     listen [::]:80 default_server;
#     server_name _;
    
#     location / {
#         return 301 https://\$host\$request_uri;
#     }
# }"
    
    local ssl_listen_directive="80"
    local ssl_certificate_block=""
    local ssl_optimization_block=""
    local www_redirect_block=""
    local admin_server_block=""
    local media_server_block=""
    
    # Generate admin server block for development
    if [ -n "${ADMIN_DOMAIN}" ]; then
        admin_server_block="
# Admin interface server block (development)
server {
    listen 80;
    server_name ${ADMIN_DOMAIN};
    
    location / {
        proxy_pass http://${COMPOSE_PROJECT_NAME}-qalitrack-admin:4000;
        include /etc/nginx/conf.d/proxy_common.conf;
    }
}"
    fi
    
    generate_final_config "$nginx_dir" "$ssl_redirect_block" "$ssl_listen_directive" "$ssl_certificate_block" "$ssl_optimization_block" "$www_redirect_block" "$admin_server_block" "$media_server_block"
}

generate_production_ssl_config() {
    local nginx_dir=$1
    
    echo -e "  ${GREEN}✓${RESET} Generating HTTPS configuration with SSL for production"
    
    # Read domains from SSL_DOMAINS or domains.txt
    local domains=""
    if [ -n "$SSL_DOMAINS" ]; then
        domains="$SSL_DOMAINS"
    elif [ -f "$nginx_dir/domains.txt" ]; then
        domains=$(cat "$nginx_dir/domains.txt" | tr '\n' ' ' | xargs)
    else
        echo -e "${YELLOW}⚠️${RESET} No domains configured, using default domain"
        domains="$DOMAIN_NAME"
    fi
    
    # Get primary domain (first in list)
    local primary_domain=$(echo $domains | awk '{print $1}')
    
    # Production SSL blocks
    local ssl_redirect_block="# Default HTTP to HTTPS redirect
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name _;
    
    location / {
        return 301 https://\$host\$request_uri;
    }
}"
    
    local ssl_listen_directive="443 ssl"
    local ssl_certificate_block="ssl_certificate /etc/letsencrypt/live/${primary_domain}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${primary_domain}/privkey.pem;"
    
    local ssl_optimization_block="# SSL optimization
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;"
    
    # Generate www redirect if www domain exists
    local www_redirect_block=""
    if echo "$domains" | grep -q "www\."; then
        www_redirect_block="
# Redirect www to non-www
server {
    listen 443 ssl;
    server_name www.${primary_domain};
    
    ssl_certificate /etc/letsencrypt/live/${primary_domain}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${primary_domain}/privkey.pem;
    
    return 301 https://${primary_domain}\$request_uri;
}"
    fi
    
    # Generate admin server block
    local admin_server_block=""
    if [ -n "${ADMIN_DOMAIN}" ]; then
        admin_server_block="
# Admin interface server block
server {
    listen 443 ssl;
    server_name ${ADMIN_DOMAIN};
    
    ssl_certificate /etc/letsencrypt/live/${primary_domain}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${primary_domain}/privkey.pem;
    
    # SSL optimization
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    
    location / {
        proxy_pass http://${COMPOSE_PROJECT_NAME}-qalitrack-admin:4000;
        include /etc/nginx/conf.d/proxy_common.conf;
    }
}"
    fi
    
    # Generate media server block
    local media_server_block=""
    if [ -n "${MEDIA_DOMAIN}" ]; then
        media_server_block="
# Media server block
server {
    listen 443 ssl;
    server_name ${MEDIA_DOMAIN};
    
    ssl_certificate /etc/letsencrypt/live/${primary_domain}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${primary_domain}/privkey.pem;
    
    # SSL optimization
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    
    # Root directory for media files
    root /usr/share/nginx/html;
    
    # Media files with caching
    location ~* ^/media/.+\.(jpg|jpeg|png|gif|webp|svg|ico|mp3|wav|pdf)\$ {
        expires 1y;
        add_header Cache-Control \"public, immutable\";
        add_header Access-Control-Allow-Origin \"*\" always;
    }
    
    location / {
        return 404 \"Media file not found\";
    }
}"
    fi
    
    generate_final_config "$nginx_dir" "$ssl_redirect_block" "$ssl_listen_directive" "$ssl_certificate_block" "$ssl_optimization_block" "$www_redirect_block" "$admin_server_block" "$media_server_block"
}

generate_final_config() {
    local nginx_dir=$1
    local ssl_redirect_block=$2
    local ssl_listen_directive=$3
    local ssl_certificate_block=$4
    local ssl_optimization_block=$5
    local www_redirect_block=$6
    local admin_server_block=$7
    local media_server_block=$8
    
    # Create conf.d directory
    mkdir -p "$nginx_dir/conf.d"
    
    # Generate the final configuration
    cat > "$nginx_dir/conf.d/default.conf" << EOF
# QaliTrack Nginx Configuration
# Auto-generated for $ENVIRONMENT environment

$ssl_redirect_block

# Main API Gateway server block
server {
    listen $ssl_listen_directive;
    server_name $API_DOMAIN;
    
    $ssl_certificate_block
    
    $ssl_optimization_block
    
    # Rate limiting for API
    limit_req zone=api burst=20 nodelay;
    
    # Security headers
    add_header X-Frame-Options "DENY" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    
    # Health check endpoint
    location /health {
        access_log off;
        return 200 "QaliTrack API Gateway healthy\n";
        add_header Content-Type text/plain;
    }
    
    # API Gateway routing (main route)
    location /api/ {
        # Remove /api prefix before forwarding to gateway
        rewrite ^/api/(.*)$ /\$1 break;
        
        proxy_pass http://$COMPOSE_PROJECT_NAME-api-gateway:$API_GATEWAY_PORT;
        include /etc/nginx/conf.d/proxy_common.conf;
        
        # API-specific timeouts
        proxy_connect_timeout 5s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
    
    # Default route to API Gateway
    location / {
        proxy_pass http://$COMPOSE_PROJECT_NAME-api-gateway:$API_GATEWAY_PORT;
        include /etc/nginx/conf.d/proxy_common.conf;
    }
}

# Main application/admin server block
server {
    listen $ssl_listen_directive;
    server_name $DOMAIN_NAME;
    
    $ssl_certificate_block
    
    $ssl_optimization_block
    
    # Security headers for main app
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    
    location / {
        # Route to future frontend application
        proxy_pass http://$COMPOSE_PROJECT_NAME-qalitrack-frontend:3000;
        include /etc/nginx/conf.d/proxy_common.conf;
    }
}

# Status/Monitoring server block (Microservices Control Hub)
server {
    listen $ssl_listen_directive;
    server_name $MICROSERVICES_HUB_HOST;
    
    $ssl_certificate_block
    
    $ssl_optimization_block
    
    # Rate limiting for status dashboard
    limit_req zone=status burst=10 nodelay;
    
    # Security headers for monitoring dashboard
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    
    # Health check for status service itself
    location /health {
        access_log off;
        return 200 "QaliTrack Status Dashboard healthy\n";
        add_header Content-Type text/plain;
    }
    
    # Microservices Control Hub
    location / {
        proxy_pass http://$COMPOSE_PROJECT_NAME-microservices-control-hub:3000;
        include /etc/nginx/conf.d/proxy_common.conf;
        
        # Monitoring-specific settings
        proxy_connect_timeout 3s;
        proxy_send_timeout 30s;
        proxy_read_timeout 30s;
    }
}

$www_redirect_block

$admin_server_block

$media_server_block
EOF

    echo -e "  ${GREEN}✓${RESET} Generated $nginx_dir/conf.d/default.conf"
}

# Main script
if [ $# -ne 1 ]; then
    usage
    exit 1
fi

ENVIRONMENT=$1

case $ENVIRONMENT in
    development|production)
        generate_nginx_config "$ENVIRONMENT"
        ;;
    *)
        echo -e "${RED}❌ Invalid environment: $ENVIRONMENT${RESET}"
        usage
        exit 1
        ;;
esac