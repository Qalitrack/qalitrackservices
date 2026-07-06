# Qalitrack Kubernetes Documentation Index

Comprehensive technical documentation for the Qalitrack platform Kubernetes infrastructure.

## Primary Documentation

### Core Guides

**[README.md](../README.md)** - Main entry point
- Platform overview
- Architecture components
- Directory structure
- Quick deployment
- Resource allocation
- Troubleshooting guide

**[DEPLOYMENT.md](../DEPLOYMENT.md)** - Deployment procedures
- Prerequisites validation
- Namespace and RBAC setup
- Infrastructure components installation
- Platform deployment
- Post-deployment verification
- Production hardening
- Rollback procedures

**[MONITORING.md](../MONITORING.md)** - Observability and monitoring
- Prometheus configuration
- Grafana dashboards
- Alertmanager setup
- Loki log aggregation
- Metrics collection
- Alert rules

### Extended Documentation

**[ARCHITECTURE.md](ARCHITECTURE.md)** - System architecture
- Logical architecture diagrams
- Service layer design
- Data layer architecture
- Network architecture
- Storage architecture
- Security architecture
- High availability design
- Design decisions and rationale

**[WORKFLOWS.md](WORKFLOWS.md)** - Operational workflows
- Development workflow
- CI/CD pipeline
- GitOps with ArgoCD
- Backup and restore procedures
- Monitoring and alerting workflow
- Scaling workflows
- Incident response procedures
- Update and maintenance workflows

**[ADD_NEW_MICROSERVICE.md](ADD_NEW_MICROSERVICE.md)** - Adding services
- Creating new microservices
- Helm chart creation
- Gateway integration
- Monitoring configuration
- Complete step-by-step guide

**[UPDATE_SERVICES.md](UPDATE_SERVICES.md)** - Deploying changes
- Code change deployment
- Image version updates
- Configuration updates
- Rolling updates
- Rollback procedures

## Component-Specific Documentation

### Infrastructure Components

- **[../argocd/README.md](../argocd/README.md)** - GitOps continuous deployment
- **[../backups/README.md](../backups/README.md)** - Automated database backups
- **[../cert-manager/README.md](../cert-manager/README.md)** - TLS certificate management
- **[../ci-cd/README.md](../ci-cd/README.md)** - CI/CD pipeline integration
- **[../flagger/README.md](../flagger/README.md)** - Progressive delivery (canary)
- **[../ingress-nginx/README.md](../ingress-nginx/README.md)** - NGINX Ingress Controller
- **[../monitoring/README.md](../monitoring/README.md)** - Prometheus/Grafana stack
- **[../namespaces/README.md](../namespaces/README.md)** - Namespace configuration
- **[../secrets/README.md](../secrets/README.md)** - Secret generation and management

### Service Helm Charts

- **[../helm-charts/qalitrack-platform/README.md](../helm-charts/qalitrack-platform/README.md)** - Umbrella chart
- **[../helm-charts/backup-service/README.md](../helm-charts/backup-service/README.md)** - Backup service
- **[../helm-charts/gateway-service/README.md](../helm-charts/gateway-service/README.md)** - API Gateway
- **[../helm-charts/user-service/README.md](../helm-charts/user-service/README.md)** - User service
- **[../helm-charts/masterdata-service/README.md](../helm-charts/masterdata-service/README.md)** - MasterData service
- **[../helm-charts/transaction-service/README.md](../helm-charts/transaction-service/README.md)** - Transaction service
- **[../helm-charts/technician-service/README.md](../helm-charts/technician-service/README.md)** - Technician service
- **[../helm-charts/qtruck-service/README.md](../helm-charts/qtruck-service/README.md)** - QTruck service

## Quick Reference

### Common Commands

**View cluster status:**
```bash
kubectl get pods -n qalitrack-prod
kubectl get svc -n qalitrack-prod
kubectl top pods -n qalitrack-prod
kubectl top nodes
```

**View logs:**
```bash
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service --tail=100
kubectl logs -n qalitrack-prod deployment/transaction-service -f
```

**Port forwarding:**
```bash
kubectl port-forward -n qalitrack-prod svc/gateway-service 7000:7000
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80
kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090
```

**Deployment operations:**
```bash
kubectl scale deployment/transaction-service --replicas=5 -n qalitrack-prod
kubectl rollout restart deployment/gateway-service -n qalitrack-prod
kubectl rollout status deployment/gateway-service -n qalitrack-prod
kubectl rollout undo deployment/gateway-service -n qalitrack-prod
```

**Helm operations:**
```bash
helm list -n qalitrack-prod
helm status qalitrack -n qalitrack-prod
helm upgrade qalitrack . -n qalitrack-prod
helm rollback qalitrack -n qalitrack-prod
```

### Architecture Diagrams

**Network Flow:**
```
Internet
  │
  └─→ NGINX Ingress (SSL termination)
       │
       └─→ Gateway Service (YARP)
            │
            ├─→ User Service → PostgreSQL
            ├─→ MasterData Service → PostgreSQL
            ├─→ Transaction Service → PostgreSQL
            ├─→ Backup Service → PostgreSQL
            └─→ Technician Service → PostgreSQL
```

