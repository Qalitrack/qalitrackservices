# Qalitrack Platform Observability

Observability infrastructure, metrics collection, and monitoring dashboard access for the Qalitrack platform.

## Overview

The Qalitrack platform implements comprehensive observability using the kube-prometheus-stack, which includes:

- **Prometheus** - Metrics collection and time-series database
- **Grafana** - Metrics visualization and dashboarding
- **Alertmanager** - Alert routing and notification
- **Loki** - Log aggregation (optional)
- **ServiceMonitors** - Automated metrics scraping configuration

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                  Application Services                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │  Gateway    │  │    User     │  │ Transaction │          │
│  │  /metrics   │  │  /metrics   │  │  /metrics   │  ...     │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘          │
└─────────┼────────────────┼────────────────┼─────────────────┘
          │                │                │
          │ Scrape (30s)   │                │
          ▼                ▼                ▼
┌──────────────────────────────────────────────────────────────┐
│                ServiceMonitor Resources                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Defines: endpoints, ports, intervals, labels        │   │
│  └────────────────────┬─────────────────────────────────┘   │
└───────────────────────┼──────────────────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────────────────┐
│                  Prometheus Server                            │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  - Scrapes metrics from endpoints                     │  │
│  │  - Stores time-series data (15d retention)            │  │
│  │  - Evaluates alert rules                              │  │
│  │  - Exposes PromQL query interface                     │  │
│  └───────────┬────────────────────────┬──────────────────┘  │
└──────────────┼────────────────────────┼─────────────────────┘
               │                        │
               │ Metrics                │ Alerts
               ▼                        ▼
    ┌──────────────────┐     ┌──────────────────┐
    │     Grafana      │     │  AlertManager    │
    │  Dashboards      │     │  Notifications   │
    └──────────────────┘     └──────────────────┘
```

## Prometheus Configuration

### Installation

Prometheus is installed as part of the kube-prometheus-stack Helm chart:

```bash
cd kubernetes/monitoring
./install.sh
```

**Installed Components:**
- Prometheus Operator
- Prometheus Server
- Alertmanager
- Node Exporter (DaemonSet)
- Kube State Metrics
- Grafana

### Configuration

**Prometheus Server:**
- Retention: 15 days
- Storage: 50GB PersistentVolume
- Scrape interval: 30 seconds
- Evaluation interval: 30 seconds

**Location:** `monitoring/values-prometheus-stack.yaml`

```yaml
prometheus:
  prometheusSpec:
    retention: 15d
    retentionSize: "45GB"
    storageSpec:
      volumeClaimTemplate:
        spec:
          accessModes: ["ReadWriteOnce"]
          resources:
            requests:
              storage: 50Gi
    resources:
      requests:
        cpu: 250m
        memory: 512Mi
      limits:
        cpu: 1000m
        memory: 2Gi
```

### ServiceMonitors

ServiceMonitors define Prometheus scraping targets. The Prometheus Operator automatically discovers ServiceMonitors and configures scraping.

**Location:** `monitoring/servicemonitors.yaml`

**Example ServiceMonitor:**

```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: gateway-service
  namespace: qalitrack-prod
  labels:
    release: prometheus
spec:
  selector:
    matchLabels:
      app.kubernetes.io/name: gateway-service
  endpoints:
    - port: http
      path: /metrics
      interval: 30s
      scrapeTimeout: 10s
```

**Available ServiceMonitors:**
- gateway-service
- user-service
- masterdata-service
- transaction-service
- backup-service
- technician-service
- qtruck-service-api

### Accessing Prometheus

**Via Port Forward:**

```bash
kubectl port-forward -n qalitrack-monitoring \
  svc/prometheus-kube-prometheus-prometheus 9090:9090

# Access: http://localhost:9090
```

**Via Ingress (Production):**

```bash
# Access: https://qalibrated.co.ke/prometheus
```

**Verify Targets:**
Navigate to Status → Targets to view all scraping endpoints and their health status.

### PromQL Queries

**Common Queries:**

```promql
# Request rate per service
rate(http_requests_total{namespace="qalitrack-prod"}[5m])

