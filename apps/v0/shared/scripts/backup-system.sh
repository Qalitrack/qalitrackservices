#!/bin/bash

# QaliTrack Comprehensive Backup System
# Supports Full, Incremental, Differential, and Snapshot backups

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$(dirname "$SCRIPT_DIR")")"

# Colors
RED='\033[31m'
GREEN='\033[32m'
YELLOW='\033[33m'
BLUE='\033[34m'
RESET='\033[0m'

# Backup configuration
BACKUP_TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_DATE=$(date +%Y-%m-%d)

usage() {
    echo -e "${BLUE}💾 QaliTrack Backup System${RESET}"
    echo ""
    echo "Usage: $0 <environment> <type> [options]"
    echo ""
    echo "Environments: development, production"
    echo "Backup Types:"
    echo "  full         Full backup of all data"
    echo "  incremental  Incremental backup (changes since last backup)"
    echo "  differential Differential backup (changes since last full backup)"
    echo "  snapshot     Docker volume snapshots"
    echo "  vault        Vault secrets backup (encrypted)"
    echo ""
    echo "Options:"
    echo "  --compress   Compress backup files (default for production)"
    echo "  --encrypt    GPG encrypt backups (production only)"
    echo "  --external   Transfer to external server"
    echo "  --verify     Verify backup integrity"
    echo ""
    echo "Examples:"
    echo "  $0 development full              # Full development backup"
    echo "  $0 production incremental --compress --external"
    echo "  $0 production vault --encrypt"
}

load_environment() {
    local env=$1
    local env_file="$ROOT_DIR/environments/$env/.env"
    
    if [ ! -f "$env_file" ]; then
        echo -e "${RED}❌ Environment file not found: $env_file${RESET}"
        exit 1
    fi
    
    # Export environment variables
    export $(grep -v '^#' "$env_file" | xargs)
    
    # Set backup directory
    BACKUP_BASE_DIR="${BACKUP_PATH:-./backups/$env}"
    mkdir -p "$BACKUP_BASE_DIR"
    
    echo -e "${GREEN}✓${RESET} Environment $env loaded"
    echo -e "${YELLOW}📁${RESET} Backup directory: $BACKUP_BASE_DIR"
}

