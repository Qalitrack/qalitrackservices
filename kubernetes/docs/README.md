# Qalitrack Kubernetes Documentation

Complete documentation for deploying and managing Qalitrack on Kubernetes.

## Quick Links

- **[Quick Start Guide](../QUICKSTART.md)** - Deploy Qalitrack in 30-45 minutes
- **[Add New Microservice](./ADD_NEW_MICROSERVICE.md)** - Step-by-step guide to add new services
- **[Update Services](./UPDATE_SERVICES.md)** - How to deploy code changes

## Documentation Index

### Getting Started

1. **[QUICKSTART.md](../QUICKSTART.md)**
   - Complete deployment guide (30-45 minutes)
   - Prerequisites and preparation
   - Step-by-step installation
   - Verification and troubleshooting

### Operations

2. **[UPDATE_SERVICES.md](./UPDATE_SERVICES.md)**
   - Deploying code changes
   - Updating Gateway and microservices
   - Rolling updates and rollbacks
   - CI/CD pipeline usage
   - **Read this when:** You've made code changes and need to deploy

3. **[ADD_NEW_MICROSERVICE.md](./ADD_NEW_MICROSERVICE.md)**
   - Adding new services to the platform
   - Helm chart creation
   - Gateway routing configuration
   - Backup and monitoring setup
   - **Read this when:** You're adding a new backend service

### Components

4. **[ArgoCD](../argocd/README.md)**
   - GitOps automation
   - Auto-deployment from Git
   - Application management

5. **[Flagger](../flagger/README.md)**
   - Progressive delivery
   - Canary deployments
   - Automatic rollbacks

6. **[Monitoring](../monitoring/README.md)**
   - Prometheus metrics collection
   - Grafana dashboards
   - AlertManager configuration

7. **[Backups](../backups/README.md)**
   - Automated CronJob backups
   - Backup restoration
   - Retention policies

8. **[BackupService](../helm-charts/backup-service/README.md)**
   - API-driven backups
   - On-demand backup/restore
   - Microservice management

9. **[Custom Dashboards](../monitoring/dashboards/README.md)**
   - Platform overview dashboard
   - Transaction monitoring
   - Business metrics

## Common Tasks

### Daily Operations

**Check System Health**
```bash
kubectl get pods -n qalitrack-prod
kubectl get svc -n qalitrack-prod
```

**View Logs**
```bash
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service -f
```

**Check ArgoCD Sync Status**
```bash
argocd app get qalitrack-platform
```

### Deployment

**Deploy Code Changes**
```bash
# 1. Push code to GitHub
git push origin main

# 2. Wait for GitHub Actions to build (2-5 min)

# 3. Restart deployment
kubectl rollout restart deployment/gateway-service -n qalitrack-prod
```

**Update Configuration**
```bash
# Edit values
vim kubernetes/helm-charts/qalitrack-platform/values.yaml

# Apply changes
helm upgrade qalitrack-platform . -n qalitrack-prod
```

### Troubleshooting

**Pod Not Starting**
```bash
kubectl describe pod <pod-name> -n qalitrack-prod
kubectl logs <pod-name> -n qalitrack-prod
```

**Service Not Accessible**
```bash
kubectl get endpoints <service-name> -n qalitrack-prod
kubectl port-forward svc/<service-name> 7000:7000
```

