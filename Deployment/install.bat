@echo off

:: Re-launch as Administrator if not already elevated
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Requesting administrator privileges...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:: Run the PowerShell installer — no execution policy changes needed
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1"

pause
