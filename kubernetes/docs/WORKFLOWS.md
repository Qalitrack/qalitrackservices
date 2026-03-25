# Qalitrack Platform Workflows

Operational workflows, CI/CD processes, and development lifecycle for the Qalitrack Kubernetes platform.

## Development Workflow

### Local Development

**Environment Setup:**

```bash
# 1. Clone repository
git clone https://github.com/qalitrack/qalitrackservices.git
cd qalitrackservices

# 2. Install dependencies
cd packages/qalitrack-gateway
dotnet restore

# 3. Run locally
dotnet run
```

**Local Testing Against Kubernetes:**

```bash
# Port-forward dependencies from cluster
kubectl port-forward -n qalitrack-prod svc/user-service-postgresql 5432:5432 &
kubectl port-forward -n qalitrack-prod svc/redis-master 6379:6379 &

# Update connection strings in appsettings.Development.json
# Run service locally
dotnet run --environment Development
```

### Code Changes to Production

```
Developer Workstation
         │
         │ 1. Write code
         │ 2. Test locally
         │ 3. Commit to feature branch
         ▼
    GitHub Repository
         │
         │ 4. Create Pull Request
         │ 5. Code review
         │ 6. Merge to main
         ▼
    GitHub Actions
         │
         │ 7. Build .NET application
         │ 8. Run unit tests
         │ 9. Build Docker image
         │10. Push to GHCR
         ▼
    Container Registry (ghcr.io)
         │
         │11. Image available
         ▼
    Kubernetes Cluster
         │
         │12. Manual rollout restart
         │    OR
         │13. ArgoCD auto-sync
         ▼
    Production Pods
```

## CI/CD Pipeline

### GitHub Actions Workflow

**Trigger:** Push to `main` branch or manual dispatch

**Pipeline Stages:**

1. **Build Stage**
   ```yaml
   - Checkout code
   - Setup .NET 8.0 SDK
   - Restore dependencies
   - Build application
   ```

2. **Test Stage**
   ```yaml
   - Run unit tests
   - Generate code coverage report
   - Upload test results
   ```

3. **Docker Build Stage**
   ```yaml
   - Login to GHCR
   - Build Docker image
   - Tag with commit SHA and 'latest'
   - Push to registry
   ```

4. **Deploy Stage** (Optional)
   ```yaml
   - Setup kubectl
   - Update deployment image
   - Rollout restart
   ```

**Example Workflow (.github/workflows/gateway-service.yml):**

```yaml
name: Gateway Service CI/CD

on:
  push:
    branches: [main]
    paths:
      - 'packages/qalitrack-gateway/**'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore packages/qalitrack-gateway

      - name: Build
        run: dotnet build packages/qalitrack-gateway --no-restore

      - name: Test
        run: dotnet test packages/qalitrack-gateway --no-build --verbosity normal

      - name: Login to GHCR
        uses: docker/login-action@v2
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build and push Docker image
        uses: docker/build-push-action@v4
        with:
          context: packages/qalitrack-gateway
          push: true
          tags: |
            ghcr.io/qalitrack/gateway-service:latest
            ghcr.io/qalitrack/gateway-service:${{ github.sha }}

      - name: Deploy to Kubernetes (if enabled)
        if: ${{ vars.ENABLE_K8S_DEPLOY == 'true' }}
        run: |
          kubectl rollout restart deployment/gateway-service -n qalitrack-prod
```

### Image Tagging Strategy

**Tags Applied:**
- `latest` - Most recent build from main
- `<commit-sha>` - Specific commit (e.g., `a1b2c3d`)
- `v<version>` - Semantic version (e.g., `v1.2.3`)

**Example:**
```
ghcr.io/qalitrack/gateway-service:latest
ghcr.io/qalitrack/gateway-service:a1b2c3d4e5f
ghcr.io/qalitrack/gateway-service:v1.0.0
```

### Deployment Strategies

#### Manual Deployment

