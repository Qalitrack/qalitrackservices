# Qalitrack — Kubernetes Operations Guide

Cluster: k3s on `164.68.116.82`  
Shell access: `ssh root@164.68.116.82`

---

## Namespaces

| Namespace | Purpose |
|---|---|
| `qalitrack-prod` | All Qalitrack services + monitoring stack |
| `qalitrack-dev` | Dev/staging environment |
| `lante-erp-prod` | Lante ERP microservices (separate project) |
| `argocd` | GitOps controller |
| `cert-manager` | TLS certificate automation |

---

## Public URLs

| Service | URL |
|---|---|
| Qalitrack API (Gateway) | https://api.qalibrated.co.ke |
| Grafana | https://grafana.qalibrated.co.ke |
| Prometheus | https://prometheus.qalibrated.co.ke |
| ArgoCD | https://argocd.qalibrated.co.ke |

---

## Qalitrack Services

| Deployment | Description | Port |
|---|---|---|
| qalitrack-gateway-service | API gateway (entry point) | 5000 |
| qalitrack-user-service | Auth, users, roles | 8080 |
| qalitrack-masterdata-service | Core domain data | 8080 |
| qalitrack-transaction-service | Transactions | 8080 |
| qalitrack-backup-service | Scheduled backups | 8080 |

### Monitoring stack (also in `qalitrack-prod`)
| Deployment | Description |
|---|---|
| qalitrack-prometheus-server | Metrics collection (scrapes both qalitrack-prod & lante-erp-prod) |
| qalitrack-grafana | Dashboards |
| qalitrack-loki / loki-gateway | Log aggregation |
| qalitrack-grafana-agent-operator | Loki agent (PodLogs) |

---

## Day-to-Day Commands

### View pods
```bash
kubectl get pods -n qalitrack-prod
kubectl get pods -n qalitrack-dev
```

### View logs
```bash
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=user-service --tail=50 -f
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=masterdata-service --tail=50 -f
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service --tail=50 -f
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=transaction-service --tail=50 -f
```

### Restart a service
```bash
kubectl rollout restart deployment qalitrack-user-service -n qalitrack-prod
kubectl rollout restart deployment qalitrack-masterdata-service -n qalitrack-prod
kubectl rollout restart deployment qalitrack-transaction-service -n qalitrack-prod
kubectl rollout restart deployment qalitrack-gateway-service -n qalitrack-prod
```

### Check rollout
```bash
kubectl rollout status deployment qalitrack-user-service -n qalitrack-prod
```

### Events in namespace
```bash
kubectl get events -n qalitrack-prod --sort-by='.lastTimestamp' | tail -20
```

---

## ArgoCD — GitOps

ArgoCD watches `Qalitrack/qalitrackservices` main branch, path `kubernetes/helm-charts/qalitrack-platform`.

### Check sync status
```bash
kubectl get applications -n argocd
```

### Force sync
```bash
kubectl patch application qalitrack -n argocd --type merge \
  -p '{"operation":{"sync":{"revision":"HEAD"}}}'
```

---

## Helm Charts

Chart path: `kubernetes/helm-charts/qalitrack-platform/`  
Service sub-charts are vendored as `.tgz` in `charts/`.

### CRITICAL: After editing a service chart template, repackage it
```bash
cd kubernetes/helm-charts

helm package user-service      -d qalitrack-platform/charts/ --version 1.0.0
helm package masterdata        -d qalitrack-platform/charts/ --version 1.0.0
helm package transaction       -d qalitrack-platform/charts/ --version 1.0.0
helm package gateway           -d qalitrack-platform/charts/ --version 1.0.0
helm package backup-service    -d qalitrack-platform/charts/ --version 1.0.0
```

Changes to `qalitrack-platform/templates/` or `qalitrack-platform/values.yaml` don't need repackaging.

### Dry-run render
```bash
helm template qalitrack kubernetes/helm-charts/qalitrack-platform \
  -f kubernetes/helm-charts/qalitrack-platform/values.yaml \
  --debug 2>&1 | head -60
```

