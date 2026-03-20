# Qalitrack Platform Service - Installation Guide

This guide explains how to build, install, and manage the Qalitrack Platform Service as a system service on Windows and Linux.

## Overview

The Qalitrack Platform Service is designed to run as:
- **Windows Service** (like Windows Bluetooth Service)
- **Linux systemd daemon** (like any system service)

### Key Features
- ✅ Runs as Administrator/root
- ✅ Auto-starts on boot (even without internet)
- ✅ Auto-restarts on crash
- ✅ Offline mode with serial fallback (platforms continue working without internet)
- ✅ Logs to Windows Event Log / Linux systemd journal
- ✅ Cross-platform (Windows & Linux)
- ✅ Runtime configuration (no rebuild needed for IP changes)

### Offline Capabilities

**Platform Weight Data:**
- **With Internet**: Uses TCP connection to weighbridge
- **Without Internet**: Automatically falls back to serial port
- Platform scales continue working even when network is down

**Cameras:**
- Require network/internet (RTSP/HTTP)
- Automatically reconnect when network comes back
- Service stays running even if cameras are offline

---

## Building the Application

You only need to build once, then deploy to multiple sites with different configurations.

### Prerequisites

**Both Windows & Linux:**
- .NET 8.0 SDK ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))
- Git (to clone the repository)

### Build for Linux

```bash
# Navigate to project directory
cd /path/to/Qalitrack

# Build Release version for Linux
dotnet build -c Release

# Output will be in:
# bin/Release/net8.0/

# Optional: Create self-contained Linux build (includes .NET runtime)
dotnet publish -c Release -r linux-x64 --self-contained
# Output: bin/Release/net8.0/linux-x64/publish/
```

### Build for Windows

```powershell
# Navigate to project directory
cd C:\path\to\Qalitrack

# Build Release version for Windows
dotnet build -c Release

# Output will be in:
# bin\Release\net8.0\

# Optional: Create self-contained Windows build (includes .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained
# Output: bin\Release\net8.0\win-x64\publish\
```

### Cross-Platform Build

You can build for both platforms from one machine:

```bash
# Build for Linux (from any OS)
dotnet publish -c Release -r linux-x64 --self-contained

# Build for Windows (from any OS)
dotnet publish -c Release -r win-x64 --self-contained
```

**Important Notes:**
- Build **once** with default settings
- Deploy the same binary to all sites
- **No rebuild needed** for different IPs/cameras
- Configuration is done at runtime via config files

---

## Windows Installation

### Prerequisites
- Windows 10/11 or Windows Server 2016+
- .NET 8.0 SDK
- Administrator privileges

### Installation Steps

1. **Open PowerShell as Administrator**
   ```powershell
   # Right-click PowerShell and select "Run as Administrator"
   ```

2. **Navigate to the project directory**
   ```powershell
   cd C:\path\to\Qalitrack
   ```

3. **Run the installation script**
   ```powershell
   .\install-windows-service.ps1
   ```

   The script will:
   - Build the application
   - Create the Windows Service
   - Configure auto-restart on failure (5s, 10s, 60s delays)
   - Add firewall rule for port 5000
   - Start the service

### Service Management (Windows)

#### Using PowerShell Scripts

**Check Status:**
```powershell
.\manage-windows-service.ps1 status
```

**Start Service:**
```powershell
.\manage-windows-service.ps1 start
```

**Stop Service:**
```powershell
.\manage-windows-service.ps1 stop
```

**Restart Service:**
```powershell
.\manage-windows-service.ps1 restart
```

**View Logs:**
```powershell
# Last 50 log entries
.\manage-windows-service.ps1 logs

# Follow logs in real-time
.\manage-windows-service.ps1 tail
```

#### Using Native Windows Commands

```powershell
# Start
Start-Service QalitrackPlatformService

# Stop
Stop-Service QalitrackPlatformService

# Restart
Restart-Service QalitrackPlatformService

# Status
Get-Service QalitrackPlatformService

# View logs in Event Viewer
Get-EventLog -LogName Application -Source QalitrackPlatformService -Newest 50
```

### Uninstall (Windows)

```powershell
# Remove service only
.\uninstall-windows-service.ps1

# Remove service and files
.\uninstall-windows-service.ps1 -RemoveFiles
```

