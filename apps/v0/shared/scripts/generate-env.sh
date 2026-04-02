#!/bin/bash

# Environment Setup Script
# Generates environment-specific configurations

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
    echo -e "${BLUE}🔧 Environment Generation Script${RESET}"
    echo ""
    echo "Usage: $0 <environment>"
    echo ""
    echo "Environments: development, production"
    echo ""
    echo "Examples:"
    echo "  $0 development    # Generate development environment"
    echo "  $0 production     # Generate production environment"
}

generate_environment() {
    local env=$1
    local env_dir="$ROOT_DIR/environments/$env"
    
    if [ ! -d "$env_dir" ]; then
        echo -e "${RED}❌ Environment directory not found: $env_dir${RESET}"
        exit 1
    fi
    
    echo -e "${BLUE}🏗️  Generating $env environment...${RESET}"
    
    # Load environment variables
    if [ -f "$env_dir/.env" ]; then
        source "$env_dir/.env"
    else
        echo -e "${RED}❌ .env file not found: $env_dir/.env${RESET}"
        exit 1
    fi
    
    # Create infrastructure directory
    mkdir -p "$env_dir/infrastructure"
    mkdir -p "$env_dir/applications"
    
    # Generate infrastructure files from templates
    for template in "$ROOT_DIR/shared/templates/infrastructure"/*.yml; do
        if [ -f "$template" ]; then
            filename=$(basename "$template")
            echo -e "  ${GREEN}✓${RESET} Generating infrastructure/$filename"
            envsubst < "$template" > "$env_dir/infrastructure/$filename"
        fi
    done
    
    # Copy nginx configuration templates
    echo -e "  ${GREEN}✓${RESET} Copying nginx configurations"
    cp -r "$ROOT_DIR/shared/templates/infrastructure/nginx-configs" "$env_dir/" 2>/dev/null || true
    
    # Generate nginx.conf from template
    if [ -f "$env_dir/nginx-configs/nginx.conf.template" ]; then
        echo -e "  ${GREEN}✓${RESET} Generating nginx.conf"
        # Load environment variables for substitution
        if [ -f "$env_file" ]; then
            export $(grep -v '^#' "$env_file" | xargs)
        fi
        envsubst < "$env_dir/nginx-configs/nginx.conf.template" > "$env_dir/nginx-configs/nginx.conf"
    fi
    
    # Generate nginx server configurations
    echo -e "  ${GREEN}✓${RESET} Generating nginx server configurations"
    "$ROOT_DIR/shared/scripts/generate-nginx-config.sh" "$env"
    
    # Create SSL directory and copy domains for production
    if [ "$env" = "production" ]; then
        mkdir -p "$env_dir/nginx-configs/ssl"
        cp "$env_dir/nginx-configs/domains.txt.example" "$env_dir/nginx-configs/domains.txt" 2>/dev/null || true
        echo -e "  ${YELLOW}⚠️${RESET}  Production SSL directory created"
        echo -e "  ${YELLOW}⚠️${RESET}  Edit nginx-configs/domains.txt with your domains"
        echo -e "  ${YELLOW}⚠️${RESET}  Run ssl-setup.sh after first startup to configure SSL"
    fi
    
    # Copy remaining templates that need manual creation
    for template_file in "02-cache-messaging.yml" "03-storage-search.yml" "05-email.yml" "99-secrets-init.yml"; do
        if [ ! -f "$env_dir/infrastructure/$template_file" ]; then
            echo -e "  ${YELLOW}⚠️${RESET}  Manual creation needed: infrastructure/$template_file"
        fi
    done
    
    echo -e "${GREEN}✅ Environment $env generated successfully!${RESET}"
    echo ""
    echo -e "${YELLOW}Next steps:${RESET}"
    echo -e "  1. Review generated files in: ${BLUE}$env_dir${RESET}"
    echo -e "  2. Create missing infrastructure templates if needed"
    echo -e "  3. Run: ${GREEN}make $env up${RESET}"
}

# Main script
if [ $# -ne 1 ]; then
    usage
    exit 1
fi

ENVIRONMENT=$1

case $ENVIRONMENT in
    development|production)
        generate_environment "$ENVIRONMENT"
        ;;
    *)
        echo -e "${RED}❌ Invalid environment: $ENVIRONMENT${RESET}"
        usage
        exit 1
        ;;
esac