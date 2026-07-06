# Qalitrack Platform Deployment Guide

Comprehensive deployment procedures for the Qalitrack Kubernetes infrastructure.

## Prerequisites Validation

### System Requirements

**Kubernetes Cluster:**
- Version: 1.24 or later
- Node capacity: Minimum 64GB RAM, 16 CPU cores
- Storage: 200GB available (for PersistentVolumes)
- StorageClass: Default dynamic provisioner

**Client Tools:**
- kubectl 1.24+
- Helm 3.10+
- (Optional) argocd CLI for GitOps

**Network Requirements:**
- Cluster has internet access for pulling images
- LoadBalancer support (for NGINX Ingress) or NodePort capability
- DNS domain (for production SSL certificates)

### Verification Commands

```bash
# Verify Kubernetes version
kubectl version --short

# Check node resources
kubectl top nodes
kubectl describe nodes | grep -A 5 "Allocated resources"

# Verify storage class
kubectl get storageclass

# Check Helm version
helm version --short
```

## Namespace and RBAC Setup

### Create Namespaces

```bash
cd kubernetes/namespaces

# Production namespace
kubectl apply -f namespace-prod.yaml

# Staging namespace (optional)
kubectl apply -f namespace-staging.yaml

# Monitoring namespace
kubectl create namespace qalitrack-monitoring
```

### Apply Resource Quotas

```bash
cd kubernetes/security

# Apply resource quotas
kubectl apply -f resource-quotas.yaml

# Verify quotas
kubectl describe resourcequota -n qalitrack-prod
```

## Secrets Management

### Generate Secrets

The secret generation script creates:
- JWT signing keys (RS256 key pair)
- PostgreSQL passwords
- Redis password
- RabbitMQ credentials
- SMTP configuration

```bash
cd kubernetes/secrets

# Generate secrets (creates secrets.env)
./generate-secrets.sh

# Edit secrets.env to configure:
# - EMAIL_SMTP_HOST
# - EMAIL_SMTP_USERNAME
# - EMAIL_SMTP_PASSWORD
# - EMAIL_FROM_ADDRESS
vi secrets.env

# Create Kubernetes Secret
./create-k8s-secrets.sh
```

### Verify Secret Creation

```bash
kubectl get secret qalitrack-secrets -n qalitrack-prod
kubectl describe secret qalitrack-secrets -n qalitrack-prod
```

Secret should contain 17+ data keys including:
- JWT_PRIVATE_KEY
- JWT_PUBLIC_KEY
- POSTGRES_PASSWORD
- REDIS_PASSWORD
- RABBITMQ_PASSWORD

## Infrastructure Components

### NGINX Ingress Controller

Required for external traffic routing and SSL termination.

```bash
cd kubernetes/ingress-nginx

# Install NGINX Ingress
./install.sh

# Wait for external IP assignment
kubectl get svc -n ingress-nginx ingress-nginx-controller -w
```

**Note:** On cloud providers (GKE, EKS, AKS), this creates a LoadBalancer service with external IP. On bare metal, configure NodePort or MetalLB.

### cert-manager (TLS Certificates)

Automates TLS certificate issuance from Let's Encrypt.

```bash
cd kubernetes/cert-manager

# Install cert-manager
./install.sh

# Verify installation
kubectl get pods -n cert-manager
kubectl get clusterissuers
```

Expected ClusterIssuers:
- letsencrypt-prod (production certificates)
- letsencrypt-staging (testing)

### DNS Configuration

Point your domain to the NGINX Ingress external IP:

```bash
# Get external IP
EXTERNAL_IP=$(kubectl get svc ingress-nginx-controller -n ingress-nginx \
  -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

echo "Configure DNS A record:"
echo "qalibrated.co.ke → $EXTERNAL_IP"
```

Create DNS A record:
```
qalibrated.co.ke        A    <EXTERNAL_IP>
*.qalibrated.co.ke      A    <EXTERNAL_IP>
```

Verify DNS propagation:
```bash
nslookup qalibrated.co.ke
dig qalibrated.co.ke +short
```

## Platform Deployment

### Helm Chart Preparation