**Rollback Deployment**
```bash
kubectl rollout undo deployment/<service-name> -n qalitrack-prod
```

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Qalitrack Platform                        │
│                     (Kubernetes)                             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  External Traffic                                            │
│       ↓                                                      │
│  ┌──────────────┐                                           │
│  │ NGINX Ingress│ ← SSL/TLS (cert-manager)                 │
│  └──────┬───────┘                                           │
│         │                                                    │
│         ↓                                                    │
│  ┌──────────────┐                                           │
│  │   Gateway    │ ← JWT Authentication                      │
│  │   Service    │   Rate Limiting                           │
│  └──────┬───────┘   API Routing                            │
│         │                                                    │
│         ├──────────┬──────────┬──────────┬─────────┐       │
│         │          │          │          │         │       │
│         ↓          ↓          ↓          ↓         ↓       │
│  ┌──────────┐┌──────────┐┌──────────┐┌────────┐┌────────┐│
│  │   User   ││MasterData││Transactio││ Backup ││Reportin││
│  │  Service ││ Service  ││n Service ││Service ││g (new) ││
│  └────┬─────┘└────┬─────┘└────┬─────┘└───┬────┘└───┬────┘│
│       │           │           │          │         │       │
│       ↓           ↓           ↓          ↓         ↓       │
│  ┌──────────┐┌──────────┐┌──────────┐┌──────────────────┐│
│  │PostgreSQL││PostgreSQL││PostgreSQL││    PostgreSQL    ││
│  └──────────┘└──────────┘└──────────┘└──────────────────┘│
│                                                              │
│  Shared Infrastructure:                                     │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐                      │
│  │  Redis  │ │RabbitMQ │ │ Backups │                      │
│  └─────────┘ └─────────┘ └─────────┘                      │
│                                                              │
│  Automation & Monitoring:                                   │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐         │
│  │ ArgoCD  │ │ Flagger │ │Prometheu│ │ Grafana │         │
│  │ (GitOps)│ │(Canary) │ │  s      │ │         │         │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Service Ports

| Service | Port | Purpose |
|---------|------|---------|
| Gateway | 7000 | API Gateway & JWT Auth |
| User Service | 7001 | User management & auth |
| MasterData Service | 7002 | Master data (vehicles, products, etc.) |
| Transaction Service | 7003 | Weighbridge transactions |
| Backup Service | 7004 | Backup management API |
| *Your New Service* | 7005+ | Next available port |

## Resource Allocation (64GB VPS)

| Component | Memory Request | Memory Limit | CPU Request | CPU Limit |
|-----------|---------------|--------------|-------------|-----------|
| **Services** |
| Gateway | 128Mi | 256Mi | 100m | 250m |
| User Service | 512Mi | 2Gi | 250m | 1000m |
| MasterData Service | 512Mi | 2Gi | 250m | 1000m |
| Transaction Service | 512Mi | 2Gi | 250m | 1000m |
| Backup Service | 256Mi | 1Gi | 100m | 500m |
| **Databases (per service)** |
| PostgreSQL | 1Gi | 3Gi | 250m | 1000m |
| **Shared** |
| Redis | 256Mi | 512Mi | 100m | 250m |
| RabbitMQ | 256Mi | 512Mi | 100m | 250m |
| **Monitoring** |
| Prometheus | 512Mi | 2Gi | 250m | 1000m |
| Grafana | 256Mi | 512Mi | 100m | 250m |
| **Total** | ~10Gi | ~32Gi | ~2.5 CPUs | ~10 CPUs |

**VPS Capacity:** 64GB RAM, leaves 32-54GB free for OS and buffers

## Workflow Diagrams

### Code Change to Deployment

```
Developer                GitHub              Kubernetes
    |                       |                     |
    | 1. git push          |                     |
    |--------------------->|                     |
    |                       |                     |
    |                       | 2. GitHub Actions  |
    |                       |    builds image    |
    |                       |                     |
    |                       | 3. Push to         |
    |                       |    ghcr.io         |
    |                       |                     |
    |                       |                     |
    |                       | 4. ArgoCD detects  |
    |                       |    new image       |
    |                       |-------------------->|
    |                       |                     |
    |                       |                     | 5. Rolling update
    |                       |                     |    (zero downtime)
    |                       |                     |
    | 6. Verify deployment  |                     |
    |-------------------------------------->|
```

### Canary Deployment with Flagger

