# QaliTrack Kiosk - Windows Build Guide

## Prerequisites

### Required Software
1. **Flutter SDK** (latest stable)
   - Download from: https://docs.flutter.dev/get-started/install/windows
   - Add Flutter to PATH

2. **Visual Studio 2022** (Community Edition is sufficient)
   - Download from: https://visualstudio.microsoft.com/downloads/
   - Required workloads:
     - Desktop development with C++
     - Windows 10/11 SDK (latest version)

3. **Git for Windows**
   - Download from: https://git-scm.com/download/win

### System Requirements
- Windows 10 version 1903 or higher (64-bit)
- At least 4 GB RAM (8 GB recommended)
- 10 GB free disk space

## Build Instructions

### 1. Clone and Setup
```cmd
git clone <repository-url>
cd qalitrackservices\apps\qalitrack_kiosk
flutter doctor -v
```

### 2. Enable Windows Desktop Support
```cmd
flutter config --enable-windows-desktop
flutter doctor -v
```

### 3. Install Dependencies
```cmd
flutter pub get
```

### 4. Build for Windows

#### Debug Build (for testing)
```cmd
flutter build windows --debug
```

#### Release Build (for production)
```cmd
flutter build windows --release
```

#### Profile Build (for performance testing)
```cmd
flutter build windows --profile
```

### 5. Run on Windows
```cmd
flutter run -d windows
```

## Build Output

The built executable will be located at:
```
build\windows\runner\Release\qalitrack_kiosk.exe
```

### Distribution Files
The following files are needed for distribution:
- `qalitrack_kiosk.exe` - Main executable
- `flutter_windows.dll` - Flutter runtime
- `data\` - Flutter assets and resources
- Any additional DLLs in the Release folder

## Kiosk Mode Configuration

### Windows Kiosk Mode Setup
1. **Create Kiosk User Account**
```cmd
net user KioskUser /add
net user KioskUser /passwordreq:no
net localgroup users KioskUser /delete
```

2. **Configure Shell Launcher**
   - Use Windows 10/11 Kiosk Mode settings
   - Or use Shell Launcher to replace Windows Shell

3. **Auto-start Configuration**
Create startup script in `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\`

### Registry Configuration (Optional)
```reg
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System]
"DisableTaskMgr"=dword:00000001
"DisableRegistryTools"=dword:00000001

[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System]
"DisableCMD"=dword:00000001
```

## Hardware Requirements

### Recommended Specifications
- **CPU**: Intel i5 or AMD Ryzen 5 (or better)
- **RAM**: 8 GB minimum
- **Storage**: 256 GB SSD
- **Camera**: USB 3.0 camera with 1080p support
- **Scale Interface**: Serial/USB scale connection
- **Display**: 1920x1080 minimum (touchscreen recommended)

### Supported Hardware
- **Cameras**: Any DirectShow compatible camera
- **Scales**: Serial (RS232/485) or USB scales
- **Printers**: Any Windows-compatible receipt printer

## Network Configuration

### Firewall Rules
The kiosk needs to communicate with QaliTrack services:
```
Outbound Rules:
- Port 7001: User Service
- Port 7004: Driver Service  
- Port 7005: Transaction Service
- Port 7006: Weight Data Service
- Port 80/443: HTTP/HTTPS
```

### Service Discovery
The app automatically discovers QaliTrack services on the local network.
Manual configuration available in admin settings (Ctrl+Shift+Alt+A).

## Troubleshooting

### Common Issues

1. **Flutter Doctor Issues**
```cmd
flutter doctor --verbose
flutter config --enable-windows-desktop
```

2. **Build Errors**
```cmd
flutter clean
flutter pub get
flutter build windows --verbose
```

3. **Camera Not Working**
   - Check camera permissions in Windows Privacy settings
   - Verify camera is DirectShow compatible
   - Test with Windows Camera app first

4. **Scale Connection Issues**
   - Verify COM port settings
   - Check scale baud rate and data format
   - Test with scale manufacturer software

### Performance Optimization

1. **Release Build Optimization**
```cmd
flutter build windows --release --split-debug-info=debug-info --obfuscate
```

2. **Reduce Bundle Size**
```cmd
flutter build windows --tree-shake-icons --split-debug-info=debug-info
```

## Development

### Hot Reload (Debug Mode)
```cmd
flutter run -d windows
# Press 'r' to hot reload
# Press 'R' to hot restart
```

### Debugging
```cmd
flutter run -d windows --debug
```

### Testing
```cmd
flutter test
flutter integration_test integration_test/app_test.dart -d windows
```

## Deployment

### Manual Deployment
1. Copy entire `build\windows\runner\Release\` folder to target machine
2. Install Visual C++ Redistributable if needed
3. Configure Windows for kiosk mode
4. Set up auto-start

### Installer Creation (Optional)
Use tools like:
- Inno Setup
- NSIS
- WiX Toolset

## Security Considerations

1. **Disable Windows Features**
   - Task Manager
   - Registry Editor
   - Command Prompt
   - File Explorer (in kiosk mode)

2. **Network Security**
   - Configure Windows Firewall
   - Use VPN for remote management
   - Enable Windows Update for security patches

3. **Physical Security**
   - Lock down physical access
   - Disable USB ports if not needed
   - Secure keyboard shortcuts

## Support

For technical support:
1. Check Flutter Windows documentation
2. Review QaliTrack service logs
3. Contact development team

---

**Note**: This build process creates a native Windows desktop application optimized for kiosk environments with face detection, weighing integration, and QaliTrack service connectivity.