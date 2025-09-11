#!/bin/bash

# SSL Certificate Request Script
set -e

DOMAIN=$1
EMAIL=${2:-"admin@qalibrated.co.ke"}
STAGING=${3:-false}

if [ -z "$DOMAIN" ]; then
    echo "Usage: $0 <domain> [email] [staging]"
    echo "Example: $0 dev.qalibrated.co.ke admin@qalibrated.co.ke"
    echo "Example: $0 staging.qalibrated.co.ke admin@qalibrated.co.ke staging"
    exit 1
fi

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1"
}

cd /opt/qalitrack/qalitrackservices/apps/nginx-proxy

# Ensure nginx proxy is running for HTTP challenge
log "Starting nginx proxy..."
docker compose up -d nginx-proxy

# Wait for nginx to be ready
sleep 5

# Request certificate
log "Requesting SSL certificate for $DOMAIN..."

STAGING_FLAG=""
if [ "$STAGING" = "staging" ]; then
    STAGING_FLAG="--staging"
    log "Using Let's Encrypt staging environment"
fi

docker compose run --rm certbot \
    certonly \
    --webroot \
    --webroot-path=/var/www/certbot \
    --email $EMAIL \
    --agree-tos \
    --no-eff-email \
    $STAGING_FLAG \
    -d $DOMAIN

if [ $? -eq 0 ]; then
    log "Certificate for $DOMAIN obtained successfully"
    log "Certificate stored in: ./ssl-certs/live/$DOMAIN/"
    
    log "To enable HTTPS, run: ./enable-ssl.sh $DOMAIN"
else
    log "Failed to obtain certificate for $DOMAIN"
    exit 1
fi