#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Installs Qalitrack Platform Service as a Windows Service
#>

param(
    [string]$ServiceName   = "QalitrackPlatformService",
    [string]$DisplayName   = "Qalitrack Platform Data Service",
    [string]$Description   = "Qalitrack weighbridge platform data collection and camera service with NPR",
    [string]$InstallPath   = "C:\Program Files\Qalitrack"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Qalitrack Windows Service Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# -------------------------------------------------
# 1. Check for existing service
# -------------------------------------------------
Write-Host "[1/7] Checking for existing service..." -ForegroundColor Yellow
$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "    Stopping and removing existing service..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
    Write-Host "    Existing service removed" -ForegroundColor Green
}

# -------------------------------------------------
# 2. Build the application
# -------------------------------------------------
Write-Host "[2/7] Building application..." -ForegroundColor Yellow
$publishPath = Join-Path $PSScriptRoot "bin\Release\net8.0\win-x64\publish"

dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed!"
    exit 1
}
Write-Host "    Build completed → $publishPath" -ForegroundColor Green

# -------------------------------------------------
# 3. Prepare installation directory
# -------------------------------------------------
Write-Host "[3/7] Preparing installation directory..." -ForegroundColor Yellow
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}
Copy-Item -Path "$publishPath\*" -Destination $InstallPath -Recurse -Force
Write-Host "    Files copied to $InstallPath" -ForegroundColor Green

# -------------------------------------------------
# 4. Create Windows Service (clean & safe way)
# -------------------------------------------------
Write-Host "[4/7] Creating Windows Service..." -ForegroundColor Yellow
$exePath = Join-Path $InstallPath "Qalitrack.exe"

# Preferred method: New-Service (pure PowerShell, no quoting issues)
New-Service -Name $ServiceName `
            -BinaryPathName "`"$exePath`"" `
            -DisplayName $DisplayName `
            -Description $Description `
            -StartupType Automatic | Out-Null

# Set description again (New-Service sometimes doesn't apply it)
sc.exe description $ServiceName "$Description" | Out-Null

Write-Host "    Service '$ServiceName' created" -ForegroundColor Green

# -------------------------------------------------
# 5. Configure auto-restart on failure
# -------------------------------------------------
Write-Host "[5/7] Configuring auto-restart on failure..." -ForegroundColor Yellow
# restart after 5s, 10s, then 60s → then repeat every 60s
sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/10000/restart/60000 | Out-Null
sc.exe failureflag $ServiceName 1 | Out-Null   # enable failure actions

Write-Host "    Auto-restart configured (5s → 10s → 60s)" -ForegroundColor Green

# -------------------------------------------------
# 6. Firewall rule for port 5000
# -------------------------------------------------
Write-Host "[6/7] Configuring Windows Firewall..." -ForegroundColor Yellow
try {
    if (Get-NetFirewallRule -DisplayName "Qalitrack Platform Service" -ErrorAction SilentlyContinue) {
        Remove-NetFirewallRule -DisplayName "Qalitrack Platform Service" -ErrorAction SilentlyContinue
    }

    New-NetFirewallRule -DisplayName "Qalitrack Platform Service" `
                        -Direction Inbound `
                        -Action Allow `
                        -Protocol TCP `
                        -LocalPort 5000 `
                        -Profile Any | Out-Null

    Write-Host "    Firewall rule added (TCP 5000)" -ForegroundColor Green
}
catch {
    Write-Host "    Could not add firewall rule. Add TCP 5000 manually if needed." -ForegroundColor Yellow
}

# -------------------------------------------------
# 7. Start the service
# -------------------------------------------------
Write-Host "[7/7] Starting service..." -ForegroundColor Yellow
Start-Service -Name $ServiceName
Start-Sleep -Seconds 4

$service = Get-Service -Name $ServiceName

if ($service.Status -eq "Running") {
    Write-Host "    Service started successfully!" -ForegroundColor Green
} else {
    Write-Host "    Service status: $($service.Status)" -ForegroundColor Yellow
}

# -------------------------------------------------
# Final summary
# -------------------------------------------------
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "     INSTALLATION COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Service Name   : $ServiceName"          -ForegroundColor Cyan
Write-Host "Install Path   : $InstallPath"          -ForegroundColor Cyan
Write-Host "Status         : $($service.Status)"    -ForegroundColor Cyan
Write-Host "API Endpoint   : http://localhost:5000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Useful commands:" -ForegroundColor Yellow
Write-Host "   Start-Service   -Name $ServiceName"     -ForegroundColor Gray
Write-Host "   Stop-Service    -Name $ServiceName"     -ForegroundColor Gray
Write-Host "   Restart-Service -Name $ServiceName"     -ForegroundColor Gray
Write-Host "   Get-Service     -Name $ServiceName"     -ForegroundColor Gray
Write-Host ""
Write-Host "Done!" -ForegroundColor Green