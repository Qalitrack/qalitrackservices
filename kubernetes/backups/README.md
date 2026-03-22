# Automated PostgreSQL Backups (CronJob)

Automated daily backups of all Qalitrack PostgreSQL databases with retention and compression.

> **Note:** This is a simple CronJob-based backup system. For API-driven backups with on-demand restore capabilities, see the [BackupService](../helm-charts/backup-service/README.md). Both systems can run together for redundancy.

## What's Backed Up

- **user-service** database (`qalitrack_user_service`)
- **masterdata-service** database (`qalitrack_master_data`)
- **transaction-service** database (`qalitrack_transaction`)

## Features

- **Automated daily backups** at 2:00 AM UTC
- **Compressed** using gzip
- **Checksums** for integrity verification
- **30-day retention** (automatically deletes older backups)
- **50Gi persistent storage**
- **Manual backup** option

## Installation

### Quick Install

```bash
cd kubernetes/backups
./install.sh
```

### Manual Installation

```bash
kubectl apply -f cronjob-postgres-backup.yaml
```

## How It Works

```
1. CronJob triggers daily at 2:00 AM UTC
   ↓
2. Creates backups directory with current date
   ↓
3. pg_dump each database (custom format)
   ↓
4. Creates SHA256 checksums
   ↓
5. Compresses everything into tar.gz
   ↓
6. Removes old backups (>30 days)
   ↓
7. Job completes, pod cleanup after 1 hour
```

## Backup Schedule

- **Frequency:** Daily
- **Time:** 2:00 AM UTC
- **Retention:** 30 days
- **Format:** PostgreSQL custom format (compressed)
- **Compression:** gzip

### Change Backup Schedule

Edit `cronjob-postgres-backup.yaml`:

```yaml
spec:
  schedule: "0 2 * * *"  # Cron format
```

Examples:
```
"0 2 * * *"      # Daily at 2:00 AM
"0 */6 * * *"    # Every 6 hours
"0 0 * * 0"      # Weekly (Sunday midnight)
"0 0 1 * *"      # Monthly (1st of month)
```

## Manual Backup

### Trigger from CronJob

```bash
kubectl create job --from=cronjob/postgres-backup manual-backup-$(date +%s) -n qalitrack-prod
```

### Using Manual Backup Job

```bash
kubectl apply -f manual-backup-job.yaml
```

## View Backup Logs

```bash
# Find the backup pod
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup

# View logs
kubectl logs -n qalitrack-prod <pod-name>

# Follow logs in real-time
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup -f
```

## List Backup Files

### Via kubectl exec

```bash
# If backup-service deployment is running
kubectl exec -it -n qalitrack-prod deployment/backup-service -- ls -lh /backups

# Or connect to any pod with the backup volume
kubectl run -it --rm backup-browser --image=alpine --restart=Never \
  --overrides='{"spec":{"volumes":[{"name":"backup-storage","persistentVolumeClaim":{"claimName":"backup-storage-pvc"}}],"containers":[{"name":"backup-browser","image":"alpine","command":["sh"],"volumeMounts":[{"name":"backup-storage","mountPath":"/backups"}]}]}}' \
  -n qalitrack-prod
```

Then inside the pod:
```bash
ls -lh /backups
```

## Restore from Backup

### Download Backup

```bash
# Find a pod with access to backups
POD_NAME=$(kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=backup-service -o jsonpath='{.items[0].metadata.name}')

# List available backups
kubectl exec -n qalitrack-prod $POD_NAME -- ls -lh /backups

# Copy backup to local machine
kubectl cp qalitrack-prod/$POD_NAME:/backups/qalitrack-backup-2026-03-22.tar.gz ./qalitrack-backup-2026-03-22.tar.gz
```

### Extract Backup

```bash
tar -xzf qalitrack-backup-2026-03-22.tar.gz
```

### Restore Database

```bash
# Restore user-service database
pg_restore -h localhost -p 5432 -U postgres \
  -d qalitrack_user_service \
  -c \ # Clean (drop) database objects before recreating
  2026-03-22/user-service.backup

# Restore masterdata-service database
pg_restore -h localhost -p 5432 -U postgres \
  -d qalitrack_master_data \
  -c \
  2026-03-22/masterdata-service.backup

# Restore transaction-service database
pg_restore -h localhost -p 5432 -U postgres \
  -d qalitrack_transaction \
  -c \
  2026-03-22/transaction-service.backup
```

### Restore in Kubernetes

```bash
# Port-forward to PostgreSQL pod
kubectl port-forward -n qalitrack-prod user-service-postgresql-0 5432:5432

# Restore (in another terminal)
pg_restore -h localhost -p 5432 -U postgres \
  -d qalitrack_user_service \
  -c \
  2026-03-22/user-service.backup
```

