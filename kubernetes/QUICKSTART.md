# Qalitrack Kubernetes Quick Start Guide

Complete step-by-step guide to deploy Qalitrack on a 64GB VPS with Kubernetes.

## Overview

This guide will help you deploy:
- ✅ All 5 Qalitrack microservices
- ✅ PostgreSQL databases (3 instances)
- ✅ Redis caching
- ✅ RabbitMQ message queue
- ✅ NGINX Ingress with SSL/TLS
- ✅ Prometheus + Grafana monitoring
- ✅ Automated daily backups

**Time required:** ~30-45 minutes

---

## Prerequisites

### Required

- **Kubernetes Cluster** (v1.24+)
  - minikube, k3s, or managed cluster (GKE, EKS, AKS)
- **kubectl** installed and configured
- **Helm 3** installed (v3.10+)
- **64GB RAM** on VPS
- **200GB SSD** storage minimum
- **Domain name** (qalibrated.co.ke) pointing to your cluster

### Check Prerequisites

```bash
# Check Kubernetes
kubectl version --client
kubectl cluster-info

# Check Helm
helm version

# Check cluster resources
kubectl top nodes
```

---

## Step-by-Step Deployment

### Step 1: Clone Repository

```bash
git clone https://github.com/Qalitrack/qalitrackservices.git
cd qalitrackservices/kubernetes
```

### Step 2: Create Namespaces

```bash
cd namespaces

# Apply namespaces with resource quotas
./apply-all.sh

# Verify
kubectl get namespaces -l managed-by=qalitrack
```

**Created namespaces:**
- `qalitrack-prod` - Production environment
- `qalitrack-staging` - Staging environment
- `qalitrack-dev` - Development environment
- `qalitrack-monitoring` - Observability stack

### Step 3: Generate Secrets

```bash
cd ../secrets

# Generate all secrets
./generate-secrets.sh

# IMPORTANT: Edit secrets.env and update EMAIL settings
nano secrets.env
# Update: EMAIL_SMTP_HOST, EMAIL_SMTP_USERNAME, EMAIL_SMTP_PASSWORD

# Create Kubernetes secrets
./create-k8s-secrets.sh

# Verify
kubectl get secrets -n qalitrack-prod
kubectl describe secret qalitrack-secrets -n qalitrack-prod
```

**✅ Checkpoint:** You should see `qalitrack-secrets` with 17 data keys.

### Step 4: Install NGINX Ingress Controller (Optional)

Skip this if you want to expose Gateway directly. Install for SSL/TLS and standard ports (80/443).

```bash
cd ../ingress-nginx

# Install NGINX Ingress
./install.sh

# Wait for external IP
kubectl get svc -n ingress-nginx ingress-nginx-controller -w
# Press Ctrl+C when EXTERNAL-IP appears
```

**Get the external IP:**
```bash
EXTERNAL_IP=$(kubectl get svc ingress-nginx-controller -n ingress-nginx \
  -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "External IP: $EXTERNAL_IP"
```

**Update DNS:**
Point your domain to this IP:
```
qalibrated.co.ke  A  <EXTERNAL-IP>
```

Verify DNS:
```bash
nslookup qalibrated.co.ke
# Should return the external IP
```

**✅ Checkpoint:** DNS should resolve to your cluster IP.

### Step 5: Install cert-manager for SSL (Optional)

Only if you installed NGINX Ingress in Step 4.

```bash
cd ../cert-manager

# Install cert-manager
./install.sh

# Verify
kubectl get pods -n cert-manager
kubectl get clusterissuers
```

**✅ Checkpoint:** You should see `letsencrypt-prod` and `letsencrypt-staging` issuers.

### Step 6: Deploy Qalitrack Platform

```bash
cd ../helm-charts/qalitrack-platform

# Add Bitnami repository (for PostgreSQL, Redis, RabbitMQ)
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update

# Dry run to check configuration
helm install qalitrack . \
  --namespace qalitrack-prod \
  --dry-run \
  --debug

# Deploy to production
helm install qalitrack . \
  --namespace qalitrack-prod \
  --create-namespace \
  --timeout 10m \
  --wait

# Check deployment status
kubectl get pods -n qalitrack-prod
```

