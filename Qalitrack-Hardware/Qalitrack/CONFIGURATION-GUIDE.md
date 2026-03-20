# Qalitrack Configuration Guide

## Overview

Qalitrack is a platform service that provides:
- **Weight Data Streaming** (TCP/Serial from weighbridge)
- **Camera Video Streams** (RTSP/MJPEG)
- **NPR Plate Recognition** (Real-time via SSE)

**Key Features:**
- ✅ **No rebuild needed** for different IPs/cameras
- ✅ **Offline mode** - Platform data via serial when internet is down
- ✅ **3 configuration methods** - Web UI, REST API, or direct file edit
- ✅ **Runtime configuration** - Changes take effect after service restart

---

## Building the Application

### Quick Build Commands

**Linux Build:**
```bash
cd /path/to/Qalitrack
dotnet publish -c Release -r linux-x64 --self-contained
# Output: bin/Release/net8.0/linux-x64/publish/
```

**Windows Build:**
```powershell
cd C:\path\to\Qalitrack
dotnet publish -c Release -r win-x64 --self-contained
# Output: bin\Release\net8.0\win-x64\publish\
```

**Cross-Platform Build (from any OS):**
```bash
# Build both from one machine
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r win-x64 --self-contained
```

**Important:**
- Build **once** with default settings
- Deploy same binary to all sites
- **No rebuild needed** for different configurations
- Each site gets its own runtime config file

---

## Configuration Methods

You have **3 ways** to configure without rebuilding:

### Method 1: Web UI (Easiest) ⭐

After service is running, open browser:
```
http://your-server-ip:5000/config.html
```

**Steps:**
1. View current configuration
2. Edit platform IPs, camera IPs, serial ports
3. Click "Save Configuration"
4. Click "Restart Service"
5. Done!

### Method 2: REST API (Programmatic)

```bash
# Get current configuration
curl http://localhost:5000/api/configuration

# Get configuration file path
curl http://localhost:5000/api/configuration/path

# Update configuration
curl -X POST http://localhost:5000/api/configuration \
  -H "Content-Type: application/json" \
  -d @new-config.json

# Restart service to apply changes
curl -X POST http://localhost:5000/api/configuration/restart
```

### Method 3: Direct File Edit

**Two configuration files exist:**

1. **appsettings.json** - Build-time defaults (in source code)
2. **qalitrack-config.json** - Runtime overrides (in deployment directory)

**Runtime Configuration (Recommended):**

```bash
# Linux - Edit runtime config
sudo nano /opt/qalitrack/qalitrack-config.json

# Windows - Edit runtime config
notepad "C:\Program Files\Qalitrack\qalitrack-config.json"

# Restart service
sudo systemctl restart qalitrack  # Linux
Restart-Service QalitrackPlatformService  # Windows
```

**How It Works:**
- First run: Service creates `qalitrack-config.json` with defaults from `appsettings.json`
- Edit `qalitrack-config.json` with site-specific settings
- Service reads `qalitrack-config.json` on startup
- No need to touch `appsettings.json` after deployment

---

## Configuration Structure

### Complete Example (appsettings.json)

```json
{
  "TcpListener": {
    "IpAddress": "172.16.1.243",
    "Port": 3002,
    "ReadTimeoutMs": 1000,
    "ReconnectDelayMs": 5000
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
      },
      {
        "Id": "npr2",
        "Name": "NPR Camera 2",
        "IpAddress": "172.16.1.228",
        "Port": 554,
        "Username": "admin",
        "Password": "admin",
        "RtspPath": "/stream1",
        "Enabled": true,
        "SupportsSnapshot": true,
        "NprSettings": {
          "Enabled": true
        }
      },
      {
        "Id": "hikvision1",
        "Name": "Hikvision Camera 1",
        "IpAddress": "172.16.1.20",
        "Port": 554,
        "Username": "admin",
        "Password": "password",
        "RtspPath": "/Streaming/Channels/101",
        "Enabled": true,
        "SupportsSnapshot": true
      }
    ],
    "ReconnectDelayMs": 5000,
    "FrameBufferSize": 100
  }
}
```

---

## Configuration Sections

### 1. TcpListener (Weighbridge Scale)

Connects to weighbridge scale via TCP or Serial with automatic fallback.

