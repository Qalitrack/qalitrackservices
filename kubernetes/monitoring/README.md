# Qalitrack Monitoring Stack

Complete monitoring solution with Prometheus, Grafana, and AlertManager for the Qalitrack platform.

## What's Included

- **Prometheus** - Metrics collection and storage
- **Grafana** - Visualization dashboards
- **AlertManager** - Alert routing and notifications
- **Node Exporter** - Node-level metrics
- **Kube State Metrics** - Kubernetes object metrics
- **ServiceMonitors** - Automatic service discovery

## Architecture

```
Microservices → ServiceMonitors → Prometheus → Grafana
                                      ↓
                                 AlertManager → Notifications
```

## Installation

### Quick Install

```bash
cd kubernetes/monitoring
./install.sh
```

### Manual Installation

```bash
# Add Helm repository
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

# Install kube-prometheus-stack
helm upgrade --install prometheus prometheus-community/kube-prometheus-stack \
  --namespace qalitrack-monitoring \
  --create-namespace \
  --values values-prometheus-stack.yaml \
  --wait

# Install ServiceMonitors
kubectl apply -f servicemonitors.yaml
```

## Access Dashboards

### Grafana (Recommended)

```bash
# Port forward to access locally
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80
```

Then open: http://localhost:3000

**Default credentials:**
- Username: `admin`
- Password: `admin` (change in `values-prometheus-stack.yaml`)

### Prometheus UI

```bash
kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090
```

Then open: http://localhost:9090

### AlertManager UI

```bash
kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-alertmanager 9093:9093
```

Then open: http://localhost:9093

## Pre-configured Dashboards

Grafana comes with these dashboards:

1. **Kubernetes Cluster** (ID: 7249)
   - Overall cluster health
   - Resource usage by namespace
   - Pod status

2. **Node Exporter** (ID: 1860)
   - CPU, memory, disk usage per node
   - Network traffic
   - System load

3. **Kubernetes Pods** (ID: 6417)
   - Pod resource usage
   - Container metrics
   - Restart counts

## What's Being Monitored

### Qalitrack Services

All microservices expose metrics at `/metrics`:

- **gateway-service** (port 7000)
- **user-service** (port 7001)
- **masterdata-service** (port 7002)
- **transaction-service** (port 7003)
- **backup-service** (port 7004)

### Kubernetes Components

- API Server
- kubelet
- Controller Manager
- Scheduler
- etcd (if available)

### Infrastructure

- Node CPU, memory, disk
- Network I/O
- Container resource usage

## Key Metrics to Watch

### Application Metrics

```promql
# Request rate
rate(http_requests_total[5m])

# Error rate
rate(http_requests_total{status=~"5.."}[5m])

# Request duration
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Pod CPU usage
container_cpu_usage_seconds_total

# Pod memory usage
container_memory_usage_bytes
```

### Infrastructure Metrics

```promql
# Node CPU usage
node_cpu_seconds_total

# Node memory available
node_memory_MemAvailable_bytes

# Disk usage
node_filesystem_avail_bytes

# Pod status
kube_pod_status_phase
```

## Alerts

### Built-in Alerts

The stack includes alerts for:

- High CPU/Memory usage
- Pod not ready
- Pod crash looping
- Node not ready
- Disk pressure
- Out of memory
- High API server latency

### Custom Alerts

Create custom alerts in `kubernetes/monitoring/alerts/`:

```yaml
apiVersion: monitoring.coreos.com/v1
kind: PrometheusRule
metadata:
  name: qalitrack-alerts
  namespace: qalitrack-monitoring
spec:
  groups:
  - name: qalitrack
    interval: 30s
    rules:
    - alert: HighErrorRate
      expr: |
        rate(http_requests_total{status=~"5.."}[5m]) > 0.05
      for: 5m
      labels:
        severity: warning
      annotations:
        summary: "High error rate detected"
        description: "Error rate is {{ $value }} req/s"
```

Apply with:
```bash
kubectl apply -f alerts/qalitrack-alerts.yaml
```

## Notification Channels

### Email Notifications

Edit `values-prometheus-stack.yaml`:

```yaml
alertmanager:
  config:
    receivers:
    - name: 'email'
      email_configs:
      - to: 'admin@qalibrated.co.ke'
        from: 'alertmanager@qalibrated.co.ke'
        smarthost: 'smtp.gmail.com:587'
        auth_username: 'your-email@gmail.com'
        auth_password: 'your-app-password'
```

### Slack Notifications

```yaml
receivers:
- name: 'slack'
  slack_configs:
  - api_url: 'https://hooks.slack.com/services/YOUR/WEBHOOK/URL'
    channel: '#alerts'
    title: 'Qalitrack Alert'
```

