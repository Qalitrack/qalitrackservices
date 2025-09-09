#!/bin/bash

# QaliTrack Backup Scheduler Setup
# Creates automated backup schedules based on environment and best practices

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
    echo -e "${BLUE}⏰ QaliTrack Backup Scheduler${RESET}"
    echo ""
    echo "Usage: $0 <environment> [options]"
    echo ""
    echo "Environments: development, production"
    echo ""
    echo "Options:"
    echo "  --remove     Remove existing backup schedules"
    echo "  --status     Show current backup schedules"
    echo ""
    echo "Default Schedules:"
    echo "  Development:"
    echo "    - Full backup: Daily at 2 AM"
    echo "    - Incremental: Every 6 hours"
    echo "  Production:"
    echo "    - Full backup: Daily at 1 AM"
    echo "    - Incremental: Every 2 hours"
    echo "    - Vault backup: Daily at 3 AM"
    echo "    - External transfer: After each backup"
}

load_environment() {
    local env=$1
    local env_file="$ROOT_DIR/environments/$env/.env"
    
    if [ ! -f "$env_file" ]; then
        echo -e "${RED}❌ Environment file not found: $env_file${RESET}"
        exit 1
    fi
    
    export $(grep -v '^#' "$env_file" | xargs)
    echo -e "${GREEN}✓${RESET} Environment $env loaded"
}

create_backup_wrapper() {
    local env=$1
    local backup_type=$2
    local options=$3
    
    local wrapper_file="$ROOT_DIR/environments/$env/backup-${backup_type}.sh"
    
    cat > "$wrapper_file" << EOF
#!/bin/bash
# Auto-generated backup wrapper for $env environment
# Backup type: $backup_type

cd "$ROOT_DIR"
exec "$SCRIPT_DIR/backup-system.sh" "$env" "$backup_type" $options >> "$ROOT_DIR/environments/$env/logs/backup-${backup_type}.log" 2>&1
EOF
    
    chmod +x "$wrapper_file"
    echo -e "  ${GREEN}✓${RESET} Created wrapper: $wrapper_file"
}

setup_development_schedule() {
    echo -e "${BLUE}📅 Setting up development backup schedule...${RESET}"
    
    # Create log directory
    mkdir -p "$ROOT_DIR/environments/development/logs"
    
    # Create backup wrappers
    create_backup_wrapper "development" "full" "--compress --verify"
    create_backup_wrapper "development" "incremental" "--compress"
    create_backup_wrapper "development" "snapshot" ""
    
    # Development cron jobs (less frequent)
    local cron_jobs="
# QaliTrack Development Backup Schedule
0 2 * * * $ROOT_DIR/environments/development/backup-full.sh
0 */6 * * * $ROOT_DIR/environments/development/backup-incremental.sh
30 2 * * 0 $ROOT_DIR/environments/development/backup-snapshot.sh
"
    
    # Add to crontab
    (crontab -l 2>/dev/null | grep -v "QaliTrack Development"; echo "$cron_jobs") | crontab -
    
    echo -e "${GREEN}✅ Development backup schedule created${RESET}"
    echo "  - Full backup: Daily at 2:00 AM"
    echo "  - Incremental: Every 6 hours"
    echo "  - Snapshot: Weekly on Sunday at 2:30 AM"
}

setup_production_schedule() {
    echo -e "${BLUE}📅 Setting up production backup schedule...${RESET}"
    
    # Create log directory
    mkdir -p "$ROOT_DIR/environments/production/logs"
    
    # Create backup wrappers with production options
    create_backup_wrapper "production" "full" "--compress --encrypt --external --verify"
    create_backup_wrapper "production" "incremental" "--compress --external"
    create_backup_wrapper "production" "vault" "--encrypt --external"
    create_backup_wrapper "production" "snapshot" "--compress --external"
    
    # Production cron jobs (more frequent and comprehensive)
    local cron_jobs="
# QaliTrack Production Backup Schedule
0 1 * * * $ROOT_DIR/environments/production/backup-full.sh
0 */2 * * * $ROOT_DIR/environments/production/backup-incremental.sh
0 3 * * * $ROOT_DIR/environments/production/backup-vault.sh
0 4 * * 0 $ROOT_DIR/environments/production/backup-snapshot.sh
"
    
    # Add to crontab
    (crontab -l 2>/dev/null | grep -v "QaliTrack Production"; echo "$cron_jobs") | crontab -
    
    echo -e "${GREEN}✅ Production backup schedule created${RESET}"
    echo "  - Full backup: Daily at 1:00 AM (encrypted, external)"
    echo "  - Incremental: Every 2 hours (external)"
    echo "  - Vault secrets: Daily at 3:00 AM (encrypted, external)"
    echo "  - Snapshot: Weekly on Sunday at 4:00 AM (external)"
}

