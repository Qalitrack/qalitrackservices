#!/bin/bash

###############################################################################
# Qalitrack Platform Service - Linux systemd Uninstaller
###############################################################################
#
# This script stops and removes the Qalitrack systemd service.
#
# Usage: sudo ./uninstall-linux-service.sh [--remove-files]
#
###############################################################################

set -e

# Configuration
SERVICE_NAME="qalitrack"
SERVICE_FILE="qalitrack.service"
INSTALL_PATH="/opt/qalitrack"
SERVICE_PATH="/etc/systemd/system/$SERVICE_FILE"
REMOVE_FILES=false

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Parse arguments
if [ "$1" == "--remove-files" ]; then
    REMOVE_FILES=true
fi

echo -e "${CYAN}========================================"
echo -e "Qalitrack Linux systemd Uninstaller"
echo -e "========================================${NC}"
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo -e "${RED}Error: This script must be run as root (use sudo)${NC}"
    exit 1
fi

echo -e "${YELLOW}[1/5] Checking for service...${NC}"
if systemctl list-unit-files | grep -q "$SERVICE_NAME.service"; then
    echo -e "${GREEN}    ✓ Service found${NC}"

    echo -e "${YELLOW}[2/5] Stopping service...${NC}"
    if systemctl is-active --quiet $SERVICE_NAME; then
        systemctl stop $SERVICE_NAME
        echo -e "${GREEN}    ✓ Service stopped${NC}"
    else
        echo -e "${GREEN}    ✓ Service already stopped${NC}"
    fi

    echo -e "${YELLOW}[3/5] Disabling service...${NC}"
    if systemctl is-enabled --quiet $SERVICE_NAME 2>/dev/null; then
        systemctl disable $SERVICE_NAME
        echo -e "${GREEN}    ✓ Service disabled${NC}"
    else
        echo -e "${GREEN}    ✓ Service already disabled${NC}"
    fi

    echo -e "${YELLOW}[4/5] Removing service file...${NC}"
    if [ -f "$SERVICE_PATH" ]; then
        rm -f "$SERVICE_PATH"
        systemctl daemon-reload
        systemctl reset-failed
        echo -e "${GREEN}    ✓ Service file removed${NC}"
    else
        echo -e "${GREEN}    ✓ Service file not found${NC}"
    fi
else
    echo -e "${YELLOW}    ⚠ Service '$SERVICE_NAME' not found${NC}"
fi

echo -e "${YELLOW}[5/5] Removing firewall rule...${NC}"
if command -v ufw &> /dev/null && ufw status | grep -q "Status: active"; then
    if ufw status | grep -q "5000/tcp"; then
        ufw delete allow 5000/tcp
        echo -e "${GREEN}    ✓ Firewall rule removed${NC}"
    else
        echo -e "${GREEN}    ✓ No firewall rule found${NC}"
    fi
else
    echo -e "${GRAY}    ✓ ufw not active or not installed${NC}"
fi

if [ "$REMOVE_FILES" = true ]; then
    echo ""
    echo -e "${YELLOW}Removing installation files...${NC}"
    if [ -d "$INSTALL_PATH" ]; then
        rm -rf "$INSTALL_PATH"
        echo -e "${GREEN}    ✓ Files removed from $INSTALL_PATH${NC}"
    else
        echo -e "${GREEN}    ✓ Installation directory not found${NC}"
    fi
fi

echo ""
echo -e "${GREEN}========================================"
echo -e "Uninstallation Complete!"
echo -e "========================================${NC}"
echo ""

if [ "$REMOVE_FILES" = false ]; then
    echo -e "${YELLOW}Note: Installation files remain at: $INSTALL_PATH${NC}"
    echo -e "${YELLOW}      Use --remove-files to delete them${NC}"
    echo ""
fi
