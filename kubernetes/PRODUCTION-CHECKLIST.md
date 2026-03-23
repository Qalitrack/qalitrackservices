# Qalitrack Platform - Production Readiness Checklist

## ✅ Currently Enabled (Production-Ready)

### Core Services
- ✅ **Gateway Service** (YARP) - API Gateway with routing
- ✅ **User Service** - Authentication & user management
- ✅ **MasterData Service** - Vehicles, products, suppliers
- ✅ **Transaction Service** - Weighbridge transactions
- ✅ **Backup Service** - Scheduled database backups
- ✅ **Technician Service** - Technician management
- ✅ **QTruck Service** - Drivers & fleet management (API + Frontend)

### Infrastructure
- ✅ **PostgreSQL** - Per-service databases with performance tuning
- ✅ **Redis** - Caching layer
- ✅ **RabbitMQ** - Message queue

### Networking & Security
- ✅ **Ingress (NGINX)** - SSL/TLS termination with Let's Encrypt
- ✅ **LoadBalancer** - Gateway service external access
- ✅ **CORS** - Cross-origin resource sharing configured
- ✅ **SSL Redirect** - Force HTTPS

### Monitoring & Observability
- ✅ **Prometheus** - Metrics collection (15d retention, 50GB)
- ✅ **Grafana** - Dashboards & visualization
- ✅ **Loki** - Log aggregation (30GB)
- ✅ **ServiceMonitors** - All services exposing metrics

### Reliability
- ✅ **HorizontalPodAutoscaler** - Auto-scaling (2-5 replicas)
- ✅ **Health Checks** - Liveness & readiness probes
- ✅ **Resource Limits** - CPU & memory constraints
- ✅ **Persistent Volumes** - Data persistence for DBs

### CI/CD
- ✅ **GitHub Actions** - Automated builds & tests
- ✅ **Docker Registry** - GHCR.io image storage
- ✅ **Kubernetes Deployment** - Auto-deploy on push

## ⚠️ Production Recommendations

### 1. Security Hardening

#### Change Default Passwords
```yaml
# In values.yaml - CHANGE THESE!
postgresql:
  auth:
    postgresPassword: "changeme"  # ⚠️ CHANGE THIS

redis:
  auth:
    password: "changeme"  # ⚠️ CHANGE THIS

rabbitmq:
  auth:
    password: "changeme"  # ⚠️ CHANGE THIS
    erlangCookie: "secretcookie"  # ⚠️ CHANGE THIS

monitoring:
  grafana:
    adminPassword: "changeme"  # ⚠️ CHANGE THIS
```

**How to change:**
```bash
# Use Kubernetes secrets
kubectl create secret generic qalitrack-secrets \
  -n qalitrack-prod \
  --from-literal=POSTGRES_PASSWORD='YOUR_SECURE_PASSWORD' \
  --from-literal=REDIS_PASSWORD='YOUR_SECURE_PASSWORD' \
  --from-literal=RABBITMQ_PASSWORD='YOUR_SECURE_PASSWORD' \
  --from-literal=GRAFANA_PASSWORD='YOUR_SECURE_PASSWORD'
```

#### Network Policies
Create network policies to restrict pod-to-pod communication:

```yaml
# Create: kubernetes/security/network-policies.yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: deny-all-ingress
  namespace: qalitrack-prod
spec:
  podSelector: {}
  policyTypes:
  - Ingress
---
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-gateway-to-services
  namespace: qalitrack-prod
spec:
  podSelector:
    matchLabels:
      app.kubernetes.io/part-of: qalitrack
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app.kubernetes.io/name: gateway-service
```

#### Pod Security Standards
Enable pod security policies:

```yaml
# Add to namespace
apiVersion: v1
kind: Namespace
metadata:
  name: qalitrack-prod
  labels:
    pod-security.kubernetes.io/enforce: restricted
    pod-security.kubernetes.io/audit: restricted
    pod-security.kubernetes.io/warn: restricted
```

### 2. Backup & Disaster Recovery

#### Database Backups
- ✅ Currently: Daily CronJob backups at 2 AM
- ✅ Retention: 30 days
- ⚠️ **TODO**: Test restore procedure
- ⚠️ **TODO**: Off-site backup storage (S3/GCS)

**Setup off-site backups:**
```yaml
# Add to cronjob-postgres-backup.yaml
# Upload to S3 after backup
aws s3 cp /backups/*.tar.gz s3://qalitrack-backups/$(date +%Y-%m-%d)/
```

