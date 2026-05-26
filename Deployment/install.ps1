#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Qalitrack backend installer — clones the repo inside WSL2 (Linux filesystem,
    no Windows filename restrictions), then builds and starts all services.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$REPO_URL   = "https://github.com/<your-org>/qalitrackservices.git"
$BRANCH     = "katani"
# Clone target is inside the WSL2 Linux filesystem — no NTFS filename restrictions.
# Files like sha256:<hash> that Windows cannot hold are checked out normally here.
$WSL_DIR    = "/home/qalitrack/source"
$WSL_DEPLOY = "$WSL_DIR/Deployment"

# Minimum versions
$MIN_DOCKER_MAJOR = 25
$MIN_GIT_MAJOR    = 2
$MIN_GIT_MINOR    = 39

# ── Helpers ───────────────────────────────────────────────────────────────────

function Write-Header($text) {
    Write-Host ""
    Write-Host "==> $text" -ForegroundColor Cyan
}

function Write-Ok($text)   { Write-Host "    OK  $text" -ForegroundColor Green }
function Write-Fail($text) { Write-Host "    ERR $text" -ForegroundColor Red; exit 1 }
function Write-Warn($text) { Write-Host "    WRN $text" -ForegroundColor Yellow }
function Write-Info($text) { Write-Host "    ... $text" -ForegroundColor Gray }

function Invoke-Wsl {
    param([string]$Command)
    wsl -- bash -c $Command
    if ($LASTEXITCODE -ne 0) { Write-Fail "WSL command failed: $Command" }
}

function Wait-DockerRunning {
    Write-Info "Waiting for Docker Desktop (up to 2 minutes)..."
    for ($i = 0; $i -lt 24; $i++) {
        docker info 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) { return }
        Start-Sleep -Seconds 5
    }
    Write-Fail "Docker Desktop did not start. Open it manually and re-run install.bat."
}

# ── Step 1: WSL2 ─────────────────────────────────────────────────────────────

Write-Header "Checking WSL2"

$wslPresent = Get-Command wsl -ErrorAction SilentlyContinue
if (-not $wslPresent) {
    Write-Info "WSL2 not found — installing..."
    # --no-distribution skips downloading Ubuntu; Docker Desktop ships its own distro
    wsl --install --no-distribution
    Write-Host ""
    Write-Host "  WSL2 has been installed." -ForegroundColor Green
    Write-Host "  A restart is required before continuing." -ForegroundColor Yellow
    Write-Host "  After restarting, double-click install.bat again." -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 0
}

# Ensure default is WSL2, not WSL1
wsl --set-default-version 2 2>&1 | Out-Null
Write-Ok "WSL2 ready"

# ── Step 2: Docker Desktop ────────────────────────────────────────────────────

Write-Header "Checking Docker Desktop"

$dockerPresent = Get-Command docker -ErrorAction SilentlyContinue

if (-not $dockerPresent) {
    Write-Info "Docker Desktop not found — downloading and installing..."
    $installer = "$env:TEMP\DockerDesktopInstaller.exe"
    Write-Info "Downloading Docker Desktop (this may take a few minutes)..."
    Invoke-WebRequest -Uri "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe" `
                      -OutFile $installer -UseBasicParsing
    Write-Info "Running Docker Desktop installer silently..."
    Start-Process -FilePath $installer -ArgumentList "install --quiet --accept-license --backend=wsl-2" -Wait
    Remove-Item $installer -ErrorAction SilentlyContinue

    # Refresh PATH so 'docker' is now found
    $env:PATH = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" +
                [System.Environment]::GetEnvironmentVariable("Path", "User")

    $dockerPresent = Get-Command docker -ErrorAction SilentlyContinue
    if (-not $dockerPresent) {
        Write-Host ""
        Write-Host "  Docker Desktop installed — a restart may be required." -ForegroundColor Yellow
        Write-Host "  After restarting, double-click install.bat again." -ForegroundColor Yellow
        Write-Host ""
        pause
        exit 0
    }
    Write-Ok "Docker Desktop installed"
}

$dockerRaw = docker --version
if ($dockerRaw -match 'Docker version (\d+)\.') {
    $dockerMajor = [int]$Matches[1]
} else {
    Write-Fail "Could not parse Docker version from: $dockerRaw"
}

if ($dockerMajor -lt $MIN_DOCKER_MAJOR) {
    Write-Info "Docker Engine $dockerMajor.x is outdated — updating Docker Desktop..."
    $installer = "$env:TEMP\DockerDesktopInstaller.exe"
    Invoke-WebRequest -Uri "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe" `
                      -OutFile $installer -UseBasicParsing
    Start-Process -FilePath $installer -ArgumentList "install --quiet --accept-license --backend=wsl-2" -Wait
    Remove-Item $installer -ErrorAction SilentlyContinue
    Write-Ok "Docker Desktop updated — re-run install.bat if prompted to restart"
}

docker compose version 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Fail "'docker compose' V2 not available — update Docker Desktop to version 4.0 or later."
}

Write-Ok "Docker Engine ready"

# ── Step 3: Check Git inside WSL2 ────────────────────────────────────────────

Write-Header "Checking Git (inside WSL2)"

$gitRaw = wsl -- git --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Info "Git not found in WSL2 — installing..."
    wsl -- bash -c "sudo apt-get update -qq && sudo apt-get install -y git"
    $gitRaw = wsl -- git --version 2>&1
}

