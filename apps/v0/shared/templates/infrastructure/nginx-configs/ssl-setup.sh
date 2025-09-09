#!/bin/bash
# SSL Certificate Setup Script for Production
# Uses Let's Encrypt for automatic SSL certificate management

set -e

# Colors
RED='\033[31m'
GREEN='\033[32m'
YELLOW='\033[33m'
BLUE='\033[34m'
RESET='\033[0m'

# Configuration
DOMAINS_FILE="/etc/nginx/domains.txt"
SSL_DIR="/etc/nginx/ssl"
CERTBOT_DIR="/etc/letsencrypt"

usage() {
    echo -e "${BLUE}🔐 SSL Certificate Setup Script${RESET}"
    echo ""
    echo "Usage: $0 [--dry-run] [--force-renew]"
    echo ""
    echo "Options:"
    echo "  --dry-run      Test certificate generation without making changes"
    echo "  --force-renew  Force certificate renewal even if not expired"
    echo ""
    echo "Environment Variables:"
    echo "  LETSENCRYPT_EMAIL  Email for Let's Encrypt registration"
    echo "  SSL_DOMAINS        Space-separated list of domains"
}

check_prerequisites() {
    echo -e "${BLUE}📋 Checking prerequisites...${RESET}"
    
    # Check if running in production environment
    if [ "${ENVIRONMENT}" != "production" ]; then
        echo -e "${YELLOW}⚠️  SSL setup is intended for production environment only${RESET}"
        echo -e "${YELLOW}Current environment: ${ENVIRONMENT}${RESET}"
        read -p "Continue anyway? [y/N]: " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            exit 1
        fi
    fi
    
    # Check required environment variables
    if [ -z "$LETSENCRYPT_EMAIL" ]; then
        echo -e "${RED}❌ LETSENCRYPT_EMAIL environment variable not set${RESET}"
        exit 1
    fi
    
    # Install certbot if not available
    if ! command -v certbot &> /dev/null; then
        echo -e "${YELLOW}📦 Installing certbot...${RESET}"
        apt-get update -qq
        apt-get install -y certbot python3-certbot-nginx
    fi
    
    echo -e "${GREEN}✅ Prerequisites check complete${RESET}"
}

read_domains() {
    local domains=""
    
    # Try SSL_DOMAINS environment variable first
    if [ -n "$SSL_DOMAINS" ]; then
        domains="$SSL_DOMAINS"
        echo -e "${GREEN}📋 Using domains from SSL_DOMAINS: $domains${RESET}"
    # Try domains.txt file
    elif [ -f "$DOMAINS_FILE" ]; then
        domains=$(cat "$DOMAINS_FILE" | tr '\n' ' ' | xargs)
        echo -e "${GREEN}📋 Using domains from $DOMAINS_FILE: $domains${RESET}"
    else
        echo -e "${RED}❌ No domains configured${RESET}"
        echo -e "${YELLOW}Set SSL_DOMAINS environment variable or create $DOMAINS_FILE${RESET}"
        exit 1
    fi
    
    if [ -z "$domains" ]; then
        echo -e "${RED}❌ No domains found${RESET}"
        exit 1
    fi
    
    echo "$domains"
}