#### Disaster Recovery Plan
1. Document restore procedures
2. Test backups monthly
3. Maintain infrastructure-as-code (✅ Already done with Helm)
4. Document DNS/SSL certificate recovery

### 3. High Availability

#### Multi-Zone Deployment
**Current**: Single-zone deployment
**Recommended**: Multi-zone for production

```yaml
# Add to deployments
affinity:
  podAntiAffinity:
    preferredDuringSchedulingIgnoredDuringExecution:
    - weight: 100
      podAffinityTerm:
        labelSelector:
          matchLabels:
            app.kubernetes.io/name: gateway-service
        topologyKey: topology.kubernetes.io/zone
```

#### Database HA
**Current**: Single PostgreSQL instance per service
**Recommended**: PostgreSQL with replication

```yaml
postgresql:
  architecture: replication
  replication:
    enabled: true
    readReplicas: 2
```

#### Redis HA
**Current**: Standalone Redis
**Recommended**: Redis Sentinel or Cluster

```yaml
redis:
  architecture: replication
  sentinel:
    enabled: true
  replica:
    replicaCount: 3
```

### 4. Resource Management

#### Review Resource Limits
Current limits are conservative. Monitor and adjust:

```bash
# Check actual resource usage
kubectl top pods -n qalitrack-prod
kubectl top nodes

# Adjust values.yaml based on actual usage
```

#### Set Resource Quotas
Prevent resource exhaustion:

```yaml
apiVersion: v1
kind: ResourceQuota
metadata:
  name: qalitrack-quota
  namespace: qalitrack-prod
spec:
  hard:
    requests.cpu: "20"
    requests.memory: 40Gi
    limits.cpu: "40"
    limits.memory: 60Gi
    persistentvolumeclaims: "20"
```

### 5. Monitoring Enhancements

#### Alert Rules (Prometheus)
Create alert rules for critical issues:

```yaml
# prometheus-alerts.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-alerts
  namespace: qalitrack-prod
data:
  alerts.yml: |
    groups:
    - name: qalitrack
      rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.05
        for: 5m
        annotations:
          summary: "High error rate detected"

      - alert: ServiceDown
        expr: up{job="kubernetes-service-endpoints"} == 0
        for: 2m
        annotations:
          summary: "Service {{ $labels.service }} is down"

      - alert: HighMemoryUsage
        expr: container_memory_usage_bytes / container_spec_memory_limit_bytes > 0.9
        for: 5m
        annotations:
          summary: "Container {{ $labels.pod }} high memory usage"
```

#### Grafana Dashboards
Import these standard dashboards:
1. **Node Exporter Full** - ID: 1860
2. **Kubernetes Cluster Monitoring** - ID: 7249
3. **PostgreSQL Database** - ID: 9628
4. **Redis Dashboard** - ID: 11835
5. **NGINX Ingress Controller** - ID: 9614

### 6. SSL/TLS Certificates

#### Verify cert-manager Installation
```bash
# Check if cert-manager is installed
kubectl get pods -n cert-manager

# If not, install it
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
```

#### Create ClusterIssuer
```yaml
# cert-manager-issuer.yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@qalibrated.co.ke  # CHANGE THIS
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
```

### 7. Performance Optimization

#### Enable HTTP/2
```yaml
ingress:
  annotations:
    nginx.ingress.kubernetes.io/http2-push-preload: "true"
```

#### Enable Compression
```yaml
ingress:
  annotations:
    nginx.ingress.kubernetes.io/enable-gzip: "true"
    nginx.ingress.kubernetes.io/gzip-types: "application/json,text/css,text/javascript"
```

#### Connection Pool Tuning
Review PostgreSQL connection strings:
- Min pool size: 50
- Max pool size: 500
- Connection idle lifetime: 300s

Monitor and adjust based on actual usage.

### 8. Logging Best Practices

#### Structured Logging
Ensure all services use structured JSON logs:

```csharp
// Example for .NET services
Log.Information("Transaction created: {TransactionId} {Amount}", txId, amount);
```

#### Log Rotation
Configure Loki retention:

```yaml
loki:
  config:
    table_manager:
      retention_deletes_enabled: true
      retention_period: 720h  # 30 days
```

### 9. Deployment Strategy

#### Blue-Green Deployments
Already configured with rolling updates:

```yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxSurge: 1
    maxUnavailable: 0
```

#### Deployment Verification
Before production deployment:

```bash
# 1. Dry-run
helm upgrade --install qalitrack-platform . --dry-run --debug

# 2. Test in staging
helm upgrade --install qalitrack-platform . -n qalitrack-staging

# 3. Deploy to production
helm upgrade --install qalitrack-platform . -n qalitrack-prod

# 4. Monitor rollout
kubectl rollout status deployment/gateway-service -n qalitrack-prod
```