if ($gitRaw -match 'git version (\d+)\.(\d+)') {
    $gitMajor = [int]$Matches[1]; $gitMinor = [int]$Matches[2]
} else {
    Write-Fail "Could not parse Git version from WSL2: $gitRaw"
}

if ($gitMajor -lt $MIN_GIT_MAJOR -or ($gitMajor -eq $MIN_GIT_MAJOR -and $gitMinor -lt $MIN_GIT_MINOR)) {
    Write-Info "Git $gitMajor.$gitMinor is old — upgrading inside WSL2..."
    wsl -- bash -c "sudo add-apt-repository -y ppa:git-core/ppa && sudo apt-get update -qq && sudo apt-get install -y git"
}

Write-Ok "WSL2 Git ready"

# ── Step 4: Start Docker Desktop ─────────────────────────────────────────────

Write-Header "Starting Docker Desktop"

docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Info "Docker Desktop is not running — starting it..."
    $desktopExe = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    if (-not (Test-Path $desktopExe)) {
        Write-Fail "Docker Desktop not found at $desktopExe. Is it installed?"
    }
    Start-Process $desktopExe
    Wait-DockerRunning
}

Write-Ok "Docker is running"

# ── Step 5: Check .env ───────────────────────────────────────────────────────

Write-Header "Checking .env"

# The installer expects the .env to be placed next to install.bat.
# It is copied into the WSL2 filesystem after cloning.
$envSource = Join-Path $PSScriptRoot ".env"

if (-not (Test-Path $envSource)) {
    Write-Host ""
    Write-Warn ".env not found at $envSource"
    Write-Host "    Place the .env file provided for this site in the same folder as install.bat." -ForegroundColor Yellow
    exit 1
}

Write-Ok ".env found"

# ── Step 6: Clone / update inside WSL2 ───────────────────────────────────────

Write-Header "Cloning repository into WSL2 (branch: $BRANCH)"

# Running inside WSL2 Linux filesystem means:
#  - No Windows filename restrictions (colons, case, long paths all work)
#  - Docker build context is read from Linux FS — faster and complete

$cloneOrPull = @"
if [ -d '$WSL_DIR/.git' ]; then
    echo 'Repository exists — pulling latest'
    git -C '$WSL_DIR' fetch origin $BRANCH
    git -C '$WSL_DIR' checkout $BRANCH
    git -C '$WSL_DIR' pull origin $BRANCH
else
    git clone -b $BRANCH $REPO_URL $WSL_DIR
fi
"@

wsl -- bash -c $cloneOrPull
if ($LASTEXITCODE -ne 0) { Write-Fail "Clone/pull failed — check the output above." }

Write-Ok "Repository ready at WSL2:$WSL_DIR"

# ── Step 7: Copy .env into WSL2 ──────────────────────────────────────────────

Write-Header "Copying .env into WSL2"

# Convert the Windows path to a WSL path and copy
$wslEnvSource = (wsl -- wslpath $envSource.Replace('\', '/')) 2>&1
wsl -- bash -c "cp '$wslEnvSource' '$WSL_DEPLOY/.env'"
if ($LASTEXITCODE -ne 0) { Write-Fail "Failed to copy .env into WSL2." }

Write-Ok ".env copied to WSL2:$WSL_DEPLOY/.env"

# ── Step 8: Build and start ───────────────────────────────────────────────────

Write-Header "Building and starting services (5-15 minutes on first run)"

wsl -- bash -c "cd '$WSL_DEPLOY' && docker compose up --build -d"
if ($LASTEXITCODE -ne 0) { Write-Fail "docker compose failed — check the output above." }

Write-Ok "All containers started"

# ── Step 9: Health check ──────────────────────────────────────────────────────

Write-Header "Waiting for services to become healthy"

Write-Info "Giving databases 30 seconds to initialise..."
Start-Sleep -Seconds 30

$statuses = wsl -- bash -c "cd '$WSL_DEPLOY' && docker compose ps --format json" | ConvertFrom-Json
$unhealthy = @($statuses | Where-Object { $_.Health -eq "unhealthy" })

if ($unhealthy.Count -gt 0) {
    Write-Warn "Some services are still starting:"
    $unhealthy | ForEach-Object { Write-Host "      - $($_.Name)" -ForegroundColor Yellow }
    Write-Host "    Run 'docker compose ps' inside WSL2 at $WSL_DEPLOY to recheck." -ForegroundColor Yellow
} else {
    Write-Ok "All services healthy"
}

# ── Done ──────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  Qalitrack backend is running." -ForegroundColor Green
Write-Host ""
Write-Host "  Gateway        http://localhost:7000"
Write-Host "  User Service   http://localhost:7001"
Write-Host "  Master Data    http://localhost:7002"
Write-Host "  Backup         http://localhost:7003"
Write-Host "  Transactions   http://localhost:7004"
Write-Host ""
Write-Host "  To stop:   wsl -- bash -c `"cd $WSL_DEPLOY && docker compose down`""
Write-Host "  To update: wsl -- bash -c `"git -C $WSL_DIR pull && cd $WSL_DEPLOY && docker compose up --build -d`""
Write-Host "==========================================================" -ForegroundColor Green