### Windows Service Configuration

**Auto-Restart Policy:**
- First failure: Restart after 5 seconds
- Second failure: Restart after 10 seconds
- Third+ failures: Restart after 60 seconds
- Reset counter: Every 24 hours

**Logs Location:**
- Windows Event Viewer → Windows Logs → Application
- Source: `QalitrackPlatformService`

**Installation Path:**
- Default: `C:\Program Files\Qalitrack`

---

## Linux Installation

### Prerequisites
- Ubuntu 20.04+ / Debian 11+ / RHEL 8+ / CentOS 8+
- .NET 8.0 SDK
- root privileges (sudo)

### Installation Steps

1. **Open Terminal**

2. **Navigate to the project directory**
   ```bash
   cd /path/to/Qalitrack
   ```

3. **Make scripts executable** (first time only)
   ```bash
   chmod +x install-linux-service.sh
   chmod +x uninstall-linux-service.sh
   chmod +x manage-linux-service.sh
   ```

4. **Run the installation script**
   ```bash
   sudo ./install-linux-service.sh
   ```

   The script will:
   - Build the application
   - Install systemd service file
   - Configure auto-restart on failure (5s delay)
   - Add firewall rule (if ufw is active)
   - Enable and start the service

### Service Management (Linux)

#### Using Management Script

**Check Status:**
```bash
sudo ./manage-linux-service.sh status
```

**Start Service:**
```bash
sudo ./manage-linux-service.sh start
```

**Stop Service:**
```bash
sudo ./manage-linux-service.sh stop
```

**Restart Service:**
```bash
sudo ./manage-linux-service.sh restart
```

**View Logs:**
```bash
# Last 50 log entries
sudo ./manage-linux-service.sh logs

# Follow logs in real-time
sudo ./manage-linux-service.sh tail
```

**Enable/Disable on Boot:**
```bash
# Enable
sudo ./manage-linux-service.sh enable

# Disable
sudo ./manage-linux-service.sh disable
```

#### Using Native systemctl Commands

```bash
# Start
sudo systemctl start qalitrack

# Stop
sudo systemctl stop qalitrack

# Restart
sudo systemctl restart qalitrack

# Status
sudo systemctl status qalitrack

# Enable on boot
sudo systemctl enable qalitrack

# Disable on boot
sudo systemctl disable qalitrack

# View logs
sudo journalctl -u qalitrack -f
sudo journalctl -u qalitrack -n 100
```

### Uninstall (Linux)

```bash
# Remove service only
sudo ./uninstall-linux-service.sh

# Remove service and files
sudo ./uninstall-linux-service.sh --remove-files
```

### Linux Service Configuration

**Auto-Restart Policy:**
- Restart delay: 5 seconds
- Max restarts: 5 times in 200 seconds
- Watchdog timeout: 60 seconds

**Startup Dependencies:**
- Starts after filesystem is ready (`local-fs.target`)
- **Does NOT wait for network** - starts immediately on boot
- Platform data works via serial when offline

**Logs Location:**
- systemd journal: `journalctl -u qalitrack`

**Installation Path:**
- Default: `/opt/qalitrack`

**Service File:**
- Location: `/etc/systemd/system/qalitrack.service`

---

## Configuration

You have **3 ways** to configure the service **without rebuilding**:

### Option 1: Web UI (Easiest) ⭐

After service is running, visit:
```
http://your-server-ip:5000/config.html
```

- Visual interface with forms
- Change platform IPs, camera IPs, serial ports
- Click "Save" → Click "Restart Service"
- Done!

### Option 2: REST API (Programmatic)

```bash
# Get current configuration
curl http://localhost:5000/api/configuration

# Update configuration (send JSON)
curl -X POST http://localhost:5000/api/configuration \
  -H "Content-Type: application/json" \
  -d @new-config.json

# Restart service to apply
curl -X POST http://localhost:5000/api/configuration/restart
```

### Option 3: Direct File Edit

**Two configuration files:**

1. **appsettings.json** - Build-time defaults (in project directory)
2. **qalitrack-config.json** - Runtime overrides (created in deployment directory)

**Runtime Configuration (Recommended):**