### 10. Documentation

#### Maintain Runbooks
- ✅ MONITORING.md - Dashboard access
- ✅ DEPLOYMENT-GUIDE.md - Deployment instructions
- ⚠️ **TODO**: Create RUNBOOK.md for common operations
- ⚠️ **TODO**: Create INCIDENT-RESPONSE.md for outages

## 🎯 Priority Actions (Do These First!)

### Critical (Do Immediately)
1. ✅ Enable monitoring (Done!)
2. ⚠️ **Change all default passwords**
3. ⚠️ **Setup SSL certificates with cert-manager**
4. ⚠️ **Test backup restore procedure**
5. ⚠️ **Configure Prometheus alerts**

### High Priority (This Week)
1. Setup off-site backups (S3/GCS)
2. Create network policies
3. Enable pod security standards
4. Import Grafana dashboards
5. Document runbooks

### Medium Priority (This Month)
1. Setup PostgreSQL replication
2. Setup Redis Sentinel
3. Configure multi-zone deployment
4. Setup automated certificate renewal
5. Create disaster recovery plan

### Low Priority (As Needed)
1. Performance tuning based on metrics
2. Custom Grafana dashboards
3. Advanced alert rules
4. Load testing and optimization

## 📊 Current Resource Usage Estimate

### 64GB VPS Allocation
```
Service                Requests    Limits      Replicas
==================    =========   ========    ========
Gateway (x2)          256Mi       512Mi       2
User Service (x2)     1Gi         4Gi         2
MasterData (x2)       1Gi         4Gi         2
Transaction (x2)      1.5Gi       5Gi         2
Backup (x1)           256Mi       512Mi       1
Technician (x2)       1Gi         4Gi         2
QTruck API (x2)       1Gi         4Gi         2
QTruck Frontend (x2)  512Mi       1Gi         2
PostgreSQL (x4)       4Gi         12Gi        4
Redis (x1)            512Mi       2Gi         1
RabbitMQ (x1)         512Mi       2Gi         1
Prometheus (x1)       2Gi         4Gi         1
Grafana (x1)          256Mi       512Mi       1
Loki (x1)             512Mi       2Gi         1
=====================================
TOTAL Requests:       ~14Gi
TOTAL Limits:         ~45Gi
OS/System Reserved:   ~19GB
```

**Status**: ✅ Fits comfortably within 64GB VPS

## 🔐 Security Checklist

- [ ] Changed PostgreSQL passwords
- [ ] Changed Redis password
- [ ] Changed RabbitMQ password
- [ ] Changed Grafana password
- [ ] Configured network policies
- [ ] Enabled pod security standards
- [ ] SSL certificates with Let's Encrypt
- [ ] Configured secrets management
- [ ] Enabled ingress rate limiting
- [ ] Configured RBAC properly
- [ ] Audit logging enabled
- [ ] Vulnerability scanning setup

## 📈 Monitoring Checklist

- [x] Prometheus installed
- [x] Grafana installed
- [x] ServiceMonitors configured
- [ ] Alert rules configured
- [ ] Grafana dashboards imported
- [ ] Alertmanager configured
- [ ] PagerDuty/Slack integration
- [ ] Uptime monitoring (external)

## 💾 Backup Checklist

- [x] Database backup CronJob
- [ ] Tested backup restore
- [ ] Off-site backup storage
- [ ] Backup monitoring/alerts
- [ ] Configuration backup
- [ ] SSL certificate backup
- [ ] Secrets backup (encrypted)

## 🚀 Performance Checklist

- [x] Resource limits configured
- [x] Autoscaling enabled
- [ ] HTTP/2 enabled
- [ ] Compression enabled
- [ ] Database query optimization
- [ ] Connection pool tuning
- [ ] CDN for static assets
- [ ] Load testing performed

## 📞 Support & Escalation

### Critical Issues (P0)
- Complete service outage
- Data loss
- Security breach

**Response**: Immediate (< 15 minutes)

### High Priority (P1)
- Partial service degradation
- Performance issues
- Failed deployments

**Response**: < 1 hour

### Medium Priority (P2)
- Non-critical bugs
- Monitoring alerts
- Documentation updates

**Response**: < 4 hours

### Low Priority (P3)
- Feature requests
- Optimization
- Nice-to-have improvements

**Response**: < 1 day

---

**Last Updated**: 2026-03-22
**Platform Version**: 1.0.0
**Kubernetes Version**: 1.24+
