# Qalitrack Platform Service - Installation Guide

This guide shows how to install and run the Qalitrack Platform Service as a system service on both Windows and Linux.

---

## 📦 Prerequisites

### Both Platforms
1. Install .NET 8 SDK (or .NET 6+)
2. Compile the application

### Compilation
```bash
# Publish as self-contained (includes runtime)
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish-linux

# For Windows
dotnet publish -c Release -r win-x64 --self-contained -o ./publish-windows

# Or framework-dependent (requires .NET installed)
dotnet publish -c Release -o ./publish
```

---

## 🐧 Linux Setup (systemd)

### 1. Required NuGet Package
Add to your `.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Hosting.Systemd" Version="8.0.0" />
</ItemGroup>
```

### 2. Create systemd Service File
Create `/etc/systemd/system/qalitrack.service`:

```ini
[Unit]
Description=Qalitrack Platform Data Service
After=network.target

[Service]
Type=notify
# Path to your published executable
WorkingDirectory=/opt/qalitrack
ExecStart=/opt/qalitrack/Qalitrack

# Run as specific user (recommended)
User=qalitrack
Group=qalitrack

# Restart policy
Restart=always
RestartSec=10

# Logging
StandardOutput=journal
StandardError=journal
SyslogIdentifier=qalitrack

# Security hardening (optional)
NoNewPrivileges=true
PrivateTmp=true

# Serial port access (if needed)
SupplementaryGroups=dialout

[Install]
WantedBy=multi-user.target
```

### 3. Installation Steps

```bash
# 1. Create user (if running as dedicated user)
sudo useradd -r -s /bin/false qalitrack
sudo usermod -a -G dialout qalitrack  # For serial port access

# 2. Copy published files
sudo mkdir -p /opt/qalitrack
sudo cp -r ./publish-linux/* /opt/qalitrack/
sudo chown -R qalitrack:qalitrack /opt/qalitrack
sudo chmod +x /opt/qalitrack/Qalitrack

# 3. Copy configuration
sudo cp appsettings.json /opt/qalitrack/
sudo chown qalitrack:qalitrack /opt/qalitrack/appsettings.json

# 4. Install and enable service
sudo systemctl daemon-reload
sudo systemctl enable qalitrack.service
sudo systemctl start qalitrack.service

# 5. Check status
sudo systemctl status qalitrack.service
```

### 4. Service Management Commands

```bash
# Start service
sudo systemctl start qalitrack

# Stop service
sudo systemctl stop qalitrack

# Restart service
sudo systemctl restart qalitrack

# Check status
sudo systemctl status qalitrack

# View logs
sudo journalctl -u qalitrack -f

# View recent logs
sudo journalctl -u qalitrack -n 100

# Disable auto-start
sudo systemctl disable qalitrack

# Enable auto-start
sudo systemctl enable qalitrack
```

### 5. Firewall Configuration

```bash
# UFW (Ubuntu/Debian)
sudo ufw allow 5000/tcp

# firewalld (RHEL/CentOS/Fedora)
sudo firewall-cmd --permanent --add-port=5000/tcp
sudo firewall-cmd --reload
```

---

## 🪟 Windows Setup (Windows Service)

### 1. Required NuGet Package
Add to your `.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Hosting.WindowsServices" Version="8.0.0" />
</ItemGroup>
```

### 2. Installation Using sc.exe

```powershell
# Run PowerShell as Administrator

# 1. Copy published files to permanent location
$servicePath = "C:\Services\Qalitrack"
New-Item -ItemType Directory -Path $servicePath -Force
Copy-Item -Path ".\publish-windows\*" -Destination $servicePath -Recurse

# 2. Create Windows Service
sc.exe create QalitrackPlatformService `
    binPath= "C:\Services\Qalitrack\Qalitrack.exe" `
    start= auto `
    DisplayName= "Qalitrack Platform Data Service"

# 3. Set service description
sc.exe description QalitrackPlatformService "Collects and streams platform weight data via TCP/Serial"

# 4. Configure recovery options (auto-restart on