```bash
# 1. Trigger CI/CD pipeline (push to main)
git push origin main

# 2. Wait for image build (2-5 minutes)
# Check: https://github.com/qalitrack/qalitrackservices/actions

# 3. Update deployment
kubectl set image deployment/gateway-service \
  gateway=ghcr.io/qalitrack/gateway-service:a1b2c3d \
  -n qalitrack-prod

# 4. Monitor rollout
kubectl rollout status deployment/gateway-service -n qalitrack-prod
```

#### Rolling Update

```bash
# Helm-based update
helm upgrade qalitrack . \
  --set gateway.image.tag=v1.1.0 \
  --reuse-values \
  -n qalitrack-prod

# Kubernetes-native
kubectl rollout restart deployment/gateway-service -n qalitrack-prod
```

#### Canary Deployment (Flagger)

```yaml
# Canary resource automatically manages progressive rollout
apiVersion: flagger.app/v1beta1
kind: Canary
metadata:
  name: gateway-service
spec:
  targetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: gateway-service
  progressDeadlineSeconds: 600
  service:
    port: 7000
  analysis:
    interval: 1m
    threshold: 5
    maxWeight: 50
    stepWeight: 10
    metrics:
      - name: request-success-rate
        thresholdRange:
          min: 99
      - name: request-duration
        thresholdRange:
          max: 500
```

**Workflow:**
1. New image deployed → Canary pod created
2. 10% traffic routed to canary
3. Metrics analyzed for 1 minute
4. If metrics pass → increase to 20%
5. Repeat until 50%
6. Final promotion to 100%
7. Old pods terminated

**Rollback:** Automatic if metrics fail

## GitOps Workflow (ArgoCD)

### Setup ArgoCD Application

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: qalitrack-platform
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/qalitrack/qalitrackservices.git
    targetRevision: main
    path: kubernetes/helm-charts/qalitrack-platform
    helm:
      valueFiles:
        - values-prod.yaml
  destination:
    server: https://kubernetes.default.svc
    namespace: qalitrack-prod
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
```

### GitOps Deployment Flow

```
Git Repository (Source of Truth)
         │
         │ 1. Developer pushes Helm chart changes
         ▼
    GitHub Main Branch
         │
         │ 2. ArgoCD polls repository (every 3 minutes)
         │    or webhook triggers sync
         ▼
    ArgoCD Controller
         │
         │ 3. Detect drift between Git and cluster
         │ 4. Generate Kubernetes manifests
         ▼
    Kubernetes API Server
         │
         │ 5. Apply changes
         ▼
    Cluster State Updated
```

### Manual Sync

```bash
# Trigger sync via CLI
argocd app sync qalitrack-platform

# Via UI
# Access ArgoCD UI → Select application → Click "Sync"

# Force sync (ignore errors)
argocd app sync qalitrack-platform --force
```

### Rollback via GitOps

```bash
# Revert Git commit
git revert <commit-hash>
git push origin main

# ArgoCD automatically syncs to previous state
```

## Backup and Restore Workflow

### Automated Backup

**Sc hedule:** Daily at 02:00 UTC (CronJob)

**Process:**
```
02:00 UTC - CronJob Triggered
     │
     ▼
Backup Pod Created
     │
     ├─> 1. Connect to PostgreSQL (user-service-postgresql)
     ├─> 2. Run pg_dump
     ├─> 3. Compress with gzip
     ├─> 4. Calculate SHA256 checksum
     ├─> 5. Store in PVC (/backups/user-service/backup-YYYY-MM-DD.tar.gz)
     ├─> 6. Repeat for other databases
     ├─> 7. Cleanup backups older than 30 days
     └─> 8. Pod terminates
```

**Verification:**
```bash
# Check CronJob
kubectl get cronjob postgres-backup -n qalitrack-prod

# View backup logs
kubectl logs -n qalitrack-prod -l app=postgres-backup --tail=100

