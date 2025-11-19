#!/bin/bash

###############################################################################
# Qalitrack Platform Service - Linux Service Manager
###############################################################################
#
# Provides commands to manage the Qalitrack systemd service.
#
# Usage: sudo ./manage-linux-service.sh <command>
#
# Commands: start, stop, restart, status, logs, tail, enable, disable
#
###############################################################################

# Configuration
SERVICE_NAME="qalitrack"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Check if running as root for certain commands
check_root() {
    if [ "$EUID" -ne 0 ]; then
        echo -e "${RED}Error: This command requires root privileges (use sudo)${NC}"
        exit 1
    fi
}

# Show usage
show_usage() {
    echo -e "${CYAN}Qalitrack Service Manager${NC}"
    echo ""
    echo -e "${YELLOW}Usage:${NC} $0 <command>"
    echo ""
    echo -e "${YELLOW}Commands:${NC}"
    echo -e "  ${GREEN}start${NC}     - Start the service"
    echo -e "  ${GREEN}stop${NC}      - Stop the service"
    echo -e "  ${GREEN}restart${NC}   - Restart the service"
    echo -e "  ${GREEN}status${NC}    - Show service status"
    echo -e "  ${GREEN}logs${NC}      - Show recent logs (last 50 lines)"
    echo -e "  ${GREEN}tail${NC}      - Follow logs in real-time"
    echo -e "  ${GREEN}enable${NC}    - Enable service to start on boot"
    echo -e "  ${GREEN}disable${NC}   - Disable service from starting on boot"
    echo ""
    exit 1
}

# Check if service exists
if ! systemctl list-unit-files | grep -q "$SERVICE_NAME.service"; then
    echo -e "${RED}Error: Service '$SERVICE_NAME' not found${NC}"
    echo -e "${YELLOW}Run install-linux-service.sh to install the service${NC}"
    exit 1
fi

# Parse command
COMMAND=${1:-status}

case $COMMAND in
    start)
        check_root
        echo -e "${YELLOW}Starting $SERVICE_NAME...${NC}"
        systemctl start $SERVICE_NAME
        sleep 2
        systemctl status $SERVICE_NAME --no-pager
        ;;

    stop)
        check_root
        echo -e "${YELLOW}Stopping $SERVICE_NAME...${NC}"
        systemctl stop $SERVICE_NAME
        sleep 2
        systemctl status $SERVICE_NAME --no-pager
        ;;

    restart)
        check_root
        echo -e "${YELLOW}Restarting $SERVICE_NAME...${NC}"
        systemctl restart $SERVICE_NAME
        sleep 2
        systemctl status $SERVICE_NAME --no-pager
        ;;

    status)
        echo -e "${CYAN}========================================"
        echo -e "Service Status"
        echo -e "========================================${NC}"
        systemctl status $SERVICE_NAME --no-pager
        echo ""
        echo -e "${CYAN}Service Info:${NC}"
        echo -e "  Loaded:  $(systemctl is-enabled $SERVICE_NAME 2>/dev/null || echo 'disabled')"
        echo -e "  Active:  $(systemctl is-active $SERVICE_NAME)"
        echo ""
        ;;

    logs)
        echo -e "${CYAN}========================================"
        echo -e "Recent Logs (Last 50 lines)"
        echo -e "========================================${NC}"
        journalctl -u $SERVICE_NAME -n 50 --no-pager
        ;;

    tail)
        echo -e "${CYAN}Tailing logs for $SERVICE_NAME (Ctrl+C to stop)...${NC}"
        echo ""
        journalctl -u $SERVICE_NAME -f
        ;;

    enable)
        check_root
        echo -e "${YELLOW}Enabling $SERVICE_NAME to start on boot...${NC}"
        systemctl enable $SERVICE_NAME
        echo -e "${GREEN}✓ Service enabled${NC}"
        ;;

    disable)
        check_root
        echo -e "${YELLOW}Disabling $SERVICE_NAME from starting on boot...${NC}"
        systemctl disable $SERVICE_NAME
        echo -e "${GREEN}✓ Service disabled${NC}"
        ;;

    *)
        echo -e "${RED}Error: Unknown command '$COMMAND'${NC}"
        echo ""
        show_usage
        ;;
esac