```bash
cd kubernetes/helm-charts/qalitrack-platform

# Update dependencies (downloads PostgreSQL, Redis, RabbitMQ charts)
helm dependency update

# Verify dependencies
helm dependency list
```

### Configuration Review

Review and modify `values.yaml` for your environment:

```yaml
# Domain configuration
ingress:
  enabled: true
  className: nginx
  hosts:
    - host: qalibrated.co.ke
      paths:
        - path: /qalitrack/api
          pathType: Prefix
  tls:
    - secretName: qalitrack-tls
      hosts:
        - qalibrated.co.ke

# Image registry and tags
gateway:
  image:
    repository: ghcr.io/qalitrack/gateway-service
    tag: latest

# Resource allocations (adjust based on cluster capacity)
userService:
  resources:
    requests:
      memory: "512Mi"
      cpu: "250m"
    limits:
      memory: "2Gi"
      cpu: "1000m"
```

### Dry Run Validation

Test deployment configuration without applying:

```bash
helm install qalitrack . \
  --namespace qalitrack-prod \
  --dry-run \
  --debug > dry-run-output.yaml

# Review output for errors
less dry-run-output.yaml
```

### Production Deployment

```bash
helm install qalitrack . \
  --namespace qalitrack-prod \
  --create-namespace \
  --timeout 10m \
  --wait

# Alternative: Use values-prod.yaml overlay
helm install qalitrack . \
  --namespace qalitrack-prod \
  --create-namespace \
  --values values-prod.yaml \
  --timeout 10m \
  --wait
```

### Deployment Verification

```bash
# Monitor pod startup
watch kubectl get pods -n qalitrack-prod

# Check deployment status
kubectl get deployments -n qalitrack-prod

# Verify services
kubectl get svc -n qalitrack-prod

# Check persistent volumes
kubectl get pvc -n qalitrack-prod

# Review events
kubectl get events -n qalitrack-prod --sort-by='.lastTimestamp' | tail -20
```

Expected pod count (default configuration):
- gateway-service: 2 replicas
- user-service: 2 replicas
- masterdata-service: 2 replicas
- transaction-service: 2 replicas
- backup-service: 1 replica
- qtruck-service-api: 2 replicas
- qtruck-service-frontend: 2 replicas
- PostgreSQL pods: 4-5 instances
- Redis: 1 instance
- RabbitMQ: 1 instance

### Post-Deployment Health Checks

```bash
# Test gateway endpoint
kubectl port-forward -n qalitrack-prod svc/gateway-service 7000:7000 &
curl http://localhost:7000/health

# Expected response: {"status":"healthy"}

# Test database connectivity (shared qalitrackdb)
kubectl exec -n qalitrack-prod -it deployment/user-service -- \
  nc -zv qalitrack-postgresql 5432

# Check service mesh connectivity
for svc in user-service masterdata-service transaction-service; do
  kubectl exec -n qalitrack-prod -it deployment/gateway-service -- \
    curl -s http://$svc:7001/health || echo "$svc unreachable"
done
```

## Monitoring Stack

### Prometheus and Grafana Installation

```bash
cd kubernetes/monitoring

# Install kube-prometheus-stack (Prometheus + Grafana + Alertmanager)
./install.sh

# Monitor installation
watch kubectl get pods -n qalitrack-monitoring
```

### ServiceMonitors Configuration

ServiceMonitors define Prometheus scraping targets:

```bash
# Apply ServiceMonitors
kubectl apply -f servicemonitors.yaml

# Verify ServiceMonitors
kubectl get servicemonitors -n qalitrack-prod

# Check Prometheus targets
kubectl port-forward -n qalitrack-monitoring \
  svc/prometheus-kube-prometheus-prometheus 9090:9090

# Access http://localhost:9090/targets
```

### Grafana Dashboard Import

```bash
cd kubernetes/monitoring/dashboards

# Import dashboards via ConfigMap
kubectl create configmap grafana-dashboards \
  --from-file=platform-overview.json \
  --from-file=transaction-service.json \
  -n qalitrack-monitoring \
  --dry-run=client -o yaml | kubectl apply -f -

# Access Grafana
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80

# Login: admin / admin (change password immediately)
```

## Backup Configuration

