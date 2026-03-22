# Adding a New Microservice to Qalitrack Platform

Complete guide to adding a new microservice to the Kubernetes deployment.

## Overview

When adding a new microservice, you need to:
1. Create Helm chart for the service
2. Add to platform umbrella chart
3. Update Gateway routing (if needed)
4. Add to backup systems
5. Configure monitoring
6. Update ArgoCD (if using)
7. Configure CI/CD

## Step-by-Step Guide

### Example: Adding "Reporting Service"

Let's walk through adding a new "reporting-service" to the platform.

---

## Step 1: Create Service Helm Chart

### 1.1 Create Chart Directory

```bash
cd kubernetes/helm-charts
mkdir -p reporting-service/templates
```

### 1.2 Create Chart.yaml

`kubernetes/helm-charts/reporting-service/Chart.yaml`:

```yaml
apiVersion: v2
name: reporting-service
description: Qalitrack Reporting Service
type: application
version: 1.0.0
appVersion: "1.0.0"

keywords:
  - qalitrack
  - reporting

maintainers:
  - name: Qalitrack Team
    email: dev@qalitrack.com

dependencies:
  - name: postgresql
    version: "12.x.x"
    repository: https://charts.bitnami.com/bitnami
    condition: postgresql.enabled
```

### 1.3 Create values.yaml

`kubernetes/helm-charts/reporting-service/values.yaml`:

```yaml
replicaCount: 2

image:
  repository: ghcr.io/qalitrack/qalitrackservices/reporting-service
  pullPolicy: Always
  tag: "latest"

service:
  type: ClusterIP
  port: 7005
  targetPort: 80

resources:
  requests:
    memory: "512Mi"
    cpu: "250m"
  limits:
    memory: "2Gi"
    cpu: "1000m"

env:
  - name: ASPNETCORE_ENVIRONMENT
    value: "Production"
  - name: ASPNETCORE_URLS
    value: "http://+:80"
  - name: ConnectionStrings__DefaultConnection
    value: "Host=reporting-service-postgresql;Port=5432;Database=qalitrack_reporting;Username=postgres;Password=$(POSTGRES_PASSWORD)"
  - name: ConnectionStrings__Redis
    value: "redis-master:6379"
  - name: JWT_SECRET_KEY
    valueFrom:
      secretKeyRef:
        name: qalitrack-secrets
        key: JWT_SECRET_KEY

postgresql:
  enabled: true
  auth:
    postgresPassword: "changeme"
    database: "qalitrack_reporting"

  primary:
    resources:
      requests:
        memory: "1Gi"
        cpu: "250m"
      limits:
        memory: "3Gi"
        cpu: "1000m"

    persistence:
      enabled: true
      size: 20Gi
```

### 1.4 Create Kubernetes Templates

`kubernetes/helm-charts/reporting-service/templates/deployment.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ include "reporting-service.fullname" . }}
  namespace: {{ .Release.Namespace }}
  labels:
    {{- include "reporting-service.labels" . | nindent 4 }}
spec:
  replicas: {{ .Values.replicaCount }}
  selector:
    matchLabels:
      {{- include "reporting-service.selectorLabels" . | nindent 6 }}
  template:
    metadata:
      labels:
        {{- include "reporting-service.selectorLabels" . | nindent 8 }}
    spec:
      containers:
      - name: {{ .Chart.Name }}
        image: "{{ .Values.image.repository }}:{{ .Values.image.tag }}"
        imagePullPolicy: {{ .Values.image.pullPolicy }}
        ports:
        - name: http
          containerPort: {{ .Values.service.targetPort }}
          protocol: TCP
        env:
        {{- toYaml .Values.env | nindent 8 }}
        resources:
          {{- toYaml .Values.resources | nindent 10 }}
        livenessProbe:
          httpGet:
            path: /health
            port: http
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: http
          initialDelaySeconds: 10
          periodSeconds: 5
```

`kubernetes/helm-charts/reporting-service/templates/service.yaml`:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: {{ include "reporting-service.fullname" . }}
  namespace: {{ .Release.Namespace }}
  labels:
    {{- include "reporting-service.labels" . | nindent 4 }}
spec:
  type: {{ .Values.service.type }}
  ports:
    - port: {{ .Values.service.port }}
      targetPort: {{ .Values.service.targetPort }}
      protocol: TCP
      name: http
  selector:
    {{- include "reporting-service.selectorLabels" . | nindent 4 }}
