# Qalitrack Kubernetes Helm Charts

Professional Helm charts for deploying the Qalitrack microservices platform on a **64GB VPS**.

## 🏗️ Architecture

```
Internet
   ↓
Gateway Service (LoadBalancer - port 7000)
   ↓ JWT Auth + API Routing
   ├─→ user-service (ClusterIP - port 7001)
   ├─→ masterdata-service (ClusterIP - port 7002)
   ├─→ transaction-service (ClusterIP - port 7003)
   └─→ backup-service (ClusterIP - port 7004)
```

**Gateway Service:**
- Exposed to internet via LoadBalancer
- Handles JWT authentication and authorization
- Routes to all microservices
- Your domain: `qalibrated.co.ke/qalitrack/api`

**Microservices:**
- Internal ClusterIP services (not exposed externally)
- Only accessible through Gateway
- Communicate via RabbitMQ for async operations

**Optional Ingress:**
- You can enable Ingress (set `ingress.enabled: true`) if you need:
  - SSL/TLS termination via cert-manager
  - Multiple domain routing
  - Advanced traffic management

## 📁 Chart Structure

```
helm-charts/
├── qalitrack-platform/        # Umbrella chart (deploys everything)
│   ├── Chart.yaml
│   ├── values.yaml            # Production config
│   ├── values-staging.yaml    # Staging config
│   ├── values-dev.yaml        # Development config
│   └── templates/
├── gateway-service/           # API Gateway
├── user-service/              # User management
├── masterdata-service/        # Vehicles, products, suppliers
├── transaction-service/       # Weighbridge transactions
└── backup-service/            # Database backups
```

## 🚀 Quick Start

### Prerequisites

```bash
# Install Helm 3
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Add Bitnami repository (for PostgreSQL, Redis, RabbitMQ)
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
```

### Deploy Production Environment

```bash
cd kubernetes/helm-charts/qalitrack-platform

# Create secrets first
kubectl create secret generic qalitrack-secrets \
  --from-literal=jwt-secret=YOUR_JWT_SECRET \
  --from-literal=postgres-password=YOUR_DB_PASSWORD \
  --from-literal=redis-password=YOUR_REDIS_PASSWORD \
  --from-literal=rabbitmq-password=YOUR_RABBITMQ_PASSWORD \
  -n qalitrack-prod

# Install the platform
helm install qalitrack . \
  --namespace qalitrack-prod \
  --create-namespace \
  --values values.yaml

# Or upgrade existing installation
helm upgrade --install qalitrack . \
  --namespace qalitrack-prod \
  --values values.yaml
```

### Deploy Staging Environment

```bash
helm install qalitrack-staging . \
  --namespace qalitrack-staging \
  --create-namespace \
  --values values-staging.yaml
```

### Deploy Development Environment

```bash
helm install qalitrack-dev . \
  --namespace qalitrack-dev \
  --create-namespace \
  --values values-dev.yaml
```

## 💾 Resource Allocation (64GB VPS)

### Production Environment
| Service | Replicas | Requests | Limits | Storage |
|---------|----------|----------|--------|---------|
| Gateway | 2 | 256Mi | 512Mi | - |
| User Service | 2 | 512Mi x2 | 2Gi x2 | - |
| MasterData | 2 | 512Mi x2 | 2Gi x2 | - |
| Transaction | 2 | 768Mi x2 | 2.5Gi x2 | - |
| Backup | 1 | 256Mi | 512Mi | - |
| PostgreSQL (user) | 1 | 1Gi | 3Gi | 20Gi |
| PostgreSQL (masterdata) | 1 | 1Gi | 3Gi | 30Gi |
| PostgreSQL (transaction) | 1 | 1Gi | 3Gi | 50Gi |
| Redis | 1 | 512Mi | 2Gi | 10Gi |
| RabbitMQ | 1 | 512Mi | 2Gi | 10Gi |
| **TOTAL** | | **~8.5Gi** | **~27Gi** | **120Gi** |

### Staging Environment
- **Total Requests:** ~3.5Gi
- **Total Limits:** ~13Gi
- **Storage:** 60Gi

### Development Environment
- **Total Requests:** ~1.5Gi
- **Total Limits:** ~7Gi
- **Storage:** 30Gi

### Summary
- **Available RAM:** 64GB
- **System Reserved:** ~8GB
- **Max Allocated:** ~47GB (leaves 9GB buffer)
- ✅ **Safe for 64GB VPS**

## 🔧 Configuration

### Secrets Management