### Automated Backup CronJob

```bash
cd kubernetes/backups

# Create backup PVC
kubectl apply -f backup-pvc.yaml

# Deploy CronJob (runs daily at 02:00)
kubectl apply -f cronjob-postgres-backup.yaml

# Verify CronJob
kubectl get cronjob -n qalitrack-prod

# Trigger manual backup for testing
kubectl create job --from=cronjob/postgres-backup \
  manual-backup-$(date +%s) -n qalitrack-prod

# Check backup logs
kubectl logs -n qalitrack-prod -l app=postgres-backup
```

### Backup Verification

```bash
# List backup files
kubectl exec -n qalitrack-prod -it <backup-pod> -- ls -lh /backups

# Verify backup integrity
kubectl exec -n qalitrack-prod -it <backup-pod> -- \
  sha256sum /backups/*.tar.gz
```

## Network Policies

Apply network segmentation policies:

```bash
cd kubernetes/security

# Apply network policies
kubectl apply -f network-policies.yaml

# Verify policies
kubectl get networkpolicies -n qalitrack-prod

# Test policy enforcement
# This should succeed (gateway to backend)
kubectl exec -n qalitrack-prod -it deployment/gateway-service -- \
  curl -s http://user-service:7001/health

# This should fail (cross-namespace blocked)
kubectl run test-pod --rm -it --image=busybox -n default -- \
  wget -O- gateway-service.qalitrack-prod:7000/health
```

## Ingress and TLS

### Ingress Resource

The platform Helm chart creates an Ingress resource automatically. Verify:

```bash
kubectl get ingress -n qalitrack-prod
kubectl describe ingress qalitrack-ingress -n qalitrack-prod
```

### Certificate Issuance

cert-manager automatically requests a Let's Encrypt certificate:

```bash
# Check certificate request status
kubectl get certificaterequest -n qalitrack-prod

# Check certificate status
kubectl get certificate -n qalitrack-prod

# Describe certificate (view events)
kubectl describe certificate qalitrack-tls -n qalitrack-prod
```

Certificate issuance takes 1-5 minutes. Status should show:
```
Status:
  Conditions:
    Type:    Ready
    Status:  True
```

### TLS Verification

```bash
# Test HTTPS endpoint
curl -v https://qalibrated.co.ke/qalitrack/api/health

# Verify certificate
openssl s_client -connect qalibrated.co.ke:443 -servername qalibrated.co.ke < /dev/null 2>/dev/null | \
  openssl x509 -noout -subject -issuer -dates
```

## GitOps with ArgoCD (Optional)

### ArgoCD Installation

```bash
cd kubernetes/argocd

# Install ArgoCD
kubectl create namespace argocd
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

# Apply ArgoCD application
kubectl apply -f argocd-application.yaml

# Get admin password
kubectl -n argocd get secret argocd-initial-admin-secret \
  -o jsonpath="{.data.password}" | base64 -d

# Access ArgoCD UI
kubectl port-forward -n argocd svc/argocd-server 8080:443

# Login: admin / <password-from-above>
```

### Configure Git Repository

```bash
# Add repository to ArgoCD
argocd repo add https://github.com/qalitrack/qalitrackservices.git \
  --username <github-username> \
  --password <github-token>

# Sync application
argocd app sync qalitrack-platform

# Enable auto-sync
argocd app set qalitrack-platform --sync-policy automated
```

## Flagger for Canary Deployments (Optional)

### Flagger Installation

```bash
cd kubernetes/flagger

# Install Flagger
kubectl apply -f install.yaml

# Verify Flagger
kubectl get pods -n flagger-system

# Apply canary configurations
kubectl apply -f canary-gateway-service.yaml
kubectl apply -f canary-transaction-service.yaml
```

### Canary Deployment

Flagger automates progressive rollouts:

```bash
# Update image (triggers canary)
kubectl set image deployment/gateway-service \
  gateway=ghcr.io/qalitrack/gateway-service:v2.0.0 \
  -n qalitrack-prod

# Monitor canary progress
watch kubectl get canary -n qalitrack-prod

# View canary events
kubectl describe canary gateway-service -n qalitrack-prod
```