remove_schedules() {
    local env=$1
    
    echo -e "${YELLOW}🗑️ Removing backup schedules for $env...${RESET}"
    
    # Remove from crontab
    crontab -l 2>/dev/null | grep -v "QaliTrack.*$env" | crontab -
    
    # Remove wrapper scripts
    rm -f "$ROOT_DIR/environments/$env/backup-"*.sh
    
    echo -e "${GREEN}✅ Backup schedules removed${RESET}"
}

show_status() {
    local env=$1
    
    echo -e "${BLUE}📊 Current backup schedules for $env:${RESET}"
    echo ""
    
    # Show relevant cron jobs
    local cron_entries=$(crontab -l 2>/dev/null | grep -i "qalitrack.*$env" || echo "No schedules found")
    echo "$cron_entries"
    
    echo ""
    echo -e "${BLUE}📁 Backup wrapper scripts:${RESET}"
    ls -la "$ROOT_DIR/environments/$env/backup-"*.sh 2>/dev/null || echo "No wrapper scripts found"
    
    echo ""
    echo -e "${BLUE}📋 Recent backup logs:${RESET}"
    if [ -d "$ROOT_DIR/environments/$env/logs" ]; then
        ls -la "$ROOT_DIR/environments/$env/logs/backup-"*.log 2>/dev/null | head -5 || echo "No backup logs found"
    else
        echo "Log directory not found"
    fi
}

validate_backup_configuration() {
    local env=$1
    
    echo -e "${BLUE}✅ Validating backup configuration...${RESET}"
    
    # Check required environment variables
    local required_vars=""
    if [ "$env" = "production" ]; then
        required_vars="EXTERNAL_BACKUP_ENABLED EXTERNAL_BACKUP_HOST BACKUP_RETENTION_DAYS GPG_RECIPIENT_EMAIL"
    else
        required_vars="BACKUP_RETENTION_DAYS"
    fi
    
    for var in $required_vars; do
        if [ -z "${!var}" ]; then
            echo -e "${YELLOW}⚠️${RESET} Warning: $var not set in environment"
        else
            echo -e "  ${GREEN}✓${RESET} $var: ${!var}"
        fi
    done
    
    # Test backup script
    if "$SCRIPT_DIR/backup-system.sh" "$env" "full" --help >/dev/null 2>&1; then
        echo -e "  ${GREEN}✓${RESET} Backup system script accessible"
    else
        echo -e "${RED}❌ Backup system script not accessible${RESET}"
        exit 1
    fi
}

# Parse arguments
if [ $# -lt 1 ]; then
    usage
    exit 1
fi

ENVIRONMENT=$1
shift

REMOVE=false
STATUS=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --remove)
            REMOVE=true
            shift
            ;;
        --status)
            STATUS=true
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
    echo -e "${BLUE}⏰ QaliTrack Backup Scheduler${RESET}"
    echo -e "${YELLOW}Environment:${RESET} $ENVIRONMENT"
    echo ""
    
    # Load environment
    load_environment "$ENVIRONMENT"
    
    if [ "$STATUS" = true ]; then
        show_status "$ENVIRONMENT"
        return 0
    fi
    
    if [ "$REMOVE" = true ]; then
        remove_schedules "$ENVIRONMENT"
        return 0
    fi
    
    # Validate configuration
    validate_backup_configuration "$ENVIRONMENT"
    
    # Setup schedules based on environment
    case $ENVIRONMENT in
        development)
            setup_development_schedule
            ;;
        production)
            setup_production_schedule
            ;;
        *)
            echo -e "${RED}❌ Unknown environment: $ENVIRONMENT${RESET}"
            usage
            exit 1
            ;;
    esac
    
    echo ""
    echo -e "${GREEN}🎉 Backup scheduling completed successfully!${RESET}"
    echo -e "${YELLOW}📋 Use '$0 $ENVIRONMENT --status' to view current schedules${RESET}"
}

main "$@"