Create a `secrets.yaml` file (DON'T commit this):

```yaml
jwtSecret: "your-super-secret-jwt-key-min-32-chars"
postgresPassword: "your-postgres-password"
redisPassword: "your-redis-password"
rabbitmqPassword: "your-rabbitmq-password"
rabbitmqErlangCookie: "your-erlang-cookie"
```

Then install with:
```bash
helm install qalitrack . --values values.yaml --values secrets.yaml
```

### Environment Variables

Edit `values.yaml` or use `--set`:

```bash
helm upgrade qalitrack . \
  --set global.environment=production \
  --set gateway.replicaCount=3 \
  --set ingress.hosts[0].host=api.yourdomain.com
```

### Domain Configuration

Update ingress hosts in values files:

```yaml
ingress:
  hosts:
    - host: api.qalitrack.com  # Change this
```

## 📊 Monitoring

### Check Deployment Status

```bash
# List all releases
helm list -A

# Get deployment status
helm status qalitrack -n qalitrack-prod

# Check pod status
kubectl get pods -n qalitrack-prod

# Check services
kubectl get svc -n qalitrack-prod

# Check resource usage
kubectl top pods -n qalitrack-prod
kubectl top nodes
```

### View Logs

```bash
# Gateway logs
kubectl logs -f deployment/gateway-service -n qalitrack-prod

# User service logs
kubectl logs -f deployment/user-service -n qalitrack-prod

# All pods logs
kubectl logs -f -l app.kubernetes.io/part-of=qalitrack -n qalitrack-prod
```

## 🔄 Updates & Rollbacks

### Update Application

```bash
# Update image version
helm upgrade qalitrack . \
  --set gateway.image.tag=v1.2.0 \
  --reuse-values

# Roll out new version
kubectl rollout restart deployment/gateway-service -n qalitrack-prod
kubectl rollout status deployment/gateway-service -n qalitrack-prod
```

### Rollback

```bash
# View history
helm history qalitrack -n qalitrack-prod

# Rollback to previous version
helm rollback qalitrack -n qalitrack-prod

# Rollback to specific revision
helm rollback qalitrack 2 -n qalitrack-prod
```

## 🗑️ Uninstall

```bash
# Uninstall release (keeps PVCs)
helm uninstall qalitrack -n qalitrack-prod

# Delete namespace and all resources
kubectl delete namespace qalitrack-prod

# Delete PVCs manually if needed
kubectl delete pvc -l app.kubernetes.io/part-of=qalitrack -n qalitrack-prod
```

## 🔐 Security Best Practices

1. **Always use secrets** for sensitive data
2. **Enable RBAC** for service accounts
3. **Use network policies** to isolate services
4. **Enable TLS** for ingress
5. **Scan images** for vulnerabilities
6. **Rotate credentials** regularly
7. **Enable pod security policies**
8. **Use read-only root filesystems** where possible

## 🎯 Production Checklist

- [ ] Create separate namespace for production
- [ ] Configure persistent volumes
- [ ] Set up secrets (JWT, DB passwords)
- [ ] Configure ingress with valid domain
- [ ] Enable TLS certificates (Let's Encrypt)
- [ ] Set up monitoring (Prometheus/Grafana)
- [ ] Configure log aggregation (Loki/ELK)
- [ ] Set up automated backups
- [ ] Configure resource quotas
- [ ] Enable autoscaling (HPA)
- [ ] Set up CI/CD pipeline
- [ ] Configure alerting
- [ ] Document runbooks
- [ ] Test disaster recovery

## 📚 Additional Resources

- [Helm Documentation](https://helm.sh/docs/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Bitnami Charts](https://github.com/bitnami/charts)
- [Qalitrack GitHub](https://github.com/Qalitrack/qalitrackservices)

## 🆘 Troubleshooting

### Pods Not Starting

```bash
kubectl describe pod <pod-name> -n qalitrack-prod
kubectl logs <pod-name> -n qalitrack-prod
```

### Out of Memory

```bash
# Check resource usage
kubectl top nodes
kubectl top pods -n qalitrack-prod

# Adjust limits in values.yaml
```

### Database Connection Issues

```bash
# Check PostgreSQL status
kubectl exec -it <postgres-pod> -n qalitrack-prod -- psql -U postgres

# Check connection string
kubectl get configmap -n qalitrack-prod
```

### Ingress Not Working

```bash
# Check ingress controller
kubectl get ingress -n qalitrack-prod
kubectl describe ingress -n qalitrack-prod

# Check ingress controller logs
kubectl logs -f -n ingress-nginx <ingress-controller-pod>
```

---

**Maintained by**: Qalitrack Team
**Last Updated**: March 2026
**Version**: 1.0.0