Canary progression:
1. Canary deployment created (10% traffic)
2. Metrics analyzed (success rate, latency)
3. Progressive traffic shift (10% → 20% → 50% → 100%)
4. Automatic rollback on metric threshold violations

## Production Hardening

### Change Default Passwords

```bash
# Update Grafana password
kubectl exec -n qalitrack-monitoring deployment/prometheus-grafana -- \
  grafana-cli admin reset-admin-password <new-password>

# Rotate PostgreSQL passwords
kubectl create secret generic qalitrack-secrets \
  --from-literal=POSTGRES_PASSWORD='<new-secure-password>' \
  --dry-run=client -o yaml | kubectl apply -f -

# Restart pods to pick up new secrets
kubectl rollout restart deployment -n qalitrack-prod
```

### Enable Audit Logging

Configure Kubernetes audit logs:

```bash
# On control plane nodes
vim /etc/kubernetes/audit-policy.yaml
# Add audit policy configuration

# Update kube-apiserver flags
--audit-log-path=/var/log/kubernetes/audit.log
--audit-policy-file=/etc/kubernetes/audit-policy.yaml
```

### Configure Alerting

```bash
cd kubernetes/monitoring

# Edit Prometheus alert rules
vim prometheus-alerts.yaml

# Apply alerts
kubectl apply -f prometheus-alerts.yaml

# Configure Alertmanager
vim alertmanager-config.yaml
kubectl create secret generic alertmanager-config \
  --from-file=alertmanager.yaml=alertmanager-config.yaml \
  -n qalitrack-monitoring
```

## Scaling Considerations

### Vertical Scaling

Increase resources for specific services:

```bash
helm upgrade qalitrack . \
  --set transaction.resources.requests.memory=1Gi \
  --set transaction.resources.limits.memory=4Gi \
  --reuse-values \
  -n qalitrack-prod
```

### Horizontal Scaling

HPA automatically scales based on CPU utilization. Manual override:

```bash
# Scale specific deployment
kubectl scale deployment/transaction-service --replicas=10 -n qalitrack-prod

# Or via Helm
helm upgrade qalitrack . \
  --set transaction.replicaCount=10 \
  --reuse-values \
  -n qalitrack-prod
```

### Database Scaling

Switch PostgreSQL to replication mode:

```yaml
userService:
  postgresql:
    architecture: replication
    replication:
      enabled: true
      readReplicas: 2
```

## Troubleshooting Deployment Issues

### Pod ImagePullBackOff

```bash
# Check image pull secrets
kubectl get secrets -n qalitrack-prod

# Describe pod for error details
kubectl describe pod <pod-name> -n qalitrack-prod

# Verify image exists
docker manifest inspect ghcr.io/qalitrack/gateway-service:latest
```

### PersistentVolumeClaim Pending

```bash
# Check PVC status
kubectl describe pvc <pvc-name> -n qalitrack-prod

# Verify StorageClass
kubectl get storageclass

# Check available PVs
kubectl get pv
```

### Service Unreachable

```bash
# Check service endpoints
kubectl get endpoints <service-name> -n qalitrack-prod

# Verify pod labels match service selector
kubectl get pods --show-labels -n qalitrack-prod
kubectl describe svc <service-name> -n qalitrack-prod

# Test direct pod connectivity
kubectl exec -it <source-pod> -n qalitrack-prod -- \
  nc -zv <target-service> <port>
```

### Certificate Not Issued

```bash
# Check cert-manager logs
kubectl logs -n cert-manager -l app=cert-manager

# Check certificate status
kubectl describe certificate qalitrack-tls -n qalitrack-prod

# Check challenges (HTTP-01 validation)
kubectl get challenges -n qalitrack-prod

# Verify DNS is pointing to cluster
nslookup qalibrated.co.ke
```

## Rollback Procedures

### Helm Rollback

```bash
# List releases
helm list -n qalitrack-prod

# View release history
helm history qalitrack -n qalitrack-prod

# Rollback to previous revision
helm rollback qalitrack -n qalitrack-prod

# Rollback to specific revision
helm rollback qalitrack 3 -n qalitrack-prod
```

### Deployment Rollback