## Verify Backup Integrity

Each backup includes a `checksums.txt` file:

```bash
# Extract backup
tar -xzf qalitrack-backup-2026-03-22.tar.gz

# Verify checksums
cd 2026-03-22
sha256sum -c checksums.txt
```

Expected output:
```
user-service.backup: OK
masterdata-service.backup: OK
transaction-service.backup: OK
```

## Storage Management

### Current Usage

```bash
kubectl exec -it -n qalitrack-prod deployment/backup-service -- df -h /backups
```

### Increase Storage

Edit `cronjob-postgres-backup.yaml`:

```yaml
spec:
  resources:
    requests:
      storage: 100Gi  # Increase from 50Gi
```

Apply changes:
```bash
kubectl apply -f cronjob-postgres-backup.yaml
```

## Backup Job History

```bash
# List all backup jobs
kubectl get jobs -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup

# View specific job details
kubectl describe job <job-name> -n qalitrack-prod
```

## Troubleshooting

### Backup Job Failed

```bash
# Check job status
kubectl get jobs -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup

# View logs of failed job
kubectl logs -n qalitrack-prod job/<failed-job-name>

# Common issues:
# 1. Database connection failed - check database is running
# 2. Insufficient storage - increase PVC size
# 3. Wrong credentials - check secrets
```

### No Backups Created

```bash
# Check CronJob exists
kubectl get cronjob -n qalitrack-prod

# Check if jobs are being created
kubectl get jobs -n qalitrack-prod

# View CronJob events
kubectl describe cronjob postgres-backup -n qalitrack-prod
```

### Storage Full

```bash
# Check current usage
kubectl exec -it -n qalitrack-prod deployment/backup-service -- du -sh /backups

# Manually delete old backups
kubectl exec -it -n qalitrack-prod deployment/backup-service -- \
  find /backups -name "qalitrack-backup-*.tar.gz" -mtime +30 -delete
```

## Off-site Backups

For disaster recovery, copy backups to external storage:

### AWS S3

```bash
# Install AWS CLI in backup pod or use separate sync job
aws s3 sync /backups s3://your-backup-bucket/qalitrack/
```

### Google Cloud Storage

```bash
gsutil -m rsync -r /backups gs://your-backup-bucket/qalitrack/
```

### Rsync to Remote Server

```bash
rsync -avz /backups/ user@backup-server:/backups/qalitrack/
```

## Monitoring Backups

### Prometheus Alert

Create alert for failed backups:

```yaml
apiVersion: monitoring.coreos.com/v1
kind: PrometheusRule
metadata:
  name: backup-alerts
  namespace: qalitrack-monitoring
spec:
  groups:
  - name: backups
    interval: 30s
    rules:
    - alert: BackupJobFailed
      expr: |
        kube_job_status_failed{job_name=~"postgres-backup.*"} > 0
      for: 5m
      labels:
        severity: critical
      annotations:
        summary: "Database backup job failed"
        description: "Backup job {{ $labels.job_name }} has failed"
```

### Check Last Successful Backup

```bash
# List recent jobs with status
kubectl get jobs -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup \
  --sort-by=.status.startTime

# Check completion time
kubectl get jobs -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup \
  -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.completionTime}{"\n"}{end}'
```

## Security

### Backup Encryption

For encrypted backups, modify the backup script:

```bash
# After creating tar.gz, encrypt it
openssl enc -aes-256-cbc -salt -pbkdf2 \
  -in qalitrack-backup-$(date +%Y-%m-%d).tar.gz \
  -out qalitrack-backup-$(date +%Y-%m-%d).tar.gz.enc \
  -k "$ENCRYPTION_PASSWORD"
```

### Access Control

Backups contain sensitive data. Limit access:

```bash
# Only allow specific service accounts to access backup PVC
kubectl create rolebinding backup-access \
  --clusterrole=admin \
  --serviceaccount=qalitrack-prod:backup-service \
  -n qalitrack-prod
```

## Best Practices

1. **Test Restores Regularly** - Verify backups work
2. **Off-site Copies** - Store backups externally
3. **Monitor Backup Jobs** - Set up alerts for failures
4. **Encrypt Sensitive Data** - Add encryption if needed
5. **Document Restore Process** - Keep restore runbook updated
6. **Version Control** - Keep backup configs in Git

## Resources

- [PostgreSQL pg_dump Documentation](https://www.postgresql.org/docs/current/app-pgdump.html)
- [Kubernetes CronJobs](https://kubernetes.io/docs/concepts/workloads/controllers/cron-jobs/)
- [PostgreSQL Backup Best Practices](https://www.postgresql.org/docs/current/backup.html)

---

**Last Updated:** March 2026
**Version:** 1.0.0
