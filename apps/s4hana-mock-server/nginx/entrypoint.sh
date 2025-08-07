#!/bin/bash
set -e

# Read domains from domains.txt
DOMAINS_FILE="/etc/nginx/domains.txt"
if [ ! -f "$DOMAINS_FILE" ]; then
  echo "Warning: domains.txt file not found at $DOMAINS_FILE. Creating default file."
  echo "qalibrated.cseco.co.ke" > "$DOMAINS_FILE"
fi

# Read domains
DOMAINS=$(cat "$DOMAINS_FILE")
PRIMARY_DOMAIN=$(echo "$DOMAINS" | awk '{print $1}')

# Check if SSL certificates exist and include all domains
CERT_PATH="/etc/letsencrypt/live/$PRIMARY_DOMAIN/fullchain.pem"
NEED_CERT_UPDATE=false

if [ ! -f "$CERT_PATH" ]; then
  echo "SSL certificates not found. Setting up Let's Encrypt certificates..."
  NEED_CERT_UPDATE=true
else
  echo "SSL certificates found. Checking if all domains are included..."
  
  # Check if certificate includes all domains from domains.txt
  for domain in $DOMAINS; do
    if ! openssl x509 -in "$CERT_PATH" -text -noout | grep -q "DNS:$domain"; then
      echo "Domain $domain not found in certificate. Certificate update needed."
      NEED_CERT_UPDATE=true
      break
    fi
  done
  
  if [ "$NEED_CERT_UPDATE" = false ]; then
    echo "All domains are included in existing certificate."
  fi
fi

if [ "$NEED_CERT_UPDATE" = true ]; then
  echo "Updating SSL certificates to include all domains..."
  
  # Install certbot if not already installed
  if ! command -v certbot &> /dev/null; then
    apt-get update
    apt-get install -y certbot python3-certbot-nginx
  fi
  
  # Format domain parameters for certbot
  DOMAIN_PARAMS=""
  for domain in $DOMAINS; do
    DOMAIN_PARAMS="$DOMAIN_PARAMS -d $domain"
  done
  
  # Get/update certificates
  certbot --nginx --agree-tos --non-interactive --email admin@$PRIMARY_DOMAIN $DOMAIN_PARAMS --expand
  
  echo "SSL certificates successfully obtained/updated."
fi

# Generate nginx configuration for each domain
echo "Generating NGINX configuration for all domains..."

mkdir -p /etc/nginx/templates

# Main configuration template
cat > /etc/nginx/templates/default.conf.template << EOF
# S/4HANA Mock Server Configuration

EOF

# Generate server blocks for each domain
for domain in $DOMAINS; do
  echo "Generating configuration for domain: $domain"
  
  # S/4HANA Mock Server block
  cat >> /etc/nginx/templates/default.conf.template << EOF
# S/4HANA Mock Server - $domain
server {
    listen 443 ssl;
    server_name $domain;
    
    ssl_certificate /etc/letsencrypt/live/${PRIMARY_DOMAIN}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${PRIMARY_DOMAIN}/privkey.pem;
    
    # SSL optimization
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    
    # Security headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Content-Type-Options nosniff always;
    add_header X-Frame-Options DENY always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    
    # Health check endpoint
    location /health {
        proxy_pass http://s4hana-mock-server:5001/health;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        access_log off;
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma no-cache;
        add_header Expires 0;
    }
    
    # API endpoints
    location /api/ {
        proxy_pass http://s4hana-mock-server:5001/api/;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        # Enable CORS for API endpoints
        add_header Access-Control-Allow-Origin "*" always;
        add_header Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS" always;
        add_header Access-Control-Allow-Headers "DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization" always;
        
        # Handle preflight requests
        if (\$request_method = 'OPTIONS') {
            add_header Access-Control-Allow-Origin "*";
            add_header Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS";
            add_header Access-Control-Allow-Headers "DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization";
            add_header Access-Control-Max-Age 1728000;
            add_header Content-Type 'text/plain; charset=utf-8';
            add_header Content-Length 0;
            return 204;
        }
    }
    
    # API documentation endpoints
    location /api-docs {
        proxy_pass http://s4hana-mock-server:5001/api-docs;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
    
    location /redoc {
        proxy_pass http://s4hana-mock-server:5001/redoc;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
    
    location /swagger.json {
        proxy_pass http://s4hana-mock-server:5001/swagger.json;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        add_header Content-Type application/json;
        add_header Cache-Control "public, max-age=300";
    }
    
    # Static files (favicon, etc.)
    location ~* \.(ico|css|js|gif|jpe?g|png|svg|woff|woff2|ttf|eot)\$ {
        proxy_pass http://s4hana-mock-server:5001;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        expires 1y;
        add_header Cache-Control "public, immutable";
        add_header Access-Control-Allow-Origin "*";
    }
    
    # Main application
    location / {
        proxy_pass http://s4hana-mock-server:5001/;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        # Buffer settings for large requests (Swagger UI)
        proxy_buffering on;
        proxy_buffer_size 4k;
        proxy_buffers 8 4k;
        proxy_busy_buffers_size 8k;
        
        # Timeout settings
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
    
    # Block access to hidden files and directories
    location ~ /\. {
        deny all;
        access_log off;
        log_not_found off;
    }
    
    # Custom error pages
    error_page 404 /404.html;
    error_page 500 502 503 504 /50x.html;
    
    location = /404.html {
        return 404 "S/4HANA Mock Server - Page Not Found";
        add_header Content-Type text/plain;
    }
    
    location = /50x.html {
        return 500 "S/4HANA Mock Server - Internal Server Error";
        add_header Content-Type text/plain;
    }
}

EOF
done

echo "NGINX configuration generated successfully."

# Copy template to conf.d
envsubst < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf

echo "NGINX configuration setup complete. Starting nginx..."
exec nginx -g "daemon off;"