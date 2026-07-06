# Qalitrack Kubernetes Infrastructure

Technical documentation for the Qalitrack platform Kubernetes deployment.

## Overview

The Qalitrack platform is a microservices-based weighbridge management system deployed on Kubernetes. The infrastructure utilizes Helm charts for package management, Prometheus/Grafana for observability, and supports both development and production environments.

### Architecture Components

**Core Services:**
- Gateway Service (YARP-based API Gateway)
- User Service (Authentication and user management)
- MasterData Service (Vehicles, products, suppliers)
- Transaction Service (Weighbridge transactions)
- Backup Service (Database backup management)
- QTruck Service (Fleet and driver management)

**Infrastructure:**
- PostgreSQL (per-service isolated databases)
- Redis (distributed caching)
- RabbitMQ (message queue)
- NGINX Ingress Controller (L7 load balancing and SSL termination)
- cert-manager (automated TLS certificate management)

**Observability:**
- Prometheus (metrics collection and storage)
- Grafana (metrics visualization)
- Loki (log aggregation)
- ServiceMonitors (metrics scraping configuration)

## Directory Structure

```
kubernetes/
├── argocd/                    # GitOps continuous deployment
├── backups/                   # Database backup CronJobs
├── cert-manager/              # TLS certificate management
├── ci-cd/                     # CI/CD integration scripts
├── configs/                   # ConfigMaps and configuration
├── databases/                 # Database-specific configurations
├── docs/                      # Extended documentation
├── flagger/                   # Progressive delivery (canary deployments)
├── helm-charts/               # Helm chart definitions
│   ├── backup-service/
│   ├── gateway-service/
│   ├── masterdata-service/
│   ├── qalitrack-platform/   # Umbrella chart (all services)
│   ├── qtruck-service/
│   ├── transaction-service/
│   └── user-service/
├── infrastructure/            # Base infrastructure components
├── ingress/                   # Ingress resource definitions
├── ingress-nginx/             # NGINX Ingress Controller
├── monitoring/                # Prometheus, Grafana, Loki
│   └── dashboards/           # Grafana dashboard definitions
├── namespaces/                # Namespace and ResourceQuota configs
├── secrets/                   # Secret generation and management
├── security/                  # NetworkPolicies, Pod Security
└── services/                  # Service-specific manifests
```

## Prerequisites

### Required Components

- Kubernetes cluster (v1.24 or later)
- kubectl (configured for cluster access)
- Helm 3.10 or later
- Minimum cluster capacity: 64GB RAM, 200GB storage

### Optional Components

- ArgoCD (for GitOps deployment)
- Flagger (for canary deployments)
- External DNS (for automated DNS management)

## Deployment

### Quick Deployment

For standard production deployment on a Kubernetes cluster:

```bash
# 1. Create namespaces
cd namespaces
kubectl apply -f namespace-prod.yaml

# 2. Generate and apply secrets
cd ../secrets
./generate-secrets.sh
./create-k8s-secrets.sh

# 3. Install NGINX Ingress (if not already installed)
cd ../ingress-nginx
./install.sh

# 4. Install cert-manager (for SSL)
cd ../cert-manager
./install.sh

# 5. Deploy platform
cd ../helm-charts/qalitrack-platform
helm dependency update
helm install qalitrack . --namespace qalitrack-prod --create-namespace

# 6. Install monitoring
cd ../../monitoring
./install.sh

# 7. Configure backups
cd ../backups
kubectl apply -f cronjob-postgres-backup.yaml
```

### Verification

```bash
# Check pod status
kubectl get pods -n qalitrack-prod

# Verify services
kubectl get svc -n qalitrack-prod

# Check ingress
kubectl get ingress -n qalitrack-prod

# Test API endpoint
kubectl port-forward -n qalitrack-prod svc/gateway-service 7000:7000
curl http://localhost:7000/health
```

## Network Architecture

```
Internet
    │
    ├─→ NGINX Ingress Controller
    │       │
    │       ├─→ /qalitrack/api/* → Gateway Service (port 7000)
    │       ├─→ /grafana/*       → Grafana (port 80)
    │       └─→ /prometheus/*    → Prometheus (port 80)
    │
    └─→ Gateway Service (LoadBalancer - optional direct access)
            │
            ├─→ /api/user/*        → User Service (port 7001)
            ├─→ /api/masterdata/*  → MasterData Service (port 7002)
            ├─→ /api/transaction/* → Transaction Service (port 7003)
            ├─→ /api/backup/*      → Backup Service (port 7004)
            └─→ /api/qtruck/*      → QTruck Service (port 7007)
```

