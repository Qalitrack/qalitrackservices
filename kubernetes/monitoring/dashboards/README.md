# Qalitrack Custom Grafana Dashboards

Custom Grafana dashboards tailored for the Qalitrack weighbridge management platform.

## Available Dashboards

### 1. Qalitrack Platform Overview
**File:** `qalitrack-overview.json`

**Purpose:** High-level view of the entire Qalitrack platform

**Panels:**
- **Services Status** - Up/Down status of all microservices
- **Total Requests/sec** - Request rate per service
- **Error Rate %** - Error percentage by service
- **Pod CPU Usage** - CPU consumption by pod
- **Pod Memory Usage** - Memory usage by pod
- **Database Connections** - Active PostgreSQL connections
- **Redis Hit Rate** - Cache efficiency

**Best for:**
- Daily platform health monitoring
- Quick overview before deep-dives
- Stakeholder presentations

### 2. Qalitrack Transactions Dashboard
**File:** `qalitrack-transactions.json`

**Purpose:** Deep dive into transaction service (core business logic)

**Panels:**
- **Transactions per Minute** - Business throughput
- **Transaction Success Rate** - SLA metric
- **Transaction Latency** - p50, p95, p99 response times
- **Failed Transactions** - Last hour failures
- **Active DB Connections** - Transaction DB connections
- **Transaction Service CPU/Memory** - Resource usage
- **Transaction Errors by Type** - HTTP status breakdown
- **Slow Transactions** - Transactions >1s

**Best for:**
- Business operations monitoring
- Performance troubleshooting
- SLA tracking
- Capacity planning

## Installation

### Quick Import

```bash
cd kubernetes/monitoring/dashboards
./import-dashboards.sh
```

### Manual Import

```bash
# Port-forward to Grafana
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80

# Open Grafana: http://localhost:3000
# Login: admin / (get password below)

# Get password
kubectl get secret prometheus-grafana -n qalitrack-monitoring \
  -o jsonpath="{.data.admin-password}" | base64 -d

# In Grafana UI:
# 1. Click + (Create) → Import
# 2. Upload JSON file or paste JSON content
# 3. Select Prometheus datasource
# 4. Click Import
```

## Dashboard Details

### Metrics Used

#### HTTP Metrics
```promql
# Request rate
rate(http_requests_total{namespace="qalitrack-prod"}[5m])

# Error rate
rate(http_requests_total{status=~"5.."}[5m]) / rate(http_requests_total[5m])

# Latency (p99)
histogram_quantile(0.99, rate(http_request_duration_seconds_bucket[5m]))
```

#### Resource Metrics
```promql
# CPU usage
rate(container_cpu_usage_seconds_total{namespace="qalitrack-prod"}[5m])

# Memory usage
container_memory_usage_bytes{namespace="qalitrack-prod"}

# Network I/O
rate(container_network_receive_bytes_total[5m])
```

#### Database Metrics
```promql
# Active connections
pg_stat_activity_count{datname="qalitrack_transaction"}

# Query duration
pg_stat_statements_mean_time_seconds
```

#### Redis Metrics
```promql
# Hit rate
rate(redis_keyspace_hits_total[5m]) /
  (rate(redis_keyspace_hits_total[5m]) + rate(redis_keyspace_misses_total[5m]))

# Memory usage
redis_memory_used_bytes
```

### Variables

Dashboards support variables for filtering:

- `$namespace` - Kubernetes namespace (default: qalitrack-prod)
- `$service` - Service name (all services by default)
- `$interval` - Time range for aggregation

Edit in Grafana: Dashboard Settings → Variables

### Alerts

Create alerts from dashboard panels:

1. Click panel title → Edit
2. Alert tab → Create Alert
3. Set conditions (e.g., error rate > 1%)
4. Configure notification channels
5. Save

Example alert:
```yaml
Name: High Transaction Error Rate
Condition: Error Rate > 1% for 5 minutes
Notify: #alerts Slack channel
```

## Customization

### Add Custom Panel

```json
{
  "id": 10,
  "title": "My Custom Metric",
  "type": "graph",
  "gridPos": {"h": 8, "w": 12, "x": 0, "y": 28},
  "targets": [
    {
      "expr": "your_prometheus_query_here",
      "legendFormat": "{{label_name}}"
    }
  ]
}
```