# List backups
kubectl exec -n qalitrack-prod -it <backup-pod> -- ls -lah /backups
```

### Manual Backup

```bash
# Trigger backup job
kubectl create job --from=cronjob/postgres-backup \
  manual-backup-$(date +%s) -n qalitrack-prod

# Monitor job
kubectl get jobs -n qalitrack-prod -l app=postgres-backup
kubectl logs -n qalitrack-prod -l job-name=manual-backup-<timestamp>
```

### Restore Workflow

```
Backup File (/backups/user-service/backup-2026-03-25.tar.gz)
     │
     ▼
1. Copy backup to PostgreSQL pod
     │
     ▼
2. Extract backup
     │
     ▼
3. Stop application pods (scale to 0)
     │
     ▼
4. Drop existing database
     │
     ▼
5. Restore from backup SQL
     │
     ▼
6. Verify restore
     │
     ▼
7. Start application pods (scale to N)
     │
     ▼
8. Verify application functionality
```

**Commands:**
```bash
# 1. Scale down services
kubectl scale deployment/user-service --replicas=0 -n qalitrack-prod

# 2. Copy backup
kubectl cp /backups/user-service/backup-2026-03-25.tar.gz \
  qalitrack-prod/user-service-postgresql-0:/tmp/backup.tar.gz

# 3. Extract and restore
kubectl exec -it user-service-postgresql-0 -n qalitrack-prod -- bash
tar -xzf /tmp/backup.tar.gz -C /tmp
psql -U postgres -d postgres -c "DROP DATABASE IF EXISTS user_service_db;"
psql -U postgres -d postgres -c "CREATE DATABASE user_service_db;"
psql -U postgres -d user_service_db -f /tmp/backup.sql

# 4. Scale up services
kubectl scale deployment/user-service --replicas=2 -n qalitrack-prod

# 5. Verify
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=user-service
```

## Monitoring and Alerting Workflow

### Metrics Collection Flow

```
Application Pod
     │
     ├─> /metrics endpoint (Prometheus format)
     │
     ▼
ServiceMonitor Resource
     │
     │ (Defines scrape config)
     ▼
Prometheus Server
     │
     ├─> Scrapes every 30s
     ├─> Stores time-series data
     ├─> Evaluates alert rules
     │
     ├─> AlertManager (if alert fires)
     │   └─> Send notification (email, Slack, PagerDuty)
     │
     └─> Grafana (visualization)
         └─> Dashboards, graphs, queries
```

### Alert Workflow

```
Metric Threshold Exceeded
     │
     ▼
Prometheus Alert Rule Triggered
     │
     ▼
Alert in "Pending" state (for 5 minutes)
     │
     ▼
Alert in "Firing" state
     │
     ▼
Sent to AlertManager
     │
     ├─> Apply routing rules
     ├─> Apply silences (if any)
     ├─> Group similar alerts
     ├─> Apply throttling
     │
     ▼
Send Notification
     │
     ├─> Email
     ├─> Slack
     ├─> PagerDuty
     └─> Webhook
     │
     ▼
On-Call Engineer Notified
     │
     ▼
Investigate and Resolve
     │
     ▼
Alert Auto-Resolves (metric returns to normal)
```

### Dashboard Access Workflow

```bash
# Option 1: Port forwarding
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80

# Option 2: Ingress (production)
# Access: https://qalibrated.co.ke/grafana

# Login
# Username: admin
# Password: <from secret or set during install>

# Navigate to Dashboards → Browse → Select dashboard
```

## Scaling Workflow

### Horizontal Pod Autoscaling (HPA)

**Automatic Scaling:**

```
Pod CPU Utilization > 70%
     │
     ▼
HPA Detects High Load
     │
     ▼
Calculate Desired Replicas
     │ desired = ceil(current * (current_metric / target_metric))
     │ Example: ceil(2 * (85% / 70%)) = ceil(2.43) = 3
     ▼
Update Deployment Replica Count
     │
     ▼
Kubernetes Creates New Pods
     │
     ├─> Image pulled
     ├─> Container started
     ├─> Health checks pass
     │
     ▼
