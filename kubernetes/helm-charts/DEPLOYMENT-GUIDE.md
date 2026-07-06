# Qalitrack Kubernetes Deployment Guide
## Professional Production Deployment on 64GB VPS

This guide covers deploying the complete Qalitrack microservices platform using Helm charts on a 64GB VPS.

---

## 🏗️ Architecture Overview

```
Internet
   ↓
qalibrated.co.ke:7000/qalitrack/api
   ↓
Gateway Service (LoadBalancer)
   ↓ JWT Auth + Routing
   ├─→ user-service       (ClusterIP - Authentication & Users)
   ├─→ masterdata-service (ClusterIP - Vehicles, Products, Customers)
   ├─→ transaction-service (ClusterIP - Weighbridge Transactions)
   └─→ backup-service     (ClusterIP - Database Backups)
```

**Key Points:**
- **Gateway Service** is exposed directly via LoadBalancer (port 7000)
- Gateway handles JWT authentication and routes to all microservices
- All microservices use **ClusterIP** (internal only, not exposed externally)
- Point your domain `qalibrated.co.ke` to the LoadBalancer IP

**API Endpoints:**
```
http://qalibrated.co.ke:7000/qalitrack/api/user/*        → user-service
http://qalibrated.co.ke:7000/qalitrack/api/masterdata/*  → masterdata-service
http://qalibrated.co.ke:7000/qalitrack/api/transaction/* → transaction-service
http://qalibrated.co.ke:7000/qalitrack/api/backup/*      → backup-service
```

**Optional:** Enable Ingress (`ingress.enabled: true`) if you need SSL/TLS or want to use standard ports (80/443).

---

## 📋 Prerequisites

### System Requirements
- **Kubernetes Cluster**: v1.24+ (minikube, k3s, or managed cluster)
- **Helm**: v3.10+
- **kubectl**: v1.24+
- **VPS RAM**: 64GB
- **Storage**: 200GB SSD minimum
- **CPU**: 8 cores minimum

### Install Tools

```bash
# Install Helm 3
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Verify installation
helm version
kubectl version --client

# Add Bitnami repository
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
```

---

## 🚀 Step-by-Step Deployment

### Step 1: Create Namespaces

```bash
cd kubernetes/namespaces

# Apply updated namespaces for 64GB VPS
kubectl apply -f qalitrack-prod-64gb.yaml

# Verify
kubectl get namespaces -l managed-by=qalitrack
kubectl get resourcequota -n qalitrack-prod
```

### Step 2: Create Secrets

Create a `secrets.yaml` file (**DO NOT commit this file!**):

```yaml
# secrets.yaml
jwtSecret: "your-super-secret-jwt-key-minimum-32-characters-long"
postgresPassword: "your-secure-postgres-password"
redisPassword: "your-secure-redis-password"
rabbitmqPassword: "your-secure-rabbitmq-password"
rabbitmqErlangCookie: "your-erlang-cookie-here"

# Email Configuration (optional)
emailSmtpHost: "smtp.gmail.com"
emailSmtpPort: "587"
emailSmtpUsername: "your-email@gmail.com"
emailSmtpPassword: "your-app-password"
emailFromEmail: "noreply@qalibrated.co.ke"
emailFromName: "Qalitrack System"
emailEnableSsl: "true"
```

Apply secrets:

```bash
kubectl create secret generic qalitrack-secrets \
  --from-literal=JWT_SECRET_KEY="$(cat secrets.yaml | grep jwtSecret | cut -d':' -f2 | xargs)" \
  --from-literal=DB_PASSWORD="$(cat secrets.yaml | grep postgresPassword | cut -d':' -f2 | xargs)" \
  --from-literal=USER_SERVICE_REDIS_PASSWORD="$(cat secrets.yaml | grep redisPassword | cut -d':' -f2 | xargs)" \
  --from-literal=RABBITMQ_PASSWORD="$(cat secrets.yaml | grep rabbitmqPassword | cut -d':' -f2 | xargs)" \
  -n qalitrack-prod

# Verify
kubectl get secrets -n qalitrack-prod
```

### Step 3: Deploy Storage Class (if needed)

For local development or k3s:

```bash
cat <<EOF | kubectl apply -f -
apiVersion: storage.k8.io/v1
kind: StorageClass
metadata:
  name: local-path
provisioner: rancher.io/local-path
volumeBindingMode: WaitForFirstConsumer
reclaimPolicy: Delete
EOF
```

### Step 4: Deploy the Platform

```bash
cd kubernetes/helm-charts/qalitrack-platform

# Dry run to verify
helm install qalitrack . \
  --namespace qalitrack-prod \
  --dry-run \
  --debug \
  --values values.yaml

# Deploy to production
helm install qalitrack . \
  --namespace qalitrack-prod \
  --create-namespace \
  --values values.yaml \
  --timeout 10m \
  --wait

# Check status
helm status qalitrack -n qalitrack-prod
```

### Step 5: Verify Deployment

```bash
# Check all pods
kubectl get pods -n qalitrack-prod
kubectl get pods -n qalitrack-prod -w  # Watch in real-time

# Check services
kubectl get svc -n qalitrack-prod

# Check persistent volumes
kubectl get pvc -n qalitrack-prod

# Check resource usage
kubectl top pods -n qalitrack-prod
kubectl top nodes
```

### Step 6: Access Services

```bash
# Port forward to access services locally
kubectl port-forward svc/gateway-service 7000:7000 -n qalitrack-prod &
kubectl port-forward svc/user-service 7001:7001 -n qalitrack-prod &

# Test endpoints
curl http://localhost:7000/health
curl http://localhost:7001/health
```

---

## 🌐 Access Your Deployment

### Option 1: Direct Access via LoadBalancer (Recommended)

By default, the Gateway Service is exposed as LoadBalancer:

```bash
# Get the LoadBalancer IP
kubectl get svc gateway-service -n qalitrack-prod

# Example output:
# NAME              TYPE           CLUSTER-IP      EXTERNAL-IP      PORT(S)
# gateway-service   LoadBalancer   10.43.100.50    YOUR_VPS_IP      7000:32000/TCP
```

Point your DNS to the EXTERNAL-IP:

```
qalibrated.co.ke  A  YOUR_VPS_IP
```

Access your API at:
```
http://qalibrated.co.ke:7000/qalitrack/api
```

---

## 🌐 Configure Ingress & SSL (Optional)

**Note:** This section is optional. Only needed if you want SSL/TLS or port 443 access.

### Install NGINX Ingress Controller

```bash
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

helm install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --set controller.service.type=LoadBalancer
```

### Install Cert-Manager for SSL

```bash
# Install cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Create Let's Encrypt issuer
cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@qalibrated.co.ke  # Change this to your admin email!
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF
```

### Configure DNS

Point your domain to your VPS IP:

```
qalibrated.co.ke          A    YOUR_VPS_IP
staging.qalibrated.co.ke  A    YOUR_VPS_IP
dev.qalibrated.co.ke      A    YOUR_VPS_IP
```

### Enable Ingress

Update `values.yaml`:

```yaml
ingress:
  enabled: true
  className: "nginx"
  hosts:
    - host: qalibrated.co.ke  # Your actual domain
```

Upgrade deployment:

```bash
helm upgrade qalitrack . \
  --namespace qalitrack-prod \
  --values values.yaml \
  --reuse-values
```

---

## 📊 Monitoring & Logs

### View Logs

```bash
# Gateway logs
kubectl logs -f deployment/gateway-service -n qalitrack-prod

# All services logs
kubectl logs -f -l app.kubernetes.io/part-of=qalitrack -n qalitrack-prod

# Tail last 50 lines
kubectl logs --tail=50 deployment/user-service -n qalitrack-prod
```

### Check Events

```bash
kubectl get events -n qalitrack-prod --sort-by='.lastTimestamp'
```

### Resource Monitoring

```bash
# Real-time resource usage
kubectl top pods -n qalitrack-prod
kubectl top nodes

# Detailed pod info
kubectl describe pod <pod-name> -n qalitrack-prod
```

---

## 🔄 Updates & Maintenance

### Update Application Version

```bash
# Update image version
helm upgrade qalitrack . \
  --set gateway.image.tag=v1.2.0 \
  --set userService.image.tag=v1.2.0 \
  --reuse-values \
  -n qalitrack-prod

# Force pod restart
kubectl rollout restart deployment/gateway-service -n qalitrack-prod
kubectl rollout status deployment/gateway-service -n qalitrack-prod
```