| Setting | Description | Example | Default |
|---------|-------------|---------|---------|
| `IpAddress` | TCP IP address of weighbridge | `172.16.1.243` | Required |
| `Port` | TCP port | `3002` | `3002` |
| `ReadTimeoutMs` | Read timeout in milliseconds | `1000` | `1000` |
| `ReconnectDelayMs` | Reconnection delay on failure | `5000` | `5000` |
| `SerialPort` | Serial port (AUTO or specific) | `AUTO` | Not set |
| `BaudRate` | Serial baud rate | `9600` | `9600` |
| `DataBits` | Serial data bits | `8` | `8` |
| `Parity` | Serial parity | `None` | `None` |
| `StopBits` | Serial stop bits | `One` | `One` |

**Connection Behavior:**
1. Service tries **TCP first** (if IpAddress is configured)
2. If TCP fails, tries **Serial port** (if SerialPort is configured)
3. Keeps retrying every `ReconnectDelayMs` milliseconds
4. Works offline via serial when internet is unavailable

**Serial Port Configuration:**
- `AUTO` - Auto-detects USB serial devices (recommended)
  - Linux: Prefers `/dev/ttyUSB*` or `/dev/ttyACM*`
  - Windows: Scans available COM ports
- `/dev/ttyUSB0` - Specific port on Linux
- `COM3` - Specific port on Windows

**Example with Serial Fallback:**
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
  }
}
```

**Offline Mode:**
- Platform weight data continues via serial when internet is down
- Cameras will be offline (require network)
- Service stays running and auto-restarts on boot
- When internet returns, TCP reconnects automatically

---

### 2. CameraSettings (Multiple Cameras)

Configure multiple cameras with different types.

#### Camera Properties

| Property | Description | Required |
|----------|-------------|----------|
| `Id` | Unique camera identifier | Yes |
| `Name` | Display name | Yes |
| `IpAddress` | Camera IP address | Yes |
| `Port` | RTSP port (usually 554) | Yes |
| `Username` | Authentication username | Yes |
| `Password` | Authentication password | Yes |
| `RtspPath` | RTSP stream path | Yes |
| `Enabled` | Enable/disable camera | Yes |
| `SupportsSnapshot` | Camera supports snapshots | No |
| `NprSettings` | NPR configuration (optional) | No |

#### NPR Settings (Optional)

For cameras with plate recognition capability:

```json
"NprSettings": {
  "Enabled": true
}
```

**How NPR Works:**
- NPR cameras **push plate data** to your service via HTTP webhook
- Service receives plate events and broadcasts via SSE stream
- No polling required - real-time push notifications

---

## Camera Types

### Type 1: NPR Cameras (Plate Recognition)

**Purpose:** License plate recognition + video stream

**Configuration:**
```json
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
```

**Setup:**
1. Add camera to `appsettings.json`
2. Configure camera to push plates to: `http://<your-service-ip>:5000/webhook`
3. Restart service
4. Listen to SSE stream: `http://<your-service-ip>:5000/api/PlatformData/plates/stream`

---

### Type 2: Regular IP Cameras (Video Only)

**Purpose:** Video streaming only (no NPR)

**Configuration:**
```json
{
  "Id": "hikvision1",
  "Name": "Hikvision Camera 1",
  "IpAddress": "172.16.1.20",
  "Port": 554,
  "Username": "admin",
  "Password": "password",
  "RtspPath": "/Streaming/Channels/101",
  "Enabled": true,
  "SupportsSnapshot": true
}
```

**Available Endpoints:**
- Video stream: `http://localhost:5000/api/Camera/hikvision1/stream`
- Snapshot: `http://localhost:5000/api/Camera/hikvision1/snapshot`
- Status: `http://localhost:5000/api/Camera/hikvision1/status`

---

## Adding New Cameras

### Adding an NPR Camera

1. **Edit appsettings.json:**

```json
{
  "Id": "npr3",
  "Name": "NPR Camera 3",
  "IpAddress": "172.16.1.50",
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
```

2. **Restart service**

3. **Configure camera's HTTP push:**
   - URL: `http://<your-service-ip>:5000/webhook`
   - Port: `5000`
   - Method: `POST`

4. **Test:**
```bash
curl -N http://localhost:5000/api/PlatformData/plates/stream
```

### Adding a Regular Camera

1. **Edit appsettings.json** (add to Cameras array)

2. **Restart service**

3. **Test:**
```bash
curl http://localhost:5000/api/Camera/<camera-id>/snapshot -o snapshot.jpg
```

---

## Available Endpoints

