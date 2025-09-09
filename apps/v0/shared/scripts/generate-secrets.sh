#!/bin/bash

# Secret Generation Script for QaliTrack Environments
# Generates unique, secure secrets for each environment setup

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
    echo -e "${BLUE}🔐 Secret Generation Script${RESET}"
    echo ""
    echo "Usage: $0 <environment> [--force]"
    echo ""
    echo "Environments: development, production"
    echo "Options:"
    echo "  --force    Overwrite existing .env file"
    echo ""
    echo "Examples:"
    echo "  $0 development           # Generate development secrets (fail if exists)"
    echo "  $0 production --force    # Force regenerate production secrets"
}

generate_secrets() {
    local env=$1
    local force=$2
    local env_file="$ROOT_DIR/environments/$env/.env"
    local env_example="$ROOT_DIR/environments/$env/.env.example"
    
    if [ ! -f "$env_example" ]; then
        echo -e "${RED}❌ .env.example not found: $env_example${RESET}"
        exit 1
    fi
    
    if [ -f "$env_file" ] && [ "$force" != "--force" ]; then
        echo -e "${RED}❌ Secrets file already exists: $env_file${RESET}"
        echo -e "${YELLOW}To regenerate secrets, use one of these options:${RESET}"
        echo -e "${YELLOW}  1. Remove existing file: rm $env_file${RESET}"
        echo -e "${YELLOW}  2. Use force flag: $0 $env --force${RESET}"
        echo -e "${YELLOW}  3. Use make target: make generate-secrets${RESET}"
        exit 1
    fi
    
    if [ -f "$env_file" ] && [ "$force" = "--force" ]; then
        echo -e "${YELLOW}⚠️  Overwriting existing secrets file${RESET}"
    fi
    
    echo -e "${BLUE}🔐 Generating unique secrets for $env environment...${RESET}"
    
    # Generate unique secrets
    local postgres_admin_password=$(openssl rand -base64 32 | tr -d '=' | tr '/' '_')
    local postgres_backup_admin_password=$(openssl rand -base64 32 | tr -d '=' | tr '/' '_')
    local redis_admin_password=$(openssl rand -hex 16)
    local rabbitmq_admin_password=$(openssl rand -base64 24 | tr -d '=' | tr '/' '_')
    local minio_admin_password=$(openssl rand -base64 24 | tr -d '=' | tr '/' '_')
    local zincsearch_admin_password=$(openssl rand -base64 20 | tr -d '=' | tr '/' '_')
    local vault_root_token="$(uuidgen | tr '[:upper:]' '[:lower:]')"
    
    # Create environment-specific prefixes
    local prefix
    if [ "$env" = "development" ]; then
        prefix="dev"
    elif [ "$env" = "production" ]; then
        prefix="prod"
    else
        prefix="$env"
    fi
    
    echo -e "  ${GREEN}✓${RESET} Generated PostgreSQL admin password"
    echo -e "  ${GREEN}✓${RESET} Generated Redis admin password"
    echo -e "  ${GREEN}✓${RESET} Generated RabbitMQ admin password"
    echo -e "  ${GREEN}✓${RESET} Generated MinIO admin password"
    echo -e "  ${GREEN}✓${RESET} Generated ZincSearch admin password"
    echo -e "  ${GREEN}✓${RESET} Generated Vault root token"
    
    # Create .env file from .env.example with generated secrets
    cp "$env_example" "$env_file"
    
    # Replace placeholder secrets with generated ones
    sed -i "s/PLACEHOLDER_POSTGRES_ADMIN_PASSWORD/${prefix}-vault-postgres-admin-pass-${postgres_admin_password}/g" "$env_file"
    sed -i "s/PLACEHOLDER_POSTGRES_BACKUP_ADMIN_PASSWORD/${prefix}-vault-backup-admin-pass-${postgres_backup_admin_password}/g" "$env_file"
    sed -i "s/PLACEHOLDER_REDIS_ADMIN_PASSWORD/${prefix}-vault-redis-admin-pass-${redis_admin_password}/g" "$env_file"
    sed -i "s/PLACEHOLDER_RABBITMQ_ADMIN_PASSWORD/${prefix}-vault-rabbitmq-admin-pass-${rabbitmq_admin_password}/g" "$env_file"
    sed -i "s/PLACEHOLDER_MINIO_ADMIN_PASSWORD/${prefix}-vault-minio-admin-pass-${minio_admin_password}/g" "$env_file"
    sed -i "s/PLACEHOLDER_ZINCSEARCH_ADMIN_PASSWORD/${prefix}-vault-zinc-admin-pass-${zincsearch_admin_password}/g" "$env_file"
    sed -i "s/PLACEHOLDER_VAULT_ROOT_TOKEN/${vault_root_token}/g" "$env_file"
    
    echo -e "${GREEN}✅ Secrets generated and saved to: $env_file${RESET}"
    echo ""
    echo -e "${YELLOW}⚠️  SECURITY NOTE:${RESET}"
    echo -e "  - Keep .env files secure and never commit them to git"
    echo -e "  - .env files contain admin credentials for Vault management"
    echo -e "  - Applications will use dynamic credentials from Vault"
    echo -e "  - Backup .env files securely for disaster recovery"
}

validate_environment() {
    local env=$1
    case $env in
        development|production)
            return 0
            ;;
        *)
            echo -e "${RED}❌ Invalid environment: $env${RESET}"
            usage
            exit 1
            ;;
    esac
}

# Main script
if [ $# -lt 1 ] || [ $# -gt 2 ]; then
    usage
    exit 1
fi

ENVIRONMENT=$1
FORCE_FLAG=$2

validate_environment "$ENVIRONMENT"
generate_secrets "$ENVIRONMENT" "$FORCE_FLAG"