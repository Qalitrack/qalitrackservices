# QaliTrack — Deployment & Developer Runbook

> Everything needed to migrate the database, run the services, build the
> frontend, and package the Electron apps.

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Environment Setup](#2-environment-setup)
3. [Database — Migrations](#3-database--migrations)
   - [How it works](#how-it-works)
   - [Masterdata Service](#masterdata-service)
   - [Transaction Service](#transaction-service)
   - [User Service](#user-service)
   - [Backup Service](#backup-service)
   - [Adding a new migration](#adding-a-new-migration)
4. [Backend — Docker Deployment](#4-backend--docker-deployment)
   - [First-time startup](#first-time-startup)
   - [Rebuilding a single service](#rebuilding-a-single-service)
   - [Logs](#logs)
   - [Stopping everything](#stopping-everything)
5. [Frontend — Web Builds](#5-frontend--web-builds)
6. [Frontend — Electron Desktop Builds](#6-frontend--electron-desktop-builds)
   - [Main app](#main-app-electron)
   - [Kiosk app](#kiosk-app-electron)
   - [Windows build via Wine](#windows-build-via-wine-linux-host)
7. [Service Map](#7-service-map)
8. [Backups — pgBackRest (Full + Incremental)](#8-backups--pgbackrest-full--incremental)
   - [One-time setup](#one-time-setup)
   - [Everyday operation](#everyday-operation)
   - [Restoring](#restoring)
   - [Checking status](#checking-status)

---

## 1. Prerequisites

| Tool | Minimum version | Purpose |
|---|---|---|
| Docker + Docker Compose | 24 / v2 | Run all backend services |
| .NET SDK | 9.0 | Build services & run migrations |
| Node.js | 20 LTS | Frontend build |
| npm | 10 | Frontend package manager |
| Wine | 9+ | Cross-compile Windows Electron installer (Linux host) |
| electron-builder | bundled via npm | Package Electron apps |

Check versions:

```bash
docker --version && docker compose version
dotnet --version
node --version && npm --version
wine --version          # only needed for Windows builds
```

---

## 2. Environment Setup

The entire stack is driven by a single `.env` file that **must not be committed**.

```bash
cd Deployment
cp .env.example .env      # if an example exists, otherwise create it manually
```

Minimum required contents of `Deployment/.env`:

```env
# Shared PostgreSQL — one DB, per-service schemas
DB_NAME=qalitrackdb
DB_USER=qalitrack
DB_PASSWORD=<choose a strong password>

# Redis
REDIS_PASSWORD=<choose a strong password>

# JWT — generate with: node -e "console.log(require('crypto').randomBytes(48).toString('base64'))"
JWT_SECRET_KEY=<256-bit base64 string>

# Email (SMTP — used by User Service)
EMAIL_FROM_EMAIL=noreply@yourcompany.com
EMAIL_FROM_NAME=QaliTrack System
EMAIL_SMTP_HOST=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USERNAME=you@gmail.com
EMAIL_SMTP_PASSWORD=<app password>
EMAIL_ENABLE_SSL=true

# Receipt numbering (Transaction Service) — optional, defaults to NCCU
RECEIPT_PREFIX=NCCU
```

---

## 3. Database — Migrations

### How it works

All four microservices share **one PostgreSQL database** (`qalitrackdb`) with
**isolated schemas**:

| Service | Schema | Migration history table |
|---|---|---|
| Masterdata | `masterdata` | `masterdata.__EFMigrationsHistory` |
| Transaction | `transactions` | `transactions.__EFMigrationsHistory` |
| User Service | `users` | `users.__EFMigrationsHistory` |
| Backup Service | `backup` | `backup.__EFMigrationsHistory` |

Schemas are created automatically by `Deployment/postgres/init-scripts/01-init-schemas.sql`
the first time the postgres container starts.

> **Each service owns its own migration history.**
> Running `dotnet ef database update` for one service never touches another.

---

### Masterdata Service

```
packages/microservices/masterdata/masterdata/
```

```bash
cd packages/microservices/masterdata/masterdata

# Restore the local dotnet-ef tool (once per machine)
dotnet tool restore

# Generate a new migration (never edits the DB)
dotnet ef migrations add <MigrationName> \
  --project src/Masterdata.Infrastructure \
  --startup-project src/Masterdata.Api

# Apply all pending migrations to the database
dotnet ef database update \
  --project src/Masterdata.Infrastructure \
  --startup-project src/Masterdata.Api
```

Migrations live in:
`src/Masterdata.Infrastructure/Data/Migrations/`

---

### Transaction Service

```
packages/microservices/masterdata/transaction/
```

```bash
cd packages/microservices/masterdata/transaction

dotnet tool restore

dotnet ef migrations add <MigrationName> \
  --project src/Transaction.Infrastructure \
  --startup-project src/Transaction.Api

dotnet ef database update \
  --project src/Transaction.Infrastructure \
  --startup-project src/Transaction.Api
```

Migrations live in:
`src/Transaction.Infrastructure/Data/Migrations/`

---

### User Service

```
packages/microservices/masterdata/user-service/
```

```bash
cd packages/microservices/masterdata/user-service

dotnet tool restore

dotnet ef migrations add <MigrationName> \
  --project src/UserService.Infrastructure \
  --startup-project src/UserService.Api

dotnet ef database update \
  --project src/UserService.Infrastructure \
  --startup-project src/UserService.Api
```

Migrations live in:
`src/UserService.Infrastructure/Migrations/`

---

### Backup Service

```
packages/microservices/masterdata/BackupService/
```

```bash
cd packages/microservices/masterdata/BackupService

dotnet tool restore

dotnet ef migrations add <MigrationName> \
  --project BackupService.Infrastructure \
  --startup-project BackupService.API

dotnet ef database update \
  --project BackupService.Infrastructure \
  --startup-project BackupService.API
```

Migrations live in:
`BackupService.Infrastructure/Migrations/`

---

### Adding a new migration

1. Make your entity / DbContext changes.
2. `cd` to the service root (the folder that has `.config/dotnet-tools.json`).
3. Run `dotnet tool restore` if you haven't already.
4. Run `dotnet ef migrations add <DescriptiveName> --project ... --startup-project ...`
5. Review the generated `.cs` file — verify `Up()` and `Down()` are correct.
6. Commit the migration files alongside your entity changes.
7. Apply with `dotnet ef database update ...` **or** let the service apply on startup
   (each service calls `dbContext.Database.Migrate()` at boot).

> **Never use `dotnet ef database update` in production.**
> The Docker containers auto-apply pending migrations on startup via
> `dbContext.Database.Migrate()`.

---

## 4. Backend — Docker Deployment

All commands run from the `Deployment/` folder.

### First-time startup

```bash
cd Deployment

# Build all images and start in background
docker compose up -d --build

# Watch all logs as they come up
docker compose logs -f
```

On first boot the postgres container runs
`postgres/init-scripts/01-init-schemas.sql` which creates the four schemas.
Each service then applies its own EF migrations automatically.

Expected healthy containers:

```
qalitrack-postgres-prod          healthy
qalitrack-redis-prod             running
qalitrack-gateway-service-prod   running  → :7000
qalitrack-user-service-prod      running  → :7001
qalitrack-masterdata-service-prod running → :7002
qalitrack-backup-service-prod    running  → :7003
qalitrack-transaction-service-prod running → :7004
```

### Rebuilding a single service

```bash
# Example: rebuild and restart only the masterdata service
docker compose up -d --build masterdata-service-prod
```

### Logs

```bash
# All services
docker compose logs -f

# Single service
docker compose logs -f user-service-prod
docker compose logs -f masterdata-service-prod
docker compose logs -f transaction-service-prod
docker compose logs -f backup-service-prod
docker compose logs -f gateway-service-prod
```

### Stopping everything

```bash
# Stop and remove containers (keeps volumes — data is safe)
docker compose down

# Stop and remove containers AND wipe all data (full reset)
docker compose down -v
```

---

## 5. Frontend — Web Builds

All commands run from `apps/qalitrack_frontend/`.

```bash
cd apps/qalitrack_frontend
npm install          # first time only
```

| Command | Output | What it produces |
|---|---|---|
| `npm run dev` | — | Dev server at `http://localhost:5173` (hot reload) |
| `npm run build:main` | `dist/` | Production web build — full main app |
| `npm run build:kiosk` | `dist-kiosk/` | Production web build — kiosk only |

**Main app** (`dist/`) is the operator/admin dashboard — requires login and a
valid license.

**Kiosk** (`dist-kiosk/`) is the `SelfServiceWeighing` screen only — no login,
no navigation, requires a license with the `kiosk` feature.

To preview a production build locally:

```bash
npm run build:main
npm run preview        # serves dist/ at http://localhost:4173
```

---

## 6. Frontend — Electron Desktop Builds

### Main app (Electron)

Produces the full operator/admin desktop app.

```bash
cd apps/qalitrack_frontend
npm run electron:build
```

Output: `release/` — contains the installer for the current platform
(`.exe` NSIS on Windows, `.AppImage` + `.deb` on Linux, `.dmg` on macOS).

---

### Kiosk app (Electron)

Produces a standalone kiosk terminal — `SelfServiceWeighing` only, gated
behind the `kiosk` license feature.

```bash
cd apps/qalitrack_frontend
npm run electron:build:kiosk
```

Output: `release/` — same installer formats, but contains only the kiosk UI.

> The kiosk Electron app and the main app share the same `appId`
> (`com.qalibrated.qalitrack`). If both are installed on the **same machine**
> they share the same license cache — activating once unlocks both.
> On **separate machines** each machine shows its own Machine ID and requires
> its own license token (machine-bound) or a floating token (no `mid` in the
> JWT) that can be reused across machines.

---

### Windows build via Wine (Linux host)

Used to cross-compile a Windows NSIS `.exe` installer from a Linux machine.

```bash
# Ensure Wine is installed
wine --version

# Set Wine environment variables
export WINEPREFIX=~/.wine
export WINEARCH=win64

# Build main app for Windows
cd apps/qalitrack_frontend
npm run electron:build -- --win

# Build kiosk for Windows
npm run electron:build:kiosk -- --win
```

The NSIS installer is written to `release/Qalitrack Setup <version>.exe`.

> If electron-builder fails with icon errors, ensure `build/icon.png` exists
> and is a valid 512×512 PNG. Generate it with:
> ```bash
> magick src/assets/qalitrack_logo.png -resize 512x512 build/icon.png
> ```

---

## 7. Service Map

```
┌─────────────────────────────────────────────────────┐
│                   Client (Browser / Electron)        │
└──────────────────────────┬──────────────────────────┘
                           │ :7000
                ┌──────────▼──────────┐
                │   API Gateway       │  qalitrack-gateway-service-prod
                └──┬───┬───┬───┬─────┘
       ┌───────────┘   │   │   └─────────────────────┐
       │ :7001         │   │ :7002       :7003        │ :7004
┌──────▼──────┐  ┌─────▼───▼────┐  ┌───────────┐  ┌─▼───────────┐
│ User Service│  │  Masterdata  │  │  Backup   │  │ Transaction │
│ (users)     │  │ (masterdata) │  │ (backup)  │  │(transactions│
└──────┬──────┘  └──────┬───────┘  └─────┬─────┘  └──────┬──────┘
       │                │                │               │
       └────────────────┴────────────────┴───────────────┘
                                │
                    ┌───────────▼───────────┐
                    │  postgres-prod         │
                    │  DB: qalitrackdb       │
                    │  schemas:              │
                    │    masterdata          │
                    │    transactions        │
                    │    users               │
                    │    backup              │
                    └───────────────────────┘
                    ┌───────────────────────┐
                    │  redis-cache-prod      │
                    │  (sessions & cache)    │
                    └───────────────────────┘
```

| Container | External port | Internal port | Description |
|---|---|---|---|
| `qalitrack-gateway-service-prod` | 7000 | 7000 | API Gateway — all client traffic enters here |
| `qalitrack-user-service-prod` | 7001 | 80 | Auth, users, roles, shifts, backup scheduler |
| `qalitrack-masterdata-service-prod` | 7002 | 80 | Vehicles, drivers, products, weighbridges |
| `qalitrack-backup-service-prod` | 7003 | 80 | Drives pgBackRest full/incremental backups & restores (see [§8](#8-backups--pgbackrest-full--incremental)) |
| `qalitrack-transaction-service-prod` | 7004 | 80 | Weigh transactions |
| `qalitrack-postgres-prod` | — | 5432 | Shared PostgreSQL (internal only) |
| `qalitrack-redis-prod` | — | 6379 | Shared Redis (internal only) |

---

## 8. Backups — pgBackRest (Full + Incremental)

`postgres-prod` runs a custom image (`Deployment/docker-images/postgres/`) with
pgBackRest installed. Backups are physical (whole-instance), not per-schema — one
stanza (`qalitrack`) covers `masterdata` + `transactions` + `users` + `backup`
together. `backup-service-prod` drives everything via `docker exec`/`docker run`
against `postgres-prod`, which is why it needs `/var/run/docker.sock` mounted in
(see the comment on that volume in `docker-compose.yml` — it's a deliberate,
accepted trade-off for this single-tenant on-prem deployment, not an oversight).

### One-time setup

After the stack is up for the first time (or after wiping the `pgbackrest_repo`
volume), the stanza needs to be created and given an initial full backup before
any incremental backup can run:

```bash
docker exec qalitrack-postgres-prod pgbackrest --stanza=qalitrack stanza-create
docker exec qalitrack-postgres-prod pgbackrest --stanza=qalitrack check
docker exec qalitrack-postgres-prod pgbackrest --stanza=qalitrack backup --type=full
```

### Everyday operation

Trigger backups the same way as before, through the BackupService API
(`POST /Backup/create` with `"type": 0` for Full, `"type": 1` for Incremental) or
its Quartz-scheduled cron jobs. An incremental backup fails with a clear error if
no full backup chain exists yet for that microservice — run a full backup first.

### Restoring

`POST /Backup/restore` with `"backupId": "latest"` (or a specific pgbackrest
label) restores the **entire** shared instance — `postgres-prod` gets stopped,
restored via a one-off container sharing its volumes, then started back up.
There is no per-schema/per-microservice restore with a physical backup; every
service using `postgres-prod` is down for the duration.

To restore by hand instead of through the API:

```bash
docker stop qalitrack-postgres-prod
docker run --rm -u postgres \
  -v postgres_prod_data:/var/lib/postgresql/data \
  -v pgbackrest_repo:/var/lib/pgbackrest \
  qalitrack-postgres-prod:latest \
  pgbackrest --stanza=qalitrack --delta --type=immediate restore
docker start qalitrack-postgres-prod
```

`--type=immediate` matters: without it, pgBackRest replays every WAL segment archived since (archiving runs continuously, independent of backup timing), which recovers to "now" rather than "this backup" — confirmed with a real restore during testing, where a row written after the last backup survived until this flag was added.

### Checking status

```bash
docker exec qalitrack-postgres-prod pgbackrest --stanza=qalitrack info
```

Shows the full/incremental chain, sizes, and timestamps — this replaces browsing
a directory of dated `.dump` files, since a physical backup isn't one file.