### Modify Thresholds

```json
"thresholds": {
  "steps": [
    {"value": 0, "color": "green"},
    {"value": 80, "color": "yellow"},  // Warning at 80%
    {"value": 95, "color": "red"}      // Critical at 95%
  ]
}
```

### Change Refresh Rate

```json
"refresh": "10s"  // Options: 5s, 10s, 30s, 1m, 5m, 15m, 30m, 1h
```

## Business Metrics

### Track Business KPIs

Add these to your application code:

```csharp
// In Transaction Service
public class TransactionMetrics
{
    private static readonly Counter TransactionsTotal = Metrics
        .CreateCounter("transactions_total", "Total transactions processed",
            new CounterConfiguration
            {
                LabelNames = new[] { "transaction_type", "status" }
            });

    public void RecordTransaction(string type, string status)
    {
        TransactionsTotal.WithLabels(type, status).Inc();
    }
}
```

Then query in Grafana:
```promql
rate(transactions_total[5m])
```

### Revenue Metrics

```csharp
private static readonly Histogram TransactionValue = Metrics
    .CreateHistogram("transaction_value_usd", "Transaction value in USD");

public void RecordRevenue(decimal amount)
{
    TransactionValue.Observe((double)amount);
}
```

Query:
```promql
sum(rate(transaction_value_usd_sum[1h]))  # Revenue per hour
```

## Dashboard Maintenance

### Export Modified Dashboard

1. Open dashboard
2. Dashboard settings (gear icon)
3. JSON Model → Copy
4. Save to file
5. Commit to Git

### Version Control

Keep dashboards in Git:
```bash
git add kubernetes/monitoring/dashboards/*.json
git commit -m "Update Qalitrack dashboards"
```

### Backup

```bash
# Export all dashboards
cd kubernetes/monitoring/dashboards

for dashboard_id in $(curl -s http://admin:$GRAFANA_PASSWORD@localhost:3000/api/search | jq -r '.[].uid'); do
    curl -s http://admin:$GRAFANA_PASSWORD@localhost:3000/api/dashboards/uid/$dashboard_id | \
      jq '.dashboard' > backup-$dashboard_id.json
done
```

## Best Practices

### 1. Use Time Ranges Wisely

- **Last 1 hour** - Real-time troubleshooting
- **Last 6 hours** - Recent trends
- **Last 24 hours** - Daily patterns
- **Last 7 days** - Weekly patterns

### 2. Set Appropriate Refresh Rates

- **5-10s** - Critical production monitoring
- **30s** - Standard monitoring
- **1-5m** - Overview dashboards

### 3. Group Related Panels

- Top row: High-level KPIs
- Middle rows: Detailed metrics
- Bottom rows: Resource metrics

### 4. Use Consistent Colors

- Green: Healthy/Success
- Yellow: Warning
- Red: Critical/Error
- Blue: Information

### 5. Add Descriptions

```json
"description": "Shows the p99 latency for transaction API calls. Alert if > 1 second."
```

## Troubleshooting

### No Data Showing

```bash
# Check if Prometheus is scraping
kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090

# Open http://localhost:9090
# Status → Targets
# Verify qalitrack-prod targets are UP
```

### Metrics Not Found

```bash
# Check if services expose /metrics
kubectl port-forward -n qalitrack-prod svc/gateway-service 7000:7000
curl http://localhost:7000/metrics

# Should return Prometheus format metrics
```

### Dashboard Import Failed

1. Check JSON syntax (use jsonlint.com)
2. Ensure datasource exists
3. Check Grafana logs:
   ```bash
   kubectl logs -n qalitrack-monitoring deployment/prometheus-grafana
   ```

## Resources

- [Grafana Documentation](https://grafana.com/docs/)
- [Prometheus Query Examples](https://prometheus.io/docs/prometheus/latest/querying/examples/)
- [Dashboard Best Practices](https://grafana.com/docs/grafana/latest/best-practices/best-practices-for-creating-dashboards/)

---

**Last Updated:** March 2026
**Version:** 1.0.0
**Compatible with:** Grafana 9.x+
