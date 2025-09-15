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

# Function to check if upstream service is available
check_upstream_health() {
    local upstream=$1
    local port=${2:-80}
    
    echo "🔍 Checking upstream health: $upstream:$port"
    
    # Try to connect to the upstream service using nc (netcat)
    if timeout 5 nc -z "$upstream" "$port" 2>/dev/null; then
        echo "✅ Upstream $upstream:$port is healthy"
        return 0
    else
        # Fallback: check if host is reachable with ping
        if timeout 3 ping -c 1 "$upstream" >/dev/null 2>&1; then
            echo "⚠️ Upstream $upstream is reachable but port $port is not responding"
        else
            echo "❌ Upstream $upstream is not reachable at all"
        fi
        return 1
    fi
}

# Function to extract upstream services from nginx config
get_upstreams_from_config() {
    local config_file=$1
    
    # Extract proxy_pass lines and get hostnames/ports
    grep -E "proxy_pass.*http://" "$config_file" | \
    sed -E 's/.*proxy_pass.*http:\/\/([^;\/]+).*/\1/' | \
    while read -r upstream; do
        if [ -n "$upstream" ]; then
            echo "$upstream"
        fi
    done
}

# Function to test basic HTTP connectivity to proxy targets
test_proxy_targets() {
    local domain=$1
    local ssl_template="/templates/enabled/ssl/${domain}.conf"
    local no_ssl_template="/templates/enabled/no-ssl/${domain}.conf" 
    
    echo "🌐 Testing proxy target connectivity for $domain..."
    
    # Determine which template to test
    local template_to_test=""
    if [ -f "$ssl_template" ]; then
        template_to_test="$ssl_template"
    elif [ -f "$no_ssl_template" ]; then
        template_to_test="$no_ssl_template"
    else
        echo "❌ No template found for $domain"
        return 1
    fi
    
    # Test each proxy_pass target
    local all_targets_healthy=true
    get_upstreams_from_config "$template_to_test" | while IFS=':' read -r upstream_host upstream_port; do
        if [ -z "$upstream_port" ]; then
            if echo "$upstream_host" | grep -q "backend"; then
                upstream_port=5000
            else
                upstream_port=80
            fi
        fi
        
        echo "🎯 Testing proxy target: $upstream_host:$upstream_port"
        if ping -c 1 "$upstream_host" >/dev/null 2>&1; then
            echo "✅ Proxy target $upstream_host is reachable"
        else
            echo "❌ Proxy target $upstream_host is not reachable"
            all_targets_healthy=false
        fi
    done
    
    if [ "$all_targets_healthy" = true ]; then
        echo "✅ All proxy targets for $domain are reachable"
        return 0
    else
        echo "❌ Some proxy targets for $domain are not reachable"
        return 1
    fi
}

# Function to test if ACME challenge path is accessible
test_acme_challenge() {
    local domain=$1
    local test_file="test-challenge-$(date +%s)"
    local challenge_path="/var/www/certbot/.well-known/acme-challenge"
    local test_url="http://$domain/.well-known/acme-challenge/$test_file"
    
    echo "🧪 Testing ACME challenge accessibility for $domain..."
    
    # Create test challenge file
    mkdir -p "$challenge_path"
    echo "test-challenge-content-$(date)" > "$challenge_path/$test_file"
    
    # Test if we can access it externally
    sleep 2  # Give nginx time to pick up the file
    
    if curl -sf --max-time 10 "$test_url" > /dev/null; then
        echo "✅ ACME challenge path is accessible for $domain"
        rm -f "$challenge_path/$test_file"
        return 0
    else
        echo "❌ ACME challenge path is NOT accessible for $domain"
        echo "   URL tested: $test_url"
        echo "   This means Let's Encrypt won't be able to verify domain ownership"
        echo "   Check nginx .well-known location configuration"
        rm -f "$challenge_path/$test_file"
        return 1
    fi
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
        -d "$domain" \
        --non-interactive \
        || {
            echo "⚠ Failed to get SSL certificate for $domain - continuing with HTTP"
            return 1
        }
    
    echo "✓ SSL certificate obtained for $domain"
    return 0
}

