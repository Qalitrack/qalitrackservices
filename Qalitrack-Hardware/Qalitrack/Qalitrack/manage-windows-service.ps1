#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Manages Qalitrack Platform Service
.DESCRIPTION
    Provides commands to start, stop, restart, and monitor the service.
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("start", "stop", "restart", "status", "logs", "tail")]
    [string]$Action,

    [string]$ServiceName = "QalitrackPlatformService",
    [int]$LogCount = 50
)

$ErrorActionPreference = "Stop"

function Show-ServiceStatus {
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

    if (-not $service) {
        Write-Host "⚠ Service not found: $ServiceName" -ForegroundColor Yellow
        return
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Service Status" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Name:         $($service.Name)" -ForegroundColor Gray
    Write-Host "Display Name: $($service.DisplayName)" -ForegroundColor Gray
    Write-Host "Status:       $($service.Status)" -ForegroundColor $(if($service.Status -eq "Running"){"Green"}else{"Yellow"})
    Write-Host "Start Type:   $($service.StartType)" -ForegroundColor Gray
    Write-Host ""
}

function Show-Logs {
    param([int]$Count = 50)

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Recent Service Logs (Last $Count)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""

    try {
        $logs = Get-EventLog -LogName Application -Source $ServiceName -Newest $Count -ErrorAction Stop

        foreach ($log in $logs) {
            $color = switch ($log.EntryType) {
                "Error" { "Red" }
                "Warning" { "Yellow" }
                "Information" { "Green" }
                default { "Gray" }
            }

            Write-Host "[$($log.TimeGenerated)] [$($log.EntryType)]" -ForegroundColor $color -NoNewline
            Write-Host " $($log.Message.Split("`n")[0])" -ForegroundColor Gray
        }
    } catch {
        Write-Host "No logs found or service hasn't logged yet." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Check Windows Event Viewer:" -ForegroundColor Gray
        Write-Host "  Windows Logs -> Application -> Filter by Source: $ServiceName" -ForegroundColor Gray
    }
    Write-Host ""
}

function Tail-Logs {
    Write-Host "Tailing logs for $ServiceName (Ctrl+C to stop)..." -ForegroundColor Cyan
    Write-Host ""

    $lastEventId = 0

    while ($true) {
        try {
            $logs = Get-EventLog -LogName Application -Source $ServiceName -Newest 10 -ErrorAction SilentlyContinue

            foreach ($log in $logs | Sort-Object TimeGenerated) {
                if ($log.Index -gt $lastEventId) {
                    $lastEventId = $log.Index

                    $color = switch ($log.EntryType) {
                        "Error" { "Red" }
                        "Warning" { "Yellow" }
                        "Information" { "Green" }
                        default { "Gray" }
                    }

                    Write-Host "[$($log.TimeGenerated)] [$($log.EntryType)]" -ForegroundColor $color -NoNewline
                    Write-Host " $($log.Message)" -ForegroundColor Gray
                }
            }
        } catch {
            # Ignore errors
        }

        Start-Sleep -Seconds 2
    }
}

# Main execution
switch ($Action) {
    "start" {
        Write-Host "Starting $ServiceName..." -ForegroundColor Yellow
        Start-Service -Name $ServiceName
        Start-Sleep -Seconds 2
        Show-ServiceStatus
    }

    "stop" {
        Write-Host "Stopping $ServiceName..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force
        Start-Sleep -Seconds 2
        Show-ServiceStatus
    }

    "restart" {
        Write-Host "Restarting $ServiceName..." -ForegroundColor Yellow
        Restart-Service -Name $ServiceName -Force
        Start-Sleep -Seconds 3
        Show-ServiceStatus
    }

    "status" {
        Show-ServiceStatus
    }

    "logs" {
        Show-Logs -Count $LogCount
    }

    "tail" {
        Tail-Logs
    }
}