New Pod Receives Traffic
     │
     ▼
Load Distributed Across Pods
     │
     ▼
CPU Utilization Decreases
```

**Manual Scaling:**

```bash
# Scale up
kubectl scale deployment/transaction-service --replicas=10 -n qalitrack-prod

# Scale down
kubectl scale deployment/transaction-service --replicas=2 -n qalitrack-prod

# Via Helm
helm upgrade qalitrack . \
  --set transaction.replicaCount=10 \
  --reuse-values \
  -n qalitrack-prod
```

### Vertical Scaling

**Resource Adjustment Workflow:**

```bash
# 1. Monitor current usage
kubectl top pods -n qalitrack-prod

# 2. Update resource limits
helm upgrade qalitrack . \
  --set transaction.resources.requests.memory=1Gi \
  --set transaction.resources.limits.memory=4Gi \
  --reuse-values \
  -n qalitrack-prod

# 3. Rolling restart applies new limits
kubectl rollout status deployment/transaction-service -n qalitrack-prod

# 4. Verify new resources
kubectl describe pod <pod-name> -n qalitrack-prod | grep -A 5 "Limits"
```

## Incident Response Workflow

### Service Outage

```
Alert Triggered (Service Down)
     │
     ▼
1. Acknowledge Alert
     │
     ▼
2. Check Pod Status
     │ kubectl get pods -n qalitrack-prod
     │ kubectl describe pod <pod-name>
     ▼
3. Check Logs
     │ kubectl logs <pod-name> -n qalitrack-prod
     │ kubectl logs <pod-name> -n qalitrack-prod --previous
     ▼
4. Identify Root Cause
     │
     ├─> Image Pull Error → Fix registry access
     ├─> CrashLoopBackOff → Fix application code/config
     ├─> OOMKilled → Increase memory limits
     ├─> Database Connection → Check DB pod, credentials
     └─> Other → Investigate further
     │
     ▼
5. Apply Fix
     │
     ├─> Update configuration
     ├─> Rollback to previous version
     ├─> Scale resources
     └─> Restart pods
     │
     ▼
6. Verify Resolution
     │ kubectl get pods -n qalitrack-prod
     │ Test API endpoints
     ▼
7. Document Incident
     │
     ▼
8. Post-Mortem (if major incident)
```

### Database Connection Failure

```
Application Logs: "Connection Refused"
     │
     ▼
1. Check Database Pod Status
     │ kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=postgresql
     ▼
2. Check Database Logs
     │ kubectl logs user-service-postgresql-0 -n qalitrack-prod
     ▼
3. Test Connectivity
     │ kubectl exec -it deployment/user-service -n qalitrack-prod -- nc -zv user-service-postgresql 5432
     ▼
4. Verify Credentials
     │ kubectl get secret qalitrack-secrets -n qalitrack-prod -o yaml
     ▼
5. Check NetworkPolicy
     │ kubectl get networkpolicies -n qalitrack-prod
     ▼
6. Resolve Issue
     │
     ├─> Database pod down → Restart or restore
     ├─> Wrong credentials → Update secret
     ├─> NetworkPolicy blocking → Adjust policy
     └─> Connection pool exhausted → Increase pool size
     │
     ▼
7. Verify Connection
     │ kubectl exec -it deployment/user-service -n qalitrack-prod -- curl http://localhost/health
```

## Update and Maintenance Workflows

### Kubernetes Cluster Upgrade

```
1. Plan Upgrade
     │ Check Kubernetes release notes
     │ Verify compatibility with applications
     │ Schedule maintenance window
     ▼
2. Backup Current State
     │ Backup all databases
     │ Export Kubernetes resources: kubectl get all --all-namespaces -o yaml > backup.yaml
     ▼
3. Upgrade Control Plane
     │ (Cloud provider: managed upgrade)
     │ (Self-hosted: kubeadm upgrade)
     ▼