## Service Communication

Services communicate internally via Kubernetes DNS:

```
<service-name>.<namespace>.svc.cluster.local
```

Examples:
- `user-service.qalitrack-prod.svc.cluster.local:7001`
- `qalitrack-postgresql.qalitrack-prod.svc.cluster.local:5432` (shared DB, all services)
- `redis-master.qalitrack-prod.svc.cluster.local:6379`

## Resource Allocation

### Default Resource Requests and Limits

| Component | CPU Request | CPU Limit | Memory Request | Memory Limit |
|-----------|-------------|-----------|----------------|--------------|
| Gateway | 100m | 250m | 128Mi | 256Mi |
| User Service | 250m | 1000m | 512Mi | 2Gi |
| MasterData | 250m | 1000m | 512Mi | 2Gi |
| Transaction | 250m | 1000m | 512Mi | 2Gi |
| Backup | 100m | 500m | 256Mi | 1Gi |
| QTruck API | 250m | 1000m | 512Mi | 2Gi |
| PostgreSQL (each) | 250m | 1000m | 1Gi | 3Gi |
| Redis | 100m | 250m | 256Mi | 512Mi |
| RabbitMQ | 100m | 250m | 256Mi | 512Mi |
| Prometheus | 250m | 1000m | 512Mi | 2Gi |
| Grafana | 100m | 250m | 256Mi | 512Mi |

**Total: ~10Gi requests, ~32Gi limits**

## Configuration Management

### Helm Values

Primary configuration is managed through Helm values files:

- `helm-charts/qalitrack-platform/values.yaml` - Main platform configuration
- `helm-charts/qalitrack-platform/values-prod.yaml` - Production overrides
- `helm-charts/qalitrack-platform/values-staging.yaml` - Staging overrides

### Secrets

Secrets are managed via Kubernetes Secret objects and generated using scripts in `secrets/`:

```bash
cd secrets
./generate-secrets.sh    # Generates secrets.env
./create-k8s-secrets.sh  # Creates Kubernetes Secret
```

**Critical secrets:**
- JWT signing keys
- PostgreSQL passwords
- Redis password
- RabbitMQ password
- SMTP credentials

### Environment-Specific Configuration

Deploy to different environments using namespace separation:

```bash
# Development
helm install qalitrack . -n qalitrack-dev -f values-dev.yaml

# Staging
helm install qalitrack . -n qalitrack-staging -f values-staging.yaml

# Production
helm install qalitrack . -n qalitrack-prod -f values-prod.yaml
```

## Observability

### Metrics Collection

Prometheus scrapes metrics from all services via ServiceMonitor resources:

```bash
# View ServiceMonitors
kubectl get servicemonitors -n qalitrack-prod

# Check Prometheus targets
kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090
# Access: http://localhost:9090/targets
```

### Dashboard Access

```bash
# Grafana
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80
# Access: http://localhost:3000
# Default credentials: admin/admin

# Prometheus
kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090
# Access: http://localhost:9090
```

### Log Aggregation

Loki aggregates logs from all pods:

```bash
# Access Loki
kubectl port-forward -n qalitrack-monitoring svc/loki 3100:3100

# Query logs via Grafana Explore
# or use LogCLI
```

## Backup and Recovery

### Automated Backups

CronJob runs daily PostgreSQL backups:

```bash
# View backup CronJob
kubectl get cronjob postgres-backup -n qalitrack-prod

# Trigger manual backup
kubectl create job --from=cronjob/postgres-backup manual-backup-$(date +%s) -n qalitrack-prod

# View backup logs
kubectl logs -n qalitrack-prod -l app=postgres-backup
```

### Backup Storage

Backups are stored in PersistentVolume with 30-day retention:

```bash
# Check backup PVC
kubectl get pvc backup-storage -n qalitrack-prod

# Access backup pod
kubectl exec -it <backup-pod> -n qalitrack-prod -- ls -lah /backups
```

### Restore Procedure

```bash
# 1. Copy backup file to PostgreSQL pod
kubectl cp /backups/<backup-file> <postgres-pod>:/tmp/backup.tar.gz -n qalitrack-prod

# 2. Restore database
kubectl exec -it <postgres-pod> -n qalitrack-prod -- bash
psql -U postgres -d <database> -f /tmp/backup.sql
```

## Security

### Network Policies

Network segmentation is enforced via NetworkPolicy resources:

```bash
# Apply network policies
kubectl apply -f security/network-policies.yaml

# View active policies
kubectl get networkpolicies -n qalitrack-prod
```

**Policy summary:**
- Default deny all ingress
- Allow ingress controller to gateway
- Allow gateway to backend services
- Allow services to databases (port-specific)
- Allow Prometheus metrics scraping
- Deny cross-namespace communication (except DNS)

### Resource Quotas

ResourceQuota prevents resource exhaustion:

```bash
# Apply resource quotas
kubectl apply -f security/resource-quotas.yaml

# Check quota usage
kubectl describe resourcequota -n qalitrack-prod
```

### TLS/SSL

cert-manager automates TLS certificate issuance from Let's Encrypt:

```bash
# Check certificate status
kubectl get certificate -n qalitrack-prod

# View certificate details
kubectl describe certificate qalitrack-tls -n qalitrack-prod

# Check cert-manager logs
kubectl logs -n cert-manager -l app=cert-manager
```

## Scaling

### Manual Scaling

```bash
# Scale specific deployment
kubectl scale deployment/transaction-service --replicas=5 -n qalitrack-prod

# Scale via Helm
helm upgrade qalitrack . --set transaction.replicaCount=5 --reuse-values -n qalitrack-prod
```

### Horizontal Pod Autoscaling

HPA is configured for services based on CPU utilization:

```bash
# View HPA status
kubectl get hpa -n qalitrack-prod

# Describe HPA
kubectl describe hpa transaction-service -n qalitrack-prod
```

**Default HPA configuration:**
- Min replicas: 2
- Max replicas: 5
- Target CPU utilization: 70%

## Updates and Deployments

### Rolling Updates

Deployments use RollingUpdate strategy with zero downtime:

```yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxSurge: 1
    maxUnavailable: 0
```

### Update Procedure

```bash
# 1. Update image tag in values.yaml
vim helm-charts/qalitrack-platform/values.yaml

# 2. Upgrade release
helm upgrade qalitrack . -n qalitrack-prod

# 3. Monitor rollout
kubectl rollout status deployment/gateway-service -n qalitrack-prod

# 4. Rollback if needed
kubectl rollout undo deployment/gateway-service -n qalitrack-prod
```

### Canary Deployments

Flagger enables automated canary deployments with progressive traffic shifting:

```bash
# Check canary status
kubectl get canary -n qalitrack-prod

# View canary events
kubectl describe canary gateway-service -n qalitrack-prod
```

## Troubleshooting

### Common Issues

**Pod CrashLoopBackOff:**
```bash
kubectl describe pod <pod-name> -n qalitrack-prod
kubectl logs <pod-name> -n qalitrack-prod --previous
```

**Service Unreachable:**
```bash
kubectl get endpoints <service-name> -n qalitrack-prod
kubectl describe svc <service-name> -n qalitrack-prod
```

**Database Connection Failures:**
```bash
# Test database connectivity
kubectl exec -it <service-pod> -n qalitrack-prod -- nc -zv <db-service> 5432

# Check PostgreSQL logs
kubectl logs -n qalitrack-prod <postgres-pod>
```

**Ingress Issues:**
```bash
kubectl describe ingress qalitrack-ingress -n qalitrack-prod
kubectl logs -n ingress-nginx -l app.kubernetes.io/name=ingress-nginx-controller
```

### Debug Commands

```bash
# Get all resources in namespace
kubectl get all -n qalitrack-prod

# Check events
kubectl get events -n qalitrack-prod --sort-by='.lastTimestamp'

# Exec into pod
kubectl exec -it <pod-name> -n qalitrack-prod -- /bin/bash

# Port forward for local access
kubectl port-forward <pod-name> 8080:7000 -n qalitrack-prod

# View resource usage
kubectl top pods -n qalitrack-prod
kubectl top nodes
```

## Documentation

- [Deployment Guide](DEPLOYMENT.md) - Comprehensive deployment procedures
- [Monitoring Guide](MONITORING.md) - Observability and dashboard access
- [Architecture](docs/ARCHITECTURE.md) - System architecture and design
- [Workflows](docs/WORKFLOWS.md) - CI/CD and operational workflows
- [Adding Services](docs/ADD_NEW_MICROSERVICE.md) - Guide for adding new microservices
- [Service Updates](docs/UPDATE_SERVICES.md) - Deploying code changes

## Support

For infrastructure issues, check component-specific README files in subdirectories.

**Version:** 1.0.0
**Last Updated:** 2026-03
**Kubernetes:** 1.24+
**Helm:** 3.10+