### Webhook

```yaml
receivers:
- name: 'webhook'
  webhook_configs:
  - url: 'http://your-webhook-url'
```

## Grafana Tips

### Create Custom Dashboard

1. Open Grafana (http://localhost:3000)
2. Click "+" → "Dashboard"
3. Click "Add visualization"
4. Select "Prometheus" as datasource
5. Enter PromQL query
6. Customize visualization
7. Save dashboard

### Import Community Dashboards

1. Go to Dashboards → Import
2. Enter dashboard ID (e.g., 7249)
3. Select Prometheus datasource
4. Click Import

Popular Qalitrack-relevant dashboards:
- 7249 - Kubernetes cluster monitoring
- 1860 - Node Exporter Full
- 6417 - Kubernetes Pods
- 11074 - Node Exporter for Prometheus
- 13770 - Kubernetes monitoring

## Storage

### Prometheus Storage

- **Size:** 20Gi
- **Retention:** 15 days
- **Location:** PersistentVolumeClaim

Adjust in `values-prometheus-stack.yaml`:

```yaml
prometheus:
  prometheusSpec:
    retention: 30d  # Increase retention
    storageSpec:
      volumeClaimTemplate:
        spec:
          resources:
            requests:
              storage: 50Gi  # Increase storage
```

### Grafana Storage

- **Size:** 5Gi (for dashboards and settings)

## Troubleshooting

### No Metrics from Services

```bash
# Check if ServiceMonitors are created
kubectl get servicemonitor -n qalitrack-prod

# Check if Prometheus is scraping targets
# Port-forward to Prometheus UI and check Status → Targets

# Check service labels match ServiceMonitor selector
kubectl get svc -n qalitrack-prod --show-labels
```

### Grafana Dashboard Empty

```bash
# Check Prometheus datasource in Grafana
# Configuration → Data Sources → Prometheus → Test

# Verify Prometheus has data
# Run query in Prometheus UI: up
```

### High Memory Usage

```bash
# Reduce scrape interval
# Edit values-prometheus-stack.yaml
# Change: interval: 30s → interval: 60s

# Reduce retention
# Change: retention: 15d → retention: 7d
```

### Pods Not Starting

```bash
# Check pod status
kubectl get pods -n qalitrack-monitoring

# Check pod logs
kubectl logs -n qalitrack-monitoring <pod-name>

# Check resource constraints
kubectl describe pod -n qalitrack-monitoring <pod-name>
```

## Performance Tuning

### For 64GB VPS

Current configuration uses:
- **Requests:** ~1.2Gi
- **Limits:** ~3.4Gi

If you need to reduce resource usage:

```yaml
# Reduce Prometheus storage
retention: 7d  # from 15d
storage: 10Gi  # from 20Gi

# Disable components
nodeExporter:
  enabled: false  # If you don't need node metrics

# Reduce scrape frequency
interval: 60s  # from 30s
```

## Upgrading

```bash
# Update Helm repository
helm repo update

# Upgrade stack
helm upgrade prometheus prometheus-community/kube-prometheus-stack \
  --namespace qalitrack-monitoring \
  --values values-prometheus-stack.yaml \
  --reuse-values
```

## Uninstall

```bash
# Delete ServiceMonitors
kubectl delete -f servicemonitors.yaml

# Uninstall Helm chart
helm uninstall prometheus -n qalitrack-monitoring

# Delete namespace
kubectl delete namespace qalitrack-monitoring
```

## Security

### Change Grafana Password

1. Edit `values-prometheus-stack.yaml`:
   ```yaml
   grafana:
     adminPassword: "your-strong-password"
   ```

2. Upgrade Helm chart:
   ```bash
   helm upgrade prometheus prometheus-community/kube-prometheus-stack \
     --namespace qalitrack-monitoring \
     --values values-prometheus-stack.yaml
   ```

### Expose Securely with Ingress

```yaml
grafana:
  ingress:
    enabled: true
    ingressClassName: nginx
    annotations:
      cert-manager.io/cluster-issuer: "letsencrypt-prod"
    hosts:
      - grafana.qalibrated.co.ke
    tls:
      - secretName: grafana-tls
        hosts:
          - grafana.qalibrated.co.ke
```

## Resources

- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [kube-prometheus-stack Chart](https://github.com/prometheus-community/helm-charts/tree/main/charts/kube-prometheus-stack)
- [PromQL Query Examples](https://prometheus.io/docs/prometheus/latest/querying/examples/)

---

**Last Updated:** March 2026
**Version:** 1.0.0