### Weight Data (SSE Stream)

```bash
# Listen for weight data
curl -N http://localhost:5000/api/PlatformData/stream
```

### Plate Data (SSE Stream - All NPR Cameras)

```bash
# Listen for plate detections from ALL NPR cameras
curl -N http://localhost:5000/api/PlatformData/plates/stream
```

**Output Example:**
```json
{
  "cameraId": "npr1",
  "cameraName": "NPR Camera 1",
  "plateNumber": "ABC123",
  "confidence": 0.95,
  "timestamp": "2025-11-07T12:30:00Z",
  "vehicleColor": "White",
  "source": "webhook"
}
```

### Camera Endpoints

```bash
# List all cameras
curl http://localhost:5000/api/Camera/cameras

# Get snapshot
curl http://localhost:5000/api/Camera/npr1/snapshot -o snapshot.jpg

# Live video stream (MJPEG)
curl http://localhost:5000/api/Camera/npr1/stream

# Camera status
curl http://localhost:5000/api/Camera/npr1/status
```

---

## Service Architecture

### Active Services

| Service | Purpose |
|---------|---------|
| `PlatformDataService` | Connects to weighbridge (TCP/Serial) |
| `CameraStreamService` | Manages camera video streams |
| `DataStreamService` | SSE broadcasting for weight data |
| `CameraDataStreamService` | SSE broadcasting for camera frames |
| `PlateDataStreamService` | SSE broadcasting for plate detections |

### Data Flow

```
Weighbridge → TCP/Serial → PlatformDataService → DataStreamService → SSE Stream
Cameras → RTSP → CameraStreamService → Video/Snapshots
NPR Cameras → HTTP Push → PlateDataStreamService → SSE Stream
```

---

## Troubleshooting

### Camera Not Streaming

1. **Check camera is enabled:**
```bash
curl http://localhost:5000/api/Camera/cameras | jq
```

2. **Verify RTSP URL:**
```bash
ffplay rtsp://admin:password@172.16.1.22:554/stream1
```

3. **Check service logs:**
```bash
# Linux
sudo journalctl -u qalitrack -f

# Windows
Get-EventLog -LogName Application -Source QalitrackPlatformService -Newest 50
```

### NPR Plates Not Appearing

1. **Verify camera is pushing to correct URL:**
   - Camera webhook: `http://<your-service-ip>:5000/webhook`

2. **Check camera IP matches configuration:**
   - Service identifies cameras by IP address in webhook payload

3. **Test SSE stream:**
```bash
curl -N http://localhost:5000/api/PlatformData/plates/stream
```

4. **Check service logs for webhook messages**

### Weight Data Not Streaming

1. **Check TCP connection:**
```bash
nc -zv 172.16.1.243 3002
```

2. **If TCP fails, check Serial:**
```bash
ls -la /dev/ttyUSB0
# Service needs root/admin to access serial port
```

3. **Test SSE stream:**
```bash
curl -N http://localhost:5000/api/PlatformData/stream
```

---

## Best Practices

### 1. Camera Naming Convention

Use descriptive IDs:
- ✅ `npr1`, `npr2`, `entry_gate`, `exit_gate`
- ❌ `camera1`, `cam`, `c1`

### 2. Security

**Change default passwords:**
```json
"Username": "admin",
"Password": "StrongPassword123!"  // Change this!
```

### 3. Network Configuration

- Keep cameras on same subnet as service
- Use static IPs for cameras
- Configure firewall to allow port 5000

### 4. Backup Configuration

```bash
# Linux
sudo cp appsettings.json appsettings.json.backup

# Windows
copy appsettings.json appsettings.json.backup
```

### 5. Testing After Changes

After editing configuration:
1. Restart service
2. Check `http://localhost:5000` to verify cameras loaded
3. Test each camera endpoint
4. Monitor SSE streams

---

## Multi-Site Deployment

### Scenario: 10 Weighbridge Sites

**Each site has:**
- Different weighbridge IPs (TCP or Serial)
- Different camera IPs
- Different network configurations
- **Same service binary**

### Deployment Workflow

**Step 1: Build Once for All Sites**

```bash
# Build for Linux (if deploying to Linux servers)
dotnet publish -c Release -r linux-x64 --self-contained

# Build for Windows (if deploying to Windows servers)
dotnet publish -c Release -r win-x64 --self-contained

# Package the build
tar -czf qalitrack-v1.0-linux.tar.gz -C bin/Release/net8.0/linux-x64/publish/ .
# Or for Windows: zip qalitrack-v1.0-windows.zip bin/Release/net8.0/win-x64/publish/*
```