**Wait for all pods to be Running:**
```bash
watch kubectl get pods -n qalitrack-prod
# Press Ctrl+C when all pods show 1/1 or 2/2 Running
```

**✅ Checkpoint:** All pods should be in `Running` state with `1/1` or `2/2` ready.

### Step 7: Verify Services

```bash
# Check all services
kubectl get svc -n qalitrack-prod

# Check deployments
kubectl get deployments -n qalitrack-prod

# Check persistent volumes
kubectl get pvc -n qalitrack-prod
```

### Step 8: Test API Access

#### If Using Ingress (with SSL):

```bash
# Test health endpoint
curl https://qalibrated.co.ke/qalitrack/api/health

# Should return: {"status":"healthy"}
```

#### If Direct Access (no Ingress):

```bash
# Port-forward Gateway Service
kubectl port-forward svc/gateway-service 7000:7000 -n qalitrack-prod &

# Test locally
curl http://localhost:7000/health

# Or get LoadBalancer IP
GATEWAY_IP=$(kubectl get svc gateway-service -n qalitrack-prod \
  -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
curl http://$GATEWAY_IP:7000/health
```

**✅ Checkpoint:** Health endpoint should return `{"status":"healthy"}`.

### Step 9: Install Monitoring (Recommended)

```bash
cd ../../monitoring

# Install Prometheus + Grafana
./install.sh

# Wait for pods to be ready
kubectl get pods -n qalitrack-monitoring -w
```

**Access Grafana:**
```bash
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80
```

Open: http://localhost:3000
- Username: `admin`
- Password: `admin` (change on first login)

**✅ Checkpoint:** Grafana dashboard loads and shows Kubernetes metrics.

### Step 10: Setup Automated Backups

```bash
cd ../backups

# Install backup CronJob
./install.sh

# Verify CronJob
kubectl get cronjob -n qalitrack-prod

# Trigger manual backup to test
kubectl create job --from=cronjob/postgres-backup manual-test-$(date +%s) -n qalitrack-prod

# Check backup job
kubectl get jobs -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=postgres-backup
```

**✅ Checkpoint:** Backup job completes successfully.

---

## Deployment Complete! 🎉

Your Qalitrack platform is now running on Kubernetes!

### Access Points

**API Gateway:**
- With Ingress: https://qalibrated.co.ke/qalitrack/api
- Direct: http://<GATEWAY_IP>:7000

**Grafana Monitoring:**
```bash
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80
# Open: http://localhost:3000
```

**Prometheus:**
```bash
kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090
# Open: http://localhost:9090
```

---

## Post-Deployment Tasks

### 1. Change Grafana Password

```bash
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80
# Login and change password from admin/admin
```

### 2. Setup Alerting

Edit `kubernetes/monitoring/values-prometheus-stack.yaml`:
```yaml
receivers:
- name: 'email'
  email_configs:
  - to: 'admin@qalibrated.co.ke'
    from: 'alertmanager@qalibrated.co.ke'
    smarthost: 'smtp.gmail.com:587'
    auth_username: 'your-email@gmail.com'
    auth_password: 'your-app-password'
```

Update:
```bash
cd kubernetes/monitoring
helm upgrade prometheus prometheus-community/kube-prometheus-stack \
  --namespace qalitrack-monitoring \
  --values values-prometheus-stack.yaml
```

### 3. Test Backup Restore

```bash
# See: kubernetes/backups/README.md for restore procedures
```

### 4. Setup Auto-Deployment

Add GitHub Actions workflow for auto-deployment after builds:
```bash
# See: kubernetes/ci-cd/README.md
```

---

## Troubleshooting

### Pods Not Starting

```bash
# Check pod status
kubectl get pods -n qalitrack-prod

# Describe problematic pod
kubectl describe pod <pod-name> -n qalitrack-prod

# Check logs
kubectl logs <pod-name> -n qalitrack-prod
kubectl logs <pod-name> -n qalitrack-prod --previous
```

### Database Connection Errors