setup_certificates() {
    local domains=$1
    local dry_run=$2
    local force_renew=$3
    
    # Get primary domain (first in list)
    local primary_domain=$(echo $domains | awk '{print $1}')
    local cert_path="$CERTBOT_DIR/live/$primary_domain/fullchain.pem"
    
    echo -e "${BLUE}🔐 Setting up SSL certificates...${RESET}"
    echo -e "${YELLOW}Primary domain: $primary_domain${RESET}"
    echo -e "${YELLOW}All domains: $domains${RESET}"
    
    # Check if certificates exist and are valid
    local need_cert_update=false
    
    if [ ! -f "$cert_path" ]; then
        echo -e "${YELLOW}📋 SSL certificates not found. Setting up new certificates...${RESET}"
        need_cert_update=true
    elif [ "$force_renew" = true ]; then
        echo -e "${YELLOW}📋 Force renewal requested.${RESET}"
        need_cert_update=true
    else
        echo -e "${GREEN}📋 SSL certificates found. Checking domains...${RESET}"
        
        # Check if all domains are included in certificate
        for domain in $domains; do
            if ! openssl x509 -in "$cert_path" -text -noout | grep -q "DNS:$domain"; then
                echo -e "${YELLOW}📋 Domain $domain not found in certificate. Update needed.${RESET}"
                need_cert_update=true
                break
            fi
        done
        
        if [ "$need_cert_update" = false ]; then
            echo -e "${GREEN}✅ All domains are included in existing certificate.${RESET}"
            return 0
        fi
    fi
    
    if [ "$need_cert_update" = true ]; then
        echo -e "${BLUE}🔐 Updating SSL certificates...${RESET}"
        
        # Format domain parameters for certbot
        local domain_params=""
        for domain in $domains; do
            domain_params="$domain_params -d $domain"
        done
        
        # Certbot command
        local certbot_cmd="certbot --nginx --agree-tos --non-interactive --email $LETSENCRYPT_EMAIL $domain_params --expand"
        
        if [ "$dry_run" = true ]; then
            certbot_cmd="$certbot_cmd --dry-run"
            echo -e "${YELLOW}🧪 Running in dry-run mode...${RESET}"
        fi
        
        echo -e "${BLUE}🚀 Executing: $certbot_cmd${RESET}"
        
        if eval $certbot_cmd; then
            if [ "$dry_run" = false ]; then
                echo -e "${GREEN}✅ SSL certificates successfully obtained/updated.${RESET}"
                setup_ssl_directory "$primary_domain"
            else
                echo -e "${GREEN}✅ Dry-run completed successfully.${RESET}"
            fi
        else
            echo -e "${RED}❌ Certificate setup failed.${RESET}"
            exit 1
        fi
    fi
}

setup_ssl_directory() {
    local primary_domain=$1
    local cert_dir="$CERTBOT_DIR/live/$primary_domain"
    
    echo -e "${BLUE}📁 Setting up SSL directory structure...${RESET}"
    
    # Create SSL directory if it doesn't exist
    mkdir -p "$SSL_DIR"
    
    # Create symlinks to certificates
    if [ -f "$cert_dir/fullchain.pem" ]; then
        ln -sf "$cert_dir/fullchain.pem" "$SSL_DIR/cert.pem"
        ln -sf "$cert_dir/privkey.pem" "$SSL_DIR/key.pem"
        ln -sf "$cert_dir/chain.pem" "$SSL_DIR/chain.pem"
        echo -e "${GREEN}✅ SSL certificate symlinks created.${RESET}"
    else
        echo -e "${RED}❌ Certificate files not found in $cert_dir${RESET}"
        exit 1
    fi
}

setup_renewal() {
    echo -e "${BLUE}⏰ Setting up automatic certificate renewal...${RESET}"
    
    # Create renewal script
    cat > /usr/local/bin/qalitrack-ssl-renew.sh << 'EOF'
#!/bin/bash
# QaliTrack SSL Certificate Renewal Script
/usr/bin/certbot renew --quiet --nginx
if [ $? -eq 0 ]; then
    /usr/bin/docker exec qalitrack-prod-nginx nginx -s reload
fi
EOF
    
    chmod +x /usr/local/bin/qalitrack-ssl-renew.sh
    
    # Add cron job for renewal (runs twice daily)
    (crontab -l 2>/dev/null; echo "0 0,12 * * * /usr/local/bin/qalitrack-ssl-renew.sh") | crontab -
    
    echo -e "${GREEN}✅ Automatic renewal configured.${RESET}"
}

# Parse command line arguments
DRY_RUN=false
FORCE_RENEW=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --force-renew)
            FORCE_RENEW=true
            shift
            ;;
        --help)
            usage
            exit 0
            ;;
        *)
            echo -e "${RED}❌ Unknown option: $1${RESET}"
            usage
            exit 1
            ;;
    esac
done

# Main execution
main() {
    echo -e "${BLUE}🚀 QaliTrack SSL Certificate Setup${RESET}"
    echo ""
    
    check_prerequisites
    
    local domains=$(read_domains)
    setup_certificates "$domains" $DRY_RUN $FORCE_RENEW
    
    if [ "$DRY_RUN" = false ]; then
        setup_renewal
        echo ""
        echo -e "${GREEN}🎉 SSL setup complete!${RESET}"
        echo -e "${YELLOW}📋 Next steps:${RESET}"
        echo -e "  1. Restart nginx: docker exec qalitrack-prod-nginx nginx -s reload"
        echo -e "  2. Test HTTPS: curl -I https://$(echo $domains | awk '{print $1}')"
        echo -e "  3. Check certificate: openssl x509 -in /etc/nginx/ssl/cert.pem -text -noout"
    fi
}

main "$@"