**Data Layer:**
```
Services
  ├─→ PostgreSQL (database-per-service)
  ├─→ Redis (distributed cache)
  └─→ RabbitMQ (message queue)
```

**Observability:**
```
Services (/metrics endpoints)
  └─→ ServiceMonitors
       └─→ Prometheus
            ├─→ Grafana (dashboards)
            └─→ Alertmanager (notifications)
```

### Service Ports

| Service | Port | Purpose |
|---------|------|---------|
| Gateway | 7000 | API Gateway |
| User | 7001 | User management |
| MasterData | 7002 | Master data |
| Transaction | 7003 | Transactions |
| Backup | 7004 | Backup API |
| Technician | 7006 | Technicians |
| QTruck API | 7007 | Fleet management |
| PostgreSQL | 5432 | Database |
| Redis | 6379 | Cache |
| RabbitMQ | 5672, 15672 | Message queue |
| Prometheus | 9090 | Metrics |
| Grafana | 3000 | Dashboards |

### DNS Names

**Internal (within cluster):**
```
<service-name>.<namespace>.svc.cluster.local
```

Examples:
- `gateway-service.qalitrack-prod.svc.cluster.local:7000`
- `qalitrack-postgresql.qalitrack-prod.svc.cluster.local:5432` (shared DB, all services)

**External (via Ingress):**
- `https://qalibrated.co.ke/qalitrack/api/*` - API Gateway
- `https://qalibrated.co.ke/grafana` - Grafana
- `https://qalibrated.co.ke/prometheus` - Prometheus

### Resource Allocation Summary

**Total Resources (default configuration):**
- CPU Requests: ~2.5 cores
- CPU Limits: ~10 cores
- Memory Requests: ~10Gi
- Memory Limits: ~32Gi
- Storage: ~250Gi (PersistentVolumes)

**Recommended Cluster:**
- Minimum: 64GB RAM, 16 CPU cores, 200GB storage
- Production: 128GB RAM, 32 CPU cores, 500GB storage

## Documentation Standards

### File Organization

- **Root level** - High-level overviews and deployment guides
- **docs/** - Extended architectural and workflow documentation
- **Component directories** - Component-specific implementation details

### Document Structure

Each document follows this structure:
1. Title and overview
2. Architecture/design (where applicable)
3. Configuration details
4. Procedures and workflows
5. Troubleshooting
6. References

### Code Examples

All code examples use proper syntax highlighting and include:
- Command description
- Expected output (where relevant)
- Error handling notes

### Versioning

- Version: 1.0.0
- Last Updated: 2026-03
- Kubernetes Version: 1.24+
- Helm Version: 3.10+

## Getting Help

### Troubleshooting

**Check pod status:**
```bash
kubectl get pods -n qalitrack-prod
kubectl describe pod <pod-name> -n qalitrack-prod
kubectl logs <pod-name> -n qalitrack-prod
```

**Check events:**
```bash
kubectl get events -n qalitrack-prod --sort-by='.lastTimestamp'
```

**Check resource usage:**
```bash
kubectl top pods -n qalitrack-prod
kubectl top nodes
```

### Documentation Updates

When updating documentation:
1. Update the relevant .md file
2. Update version/date in footer
3. Update this index if adding new documents
4. Commit with descriptive message

### Contact

For infrastructure issues:
- Check component-specific README files
- Review troubleshooting sections
- Examine Kubernetes events and logs
- Check Grafana dashboards for metrics

## Document Map

```
kubernetes/
│
├── README.md (Start here - Platform overview)
│
├── DEPLOYMENT.md (Deployment procedures)
├── MONITORING.md (Observability)
│
└── docs/
    ├── README.md (This file - Documentation index)
    ├── ARCHITECTURE.md (System design)
    ├── WORKFLOWS.md (Operational workflows)
    ├── ADD_NEW_MICROSERVICE.md (Add services)
    └── UPDATE_SERVICES.md (Deploy changes)
```

## External References

**Kubernetes:**
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [kubectl Reference](https://kubernetes.io/docs/reference/kubectl/)

**Helm:**
- [Helm Documentation](https://helm.sh/docs/)
- [Chart Development](https://helm.sh/docs/chart_template_guide/)

**Prometheus:**
- [Prometheus Documentation](https://prometheus.io/docs/)
- [PromQL Basics](https://prometheus.io/docs/prometheus/latest/querying/basics/)

**Grafana:**
- [Grafana Documentation](https://grafana.com/docs/grafana/latest/)
- [Dashboard Best Practices](https://grafana.com/docs/grafana/latest/best-practices/)

**ArgoCD:**
- [ArgoCD Documentation](https://argo-cd.readthedocs.io/)

**Flagger:**
- [Flagger Documentation](https://docs.flagger.app/)

**Version:** 1.0.0
**Last Updated:** 2026-03