```bash
# Check rollout history
kubectl rollout history deployment/gateway-service -n qalitrack-prod

# Rollback deployment
kubectl rollout undo deployment/gateway-service -n qalitrack-prod

# Rollback to specific revision
kubectl rollout undo deployment/gateway-service --to-revision=2 -n qalitrack-prod
```

## Maintenance Procedures

### Update Procedure

```bash
# 1. Update Helm chart version
helm repo update

# 2. Review changes
helm diff upgrade qalitrack . -n qalitrack-prod

# 3. Apply upgrade
helm upgrade qalitrack . -n qalitrack-prod

# 4. Monitor rollout
kubectl rollout status deployment/gateway-service -n qalitrack-prod
```

### Database Maintenance

```bash
# Access PostgreSQL pod
kubectl exec -it <postgres-pod> -n qalitrack-prod -- psql -U postgres

# Run VACUUM
VACUUM ANALYZE;

# Reindex
REINDEX DATABASE <database-name>;

# Check database size
SELECT pg_size_pretty(pg_database_size('<database-name>'));
```

## Environment-Specific Deployments

### Staging Environment

```bash
# Deploy to staging namespace
helm install qalitrack . \
  --namespace qalitrack-staging \
  --create-namespace \
  --values values-staging.yaml

# Use different ingress host
# values-staging.yaml:
# ingress:
#   hosts:
#     - host: staging.qalibrated.co.ke
```

### Development Environment

```bash
# Deploy with minimal resources
helm install qalitrack . \
  --namespace qalitrack-dev \
  --create-namespace \
  --values values-dev.yaml \
  --set global.resources.requests.memory=128Mi \
  --set global.resources.requests.cpu=50m
```

## Performance Optimization

### Connection Pooling

Optimize PostgreSQL connection strings:

```yaml
userService:
  postgresql:
    primary:
      configuration: |
        max_connections = 500
        shared_buffers = 2GB
        effective_cache_size = 6GB
        work_mem = 16MB
```

### Redis Optimization

```yaml
redis:
  master:
    configuration: |
      maxmemory 512mb
      maxmemory-policy allkeys-lru
      tcp-backlog 511
```

### Ingress Optimization

```yaml
ingress:
  annotations:
    nginx.ingress.kubernetes.io/proxy-body-size: "50m"
    nginx.ingress.kubernetes.io/proxy-connect-timeout: "600"
    nginx.ingress.kubernetes.io/proxy-send-timeout: "600"
    nginx.ingress.kubernetes.io/proxy-read-timeout: "600"
    nginx.ingress.kubernetes.io/enable-cors: "true"
```

## Appendix

### Helm Values Reference

Complete values.yaml structure documentation available at:
`helm-charts/qalitrack-platform/README.md`

### Port Reference

| Service | Container Port | Service Port | External Access |
|---------|---------------|--------------|-----------------|
| Gateway | 80 | 7000 | LoadBalancer or Ingress |
| User Service | 80 | 7001 | ClusterIP |
| MasterData | 80 | 7002 | ClusterIP |
| Transaction | 80 | 7003 | ClusterIP |
| Backup | 80 | 7004 | ClusterIP |
| QTruck API | 80 | 7007 | ClusterIP |
| PostgreSQL | 5432 | 5432 | ClusterIP |
| Redis | 6379 | 6379 | ClusterIP |
| RabbitMQ | 5672, 15672 | 5672, 15672 | ClusterIP |
| Prometheus | 9090 | 9090 | ClusterIP or Ingress |
| Grafana | 3000 | 80 | ClusterIP or Ingress |

### Resource Calculation

For N concurrent users, estimate resources:

**Formula:**
- Gateway: 128Mi + (N * 0.1Mi)
- Services: 512Mi + (N * 0.5Mi)
- PostgreSQL: 1Gi + (DB size * 0.25)
- Redis: Max(256Mi, cache size * 1.2)

Example (1000 concurrent users):
- Gateway: 228Mi
- Services: 1Gi per service
- PostgreSQL: 2Gi per database
- Redis: 512Mi

Total: ~15Gi + database sizes

**Last Updated:** 2026-03
**Version:** 1.0.0