```bash
# Linux - Edit runtime config (created automatically on first run)
sudo nano /opt/qalitrack/qalitrack-config.json

# Windows - Edit runtime config
notepad "C:\Program Files\Qalitrack\qalitrack-config.json"

# Restart service after editing
sudo systemctl restart qalitrack  # Linux
Restart-Service QalitrackPlatformService  # Windows
```

**Example Configuration:**

```json
{
  "TcpListener": {
    "IpAddress": "172.16.1.243",
    "Port": 3002,
    "ReadTimeoutMs": 1000,
    "ReconnectDelayMs": 5000,
    "SerialPort": "AUTO",
    "BaudRate": 9600,
    "DataBits": 8,
    "Parity": "None",
    "StopBits": "One"
  },
  "CameraSettings": {
    "Cameras": [
      {
        "Id": "npr1",
        "Name": "NPR Camera 1",
        "IpAddress": "172.16.1.22",
        "Port": 554,
        "Username": "admin",
        "Password": "admin",
        "RtspPath": "/stream1",
        "Enabled": true,
        "SupportsSnapshot": true,
        "NprSettings": {
          "Enabled": true
        }
      }
    ],
    "ReconnectDelayMs": 5000,
    "FrameBufferSize": 100
  }
}
```

**Serial Port Configuration:**
- `AUTO` - Auto-detects USB serial devices (ttyUSB*, ttyACM* on Linux)
- `/dev/ttyUSB0` - Specific port on Linux
- `COM3` - Specific port on Windows
- Service tries TCP first, falls back to serial if TCP fails

### Logging Configuration

**Log Levels:** (from most to least verbose)
- `Trace` - Very detailed diagnostic information
- `Debug` - Internal system events
- `Information` - Normal operation events (default)
- `Warning` - Abnormal or unexpected events
- `Error` - Errors and exceptions
- `Critical` - Critical failures

**Change log level in `appsettings.json`:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Qalitrack": "Debug"  // Change to Debug for more detail
    }
  }
}
```

---

## API Endpoints

Once the service is running, it exposes the following endpoints:

### Weight Data (Platform Scale)
- **SSE Stream:** `http://localhost:5000/api/PlatformData/stream`
- **Test Page:** `http://localhost:5000/sse-test.html`

### Camera Data
- **SSE Stream:** `http://localhost:5000/api/Camera/data`
- **Video Stream:** `http://localhost:5000/api/Camera/stream`
- **Snapshot:** `http://localhost:5000/api/Camera/snapshot`
- **Status:** `http://localhost:5000/api/Camera/status`
- **Test Page:** `http://localhost:5000/camera-test.html`

### Plate Recognition (NPR)
- **SSE Stream:** `http://localhost:5000/api/Camera/plates`
- **Latest Plate:** `http://localhost:5000/api/Camera/plates/latest`

**Access from other machines:**
Replace `localhost` with the server's IP address.

---

## Multi-Site Deployment

### Scenario: Deploying to 10 Different Sites

Each site has different:
- Platform (weighbridge) IP addresses
- Camera IP addresses
- Serial port configurations

### Deployment Workflow

**Step 1: Build Once**
```bash
# Build for Linux
dotnet publish -c Release -r linux-x64 --self-contained

# Or build for Windows
dotnet publish -c Release -r win-x64 --self-contained

# Package the build
tar -czf qalitrack-linux.tar.gz -C bin/Release/net8.0/linux-x64/publish/ .
# Or: zip qalitrack-windows.zip bin/Release/net8.0/win-x64/publish/*
```

**Step 2: Deploy Same Binary to All Sites**
```bash
# Copy to each site
scp qalitrack-linux.tar.gz user@site1:/tmp/
scp qalitrack-linux.tar.gz user@site2:/tmp/
# ... repeat for all sites
```

**Step 3: Install at Each Site**
```bash
# On each site
sudo tar -xzf /tmp/qalitrack-linux.tar.gz -C /opt/qalitrack/
sudo ./install-linux-service.sh
```

**Step 4: Configure Each Site (No Rebuild!)**

**Option A: Use Web UI**
1. Visit `http://site-ip:5000/config.html`
2. Enter site-specific IPs for platforms and cameras
3. Click "Save & Restart"

**Option B: Edit Config File**
```bash
sudo nano /opt/qalitrack/qalitrack-config.json
# Change IPs for this site
sudo systemctl restart qalitrack
```