4. Upgrade Worker Nodes
     │ Drain node
     │ Upgrade node
     │ Uncordon node
     │ Repeat for each node
     ▼
5. Verify Cluster
     │ kubectl get nodes
     │ kubectl get pods --all-namespaces
     ▼
6. Test Applications
     │ Smoke tests on critical endpoints
     ▼
7. Monitor for Issues
     │ Check Grafana dashboards
     │ Review logs
```

### Application Dependency Updates

**Example: Update PostgreSQL Version**

```bash
# 1. Update Helm chart dependency version
# Edit helm-charts/qalitrack-platform/Chart.yaml
dependencies:
  - name: postgresql
    version: 13.0.0  # Updated from 12.x
    repository: https://charts.bitnami.com/bitnami

# 2. Update Helm dependencies
helm dependency update

# 3. Test in staging
helm upgrade qalitrack . -n qalitrack-staging

# 4. Backup production databases
kubectl create job --from=cronjob/postgres-backup \
  pre-upgrade-backup-$(date +%s) -n qalitrack-prod

# 5. Deploy to production
helm upgrade qalitrack . -n qalitrack-prod

# 6. Verify
kubectl get pods -n qalitrack-prod
kubectl exec -it user-service-postgresql-0 -n qalitrack-prod -- psql -U postgres -c "SELECT version();"
```

## Adding a New Microservice

**Workflow:**

```
1. Develop Service
     │ Create .NET project
     │ Implement API endpoints
     │ Add health checks, metrics
     │ Create Dockerfile
     ▼
2. Create Helm Chart
     │ Copy template from existing service
     │ Update values.yaml
     │ Configure service-specific settings
     ▼
3. Update Platform Chart
     │ Add service to qalitrack-platform/values.yaml
     │ Add service to qalitrack-platform/templates/
     │ Configure gateway routing
     ▼
4. Update CI/CD
     │ Create GitHub Actions workflow
     │ Configure image build and push
     ▼
5. Deploy to Development
     │ helm upgrade qalitrack . -n qalitrack-dev
     ▼
6. Configure Monitoring
     │ Add ServiceMonitor
     │ Create Grafana dashboard
     │ Add alert rules
     ▼
7. Update Documentation
     │ Add service to architecture docs
     │ Update port mappings
     │ Document API endpoints
     ▼
8. Deploy to Staging
     │ Test end-to-end
     │ Performance testing
     ▼
9. Deploy to Production
     │ Coordinate deployment window
     │ Deploy via GitOps or Helm
     │ Monitor metrics
```

**Detailed guide:** See `docs/ADD_NEW_MICROSERVICE.md`

## Service Removal Workflow

```
1. Deprecation Notice
     │ Announce removal timeline
     │ Document alternatives
     ▼
2. Disable Service in Gateway
     │ Remove routing rules
     │ Return 410 Gone for endpoints
     ▼
3. Monitor for Usage
     │ Check access logs
     │ Identify remaining clients
     ▼
4. Scale to Zero
     │ kubectl scale deployment/<service> --replicas=0
     ▼
5. Backup Data
     │ Final database backup
     │ Export to archive storage
     ▼
6. Remove from Helm Chart
     │ Delete service configuration
     │ Remove dependencies
     ▼
7. Delete Resources
     │ kubectl delete deployment/<service>
     │ kubectl delete svc/<service>
     │ kubectl delete pvc/<service-db-pvc>
     ▼
8. Update Documentation
     │ Remove from architecture diagrams
     │ Update port mappings
     │ Archive API docs
```

## Summary

This document covers the operational workflows for the Qalitrack platform. For specific component details, refer to:

- **Architecture:** `docs/ARCHITECTURE.md`
- **Deployment:** `DEPLOYMENT.md`
- **Monitoring:** `MONITORING.md`
- **Adding Services:** `docs/ADD_NEW_MICROSERVICE.md`
- **Updating Services:** `docs/UPDATE_SERVICES.md`

**Version:** 1.0.0
**Last Updated:** 2026-03