```bash
# Check PostgreSQL pods
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=postgresql

# Check PostgreSQL logs
kubectl logs -n qalitrack-prod user-service-postgresql-0

# Test database connection
kubectl exec -it user-service-postgresql-0 -n qalitrack-prod -- psql -U postgres
```

### Ingress Not Working

```bash
# Check Ingress resource
kubectl get ingress -n qalitrack-prod
kubectl describe ingress qalitrack-ingress -n qalitrack-prod

# Check NGINX logs
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller -f

# Check certificate (if using SSL)
kubectl get certificate -n qalitrack-prod
kubectl describe certificate qalitrack-tls -n qalitrack-prod
```

### Out of Memory

```bash
# Check node resources
kubectl top nodes

# Check pod resource usage
kubectl top pods -n qalitrack-prod

# Scale down if needed
kubectl scale deployment/transaction-service --replicas=1 -n qalitrack-prod
```

---

## Common Commands

### View Logs

```bash
# All services
kubectl logs -f -l app.kubernetes.io/part-of=qalitrack -n qalitrack-prod

# Specific service
kubectl logs -f deployment/gateway-service -n qalitrack-prod
```

### Restart Services

```bash
# Restart all
kubectl rollout restart deployment -n qalitrack-prod

# Restart specific
kubectl rollout restart deployment/gateway-service -n qalitrack-prod
```

### Scale Services

```bash
# Manual scaling
kubectl scale deployment/transaction-service --replicas=4 -n qalitrack-prod

# Or via Helm
helm upgrade qalitrack . \
  --set transactionService.replicaCount=4 \
  --reuse-values \
  -n qalitrack-prod
```

### Update Application

```bash
# Update image version
helm upgrade qalitrack . \
  --set gateway.image.tag=v1.2.0 \
  --reuse-values \
  -n qalitrack-prod

# Or trigger rollout restart to pull latest
cd kubernetes/ci-cd
./rollout-restart.sh all
```

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                        Internet                          │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────▼──────────┐
         │ NGINX Ingress (SSL)  │
         │   qalibrated.co.ke   │
         └───────────┬──────────┘
                     │
         ┌───────────▼──────────┐
         │  Gateway Service     │◄────── JWT Auth
         │  (Your Custom GW)    │        API Routing
         └───────────┬──────────┘
                     │
      ┌──────────────┼──────────────┐
      │              │              │
┌─────▼─────┐  ┌────▼────┐  ┌─────▼─────┐
│   User    │  │ Master  │  │Transaction│
│  Service  │  │  Data   │  │  Service  │
│           │  │ Service │  │           │
└─────┬─────┘  └────┬────┘  └─────┬─────┘
      │             │              │
      │         ┌───▼───┐          │
      └────────►│ Redis │◄─────────┘
                └───────┘
      │             │              │
      └─────────────┼──────────────┘
                    │
            ┌───────▼────────┐
            │   RabbitMQ     │
            └────────────────┘

┌──────────────────────────────────────────┐
│         Monitoring Stack                  │
│  Prometheus  +  Grafana  +  AlertManager │
└──────────────────────────────────────────┘
```

---

## Resource Usage Summary

**Total resource requests:** ~10Gi
**Total resource limits:** ~32Gi
**Available on 64GB VPS:** ✅ Fits comfortably

Breakdown:
- Platform services: ~8.5Gi
- Monitoring: ~1.2Gi
- NGINX Ingress: ~0.3Gi

---

## Next Steps

1. **Configure CI/CD**: See `kubernetes/ci-cd/README.md`
2. **Add Log Aggregation**: Optional Loki setup
3. **Setup GitOps**: Optional ArgoCD for automated deployments
4. **Load Testing**: Test your setup under load
5. **DR Planning**: Document disaster recovery procedures

---

## Support

- **Documentation**: Check README.md files in each directory
- **Troubleshooting**: See DEPLOYMENT-GUIDE.md for detailed troubleshooting
- **Issues**: https://github.com/Qalitrack/qalitrackservices/issues

---

**Deployment Version:** 1.0.0
**Last Updated:** March 2026
**Kubernetes Version:** 1.24+
**Helm Version:** 3.10+