**Step 2: Deploy Same Binary to All Sites**

```bash
# Transfer to sites (example for Linux)
scp qalitrack-v1.0-linux.tar.gz user@site1:/tmp/
scp qalitrack-v1.0-linux.tar.gz user@site2:/tmp/
scp qalitrack-v1.0-linux.tar.gz user@site3:/tmp/
# ... repeat for all 10 sites
```

**Step 3: Install at Each Site**

```bash
# On each Linux site
ssh user@site1
sudo mkdir -p /opt/qalitrack
sudo tar -xzf /tmp/qalitrack-v1.0-linux.tar.gz -C /opt/qalitrack/
sudo ./install-linux-service.sh
```

**Step 4: Configure Each Site (No Rebuild!)**

Choose one of three methods:

**Method A: Web UI (Fastest)**
1. Visit `http://site-ip:5000/config.html`
2. Enter site-specific settings:
   - Platform IP or Serial port
   - Camera IPs
   - Serial port settings
3. Click "Save & Restart"

**Method B: Direct File Edit**
```bash
# On each site
sudo nano /opt/qalitrack/qalitrack-config.json

# Example for Site 1 (has internet, uses TCP)
{
  "TcpListener": {
    "IpAddress": "172.16.1.10",
    "Port": 3002,
    "SerialPort": "AUTO"
  },
  "CameraSettings": {
    "Cameras": [
      {"Id": "npr1", "IpAddress": "172.16.1.11", ...}
    ]
  }
}

# Example for Site 2 (no internet, uses serial only)
{
  "TcpListener": {
    "IpAddress": "",
    "SerialPort": "/dev/ttyUSB0",
    "BaudRate": 9600
  },
  "CameraSettings": {
    "Cameras": []
  }
}

# Restart service
sudo systemctl restart qalitrack
```

**Method C: API Configuration**
```bash
# Prepare site-specific configs
# site1-config.json, site2-config.json, etc.

# Deploy via API
curl -X POST http://site1:5000/api/configuration \
  -H "Content-Type: application/json" \
  -d @site1-config.json

curl -X POST http://site1:5000/api/configuration/restart
```

### Key Benefits

✅ **Build once** - Same binary for all 10 sites
✅ **No recompilation** - Change IPs anytime without rebuilding
✅ **Version consistency** - All sites run identical code
✅ **Easy updates** - Update all sites with same binary
✅ **Site-specific configs** - Each site has its own qalitrack-config.json
✅ **Offline support** - Sites without internet use serial communication

### Deployment Timeline Example

| Day | Activity | Sites Covered |
|-----|----------|---------------|
| 1 | Build application once | N/A |
| 2 | Deploy binary to all sites | 10 sites |
| 3 | Configure sites 1-5 (Web UI) | 5 sites |
| 4 | Configure sites 6-10 (Web UI) | 5 sites |
| 5 | Testing and verification | All 10 sites |
| **Total** | **5 days** | **10 sites deployed** |

Compare to: 10 site-specific builds = **weeks of work**

---

## Summary

### Build Once, Deploy Everywhere

✅ **Build for Linux and Windows** - Cross-platform support
✅ **No rebuild needed** - Change IPs/settings without recompiling
✅ **3 configuration methods** - Web UI, REST API, or direct file edit
✅ **Runtime configuration** - qalitrack-config.json overrides defaults

### Connectivity Options

✅ **TCP + Serial fallback** - Platform data works offline via serial
✅ **Auto-reconnect** - Cameras reconnect when internet returns
✅ **Multi-camera support** - NPR + regular IP cameras
✅ **Real-time SSE streams** - Weight data + plate recognition

### Deployment Features

✅ **Same binary for all sites** - Deploy once, configure per site
✅ **Auto-starts on boot** - Even without internet
✅ **Auto-restarts on crash** - Built-in resilience
✅ **Web-based configuration** - Easy setup via browser

### Data Streaming

✅ **Platform weight data** - TCP/Serial with SSE streaming
✅ **Camera video streams** - RTSP/MJPEG live streaming
✅ **Plate recognition** - Real-time NPR via SSE
✅ **HTTP push webhooks** - No polling required

**Build. Deploy. Configure. Done.** 🚀