```

`kubernetes/helm-charts/reporting-service/templates/_helpers.tpl`:

```yaml
{{/*
Expand the name of the chart.
*/}}
{{- define "reporting-service.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "reporting-service.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "reporting-service.labels" -}}
helm.sh/chart: {{ include "reporting-service.chart" . }}
{{ include "reporting-service.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/part-of: qalitrack
{{- end }}

{{/*
Selector labels
*/}}
{{- define "reporting-service.selectorLabels" -}}
app.kubernetes.io/name: {{ include "reporting-service.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "reporting-service.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}
```

---

## Step 2: Add to Platform Umbrella Chart

Edit `kubernetes/helm-charts/qalitrack-platform/values.yaml`:

```yaml
# Add after transaction section:

# ============================================================================
# REPORTING SERVICE - Reports & Analytics
# ============================================================================
reportingService:
  enabled: true
  replicaCount: 2

  image:
    repository: reporting-service
    tag: latest
    pullPolicy: Always

  resources:
    requests:
      memory: "512Mi"
      cpu: "250m"
    limits:
      memory: "2Gi"
      cpu: "1000m"

  service:
    type: ClusterIP
    port: 7005
    targetPort: 80

  env:
    ASPNETCORE_ENVIRONMENT: "Production"
    ASPNETCORE_URLS: "http://+:80"
    UsePostgreSQL: "true"
    UseRedis: "true"

  postgresql:
    enabled: true
    primary:
      resources:
        requests:
          memory: "1Gi"
          cpu: "250m"
        limits:
          memory: "3Gi"
          cpu: "1000m"
      persistence:
        size: 20Gi
```

Edit `kubernetes/helm-charts/qalitrack-platform/Chart.yaml`:

```yaml
dependencies:
  # ... existing dependencies ...

  - name: reporting-service
    version: "1.0.0"
    repository: "file://../reporting-service"
    condition: reportingService.enabled
```

---

## Step 3: Update Gateway Routing

If your Gateway needs to route to this service, update Gateway configuration.

**Option A: Environment Variable (if Gateway auto-discovers)**

Add to Gateway env in `qalitrack-platform/values.yaml`:

```yaml
gateway:
  env:
    # ... existing env vars ...
    REPORTING_SERVICE_URL: "http://reporting-service:7005"
```

**Option B: Manual Gateway Configuration**

If you have a `gateway-config.yaml` or similar:

```yaml
routes:
  - path: "/api/reporting"
    service: "reporting-service"
    port: 7005
    stripPrefix: true
```

---

## Step 4: Add to Backup Systems

### 4.1 Update CronJob Backup

Edit `kubernetes/backups/cronjob-postgres-backup.yaml`:

Add new database backup section:

```yaml
# Backup Reporting Service DB
echo "Backing up reporting-service database..."
PGPASSWORD=$REPORTING_DB_PASSWORD pg_dump \
  -h $REPORTING_DB_HOST \
  -p 5432 \
  -U postgres \
  -d qalitrack_reporting \
  -F c \
  -f $BACKUP_DIR/reporting-service.backup
```

Add environment variables:

```yaml
env:
  - name: REPORTING_DB_HOST
    value: "reporting-service-postgresql"
  - name: REPORTING_DB_PASSWORD
    valueFrom:
      secretKeyRef:
        name: qalitrack-secrets
        key: REPORTING_DB_PASSWORD
```

### 4.2 Register with BackupService

```bash
# Port-forward to backup service
kubectl port-forward -n qalitrack-prod svc/backup-service 7004:7004

# Register new microservice
curl -X POST http://localhost:7004/api/Microservice/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "reporting-service",
    "connectionString": "Host=reporting-service-postgresql;Port=5432;Database=qalitrack_reporting;Username=postgres;Password=...",
    "backupPath": "/backups/reporting-service",
    "status": "Active"
  }'
```

---

## Step 5: Add Monitoring

### 5.1 Create ServiceMonitor

`kubernetes/monitoring/servicemonitors.yaml`:

```yaml
---
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: reporting-service
  namespace: qalitrack-prod
  labels:
    app.kubernetes.io/name: reporting-service
    app.kubernetes.io/part-of: qalitrack
spec:
  selector:
    matchLabels:
      app.kubernetes.io/name: reporting-service
  endpoints:
  - port: http
    path: /metrics
    interval: 30s
```

### 5.2 Add to Grafana Dashboard

Add panels to `kubernetes/monitoring/dashboards/qalitrack-overview.json` for the new service.

---

## Step 6: Update ArgoCD (if using)

Edit `kubernetes/argocd/qalitrack-application.yaml`:

The umbrella chart automatically includes new services, but verify:

```bash
# Check ArgoCD sync status
kubectl get application qalitrack-platform -n argocd

# Force sync if needed
argocd app sync qalitrack-platform
```

---

## Step 7: Configure CI/CD

### 7.1 Create GitHub Actions Workflow

`.github/workflows/build-reporting-service.yml`:

```yaml
name: Build Reporting Service

on:
  push:
    branches: [main]
    paths:
      - 'packages/microservices/reporting/**'
      - '.github/workflows/build-reporting-service.yml'
  workflow_dispatch:

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}/reporting-service

jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Log in to Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=ref,event=branch
            type=sha
            type=raw,value=latest,enable={{is_default_branch}}

      - name: Build and push Docker image
        uses: docker/build-push-action@v5
        with:
          context: ./packages/microservices/reporting
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
```

### 7.2 Add Deployment Workflow (Optional)

`.github/workflows/deploy-reporting-service.yml`:

```yaml
name: Deploy Reporting Service

on:
  workflow_run:
    workflows: ["Build Reporting Service"]
    types: [completed]
    branches: [main]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Configure kubectl
        uses: azure/setup-kubectl@v3

      - name: Set kubeconfig
        run: |
          mkdir -p ~/.kube
          echo "${{ secrets.KUBECONFIG }}" > ~/.kube/config

      - name: Restart deployment
        run: |
          kubectl rollout restart deployment/reporting-service -n qalitrack-prod
          kubectl rollout status deployment/reporting-service -n qalitrack-prod --timeout=5m
```

---

## Step 8: Add to Flagger (Optional)

For canary deployments with automatic rollback:

`kubernetes/flagger/reporting-canary.yaml`:

```yaml
apiVersion: flagger.app/v1beta1
kind: Canary
metadata:
  name: reporting-service
  namespace: qalitrack-prod
spec:
  targetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: reporting-service
  service:
    port: 7005
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

---

## Step 9: Deploy

### Install Individual Service

```bash
cd kubernetes/helm-charts/reporting-service

helm install reporting-service . \
  --namespace qalitrack-prod \
  --create-namespace
```

### Or Update Platform

```bash
cd kubernetes/helm-charts/qalitrack-platform

# Update dependencies
helm dependency update

# Upgrade platform
helm upgrade qalitrack-platform . \
  --namespace qalitrack-prod \
  --install
```

---

## Verification Checklist

After deployment, verify:

```bash
# 1. Check pods are running
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=reporting-service

# 2. Check service
kubectl get svc -n qalitrack-prod reporting-service

# 3. Check logs
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=reporting-service

# 4. Test health endpoint
kubectl port-forward -n qalitrack-prod svc/reporting-service 7005:7005
curl http://localhost:7005/health

# 5. Check database
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=reporting-service-postgresql

# 6. Verify monitoring
kubectl get servicemonitor -n qalitrack-prod reporting-service

# 7. Check ArgoCD sync (if using)
kubectl get application -n argocd qalitrack-platform

# 8. Verify backups registered
# (Check via BackupService API)
```

---

## Quick Reference

### File Locations

```
kubernetes/
├── helm-charts/
│   ├── reporting-service/           # New service chart
│   │   ├── Chart.yaml
│   │   ├── values.yaml
│   │   └── templates/
│   │       ├── deployment.yaml
│   │       ├── service.yaml
│   │       └── _helpers.tpl
│   └── qalitrack-platform/
│       ├── Chart.yaml              # Add dependency here
│       └── values.yaml             # Add service config here
├── backups/
│   └── cronjob-postgres-backup.yaml  # Add DB backup
├── monitoring/
│   └── servicemonitors.yaml        # Add ServiceMonitor
└── flagger/
    └── reporting-canary.yaml       # Optional: canary deployment

.github/workflows/
├── build-reporting-service.yml     # Build workflow
└── deploy-reporting-service.yml    # Deploy workflow (optional)
```

### Port Allocation

Keep track of service ports:

```
7000 - Gateway
7001 - User Service
7002 - MasterData Service
7003 - Transaction Service
7004 - Backup Service
7005 - Reporting Service  ← New
7006 - Next service...
```

---

## Common Issues

### Issue: Pods not starting

```bash
# Check events
kubectl describe pod <pod-name> -n qalitrack-prod

# Common causes:
# - Image pull errors (check image name/tag)
# - Resource limits (check if node has capacity)
# - Configuration errors (check ConfigMaps/Secrets)
```

### Issue: Service not accessible

```bash
# Check service endpoints
kubectl get endpoints reporting-service -n qalitrack-prod

# Should show pod IPs. If empty, selector labels don't match
```

### Issue: Database connection failed

```bash
# Check PostgreSQL pod
kubectl get pods -n qalitrack-prod | grep reporting-service-postgresql

# Check connection from service pod
kubectl exec -it <service-pod> -n qalitrack-prod -- \
  psql -h reporting-service-postgresql -U postgres -d qalitrack_reporting
```

---

**Last Updated:** March 2026
**Version:** 1.0.0
