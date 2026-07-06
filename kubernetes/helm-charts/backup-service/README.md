# Backup Service - API-Driven Backup Management

API-driven backup service with scheduling, restore capabilities, and microservice management.

## What This Service Does

**BackupService** is a full-featured microservice that provides:

- ✅ **REST API** for on-demand backups
- ✅ **Scheduled backups** (using Quartz.NET)
- ✅ **Full & incremental backups**
- ✅ **WAL (Write-Ahead Log) archiving**
- ✅ **Per-microservice backup management**
- ✅ **Restore via API**
- ✅ **Download backups via API**
- ✅ **Backup metadata tracking**
- ✅ **Backup health monitoring**
- ✅ **Backup verification**

## How It Works with CronJob Backups

You have **TWO backup systems** working together:

### 1. **BackupService** (This Chart)
**Purpose:** API-driven, on-demand, and scheduled backups

**Features:**
- API endpoints for backup/restore
- Quartz.NET job scheduling
- Backup metadata database
- Per-microservice configuration
- Backup chain management
- Incremental backups

**Use when:**
- Need on-demand backup via API
- Want to restore via API
- Need backup verification
- Want to download specific backups
- Managing multiple microservices separately

### 2. **CronJob Backups** (in `kubernetes/backups/`)
**Purpose:** Simple, automated daily backups

**Features:**
- Simple pg_dump backups
- No API required
- Lightweight (only runs when needed)
- 30-day retention

**Use when:**
- Just need automated nightly backups
- Don't need API access
- Want simple disaster recovery

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Qalitrack Platform                    │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  User Services:                                          │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐        │
│  │ User       │  │ MasterData │  │Transaction │        │
│  │ PostgreSQL │  │ PostgreSQL │  │ PostgreSQL │        │
│  └────────────┘  └────────────┘  └────────────┘        │
│         │               │               │                │
│         └───────────────┴───────────────┘                │
│                         │                                │
│              Backed up by BOTH:                          │
│                         │                                │
│         ┌───────────────┴────────────────┐              │
│         │                                 │              │
│         ▼                                 ▼              │
│  ┌────────────────┐            ┌─────────────────┐     │
│  │ BackupService  │            │  CronJob        │     │
│  │ (API-driven)   │            │  (Simple)       │     │
│  │                │            │                 │     │
│  │ - REST API     │            │ - Daily 2AM     │     │
│  │ - On-demand    │            │ - pg_dump       │     │
│  │ - Scheduling   │            │ - 30 days       │     │
│  │ - Restore API  │            │                 │     │
│  └────────────────┘            └─────────────────┘     │
│         │                                 │              │
│         ▼                                 ▼              │
│  ┌────────────────┐            ┌─────────────────┐     │
│  │ Backup Storage │            │ Backup Storage  │     │
│  │ PVC (50Gi)     │            │ PVC (50Gi)      │     │
│  └────────────────┘            └─────────────────┘     │
│                                                          │
│  BackupService Infrastructure:                          │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐       │
│  │ PostgreSQL │  │   Redis    │  │ RabbitMQ   │       │
│  │ (metadata) │  │  (cache)   │  │ (queue)    │       │
│  └────────────┘  └────────────┘  └────────────┘       │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

## Installation

### Prerequisites

Make sure the main services are running:
```bash
# User, MasterData, Transaction services
kubectl get pods -n qalitrack-prod
```

### Install Backup Service

```bash
cd kubernetes/helm-charts/backup-service

# Install
helm install backup-service . \
  --namespace qalitrack-prod \
  --create-namespace \
  --values values.yaml
```

## API Endpoints

### Create Backup

```bash
POST /api/Backup/create
Content-Type: application/json

{
  "microservice": "user-service",
  "type": 1,  # 0=Full, 1=Incremental
  "cronSchedule": "0 2 * * *"
}
```

### List Available Backups

```bash
GET /api/Backup/list/{microservice}

# Example
GET /api/Backup/list/user-service
```

### Download Backup

```bash
GET /api/Backup/download/{microservice}/{backupId}

# Get latest backup
GET /api/Backup/download/user-service/latest
```

### Restore Backup

```bash
POST /api/Backup/restore
Content-Type: application/json

{
  "microservice": "QalitrackDB",
  "backupId": "backup-20260322-020000"
}
```