# CPU usage per pod
rate(container_cpu_usage_seconds_total{namespace="qalitrack-prod"}[5m])

# Memory usage per pod
container_memory_usage_bytes{namespace="qalitrack-prod"}

# HTTP error rate
rate(http_requests_total{status=~"5.."}[5m])

# Pod restart count
kube_pod_container_status_restarts_total{namespace="qalitrack-prod"}

# Database connections
pg_stat_database_numbackends{namespace="qalitrack-prod"}

# Redis memory usage
redis_memory_used_bytes{namespace="qalitrack-prod"}

# Request latency (95th percentile)
histogram_quantile(0.95,
  rate(http_request_duration_seconds_bucket{namespace="qalitrack-prod"}[5m])
)
```

## Grafana Configuration

### Installation

Grafana is included in the kube-prometheus-stack:

```bash
# Grafana is automatically installed with Prometheus
kubectl get pods -n qalitrack-monitoring -l app.kubernetes.io/name=grafana
```

### Configuration

**Default Credentials:**
- Username: `admin`
- Password: Retrieve from secret or set during installation

```bash
kubectl get secret prometheus-grafana -n qalitrack-monitoring \
  -o jsonpath="{.data.admin-password}" | base64 -d
```

**Data Sources:**
- Prometheus (pre-configured)
- Loki (if installed)

**Storage:**
- 10GB PersistentVolume for dashboards
- Dashboard persistence enabled

### Accessing Grafana

**Via Port Forward:**

```bash
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80

# Access: http://localhost:3000
# Login: admin / <password-from-secret>
```

**Via Ingress (Production):**

```bash
# Access: https://qalibrated.co.ke/grafana
```

### Dashboard Management

**Import Dashboard:**

```bash
# Via ConfigMap
kubectl create configmap grafana-dashboard-platform \
  --from-file=monitoring/dashboards/platform-overview.json \
  -n qalitrack-monitoring

# Label for auto-discovery
kubectl label configmap grafana-dashboard-platform \
  grafana_dashboard=1 \
  -n qalitrack-monitoring
```

**Via Grafana UI:**
1. Navigate to Dashboards → Import
2. Upload JSON file or paste JSON
3. Select Prometheus data source
4. Click Import

**Available Dashboards:**
- **Platform Overview** - All services metrics (CPU, memory, requests)
- **Transaction Service** - Business metrics (transactions/min, avg weight, etc.)
- **Kubernetes Cluster** - Node and pod resource usage
- **PostgreSQL Performance** - Database metrics
- **Redis Performance** - Cache hit rate, memory

**Location:** `monitoring/dashboards/`

### Dashboard Configuration

**Platform Overview Dashboard:**
- Service health status
- Request rate (requests/sec)
- Error rate (%)
- Response time (p50, p95, p99)
- Pod CPU and memory usage
- Pod count and restarts

**Transaction Service Dashboard:**
- Transactions per minute
- Average transaction weight
- Transaction status distribution
- Reweigh request rate
- Database query performance
- API endpoint latency

## Alertmanager Configuration

### Configuration

Alertmanager routes and groups alerts from Prometheus.

**Configuration File:** `monitoring/alertmanager-config.yaml`

**Example Configuration:**

```yaml
global:
  resolve_timeout: 5m

route:
  group_by: ['alertname', 'namespace']
  group_wait: 30s
  group_interval: 5m
  repeat_interval: 12h
  receiver: 'default'
  routes:
    - match:
        severity: critical
      receiver: 'critical'
    - match:
        severity: warning
      receiver: 'warning'

receivers:
  - name: 'default'
    email_configs:
      - to: 'alerts@qalibrated.co.ke'
        from: 'prometheus@qalibrated.co.ke'
        smarthost: 'smtp.gmail.com:587'
        auth_username: 'prometheus@qalibrated.co.ke'
        auth_password: '<app-password>'

  - name: 'critical'
    email_configs:
      - to: 'critical@qalibrated.co.ke'
    pagerduty_configs:
      - service_key: '<pagerduty-key>'

  - name: 'warning'
    email_configs:
      - to: 'warnings@qalibrated.co.ke'