---

## Prometheus

Prometheus scrapes **both** `qalitrack-prod` and `lante-erp-prod` namespaces.  
Services need the annotations in their pod template:
```yaml
annotations:
  prometheus.io/scrape: "true"
  prometheus.io/port: "8080"
  prometheus.io/path: "/metrics"
```

### Port-forward Prometheus UI
```bash
kubectl port-forward -n qalitrack-prod svc/qalitrack-prometheus-server 9090:80
# Open http://localhost:9090
```

### Query Prometheus from CLI
```bash
POD=$(kubectl get pod -n qalitrack-prod -l app=prometheus,component=server -o name | head -1)
kubectl exec -n qalitrack-prod $POD -c prometheus-server -- \
  wget -qO- 'http://localhost:9090/api/v1/query?query=up'
```

### Check scrape targets
```bash
POD=$(kubectl get pod -n qalitrack-prod -l app=prometheus,component=server -o name | head -1)
kubectl exec -n qalitrack-prod $POD -c prometheus-server -- \
  wget -qO- 'http://localhost:9090/api/v1/targets' | python3 -m json.tool | grep '"health"'
```

---

## Loki / Log Aggregation

Logs are collected via `PodLogs` CRD (Grafana Agent Operator) from both `qalitrack-prod` and `lante-erp-prod`.  
The `PodLogs` resource is at `kubernetes/helm-charts/qalitrack-platform/templates/podlogs-backend.yaml`.

### View Loki in Grafana
1. Open https://grafana.qalibrated.co.ke
2. Explore → select Loki data source
3. Filter: `{namespace="qalitrack-prod"}` or `{namespace="lante-erp-prod"}`

---

## Grafana

### Key dashboards
- **ASP.NET Core controller summary** — per-controller request rates (both projects)
- **Node Exporter** — server CPU/memory/disk
- **Kubernetes pods** — pod resource usage

### Access Grafana from CLI (port-forward)
```bash
kubectl port-forward -n qalitrack-prod svc/qalitrack-grafana 3000:80
# Open http://localhost:3000
```

---

## Database Operations

All services share one PostgreSQL StatefulSet (`qalitrack-postgresql`),
database `qalitrackdb`, isolated by schema per service
(`masterdata`, `transactions`, `users`, `backup`).

### Connect to the database
```bash
kubectl exec -it -n qalitrack-prod qalitrack-postgresql-0 -- psql -U qalitrack -d qalitrackdb

# List schemas
kubectl exec -it -n qalitrack-prod qalitrack-postgresql-0 -- \
  psql -U qalitrack -d qalitrackdb -c "\dn"
```

---

## Git Push

```bash
git push origin main   # Qalitrack/qalitrackservices (ArgoCD source)
git push fork main     # Joshuaisikah/qalitrackservices
```

### Check remote URLs
```bash
git remote -v
# Expected:
# fork    https://github.com/Joshuaisikah/qalitrackservices.git
# origin  https://github.com/Qalitrack/qalitrackservices.git
```

---

## Common Troubleshooting

### Pod stuck in CrashLoopBackOff
```bash
kubectl logs -n qalitrack-prod <pod-name> --previous
kubectl describe pod <pod-name> -n qalitrack-prod
```

### ArgoCD shows Degraded / OutOfSync
```bash
kubectl get application qalitrack -n argocd -o yaml | grep -A20 conditions
kubectl patch application qalitrack -n argocd --type merge \
  -p '{"operation":{"sync":{"revision":"HEAD"}}}'
```

### Prometheus not scraping a new service
1. Verify pod has scrape annotations in the deployed pod spec:
   ```bash
   kubectl get pod <pod-name> -n qalitrack-prod -o jsonpath='{.metadata.annotations}'
   ```
2. If missing, add annotations to the service chart's `deployment.yaml` under `spec.template.metadata.annotations`, then repackage and push.

### 502/503 at the gateway
```bash
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service --tail=50
kubectl get pods -n qalitrack-prod
```
