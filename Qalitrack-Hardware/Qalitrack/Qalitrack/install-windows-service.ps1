#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Installs Qalitrack Platform Service as a Windows Service
.DESCRIPTION
    This script builds the application, creates the Windows Service,
    and configures it to run as LocalSystem with auto-restart on failure.
#>

param(
    [string]$ServiceName = "QalitrackPlatformService",
    [string]$DisplayName = "Qalitrack Platform Data Service",
    [string]$Description = "Qalitrack weighbridge platform data collection and camera service with NPR",
    [string]$InstallPath = "C:\Program Files\Qalitrack"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Qalitrack Windows Service Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Error "This script must be run as Administrator!"
    exit 1
}

Write-Host "[1/7] Checking for existing service..." -ForegroundColor Yellow
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "    Service already exists. Stopping and removing..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2

    # Use sc.exe to delete the service
    sc.exe delete $ServiceName
    Start-Sleep -Seconds 2
    Write-Host "    ✓ Existing service removed" -ForegroundColor Green
}

Write-Host "[2/7] Building application..." -ForegroundColor Yellow
$publishPath = Join-Path $PSScriptRoot "bin\Release\net8.0\win-x64\publish"

# Build and publish
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}
Write-Host "    ✓ Build completed" -ForegroundColor Green

Write-Host "[3/7] Creating installation directory..." -ForegroundColor Yellow
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}

# Copy published files
Copy-Item -Path "$publishPath\*" -Destination $InstallPath -Recurse -Force
Write-Host "    ✓ Files copied to $InstallPath" -ForegroundColor Green

Write-Host "[4/7] Creating Windows Service..." -ForegroundColor Yellow
$exePath = Join-Path $InstallPath "Qalitrack.exe"

# Create service using sc.exe for better control
$scCreate = "create `"$ServiceName`" binPath= `"$exePath`" DisplayName= `"$DisplayName`" start= auto"
sc.exe $scCreate.Split(' ')

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create service!"
    exit 1
}

# Set service description
sc.exe description $ServiceName "$Description"

# Configure service to run as LocalSystem (already default, but explicit)
sc.exe config $ServiceName obj= "LocalSystem"

Write-Host "    ✓ Service created" -ForegroundColor Green

Write-Host "[5/7] Configuring auto-restart on failure..." -ForegroundColor Yellow

# Configure service recovery options (restart on failure)
sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/10000/restart/60000

# Set service to restart after 5 seconds on failure
$servicePath = "HKLM:\SYSTEM\CurrentControlSet\Services\$ServiceName"
Set-ItemProperty -Path $servicePath -Name "DelayedAutostart" -Value 0 -Type DWord -ErrorAction SilentlyContinue

Write-Host "    ✓ Auto-restart configured:" -ForegroundColor Green
Write-Host "      - First failure:  Restart after 5 seconds" -ForegroundColor Gray
Write-Host "      - Second failure: Restart after 10 seconds" -ForegroundColor Gray
Write-Host "      - Third failure:  Restart after 60 seconds" -ForegroundColor Gray

Write-Host "[6/7] Configuring firewall..." -ForegroundColor Yellow
try {
    $firewallRule = Get-NetFirewallRule -DisplayName "Qalitrack Platform Service" -ErrorAction SilentlyContinue
    if ($firewallRule) {
        Remove-NetFirewallRule -DisplayName "Qalitrack Platform Service" -ErrorAction SilentlyContinue
    }

    New-NetFirewallRule -DisplayName "Qalitrack Platform Service" `
                        -Direction Inbound `
                        -Protocol TCP `
                        -LocalPort 5000 `
                        -Action Allow `
                        -Profile Domain,Private,Public `
                        -ErrorAction Stop | Out-Null

    Write-Host "    ✓ Firewall rule added for port 5000" -ForegroundColor Green
} catch {
    Write-Host "    ⚠ Could not configure firewall automatically" -ForegroundColor Yellow
    Write-Host "      Please manually allow port 5000 in Windows Firewall" -ForegroundColor Yellow
}

Write-Host "[7/7] Starting service..." -ForegroundColor Yellow
Start-Service -Name $ServiceName
Start-Sleep -Seconds 3

$service = Get-Service -Name $ServiceName
if ($service.Status -eq "Running") {
    Write-Host "    ✓ Service started successfully" -ForegroundColor Green
} else {
    Write-Host "    ⚠ Service status: $($service.Status)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Installation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Service Name:    $ServiceName" -ForegroundColor Cyan
Write-Host "Install Path:    $InstallPath" -ForegroundColor Cyan
Write-Host "Status:          $($service.Status)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Management Commands:" -ForegroundColor Yellow
Write-Host "  Start:   Start-Service -Name $ServiceName" -ForegroundColor Gray
Write-Host "  Stop:    Stop-Service -Name $ServiceName" -ForegroundColor Gray
Write-Host "  Restart: Restart-Service -Name $ServiceName" -ForegroundColor Gray
Write-Host "  Status:  Get-Service -Name $ServiceName" -ForegroundColor Gray
Write-Host "  Logs:    Get-EventLog -LogName Application -Source $ServiceName -Newest 50" -ForegroundColor Gray
Write-Host ""
Write-Host "API Endpoint: http://localhost:5000" -ForegroundColor Cyan
Write-Host ""