```

### Alert Rules

**Location:** `monitoring/prometheus-alerts.yaml`

**Example Alert Rules:**

```yaml
apiVersion: monitoring.coreos.com/v1
kind: PrometheusRule
metadata:
  name: qalitrack-alerts
  namespace: qalitrack-monitoring
spec:
  groups:
    - name: qalitrack.rules
      interval: 30s
      rules:
        - alert: ServiceDown
          expr: up{job="kubernetes-service-endpoints"} == 0
          for: 2m
          labels:
            severity: critical
          annotations:
            summary: "Service {{ $labels.service }} is down"
            description: "Service has been down for more than 2 minutes"

        - alert: HighErrorRate
          expr: |
            rate(http_requests_total{status=~"5.."}[5m]) /
            rate(http_requests_total[5m]) > 0.05
          for: 5m
          labels:
            severity: warning
          annotations:
            summary: "High error rate on {{ $labels.service }}"
            description: "Error rate is {{ $value | humanizePercentage }}"

        - alert: HighMemoryUsage
          expr: |
            container_memory_usage_bytes /
            container_spec_memory_limit_bytes > 0.9
          for: 5m
          labels:
            severity: warning
          annotations:
            summary: "Pod {{ $labels.pod }} high memory usage"
            description: "Memory usage is {{ $value | humanizePercentage }}"

        - alert: PodCrashLooping
          expr: rate(kube_pod_container_status_restarts_total[15m]) > 0
          for: 5m
          labels:
            severity: critical
          annotations:
            summary: "Pod {{ $labels.pod }} is crash looping"
            description: "Pod has restarted {{ $value }} times in 15 minutes"

        - alert: DatabaseConnectionsHigh
          expr: pg_stat_database_numbackends > 450
          for: 5m
          labels:
            severity: warning
          annotations:
            summary: "Database {{ $labels.datname }} connection count high"
            description: "Current connections: {{ $value }}, limit: 500"
```

### Accessing Alertmanager

```bash
kubectl port-forward -n qalitrack-monitoring \
  svc/prometheus-kube-prometheus-alertmanager 9093:9093

# Access: http://localhost:9093
```

## Loki (Log Aggregation)

### Installation

```bash
cd kubernetes/monitoring

# Install Loki stack (Loki + Promtail)
helm repo add grafana https://grafana.github.io/helm-charts
helm repo update

helm install loki grafana/loki-stack \
  --namespace qalitrack-monitoring \
  --set grafana.enabled=false \
  --set prometheus.enabled=false \
  --set loki.persistence.enabled=true \
  --set loki.persistence.size=30Gi
```

### Configuration

**Loki Server:**
- Retention: 30 days
- Storage: 30GB PersistentVolume
- Ingestion rate limit: 4MB/s

**Promtail:**
- Deployed as DaemonSet (runs on all nodes)
- Scrapes logs from all pods
- Labels: namespace, pod, container

### Querying Logs

**Via Grafana Explore:**
1. Navigate to Explore
2. Select Loki data source
3. Use LogQL queries

**Example LogQL Queries:**

```logql
# All logs from transaction-service
{namespace="qalitrack-prod", app="transaction-service"}

# Error logs only
{namespace="qalitrack-prod"} |= "ERROR"

# Logs with specific transaction ID
{namespace="qalitrack-prod", app="transaction-service"}
  |= "TX-12345"

# Rate of errors per minute
rate({namespace="qalitrack-prod"} |= "ERROR" [5m])

# Logs from crashed pods
{namespace="qalitrack-prod"}
  | json
  | level="FATAL"
