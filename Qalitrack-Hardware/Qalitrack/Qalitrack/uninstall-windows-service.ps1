#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Uninstalls Qalitrack Platform Service from Windows
.DESCRIPTION
    Stops and removes the Windows Service and optionally removes installation files.
#>

param(
    [string]$ServiceName = "QalitrackPlatformService",
    [string]$InstallPath = "C:\Program Files\Qalitrack",
    [switch]$RemoveFiles = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Qalitrack Windows Service Uninstaller" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Error "This script must be run as Administrator!"
    exit 1
}

Write-Host "[1/4] Checking for service..." -ForegroundColor Yellow
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $service) {
    Write-Host "    ⚠ Service '$ServiceName' not found" -ForegroundColor Yellow
} else {
    Write-Host "    ✓ Service found" -ForegroundColor Green

    Write-Host "[2/4] Stopping service..." -ForegroundColor Yellow
    if ($service.Status -eq "Running") {
        Stop-Service -Name $ServiceName -Force
        Start-Sleep -Seconds 2
        Write-Host "    ✓ Service stopped" -ForegroundColor Green
    } else {
        Write-Host "    ✓ Service already stopped" -ForegroundColor Green
    }

    Write-Host "[3/4] Removing service..." -ForegroundColor Yellow
    sc.exe delete $ServiceName
    Start-Sleep -Seconds 2
    Write-Host "    ✓ Service removed" -ForegroundColor Green
}

Write-Host "[4/4] Removing firewall rule..." -ForegroundColor Yellow
try {
    $firewallRule = Get-NetFirewallRule -DisplayName "Qalitrack Platform Service" -ErrorAction SilentlyContinue
    if ($firewallRule) {
        Remove-NetFirewallRule -DisplayName "Qalitrack Platform Service" -ErrorAction SilentlyContinue
        Write-Host "    ✓ Firewall rule removed" -ForegroundColor Green
    } else {
        Write-Host "    ✓ No firewall rule found" -ForegroundColor Green
    }
} catch {
    Write-Host "    ⚠ Could not remove firewall rule" -ForegroundColor Yellow
}

if ($RemoveFiles) {
    Write-Host ""
    Write-Host "Removing installation files..." -ForegroundColor Yellow
    if (Test-Path $InstallPath) {
        Remove-Item -Path $InstallPath -Recurse -Force
        Write-Host "    ✓ Files removed from $InstallPath" -ForegroundColor Green
    } else {
        Write-Host "    ✓ Installation directory not found" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Uninstallation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

if (-not $RemoveFiles) {
    Write-Host "Note: Installation files remain at: $InstallPath" -ForegroundColor Yellow
    Write-Host "      Use -RemoveFiles switch to delete them" -ForegroundColor Yellow
    Write-Host ""
}