### Scale Services

```bash
# Manual scaling
kubectl scale deployment/gateway-service --replicas=3 -n qalitrack-prod

# Or via Helm
helm upgrade qalitrack . \
  --set gateway.replicaCount=3 \
  --reuse-values \
  -n qalitrack-prod
```

### Rollback Deployment

```bash
# View history
helm history qalitrack -n qalitrack-prod

# Rollback to previous version
helm rollback qalitrack -n qalitrack-prod

# Rollback to specific revision
helm rollback qalitrack 2 -n qalitrack-prod
```

---

## 🔒 Security Hardening

### 1. Network Policies

Create network policy to isolate services:

```yaml
apiVersion: networking.k8.io/v1
kind: NetworkPolicy
metadata:
  name: allow-same-namespace
  namespace: qalitrack-prod
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: qalitrack-prod
  egress:
  - to:
    - namespaceSelector:
        matchLabels:
          name: qalitrack-prod
```

### 2. RBAC

The charts include service accounts with minimal permissions.

### 3. Pod Security

All deployments include:
- Non-root user execution
- Read-only root filesystem
- Dropped capabilities
- Security context constraints

---

## 💾 Backup & Recovery

### Database Backups

The backup service runs automatically (daily at 2 AM).

Manual backup:

```bash
# PostgreSQL backup
kubectl exec -it postgresql-0 -n qalitrack-prod -- \
  pg_dump -U postgres qalitrack > backup-$(date +%Y%m%d).sql

# Copy from pod
kubectl cp qalitrack-prod/postgresql-0:/backup-file.sql ./local-backup.sql
```

### Disaster Recovery

```bash
# Export all resources
kubectl get all -n qalitrack-prod -o yaml > qalitrack-backup.yaml

# Restore
kubectl apply -f qalitrack-backup.yaml
```

---

## 🐛 Troubleshooting

### Pods Not Starting

```bash
kubectl describe pod <pod-name> -n qalitrack-prod
kubectl logs <pod-name> -n qalitrack-prod --previous
```

### Out of Memory

```bash
# Check node resources
kubectl describe node

# Check pod limits
kubectl describe pod <pod-name> -n qalitrack-prod | grep -A 5 "Limits"
```

### Database Connection Issues

```bash
# Test PostgreSQL connection
kubectl exec -it postgresql-0 -n qalitrack-prod -- psql -U postgres

# Check service DNS
kubectl run -it --rm debug --image=busybox --restart=Never -- \
  nslookup postgresql.qalitrack-prod.svc.cluster.local
```

### Ingress Issues

```bash
# Check ingress
kubectl describe ingress -n qalitrack-prod

# Check ingress controller logs
kubectl logs -f -n ingress-nginx deployment/ingress-nginx-controller
```

---

## 📈 Performance Tuning

### PostgreSQL Tuning

Already configured in `values.yaml`:
- `shared_buffers = 1GB`
- `effective_cache_size = 3GB`
- `max_connections = 200`

### Redis Tuning

Configured for LRU eviction:
- `maxmemory 2gb`
- `maxmemory-policy allkeys-lru`

### Application Tuning

Adjust resources in `values.yaml` based on actual usage:

```yaml
resources:
  requests:
    memory: "512Mi"  # Increase if needed
    cpu: "250m"
  limits:
    memory: "2Gi"
    cpu: "1000m"
```

---

## 🎯 Production Checklist

- [ ] Kubernetes cluster ready
- [ ] Namespaces created
- [ ] Secrets configured
- [ ] Storage class configured
- [ ] Helm charts deployed
- [ ] All pods running
- [ ] Ingress controller installed
- [ ] SSL certificates configured
- [ ] DNS configured
- [ ] Monitoring enabled
- [ ] Backup strategy in place
- [ ] Security policies applied
- [ ] Load testing completed
- [ ] Documentation updated
- [ ] Team trained

---

## 📞 Support

For issues or questions:
- **GitHub**: https://github.com/Qalitrack/qalitrackservices/issues
- **Email**: support@qalibrated.co.ke
- **Documentation**: Check README files in each chart directory

---

**Deployment Guide Version**: 1.0.0
**Last Updated**: March 2026
**Maintained by**: Qalitrack Team
