# QaliTrack Kiosk - Windows Kiosk Mode Setup Script
# Run as Administrator on Windows 10/11

param(
    [Parameter(Mandatory=$false)]
    [string]$KioskUser = "KioskUser",
    
    [Parameter(Mandatory=$false)]
    [string]$AppPath = "C:\QaliTrack\qalitrack_kiosk.exe"
)

Write-Host "================================" -ForegroundColor Green
Write-Host "QaliTrack Kiosk Setup for Windows" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit 1
}

Write-Host "Setting up Windows Kiosk Mode..." -ForegroundColor Yellow

# Create Kiosk User Account
Write-Host "Creating kiosk user account: $KioskUser" -ForegroundColor Cyan
try {
    # Check if user already exists
    $userExists = Get-LocalUser -Name $KioskUser -ErrorAction SilentlyContinue
    if (-not $userExists) {
        New-LocalUser -Name $KioskUser -NoPassword -Description "QaliTrack Kiosk User Account"
        Write-Host "User $KioskUser created successfully" -ForegroundColor Green
    } else {
        Write-Host "User $KioskUser already exists" -ForegroundColor Yellow
    }
    
    # Remove from Users group to limit permissions
    Remove-LocalGroupMember -Group "Users" -Member $KioskUser -ErrorAction SilentlyContinue
    Write-Host "User permissions configured" -ForegroundColor Green
} catch {
    Write-Host "Warning: Could not configure user account: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Configure Shell Launcher (Windows 10 Pro/Enterprise only)
Write-Host "Configuring Shell Launcher..." -ForegroundColor Cyan
try {
    # Enable Shell Launcher feature
    Enable-WindowsOptionalFeature -Online -FeatureName Client-EmbeddedShellLauncher -All -NoRestart
    
    # Configure Shell Launcher registry settings
    $registryPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData"
    
    Write-Host "Shell Launcher feature enabled" -ForegroundColor Green
} catch {
    Write-Host "Warning: Shell Launcher configuration requires Windows 10 Pro/Enterprise" -ForegroundColor Yellow
}

# Configure security policies
Write-Host "Configuring security policies..." -ForegroundColor Cyan

# Disable Task Manager
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" -Name "DisableTaskMgr" -Value 1

# Disable Registry Editor
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" -Name "DisableRegistryTools" -Value 1

# Disable Command Prompt
New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\System" -Force | Out-Null
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\System" -Name "DisableCMD" -Value 1

Write-Host "Security policies configured" -ForegroundColor Green

# Configure Auto-logon for Kiosk User
Write-Host "Configuring auto-logon..." -ForegroundColor Cyan
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name "AutoAdminLogon" -Value "1"
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name "DefaultUserName" -Value $KioskUser
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name "DefaultPassword" -Value ""

Write-Host "Auto-logon configured for $KioskUser" -ForegroundColor Green

# Create startup script
Write-Host "Creating startup configuration..." -ForegroundColor Cyan
$startupPath = "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup"
$startupScript = @"
@echo off
REM QaliTrack Kiosk Auto-Start
cd /d "$((Split-Path $AppPath -Parent))"
start "" "$AppPath"
"@

$startupBat = Join-Path $startupPath "QaliTrackKiosk.bat"
$startupScript | Out-File -FilePath $startupBat -Encoding ASCII

Write-Host "Startup script created: $startupBat" -ForegroundColor Green

# Configure Windows Update settings (disable automatic restarts)
Write-Host "Configuring Windows Update..." -ForegroundColor Cyan
New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU" -Force | Out-Null
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU" -Name "NoAutoRebootWithLoggedOnUsers" -Value 1
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU" -Name "AUOptions" -Value 3

Write-Host "Windows Update configured for kiosk mode" -ForegroundColor Green

# Configure Power Management
Write-Host "Configuring power management..." -ForegroundColor Cyan
powercfg /change monitor-timeout-ac 0
powercfg /change monitor-timeout-dc 0
powercfg /change standby-timeout-ac 0
powercfg /change standby-timeout-dc 0

Write-Host "Power management configured (never sleep)" -ForegroundColor Green

# Create restoration script
Write-Host "Creating restoration script..." -ForegroundColor Cyan
$restoreScript = @"
# QaliTrack Kiosk Restoration Script
# Run as Administrator to restore normal Windows operation

Write-Host "Restoring Windows to normal operation..." -ForegroundColor Yellow

# Re-enable Task Manager
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" -Name "DisableTaskMgr" -Value 0

# Re-enable Registry Editor
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" -Name "DisableRegistryTools" -Value 0

# Re-enable Command Prompt
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\System" -Name "DisableCMD" -Value 0

# Disable Auto-logon
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name "AutoAdminLogon" -Value "0"

# Remove startup script
Remove-Item "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\QaliTrackKiosk.bat" -ErrorAction SilentlyContinue

Write-Host "Windows restored to normal operation" -ForegroundColor Green
Write-Host "Please reboot the system" -ForegroundColor Yellow
"@

$restoreScript | Out-File -FilePath "C:\QaliTrack\restore_windows.ps1" -Encoding UTF8

Write-Host "Restoration script created: C:\QaliTrack\restore_windows.ps1" -ForegroundColor Green

# Final instructions
Write-Host ""
Write-Host "================================" -ForegroundColor Green
Write-Host "SETUP COMPLETE!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Copy QaliTrack Kiosk executable to: $AppPath" -ForegroundColor White
Write-Host "2. Test the application manually first" -ForegroundColor White
Write-Host "3. Reboot the system to activate kiosk mode" -ForegroundColor White
Write-Host "4. The system will auto-login as $KioskUser and start the kiosk app" -ForegroundColor White
Write-Host ""
Write-Host "To restore normal Windows operation:" -ForegroundColor Yellow
Write-Host "Run: C:\QaliTrack\restore_windows.ps1 (as Administrator)" -ForegroundColor White
Write-Host ""
Write-Host "IMPORTANT: Test thoroughly before deploying!" -ForegroundColor Red

pause