# Qalitrack Platform - Monitoring & Observability

This guide explains how to access Grafana, Prometheus, and other monitoring dashboards from your laptop.

## 🚀 Quick Access

### Dashboard URLs (via Subdomains)

Once the platform is deployed with monitoring enabled:

- **Grafana**: https://grafana.qalibrated.co.ke
- **Prometheus**: https://prometheus.qalibrated.co.ke
- **Main API**: https://api.qalibrated.co.ke

### Default Credentials

**Grafana:**
- Username: `admin`
- Password: `changeme` (⚠️ CHANGE THIS IMMEDIATELY!)

## 📊 What's Included

### Prometheus (Metrics Collection)
- Collects metrics from all microservices via ServiceMonitors
- 15-day retention period
- 50GB storage
- Scrapes every 30 seconds

### Grafana (Visualization)
- Pre-configured Prometheus datasource
- Pre-configured Loki datasource (logs)
- Persistent dashboards storage (10GB)
- Built-in Kubernetes dashboards

### Loki (Log Aggregation)
- Collects logs from all pods
- 30GB storage
- Query logs via Grafana

## 🔌 Alternative Access Methods

### 1. Port Forwarding (Local Development)

If you don't have domain access or ingress is disabled:

```bash
# Access Grafana
kubectl port-forward -n qalitrack-prod svc/grafana 3000:80
# Then open: http://localhost:3000

# Access Prometheus
kubectl port-forward -n qalitrack-prod svc/prometheus-server 9090:80
# Then open: http://localhost:9090

# Access Loki
kubectl port-forward -n qalitrack-prod svc/loki 3100:3100
# Then open: http://localhost:3100
```

### 2. Kubernetes Dashboard

Install the official Kubernetes dashboard:

```bash
# Install dashboard
kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml

# Create admin user
kubectl create serviceaccount dashboard-admin -n kubernetes-dashboard
kubectl create clusterrolebinding dashboard-admin --clusterrole=cluster-admin --serviceaccount=kubernetes-dashboard:dashboard-admin

# Get access token
kubectl -n kubernetes-dashboard create token dashboard-admin

# Port forward
kubectl port-forward -n kubernetes-dashboard svc/kubernetes-dashboard 8443:443

# Open: https://localhost:8443
# Use the token from above to login
```

## 📈 Available Metrics

All services expose metrics at `/metrics` endpoint:

- **Gateway Service**: Port 7000
- **User Service**: Port 7001
- **MasterData Service**: Port 7002
- **Transaction Service**: Port 7003
- **Backup Service**: Port 7004
- **Technician Service**: Port 7006
- **QTruck API**: Port 7007
- **QTruck Frontend**: Port 7008

## 🎯 Common Grafana Dashboards

### Pre-installed Dashboards
1. **Kubernetes Cluster Monitoring** - Overall cluster health
2. **Pod Metrics** - CPU, Memory, Network per pod
3. **Service Performance** - Request rates, latencies, errors
4. **Database Monitoring** - PostgreSQL metrics
5. **Redis Monitoring** - Cache hit rates, memory usage

### Creating Custom Dashboards

1. Login to Grafana
2. Click **+ → Dashboard**
3. Add panels with PromQL queries
4. Example queries:
   ```promql
   # Request rate per service
   rate(http_requests_total[5m])

   # Memory usage
   container_memory_usage_bytes{namespace="qalitrack-prod"}

   # CPU usage
   rate(container_cpu_usage_seconds_total{namespace="qalitrack-prod"}[5m])
   ```

## 🔒 Security Recommendations

### Change Default Passwords

```bash
# Update Grafana password in values.yaml
helm upgrade qalitrack-platform ./kubernetes/helm-charts/qalitrack-platform \
  --set monitoring.grafana.adminPassword="YOUR_SECURE_PASSWORD"
```

### Enable Authentication

Edit `values.yaml`:

```yaml
monitoring:
  grafana:
    grafana.ini:
      auth.anonymous:
        enabled: false  # Disable anonymous access
      auth.basic:
        enabled: true   # Require login
```

### Restrict Ingress Access

Add IP whitelist to ingress annotations:

```yaml
ingress:
  annotations:
    nginx.ingress.kubernetes.io/whitelist-source-range: "YOUR_IP/32"
```

## 📊 Monitoring Architecture

```
┌─────────────────────────────────────────────────┐
│                  Your Laptop                    │
│  https://qalibrated.co.ke/grafana              │
└──────────────────┬──────────────────────────────┘
                   │
                   │ HTTPS (443)
                   ▼
┌─────────────────────────────────────────────────┐
│          NGINX Ingress Controller               │
│  - SSL Termination (Let's Encrypt)             │
│  - Path-based routing                           │
└──────────────┬──────────────────────────────────┘
               │
     ┌─────────┼──────────┬─────────────┐
     │         │          │             │
     ▼         ▼          ▼             ▼
┌─────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│ Grafana │ │Prometheus│ │   Loki   │ │ Gateway  │
│  :80    │ │   :80    │ │  :3100   │ │  :7000   │
└─────────┘ └──────────┘ └──────────┘ └──────────┘
               │                          │
               │ Scrape /metrics          │
               └──────────────────────────┘
                         │
          ┌──────────────┴──────────────┐
          │    All Microservices         │
          │  (User, Transaction, etc.)   │
          └──────────────────────────────┘
```

## 🛠️ Troubleshooting

### Grafana Not Accessible

```bash
# Check Grafana pod status
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=grafana

# Check Grafana logs
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=grafana

# Check ingress status
kubectl get ingress -n qalitrack-prod
kubectl describe ingress qalitrack-ingress -n qalitrack-prod
```

### Prometheus Not Scraping Metrics

```bash
# Check ServiceMonitors
kubectl get servicemonitors -n qalitrack-prod

# Check Prometheus targets
# Forward Prometheus port, then visit: http://localhost:9090/targets
kubectl port-forward -n qalitrack-prod svc/prometheus-server 9090:80
```

### No Metrics in Grafana

1. Check Prometheus datasource configuration in Grafana
2. Verify Prometheus is scraping targets: http://localhost:9090/targets
3. Check ServiceMonitor labels match Prometheus serviceMonitorSelector
4. Verify services have `/metrics` endpoints

## 📚 Useful Resources

- [Grafana Documentation](https://grafana.com/docs/)
- [Prometheus Querying](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [Loki Query Language](https://grafana.com/docs/loki/latest/logql/)
- [Kubernetes Monitoring Guide](https://kubernetes.io/docs/tasks/debug/debug-cluster/resource-metrics-pipeline/)

## 🔄 Updating Configuration

After changing monitoring configuration:

```bash
# Update Helm dependencies
cd kubernetes/helm-charts/qalitrack-platform
helm dependency update

# Apply changes
helm upgrade qalitrack-platform . -n qalitrack-prod --values values.yaml

# Verify deployment
kubectl rollout status deployment/grafana -n qalitrack-prod
kubectl rollout status deployment/prometheus-server -n qalitrack-prod
```

## 📞 Support

For monitoring issues:
1. Check this guide first
2. Review logs: `kubectl logs -n qalitrack-prod <pod-name>`
3. Check ingress: `kubectl describe ingress qalitrack-ingress -n qalitrack-prod`
4. Verify DNS: `nslookup qalibrated.co.ke`
