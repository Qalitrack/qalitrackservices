#!/bin/bash
set -e

echo "Starting nginx proxy with smart SSL detection and auto-certificate requests..."

# Create active config directory
mkdir -p /etc/nginx/conf.d

# Function to check if SSL certificate exists for a domain
has_ssl_cert() {
    local domain=$1
    [ -f "/etc/letsencrypt/live/$domain/fullchain.pem" ]
}

# Function to request SSL certificate
request_ssl_cert() {
    local domain=$1
    echo "🔐 Requesting SSL certificate for $domain..."
    
    certbot certonly \
        --webroot \
        --webroot-path=/var/www/certbot \
        --email admin@qalibrated.co.ke \
        --agree-tos \
        --no-eff-email \
        --staging \
        -d "$domain" \
        --non-interactive \
        || {
            echo "⚠ Failed to get SSL certificate for $domain - continuing with HTTP"
            return 1
        }
    
    echo "✓ SSL certificate obtained for $domain"
    return 0
}

# Function to setup domain configuration
setup_domain_config() {
    local domain=$1
    local ssl_template="/templates/enabled/ssl/${domain}.conf"
    local no_ssl_template="/templates/enabled/no-ssl/${domain}.conf" 
    local target="/etc/nginx/conf.d/${domain}.conf"
    
    # Check if SSL template exists (indicates we want SSL for this domain)
    if [ -f "$ssl_template" ]; then
        echo "📋 SSL template exists for $domain"
        
        if has_ssl_cert "$domain"; then
            echo "✓ SSL certificate found for $domain - using HTTPS config"
            cp "$ssl_template" "$target"
        else
            echo "○ No SSL certificate for $domain - using HTTP config temporarily"
            if [ -f "$no_ssl_template" ]; then
                cp "$no_ssl_template" "$target"
                
                # Request certificate for this domain
                if request_ssl_cert "$domain"; then
                    echo "✓ Certificate obtained, switching to HTTPS config"
                    cp "$ssl_template" "$target"
                fi
            else
                echo "⚠ No HTTP template found for $domain"
            fi
        fi
    elif [ -f "$no_ssl_template" ]; then
        # Only HTTP template exists - domain doesn't need SSL
        echo "○ HTTP-only domain: $domain"
        cp "$no_ssl_template" "$target"
    else
        echo "⚠ No templates found for $domain"
    fi
}

# Process all domain templates
echo "Processing domain configurations..."

# Find all domains from enabled SSL templates (these are domains we want SSL for)
for template in /templates/enabled/ssl/*.conf; do
    if [ -f "$template" ]; then
        domain=$(basename "$template" .conf)
        setup_domain_config "$domain"
    fi
done

# Also process HTTP-only domains (that don't have SSL templates)
for template in /templates/enabled/no-ssl/*.conf; do
    if [ -f "$template" ]; then
        domain=$(basename "$template" .conf)
        ssl_template="/templates/enabled/ssl/${domain}.conf"
        
        # Skip if we already processed this domain (has SSL template)
        if [ ! -f "$ssl_template" ]; then
            setup_domain_config "$domain"
        fi
    fi
done

# Test nginx configuration
echo "Testing nginx configuration..."
if nginx -t; then
    echo "✓ Nginx configuration is valid"
else
    echo "✗ Nginx configuration error!"
    exit 1
fi

echo "Configuration complete. Starting nginx..."

# Start nginx
exec nginx -g "daemon off;"