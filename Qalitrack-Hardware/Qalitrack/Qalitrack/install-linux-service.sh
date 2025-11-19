#!/bin/bash

###############################################################################
# Qalitrack Platform Service - Linux systemd Installer
###############################################################################
#
# This script builds the application, installs it as a systemd service,
# and configures it to run as root with auto-restart on failure.
#
# Usage: sudo ./install-linux-service.sh
#
###############################################################################

set -e

# Configuration
SERVICE_NAME="qalitrack"
SERVICE_FILE="qalitrack.service"
INSTALL_PATH="/opt/qalitrack"
SERVICE_PATH="/etc/systemd/system/$SERVICE_FILE"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

echo -e "${CYAN}========================================"
echo -e "Qalitrack Linux systemd Installer"
echo -e "========================================${NC}"
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo -e "${RED}Error: This script must be run as root (use sudo)${NC}"
    exit 1
fi

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK is not installed${NC}"
    echo -e "${YELLOW}Install .NET 8.0 SDK from: https://dotnet.microsoft.com/download${NC}"
    exit 1
fi

echo -e "${YELLOW}[1/8] Checking for existing service...${NC}"
if systemctl is-active --quiet $SERVICE_NAME; then
    echo -e "${YELLOW}    Service is running. Stopping...${NC}"
    systemctl stop $SERVICE_NAME
    echo -e "${GREEN}    ✓ Service stopped${NC}"
elif systemctl is-enabled --quiet $SERVICE_NAME 2>/dev/null; then
    echo -e "${YELLOW}    Service exists but not running${NC}"
else
    echo -e "${GREEN}    ✓ No existing service${NC}"
fi

echo -e "${YELLOW}[2/8] Building application...${NC}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true

if [ $? -ne 0 ]; then
    echo -e "${RED}Build failed!${NC}"
    exit 1
fi
echo -e "${GREEN}    ✓ Build completed${NC}"

echo -e "${YELLOW}[3/8] Creating installation directory...${NC}"
mkdir -p "$INSTALL_PATH"

# Copy published files
PUBLISH_PATH="bin/Release/net8.0/linux-x64/publish"
if [ ! -d "$PUBLISH_PATH" ]; then
    echo -e "${RED}Error: Publish directory not found at $PUBLISH_PATH${NC}"
    exit 1
fi

cp -r "$PUBLISH_PATH"/* "$INSTALL_PATH/"
chmod +x "$INSTALL_PATH/Qalitrack"

# Copy configuration if it exists
if [ -f "appsettings.json" ]; then
    cp "appsettings.json" "$INSTALL_PATH/"
fi

echo -e "${GREEN}    ✓ Files copied to $INSTALL_PATH${NC}"

echo -e "${YELLOW}[4/8] Installing systemd service file...${NC}"
if [ ! -f "$SERVICE_FILE" ]; then
    echo -e "${RED}Error: Service file $SERVICE_FILE not found${NC}"
    exit 1
fi

cp "$SERVICE_FILE" "$SERVICE_PATH"
chmod 644 "$SERVICE_PATH"
echo -e "${GREEN}    ✓ Service file installed${NC}"

echo -e "${YELLOW}[5/8] Reloading systemd daemon...${NC}"
systemctl daemon-reload
echo -e "${GREEN}    ✓ Daemon reloaded${NC}"

echo -e "${YELLOW}[6/8] Enabling service to start on boot...${NC}"
systemctl enable $SERVICE_NAME
echo -e "${GREEN}    ✓ Service enabled${NC}"

echo -e "${YELLOW}[7/8] Configuring firewall (if ufw is active)...${NC}"
if command -v ufw &> /dev/null && ufw status | grep -q "Status: active"; then
    ufw allow 5000/tcp
    echo -e "${GREEN}    ✓ Firewall rule added for port 5000${NC}"
else
    echo -e "${GRAY}    ✓ ufw not active or not installed${NC}"
fi

echo -e "${YELLOW}[8/8] Starting service...${NC}"
systemctl start $SERVICE_NAME
sleep 3

# Check service status
if systemctl is-active --quiet $SERVICE_NAME; then
    echo -e "${GREEN}    ✓ Service started successfully${NC}"
else
    echo -e "${RED}    ⚠ Service failed to start${NC}"
    echo -e "${YELLOW}    Check logs with: journalctl -u $SERVICE_NAME -n 50${NC}"
fi

echo ""
echo -e "${GREEN}========================================"
echo -e "Installation Complete!"
echo -e "========================================${NC}"
echo ""
echo -e "${CYAN}Service Name:${NC}    $SERVICE_NAME"
echo -e "${CYAN}Install Path:${NC}    $INSTALL_PATH"
echo -e "${CYAN}Status:${NC}          $(systemctl is-active $SERVICE_NAME)"
echo ""
echo -e "${YELLOW}Management Commands:${NC}"
echo -e "${GRAY}  Start:   sudo systemctl start $SERVICE_NAME${NC}"
echo -e "${GRAY}  Stop:    sudo systemctl stop $SERVICE_NAME${NC}"
echo -e "${GRAY}  Restart: sudo systemctl restart $SERVICE_NAME${NC}"
echo -e "${GRAY}  Status:  sudo systemctl status $SERVICE_NAME${NC}"
echo -e "${GRAY}  Logs:    sudo journalctl -u $SERVICE_NAME -f${NC}"
echo -e "${GRAY}  Disable: sudo systemctl disable $SERVICE_NAME${NC}"
echo ""
echo -e "${CYAN}API Endpoint:${NC} http://localhost:5000"
echo ""
echo -e "${YELLOW}View service status:${NC}"
systemctl status $SERVICE_NAME --no-pager
echo ""