# Function to request SSL certificates for domains that need them but don't have them
request_missing_ssl_certificates() {
    echo "🔐 Checking for missing SSL certificates..."
    
    # Find all domains that have SSL templates (want SSL) but don't have certificates
    for template in /templates/enabled/ssl/*.conf; do
        if [ -f "$template" ]; then
            domain=$(basename "$template" .conf)
            
            if ! has_ssl_cert "$domain"; then
                echo "📋 Domain $domain needs SSL certificate - requesting..."
                if request_ssl_cert "$domain"; then
                    echo "✅ SSL certificate obtained for $domain"
                else
                    echo "❌ SSL certificate request failed for $domain - will use HTTP"
                fi
            else
                echo "✓ Domain $domain already has SSL certificate"
            fi
        fi
    done
}

# Function to setup domain configuration based on available certificates and upstream health
setup_domain_config() {
    local domain=$1
    local ssl_template="/templates/enabled/ssl/${domain}.conf"
    local no_ssl_template="/templates/enabled/no-ssl/${domain}.conf" 
    local target="/etc/nginx/conf.d/${domain}.conf"
    
    # Determine which template to use
    local selected_template=""
    if [ -f "$ssl_template" ]; then
        echo "📋 SSL template exists for $domain"
        
        if has_ssl_cert "$domain"; then
            echo "✓ SSL certificate found for $domain - using HTTPS config"
            selected_template="$ssl_template"
        else
            echo "○ No SSL certificate for $domain - using HTTP config"
            if [ -f "$no_ssl_template" ]; then
                selected_template="$no_ssl_template"
            else
                echo "⚠ No HTTP template found for $domain"
                return 1
            fi
        fi
    elif [ -f "$no_ssl_template" ]; then
        # Only HTTP template exists - domain doesn't need SSL
        echo "○ HTTP-only domain: $domain"
        selected_template="$no_ssl_template"
    else
        echo "⚠ No templates found for $domain"
        return 1
    fi
    
    # Check upstream health before enabling configuration
    echo "🏥 Checking upstream services for $domain..."
    local all_healthy=true
    
    # Get upstream services from the selected template
    get_upstreams_from_config "$selected_template" | while IFS=':' read -r upstream_host upstream_port; do
        # Default port based on service type
        if [ -z "$upstream_port" ]; then
            if echo "$upstream_host" | grep -q "backend"; then
                upstream_port=5000
            else
                upstream_port=80
            fi
        fi
        
        if ! check_upstream_health "$upstream_host" "$upstream_port"; then
            all_healthy=false
            echo "💔 Upstream $upstream_host:$upstream_port is unhealthy"
        fi
    done
    
    # Use a subshell to capture the health check results
    local health_check_failed=false
    get_upstreams_from_config "$selected_template" | while IFS=':' read -r upstream_host upstream_port; do
        if [ -z "$upstream_port" ]; then
            if echo "$upstream_host" | grep -q "backend"; then
                upstream_port=5000
            else
                upstream_port=80
            fi
        fi
        
        if ! check_upstream_health "$upstream_host" "$upstream_port"; then
            exit 1
        fi
    done
    
    if [ $? -eq 0 ]; then
        echo "✅ All upstream services healthy for $domain - enabling configuration"
        cp "$selected_template" "$target"
    else
        echo "🚫 Some upstream services unhealthy for $domain - skipping configuration"
        echo "# Configuration disabled due to unhealthy upstream services" > "$target"
        echo "# Domain: $domain" >> "$target"
        echo "# This file was generated automatically on $(date)" >> "$target"
        return 1
    fi
}

# PHASE 1: Setup all domains with HTTP-only configurations first
echo "🔧 Phase 1: Setting up HTTP-only configurations for all domains..."

# Initialize counters
enabled_domains=0
disabled_domains=0
enabled_list=""
disabled_list=""

# Function to setup HTTP-only config for a domain
setup_http_only_config() {
    local domain=$1
    local no_ssl_template="/templates/enabled/no-ssl/${domain}.conf" 
    local target="/etc/nginx/conf.d/${domain}.conf"
    
    if [ -f "$no_ssl_template" ]; then
        echo "🌐 Setting up HTTP-only config for $domain..."
        
        # Check upstream health before enabling configuration
        local all_healthy=true
        get_upstreams_from_config "$no_ssl_template" | while IFS=':' read -r upstream_host upstream_port; do
            if [ -z "$upstream_port" ]; then
                if echo "$upstream_host" | grep -q "backend"; then
                    upstream_port=5000
                else
                    upstream_port=80
                fi
            fi
            
            if ! check_upstream_health "$upstream_host" "$upstream_port"; then
                exit 1
            fi
        done
        
        if [ $? -eq 0 ]; then
            echo "✅ Upstream services healthy for $domain - enabling HTTP config"
            cp "$no_ssl_template" "$target"
            return 0
        else
            echo "🚫 Upstream services unhealthy for $domain - skipping"
            return 1
        fi
    else
        echo "⚠ No HTTP template found for $domain"
        return 1
    fi
}

# Setup HTTP configs for all domains that want SSL
for template in /templates/enabled/ssl/*.conf; do
    if [ -f "$template" ]; then
        domain=$(basename "$template" .conf)
        if setup_http_only_config "$domain"; then
            enabled_domains=$((enabled_domains + 1))
            enabled_list="$enabled_list $domain"
        else
            disabled_domains=$((disabled_domains + 1))
            disabled_list="$disabled_list $domain"
        fi
    fi
done

# Also process HTTP-only domains (that don't have SSL templates)
for template in /templates/enabled/no-ssl/*.conf; do
    if [ -f "$template" ]; then
        domain=$(basename "$template" .conf)
        ssl_template="/templates/enabled/ssl/${domain}.conf"
        
        # Skip if we already processed this domain (has SSL template)
        if [ ! -f "$ssl_template" ]; then
            if setup_http_only_config "$domain"; then
                enabled_domains=$((enabled_domains + 1))
                enabled_list="$enabled_list $domain"
            else
                disabled_domains=$((disabled_domains + 1))
                disabled_list="$disabled_list $domain"
            fi
        fi
    fi
done

echo ""
echo "📊 Phase 1 Summary - HTTP-only configs:"
echo "✅ Enabled domains ($enabled_domains):$enabled_list"
if [ $disabled_domains -gt 0 ]; then
    echo "🚫 Disabled domains ($disabled_domains):$disabled_list"
fi
echo ""

# Test nginx configuration
echo "Testing nginx HTTP-only configuration..."
if nginx -t; then
    echo "✓ Nginx HTTP-only configuration is valid"
else
    echo "✗ Nginx configuration error!"
    exit 1
fi

# PHASE 2: Start nginx with HTTP-only configs
echo "🚀 Phase 2: Starting nginx with HTTP-only configurations..."
nginx -g "daemon off;" &
NGINX_PID=$!

# Wait for nginx to start
sleep 3

# PHASE 3: Test ACME challenges and request certificates (now that nginx is running)
echo "🔐 Phase 3: Testing ACME challenges and requesting certificates..."

# Test ACME and request certificates for domains that need them
for template in /templates/enabled/ssl/*.conf; do
    if [ -f "$template" ]; then
        domain=$(basename "$template" .conf)
        
        if ! has_ssl_cert "$domain"; then
            echo "📋 Domain $domain needs SSL certificate - testing ACME accessibility..."
            if test_acme_challenge "$domain"; then
                echo "🚀 ACME test passed for $domain. Requesting certificate..."
                if request_ssl_cert "$domain"; then
                    echo "✅ SSL certificate obtained for $domain"
                else
                    echo "❌ SSL certificate request failed for $domain"
                fi
            else
                echo "⚠ ACME test failed for $domain - skipping certificate request"
            fi
        else
            echo "✓ Domain $domain already has SSL certificate"
        fi
    fi
done

# PHASE 4: Stop nginx and reconfigure with proper SSL/HTTP configs
echo "🔄 Phase 4: Stopping nginx to reconfigure with SSL certificates..."
kill $NGINX_PID
wait $NGINX_PID 2>/dev/null

# Now setup final configurations (SSL where available, HTTP as fallback)
echo "🔧 Setting up final configurations with SSL certificates..."

enabled_domains=0
disabled_domains=0
enabled_list=""
disabled_list=""

for template in /templates/enabled/ssl/*.conf; do
    if [ -f "$template" ]; then
        domain=$(basename "$template" .conf)
        if setup_domain_config "$domain"; then
            enabled_domains=$((enabled_domains + 1))
            enabled_list="$enabled_list $domain"
        else
            disabled_domains=$((disabled_domains + 1))
            disabled_list="$disabled_list $domain"
        fi
    fi
done

# Also process HTTP-only domains
for template in /templates/enabled/no-ssl/*.conf; do
    if [ -f "$template" ]; then
        domain=$(basename "$template" .conf)
        ssl_template="/templates/enabled/ssl/${domain}.conf"
        
        if [ ! -f "$ssl_template" ]; then
            if setup_domain_config "$domain"; then
                enabled_domains=$((enabled_domains + 1))
                enabled_list="$enabled_list $domain"
            else
                disabled_domains=$((disabled_domains + 1))
                disabled_list="$disabled_list $domain"
            fi
        fi
    fi
done

echo ""
echo "📊 Final Configuration Summary:"
echo "✅ Enabled domains ($enabled_domains):$enabled_list"
if [ $disabled_domains -gt 0 ]; then
    echo "🚫 Disabled domains ($disabled_domains):$disabled_list"
fi
echo ""

# Test final nginx configuration
echo "Testing final nginx configuration..."
if nginx -t; then
    echo "✓ Final nginx configuration is valid"
else
    echo "✗ Final nginx configuration error!"
    exit 1
fi

echo "🏁 Configuration complete. Starting nginx with final configuration..."

# Start nginx with final configuration
exec nginx -g "daemon off;"