**Option C: Use API**
```bash
# Upload site-specific config via API
curl -X POST http://localhost:5000/api/configuration \
  -H "Content-Type: application/json" \
  -d @site1-config.json

curl -X POST http://localhost:5000/api/configuration/restart
```

### Key Benefits

✅ **Build once** - Same binary for all sites
✅ **No recompilation** - Change IPs anytime without rebuilding
✅ **Easy updates** - Update all sites with same binary
✅ **Site-specific configs** - Each site has its own qalitrack-config.json
✅ **Version consistency** - All sites run identical code

### Example: 10-Site Deployment Timeline

| Time | Activity |
|------|----------|
| Day 1 | Build application once |
| Day 2 | Deploy binary to all 10 sites |
| Day 3-5 | Configure each site (via Web UI or config files) |
| **Total** | **3-5 days** vs weeks of site-specific builds |

---

## Troubleshooting

### Windows

**Service won't start:**
1. Check Event Viewer for errors
2. Verify .NET 8.0 Runtime is installed
3. Check if port 5000 is available: `netstat -ano | findstr :5000`

**Cannot connect to API:**
1. Check firewall: `netsh advfirewall firewall show rule name="Qalitrack Platform Service"`
2. Verify service is running: `Get-Service QalitrackPlatformService`

**View detailed logs:**
```powershell
Get-EventLog -LogName Application -Source QalitrackPlatformService -Newest 100 | Format-List
```

### Linux

**Service won't start:**
1. Check logs: `sudo journalctl -u qalitrack -n 100 --no-pager`
2. Verify .NET 8.0 Runtime is installed: `dotnet --version`
3. Check if port 5000 is available: `sudo netstat -tlnp | grep :5000`

**Permission denied errors:**
1. Ensure service runs as root: Check service file
2. Check file permissions: `ls -la /opt/qalitrack/`
3. Serial port access: Service needs root and dialout group membership

**Serial port not detected:**
```bash
# List available serial ports
ls -la /dev/ttyUSB* /dev/ttyACM*

# Check permissions
sudo usermod -a -G dialout root

# Test serial port manually
sudo screen /dev/ttyUSB0 9600
# Press Ctrl+A then K to exit

# View service logs for serial connection
sudo journalctl -u qalitrack -f | grep -i serial
```

**Cannot connect to API:**
1. Check firewall: `sudo ufw status`
2. Verify service is running: `sudo systemctl status qalitrack`

**View detailed logs:**
```bash
# All logs for today
sudo journalctl -u qalitrack --since today

# Follow logs with filtering
sudo journalctl -u qalitrack -f | grep -i error

# Export logs to file
sudo journalctl -u qalitrack --since "2 hours ago" > qalitrack.log
```

---

## Security Notes

### Running as Administrator/root

The service runs with elevated privileges because it needs:
- Access to serial ports (weighbridge communication)
- Binding to network ports
- Access to camera devices
- System-level resource access

### Network Security

- Service listens on `0.0.0.0:5000` (all interfaces)
- Intended for internal network use only
- No authentication on API endpoints
- **Recommendation:** Use firewall to restrict access to trusted IPs

### Hardening (Linux)

The systemd service includes security options:
```ini
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
```

To further harden, edit `/etc/systemd/system/qalitrack.service` and add:
```ini
ReadOnlyPaths=/usr /etc
InaccessiblePaths=/home
```

---

## Performance

### Resource Usage (Typical)
- **CPU:** 1-5%
- **Memory:** 50-150 MB
- **Network:** Minimal (only when streaming)

### Monitoring

**Windows:**
```powershell
# Check resource usage
Get-Process -Name Qalitrack | Select-Object CPU,WorkingSet
```

**Linux:**
```bash
# Check resource usage
systemctl status qalitrack

# Detailed process info
top -p $(pgrep Qalitrack)
```

---

## Support

For issues or questions:
1. Check service logs
2. Verify configuration in `appsettings.json`
3. Ensure all external devices (cameras, scales) are accessible
4. Check network connectivity

**Log Collection:**

Windows:
```powershell
.\manage-windows-service.ps1 logs > qalitrack-logs.txt
```

Linux:
```bash
sudo journalctl -u qalitrack --since "1 hour ago" > qalitrack-logs.txt
```