```

### Log Format

Applications should emit structured JSON logs:

```json
{
  "timestamp": "2026-03-25T10:30:00.123Z",
  "level": "INFO",
  "logger": "TransactionService",
  "message": "Transaction created successfully",
  "transactionId": "TX-67890",
  "userId": "user-123",
  "duration": 45
}
```

## Metrics Exposed by Services

### Standard Metrics

All services expose the following metrics at `/metrics`:

**HTTP Metrics:**
- `http_requests_total` - Total HTTP requests (counter)
- `http_request_duration_seconds` - Request duration (histogram)
- `http_requests_in_progress` - Current in-flight requests (gauge)

**Process Metrics:**
- `process_cpu_seconds_total` - CPU time (counter)
- `process_resident_memory_bytes` - Memory usage (gauge)
- `process_open_fds` - Open file descriptors (gauge)

**Runtime Metrics (.NET):**
- `dotnet_collection_count_total` - GC collections (counter)
- `dotnet_total_memory_bytes` - Total memory (gauge)
- `dotnet_threadpool_num_threads` - Thread count (gauge)

### Service-Specific Metrics

**Transaction Service:**
- `transactions_created_total` - Total transactions created
- `transactions_completed_total` - Total transactions completed
- `transactions_pending` - Current pending transactions
- `average_transaction_weight_kg` - Average weight per transaction
- `reweigh_requests_total` - Total reweigh requests

**User Service:**
- `users_registered_total` - Total users
- `login_attempts_total` - Login attempts (successful/failed)
- `active_sessions` - Current active sessions

**Database Metrics (PostgreSQL Exporter):**
- `pg_stat_database_numbackends` - Active connections
- `pg_stat_database_tup_fetched` - Rows fetched
- `pg_stat_database_tup_inserted` - Rows inserted
- `pg_stat_database_blks_hit` - Cache hits
- `pg_database_size_bytes` - Database size

**Redis Metrics (Redis Exporter):**
- `redis_connected_clients` - Connected clients
- `redis_memory_used_bytes` - Memory usage
- `redis_keyspace_hits_total` - Cache hits
- `redis_keyspace_misses_total` - Cache misses

## Accessing Metrics Endpoints

### Direct Pod Access

```bash
# Port-forward to service pod
kubectl port-forward -n qalitrack-prod deployment/gateway-service 7000:80

# Fetch metrics
curl http://localhost:7000/metrics
```

### Service Metrics

```bash
# Access via service
kubectl port-forward -n qalitrack-prod svc/gateway-service 7000:7000

curl http://localhost:7000/metrics
```

### All Metrics

```bash
# Query Prometheus for all metrics
curl http://localhost:9090/api/v1/label/__name__/values | jq
```

## Dashboard Access Matrix

| Component | Internal Access | External Access (Ingress) | Port |
|-----------|----------------|---------------------------|------|
| Prometheus | kubectl port-forward | https://qalibrated.co.ke/prometheus | 9090 |
| Grafana | kubectl port-forward | https://qalibrated.co.ke/grafana | 3000 |
| Alertmanager | kubectl port-forward | https://qalibrated.co.ke/alertmanager | 9093 |
| Loki | kubectl port-forward | N/A | 3100 |

## Troubleshooting

### Prometheus Not Scraping Targets

```bash
# Check ServiceMonitor exists
kubectl get servicemonitors -n qalitrack-prod

# Check ServiceMonitor labels match Prometheus serviceMonitorSelector
kubectl describe servicemonitor gateway-service -n qalitrack-prod

# Check Prometheus logs
kubectl logs -n qalitrack-monitoring -l app.kubernetes.io/name=prometheus

# Verify service has /metrics endpoint
kubectl port-forward -n qalitrack-prod svc/gateway-service 7000:7000
curl http://localhost:7000/metrics
```

### Grafana Dashboard Not Loading Data

```bash
# Check Prometheus data source connection
# Grafana UI → Configuration → Data Sources → Prometheus → Test

# Verify Prometheus has data
# Access Prometheus UI → Status → Targets

# Check query syntax
# Use Grafana Explore to test queries
```

### Alerts Not Firing

```bash
# Check Prometheus alert rules
kubectl get prometheusrules -n qalitrack-monitoring