> Backup Service now targets a single shared database (`qalitrackdb`, all
> per-service schemas) instead of a per-microservice registry — seeded
> automatically by `DatabaseSeeder` on startup. The `/api/Microservice/*`
> registry endpoints still exist but the UI no longer exposes add/edit/delete.

## Usage Examples

### Trigger On-Demand Backup

```bash
# Port-forward to backup service
kubectl port-forward -n qalitrack-prod svc/backup-service 7004:7004

# Create backup
curl -X POST http://localhost:7004/api/Backup/create \
  -H "Content-Type: application/json" \
  -d '{
    "microservice": "user-service",
    "type": 1,
    "cronSchedule": "0 2 * * *"
  }'
```

### Download Latest Backup

```bash
curl -X GET http://localhost:7004/api/Backup/download/user-service/latest \
  -o user-service-backup.dump
```

### Restore from Backup

```bash
curl -X POST http://localhost:7004/api/Backup/restore \
  -H "Content-Type: application/json" \
  -d '{
    "microservice": "user-service",
    "backupId": "backup-20260322-020000"
  }'
```

## Configuration

### Environment Variables

Key environment variables (configured in `values.yaml`):

```yaml
env:
  - name: BACKUP_SCHEDULE
    value: "0 2 * * *"  # Daily at 2 AM
  - name: BACKUP_RETENTION_DAYS
    value: "30"
  - name: UsePostgreSQL
    value: "true"
  - name: UseRedis
    value: "true"
  - name: Backup__Path
    value: "/backups"
```

### Storage

Backup storage is configured via PVC:

```yaml
persistence:
  enabled: true
  size: 50Gi  # Adjust based on database sizes
  storageClass: ""  # Uses default storage class
```

## Monitoring

### Check Service Health

```bash
# Service status
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=backup-service

# Service logs
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=backup-service -f
```

### Check Backup Jobs

```bash
# View scheduled jobs (in-app)
kubectl exec -it -n qalitrack-prod deployment/backup-service -- \
  curl http://localhost:7004/api/Backup/jobs
```

### Storage Usage

```bash
kubectl exec -it -n qalitrack-prod deployment/backup-service -- \
  df -h /backups
```

## Backup Strategy

### Recommended Setup

**BackupService (API-driven):**
- Full backup: Weekly (Sunday 2 AM)
- Incremental: Daily (2 AM)
- Retention: 30 days
- Use for: Restore testing, on-demand backups

**CronJob (Simple):**
- Full backup: Daily (2 AM)
- Retention: 30 days
- Use for: Disaster recovery safety net

### Why Both?

1. **Redundancy** - Two independent backup systems
2. **Flexibility** - API access when needed
3. **Simplicity** - CronJob as reliable fallback
4. **Different use cases** - On-demand vs scheduled

## Troubleshooting

### Backup Job Failed

```bash
# Check logs
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=backup-service

# Common issues:
# 1. Database connection failed
# 2. Insufficient storage
# 3. Permission issues
```

### Cannot Access API

```bash
# Check service
kubectl get svc -n qalitrack-prod backup-service

# Port-forward
kubectl port-forward -n qalitrack-prod svc/backup-service 7004:7004

# Test endpoint
curl http://localhost:7004/health
```

### PostgreSQL Connection Issues

```bash
# Verify PostgreSQL is running
kubectl get pods -n qalitrack-prod | grep postgresql

# Check service connectivity (shared qalitrackdb)
kubectl run -it --rm debug --image=postgres:15-alpine --restart=Never -- \
  psql -h qalitrack-postgresql -U qalitrack -d qalitrackdb
```

## Resource Usage

Total resources for BackupService:

```
Requests: ~1Gi memory, ~600m CPU
Limits:   ~3.5Gi memory, ~2.25 CPUs

Breakdown:
- Backup Service:    256Mi-512Mi, 100m-250m CPU
- PostgreSQL:        512Mi-2Gi, 250m-1000m CPU
- Redis:             128Mi-256Mi, 100m-250m CPU
- RabbitMQ:          256Mi-512Mi, 100m-250m CPU
```

## Upgrade

```bash
cd kubernetes/helm-charts/backup-service

# Upgrade
helm upgrade backup-service . \
  --namespace qalitrack-prod \
  --values values.yaml
```

## Uninstall

```bash
helm uninstall backup-service -n qalitrack-prod
```

**Note:** PVC with backups will remain. Delete manually if needed:
```bash
kubectl delete pvc backup-storage-pvc -n qalitrack-prod
```

---

**Last Updated:** March 2026
**Version:** 1.0.0