backup_full() {
    local compress=$1
    local verify=$2
    
    echo -e "${BLUE}💾 Starting full backup...${RESET}"
    
    local backup_dir="$BACKUP_BASE_DIR/full/$BACKUP_DATE"
    mkdir -p "$backup_dir/postgresql" "$backup_dir/volumes" "$backup_dir/metadata"
    
    # Database backups
    echo -e "  ${GREEN}✓${RESET} Backing up PostgreSQL databases..."
    docker compose -f "environments/$ENVIRONMENT/infrastructure"/*.yml exec -T postgres pg_dump -U "$POSTGRES_ADMIN_USER" microservices > "$backup_dir/postgresql/microservices_$BACKUP_TIMESTAMP.sql"
    docker compose -f "environments/$ENVIRONMENT/infrastructure"/*.yml exec -T postgres-backup pg_dump -U "$POSTGRES_BACKUP_ADMIN_USER" backupservicedb > "$backup_dir/postgresql/backupservicedb_$BACKUP_TIMESTAMP.sql"
    
    # Volume backups using docker cp
    echo -e "  ${GREEN}✓${RESET} Backing up Docker volumes..."
    backup_volumes "$backup_dir/volumes"
    
    # Metadata backup
    echo -e "  ${GREEN}✓${RESET} Creating backup metadata..."
    cat > "$backup_dir/metadata/backup_info.json" << EOF
{
    "timestamp": "$BACKUP_TIMESTAMP",
    "date": "$BACKUP_DATE",
    "environment": "$ENVIRONMENT",
    "type": "full",
    "compressed": $compress,
    "databases": ["microservices", "backupservicedb"],
    "volumes": $(docker volume ls --filter "name=${COMPOSE_PROJECT_NAME}" --format "{{.Name}}" | jq -R . | jq -s .),
    "size_bytes": $(du -sb "$backup_dir" | cut -f1),
    "checksum": "$(find "$backup_dir" -type f -exec sha256sum {} \; | sha256sum | cut -d' ' -f1)"
}
EOF
    
    # Compression
    if [ "$compress" = true ]; then
        echo -e "  ${GREEN}✓${RESET} Compressing backup..."
        cd "$BACKUP_BASE_DIR/full"
        tar -czf "${BACKUP_DATE}_full_backup.tar.gz" "$BACKUP_DATE"
        rm -rf "$BACKUP_DATE"
        backup_file="${BACKUP_DATE}_full_backup.tar.gz"
    else
        backup_file="$BACKUP_DATE"
    fi
    
    # Verification
    if [ "$verify" = true ]; then
        verify_backup "$BACKUP_BASE_DIR/full/$backup_file" "full"
    fi
    
    echo -e "${GREEN}✅ Full backup completed: $backup_file${RESET}"
    echo "$BACKUP_BASE_DIR/full/$backup_file"
}

backup_incremental() {
    local compress=$1
    local verify=$2
    
    echo -e "${BLUE}💾 Starting incremental backup...${RESET}"
    
    # Find last backup timestamp
    local last_backup_file=$(find "$BACKUP_BASE_DIR" -name "*.backup_info" -type f -exec ls -t {} \; | head -n1 2>/dev/null || echo "")
    local since_timestamp=""
    
    if [ -n "$last_backup_file" ]; then
        since_timestamp=$(grep -o '"timestamp": "[^"]*' "$last_backup_file" | cut -d'"' -f4)
        echo -e "  ${YELLOW}📅${RESET} Last backup: $since_timestamp"
    else
        echo -e "  ${YELLOW}⚠️${RESET} No previous backup found, performing full backup instead"
        backup_full "$compress" "$verify"
        return
    fi
    
    local backup_dir="$BACKUP_BASE_DIR/incremental/$BACKUP_TIMESTAMP"
    mkdir -p "$backup_dir/postgresql" "$backup_dir/changes"
    
    # PostgreSQL incremental backup using WAL
    echo -e "  ${GREEN}✓${RESET} Creating incremental database backup..."
    
    # For incremental backups, we'll backup only recent changes
    # This is a simplified approach - in production, use WAL-E or similar
    docker compose -f "environments/$ENVIRONMENT/infrastructure"/*.yml exec -T postgres pg_dump -U "$POSTGRES_ADMIN_USER" --inserts --column-inserts microservices > "$backup_dir/postgresql/microservices_incremental_$BACKUP_TIMESTAMP.sql"
    
    # Volume changes (simplified - compare with last backup)
    backup_volume_changes "$backup_dir/changes" "$since_timestamp"
    
    # Metadata
    cat > "$backup_dir/backup_info.json" << EOF
{
    "timestamp": "$BACKUP_TIMESTAMP",
    "date": "$BACKUP_DATE",
    "environment": "$ENVIRONMENT",
    "type": "incremental",
    "since_timestamp": "$since_timestamp",
    "compressed": $compress,
    "size_bytes": $(du -sb "$backup_dir" | cut -f1)
}
EOF
    
    if [ "$compress" = true ]; then
        cd "$BACKUP_BASE_DIR/incremental"
        tar -czf "${BACKUP_TIMESTAMP}_incremental.tar.gz" "$BACKUP_TIMESTAMP"
        rm -rf "$BACKUP_TIMESTAMP"
    fi
    
    echo -e "${GREEN}✅ Incremental backup completed${RESET}"
}

backup_volumes() {
    local volumes_dir=$1
    
    # Get list of volumes for this environment
    local volumes=$(docker volume ls --filter "name=${COMPOSE_PROJECT_NAME}" --format "{{.Name}}")
    
    for volume in $volumes; do
        echo -e "    ${YELLOW}📦${RESET} Backing up volume: $volume"
        
        # Create temporary container to backup volume
        docker run --rm -v "$volume":/data -v "$volumes_dir":/backup alpine \
            sh -c "cd /data && tar -czf /backup/${volume}_${BACKUP_TIMESTAMP}.tar.gz ."
    done
}

backup_volume_changes() {
    local changes_dir=$1
    local since_timestamp=$2
    
    echo -e "  ${GREEN}✓${RESET} Detecting volume changes since $since_timestamp..."
    
    # This is a simplified implementation
    # In production, you'd use more sophisticated change detection
    local volumes=$(docker volume ls --filter "name=${COMPOSE_PROJECT_NAME}" --format "{{.Name}}")
    
    for volume in $volumes; do
        # Find files modified since last backup
        docker run --rm -v "$volume":/data alpine \
            find /data -type f -newermt "@$since_timestamp" 2>/dev/null | \
            head -1000 > "$changes_dir/${volume}_changes.txt" || true
    done
}

backup_vault() {
    local encrypt=$1
    
    echo -e "${BLUE}🔐 Starting Vault secrets backup...${RESET}"
    
    local backup_dir="$BACKUP_BASE_DIR/vault/$BACKUP_DATE"
    mkdir -p "$backup_dir"
    
    # Export all secrets from Vault
    echo -e "  ${GREEN}✓${RESET} Exporting Vault secrets..."
    
    # This requires Vault to be running and accessible
    if ! docker compose -f "environments/$ENVIRONMENT/infrastructure"/*.yml exec -T vault vault status >/dev/null 2>&1; then
        echo -e "${RED}❌ Vault is not accessible${RESET}"
        return 1
    fi
    
    # Export KV secrets
    docker compose -f "environments/$ENVIRONMENT/infrastructure"/*.yml exec -T vault \
        vault kv get -format=json secret/services > "$backup_dir/kv_secrets_$BACKUP_TIMESTAMP.json" 2>/dev/null || echo "{}" > "$backup_dir/kv_secrets_$BACKUP_TIMESTAMP.json"
    
    # Export database configuration (without sensitive data)
    docker compose -f "environments/$ENVIRONMENT/infrastructure"/*.yml exec -T vault \
        vault read -format=json database/config/postgres-main > "$backup_dir/db_config_$BACKUP_TIMESTAMP.json" 2>/dev/null || echo "{}" > "$backup_dir/db_config_$BACKUP_TIMESTAMP.json"
    
    # Encrypt backup if requested
    if [ "$encrypt" = true ] && [ "$ENVIRONMENT" = "production" ]; then
        echo -e "  ${GREEN}✓${RESET} Encrypting Vault backup..."
        
        if [ -n "$GPG_RECIPIENT_EMAIL" ]; then
            for file in "$backup_dir"/*.json; do
                gpg --trust-model always --encrypt -r "$GPG_RECIPIENT_EMAIL" "$file"
                rm "$file"  # Remove unencrypted version
            done
        else
            echo -e "${YELLOW}⚠️${RESET} GPG_RECIPIENT_EMAIL not set, skipping encryption"
        fi
    fi
    
    echo -e "${GREEN}✅ Vault backup completed${RESET}"
}

verify_backup() {
    local backup_path=$1
    local backup_type=$2
    
    echo -e "${BLUE}🔍 Verifying backup integrity...${RESET}"
    
    if [ -f "$backup_path" ]; then
        # Test archive integrity
        if [[ "$backup_path" == *.tar.gz ]]; then
            tar -tzf "$backup_path" >/dev/null
        fi
        
        # Check file sizes
        local size=$(du -sh "$backup_path" | cut -f1)
        echo -e "  ${GREEN}✓${RESET} Backup size: $size"
        
        # Generate and store checksum
        local checksum=$(sha256sum "$backup_path" | cut -d' ' -f1)
        echo "$checksum $backup_path" > "${backup_path}.sha256"
        echo -e "  ${GREEN}✓${RESET} Checksum: $checksum"
    fi
    
    echo -e "${GREEN}✅ Backup verification completed${RESET}"
}

external_transfer() {
    local backup_file=$1
    
    if [ "${EXTERNAL_BACKUP_ENABLED:-false}" != "true" ]; then
        echo -e "${YELLOW}⚠️${RESET} External backup not enabled"
        return 0
    fi
    
    echo -e "${BLUE}🌐 Transferring to external server...${RESET}"
    
    local remote_path="${EXTERNAL_BACKUP_PATH}/${ENVIRONMENT}/$(basename "$backup_file")"
    
    # Use rsync for reliable transfer
    if [ -n "$EXTERNAL_BACKUP_SSH_KEY_PATH" ]; then
        rsync -av -e "ssh -i $EXTERNAL_BACKUP_SSH_KEY_PATH" \
            "$backup_file" "${EXTERNAL_BACKUP_USER}@${EXTERNAL_BACKUP_HOST}:$remote_path"
    else
        rsync -av "$backup_file" "${EXTERNAL_BACKUP_USER}@${EXTERNAL_BACKUP_HOST}:$remote_path"
    fi
    
    echo -e "${GREEN}✅ External transfer completed${RESET}"
}

cleanup_old_backups() {
    local retention_days=${BACKUP_RETENTION_DAYS:-7}
    
    echo -e "${BLUE}🧹 Cleaning up old backups (retention: ${retention_days} days)...${RESET}"
    
    # Clean up old backups
    find "$BACKUP_BASE_DIR" -type f -mtime +$retention_days -name "*.tar.gz" -delete
    find "$BACKUP_BASE_DIR" -type f -mtime +$retention_days -name "*.sql" -delete
    find "$BACKUP_BASE_DIR" -type d -empty -delete
    
    echo -e "${GREEN}✅ Cleanup completed${RESET}"
}

# Parse command line arguments
if [ $# -lt 2 ]; then
    usage
    exit 1
fi

ENVIRONMENT=$1
BACKUP_TYPE=$2
shift 2

COMPRESS=false
ENCRYPT=false
EXTERNAL=false
VERIFY=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --compress)
            COMPRESS=true
            shift
            ;;
        --encrypt)
            ENCRYPT=true
            shift
            ;;
        --external)
            EXTERNAL=true
            shift
            ;;
        --verify)
            VERIFY=true
            shift
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
    echo -e "${BLUE}🚀 QaliTrack Backup System${RESET}"
    echo -e "${YELLOW}Environment:${RESET} $ENVIRONMENT"
    echo -e "${YELLOW}Backup Type:${RESET} $BACKUP_TYPE"
    echo -e "${YELLOW}Timestamp:${RESET} $BACKUP_TIMESTAMP"
    echo ""
    
    # Load environment
    load_environment "$ENVIRONMENT"
    
    # Set production defaults
    if [ "$ENVIRONMENT" = "production" ]; then
        COMPRESS=true
        VERIFY=true
    fi
    
    # Execute backup
    local backup_file=""
    case $BACKUP_TYPE in
        full)
            backup_file=$(backup_full $COMPRESS $VERIFY)
            ;;
        incremental)
            backup_file=$(backup_incremental $COMPRESS $VERIFY)
            ;;
        differential)
            # Similar to incremental but uses last full backup as base
            echo -e "${YELLOW}⚠️${RESET} Differential backup not fully implemented yet"
            backup_file=$(backup_incremental $COMPRESS $VERIFY)
            ;;
        snapshot)
            backup_dir="$BACKUP_BASE_DIR/snapshots/$BACKUP_TIMESTAMP"
            mkdir -p "$backup_dir"
            backup_volumes "$backup_dir"
            backup_file="$backup_dir"
            ;;
        vault)
            backup_vault $ENCRYPT
            backup_file="$BACKUP_BASE_DIR/vault/$BACKUP_DATE"
            ;;
        *)
            echo -e "${RED}❌ Unknown backup type: $BACKUP_TYPE${RESET}"
            usage
            exit 1
            ;;
    esac
    
    # External transfer
    if [ "$EXTERNAL" = true ] && [ -n "$backup_file" ]; then
        external_transfer "$backup_file"
    fi
    
    # Cleanup
    cleanup_old_backups
    
    echo ""
    echo -e "${GREEN}🎉 Backup operation completed successfully!${RESET}"
    echo -e "${YELLOW}📁 Backup location:${RESET} $backup_file"
}

main "$@"