# View alert status in Prometheus UI
# http://localhost:9090/alerts

# Check Alertmanager configuration
kubectl get secret alertmanager-prometheus-kube-prometheus-alertmanager \
  -n qalitrack-monitoring -o yaml

# View Alertmanager logs
kubectl logs -n qalitrack-monitoring -l app.kubernetes.io/name=alertmanager
```

### High Prometheus Storage Usage

```bash
# Check Prometheus storage usage
kubectl exec -n qalitrack-monitoring prometheus-prometheus-kube-prometheus-prometheus-0 -- \
  df -h /prometheus

# Reduce retention period (edit values)
helm upgrade prometheus prometheus-community/kube-prometheus-stack \
  --set prometheus.prometheusSpec.retention=7d \
  -n qalitrack-monitoring
```

## Performance Optimization

### Prometheus

**Reduce Cardinality:**
- Limit label values
- Drop unnecessary metrics
- Use relabeling

**Configuration:**
```yaml
prometheus:
  prometheusSpec:
    # Drop high-cardinality metrics
    metricRelabelings:
      - sourceLabels: [__name__]
        regex: 'go_gc_.*'
        action: drop
```

### Grafana

**Dashboard Optimization:**
- Limit time range for queries
- Use recording rules for complex queries
- Enable query caching

**Recording Rules:**
```yaml
- name: aggregated.metrics
  interval: 60s
  rules:
    - record: job:http_requests:rate5m
      expr: sum by (job) (rate(http_requests_total[5m]))
```

## Security Considerations

### Authentication

**Prometheus:** No built-in authentication (use Ingress with basic auth or OAuth)
**Grafana:** Username/password (default: admin/admin)
**Alertmanager:** No authentication (restrict access via NetworkPolicy)

### Network Policies

```yaml
# Allow Prometheus to scrape metrics
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-prometheus-scraping
  namespace: qalitrack-prod
spec:
  podSelector: {}
  ingress:
    - from:
        - namespaceSelector:
            matchLabels:
              name: qalitrack-monitoring
        - podSelector:
            matchLabels:
              app.kubernetes.io/name: prometheus
      ports:
        - protocol: TCP
          port: 80
```

### Secret Management

Sensitive configuration (SMTP passwords, API keys) should be stored in Kubernetes Secrets:

```bash
kubectl create secret generic alertmanager-config \
  --from-file=alertmanager.yaml \
  -n qalitrack-monitoring
```

## Maintenance

### Backup Grafana Dashboards

```bash
# Export dashboard JSON
# Via Grafana UI: Dashboard → Share → Export → Save to file

# Or via API
curl -H "Authorization: Bearer <api-token>" \
  http://localhost:3000/api/dashboards/uid/<dashboard-uid> \
  | jq .dashboard > dashboard-backup.json
```

### Prometheus Data Backup

```bash
# Create snapshot
curl -X POST http://localhost:9090/api/v1/admin/tsdb/snapshot

# Copy snapshot data
kubectl cp qalitrack-monitoring/prometheus-prometheus-kube-prometheus-prometheus-0:/prometheus/snapshots \
  ./prometheus-backup
```

### Update Monitoring Stack

```bash
# Update Helm repository
helm repo update

# Upgrade kube-prometheus-stack
helm upgrade prometheus prometheus-community/kube-prometheus-stack \
  --namespace qalitrack-monitoring \
  --values monitoring/values-prometheus-stack.yaml
```

## Additional Resources

**Documentation:**
- Prometheus: https://prometheus.io/docs/
- Grafana: https://grafana.com/docs/grafana/latest/
- PromQL: https://prometheus.io/docs/prometheus/latest/querying/basics/
- LogQL: https://grafana.com/docs/loki/latest/logql/

**Best Practices:**
- [Prometheus Best Practices](https://prometheus.io/docs/practices/)
- [Grafana Dashboard Best Practices](https://grafana.com/docs/grafana/latest/best-practices/best-practices-for-creating-dashboards/)

**Version:** 1.0.0
**Last Updated:** 2026-03