```
New Version Deployed
        ↓
    Canary Pod Created
        ↓
    10% traffic → Canary
        ↓
    Check metrics (1 min)
        ↓
    ✓ Success rate > 99%?
    ✓ Latency < 500ms?
        ↓
    YES → Increase to 20%
        ↓
    Repeat until 50%
        ↓
    All checks passed?
        ↓
    YES → Promote to 100%
        ↓
    Old version terminated
```

## Environment Structure

```
qalitrack-prod/          # Production namespace
├── Services
│   ├── gateway-service
│   ├── user-service
│   ├── masterdata-service
│   ├── transaction-service
│   └── backup-service
│
├── Databases
│   ├── user-service-postgresql
│   ├── masterdata-service-postgresql
│   └── transaction-service-postgresql
│
├── Infrastructure
│   ├── redis-master
│   ├── rabbitmq
│   └── backup-storage-pvc
│
└── Secrets
    └── qalitrack-secrets

qalitrack-monitoring/    # Monitoring namespace
├── prometheus
├── grafana
├── alertmanager
└── servicemonitors

ingress-nginx/           # Ingress controller
└── ingress-nginx-controller

cert-manager/            # SSL certificates
└── cert-manager

flagger-system/          # Progressive delivery
├── flagger
└── flagger-loadtester

argocd/                  # GitOps
└── argocd-server
```

## Key Concepts

### GitOps (ArgoCD)
- Git is the single source of truth
- All changes go through Git
- Automatic deployment on git push
- Declarative configuration

### Progressive Delivery (Flagger)
- Gradual rollout of new versions
- Automatic monitoring during rollout
- Auto-rollback on failure
- Canary, blue-green, A/B testing

### Rolling Updates
- Zero-downtime deployments
- New pods created before old ones terminated
- Health checks ensure readiness
- Automatic rollback on health check failure

### Observability
- **Prometheus** - Metrics collection
- **Grafana** - Visualization
- **Logs** - kubectl logs, aggregation
- **Tracing** - (Optional: Jaeger/Zipkin)

## Best Practices

### Security
- ✅ Secrets stored in Kubernetes Secrets
- ✅ RBAC for access control
- ✅ Network policies (optional)
- ✅ Pod security policies
- ✅ SSL/TLS with cert-manager

### High Availability
- ✅ Multiple replicas for services
- ✅ Pod anti-affinity (optional)
- ✅ Health checks (liveness/readiness)
- ✅ Resource limits to prevent OOM

### Monitoring
- ✅ Prometheus metrics from all services
- ✅ Custom Grafana dashboards
- ✅ Alerts for critical issues
- ✅ Log aggregation

### Backups
- ✅ Automated daily backups (CronJob)
- ✅ API-driven backups (BackupService)
- ✅ 30-day retention
- ✅ Checksums for integrity

### Updates
- ✅ Rolling updates for zero downtime
- ✅ Canary deployments with Flagger
- ✅ Easy rollback capability
- ✅ Image versioning

## Getting Help

### Check Logs
```bash
# Service logs
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=<service-name>

# ArgoCD logs
kubectl logs -n argocd -l app.kubernetes.io/name=argocd-server

# Flagger logs
kubectl logs -n flagger-system deployment/flagger
```

### Check Events
```bash
kubectl get events -n qalitrack-prod --sort-by='.lastTimestamp'
```

### Describe Resources
```bash
kubectl describe pod <pod-name> -n qalitrack-prod
kubectl describe deployment <deployment-name> -n qalitrack-prod
```

### Access Dashboards
```bash
# Grafana
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80

# ArgoCD
kubectl port-forward -n argocd svc/argocd-server 8080:443

# Prometheus
kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090
```

## Support & Contributing

- **Issues:** Report at [GitHub Issues](https://github.com/Qalitrack/qalitrackservices/issues)
- **Docs:** Keep documentation updated with changes
- **Feedback:** Share improvements via pull requests

---

**Last Updated:** March 2026
**Version:** 1.0.0
**Kubernetes Version:** 1.24+
**Helm Version:** 3.x
