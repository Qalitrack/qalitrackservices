# ArgoCD GitOps for Qalitrack

Continuous delivery and GitOps automation for Qalitrack using ArgoCD.

## What is ArgoCD?

ArgoCD is a declarative, GitOps continuous delivery tool for Kubernetes that:
- **Watches your Git repository** for Helm chart changes
- **Automatically syncs** changes to your cluster
- **Self-heals** if someone makes manual changes
- **Provides visual UI** showing deployment status
- **Enables rollbacks** with one click

## Architecture

```
Git Repository (Source of Truth)
   ↓ (watches)
ArgoCD Controller
   ↓ (auto-syncs)
Kubernetes Cluster
```

**How it works:**
1. You push Helm chart changes to Git
2. ArgoCD detects the change
3. ArgoCD automatically applies it to Kubernetes
4. If manual changes are made, ArgoCD reverts them (self-heal)

## Installation

### Quick Install

```bash
cd kubernetes/argocd
./install.sh
```

### Manual Installation

```bash
# Create namespace
kubectl create namespace argocd

# Install ArgoCD
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/v2.9.3/manifests/install.yaml

# Wait for pods
kubectl wait --for=condition=ready pod -l app.kubernetes.io/name=argocd-server -n argocd --timeout=300s

# Get initial password
kubectl -n argocd get secret argocd-initial-admin-secret \
  -o jsonpath="{.data.password}" | base64 -d
```

## Access ArgoCD UI

### Option 1: Port Forward (Quick)

```bash
kubectl port-forward svc/argocd-server -n argocd 8080:443
```

Open: https://localhost:8080
- Username: `admin`
- Password: (from install output)

### Option 2: Ingress (Production)

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: argocd-server
  namespace: argocd
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-passthrough: "true"
    nginx.ingress.kubernetes.io/backend-protocol: "HTTPS"
spec:
  ingressClassName: nginx
  rules:
  - host: argocd.qalibrated.co.ke
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: argocd-server
            port:
              number: 443
  tls:
  - hosts:
    - argocd.qalibrated.co.ke
    secretName: argocd-tls
```

Apply:
```bash
kubectl apply -f argocd-ingress.yaml
```

Access: https://argocd.qalibrated.co.ke

## Deploy Qalitrack with ArgoCD

### Step 1: Create Application

```bash
kubectl apply -f qalitrack-application.yaml
```

This creates:
- `qalitrack-platform` Application
- `qalitrack` AppProject

### Step 2: Verify Deployment

```bash
# Check application status
kubectl get application -n argocd

# Watch synchronization
kubectl get application qalitrack-platform -n argocd -w
```

In ArgoCD UI, you'll see:
- Application status (Synced/OutOfSync)
- Health status (Healthy/Progressing/Degraded)
- Visual resource tree

### Step 3: Automatic Sync

ArgoCD will now automatically:
- Detect changes in Git
- Sync changes to cluster
- Self-heal if manual changes are made

## How GitOps Works

### Make a Change

```bash
# Edit Helm values
cd kubernetes/helm-charts/qalitrack-platform
nano values.yaml

# Change replica count
gateway:
  replicaCount: 3  # Was 2

# Commit and push
git add values.yaml
git commit -m "Scale gateway to 3 replicas"
git push
```

### ArgoCD Automatically:

1. **Detects** change in Git (within 3 minutes)
2. **Syncs** new replica count to cluster
3. **Updates** pods to 3 replicas
4. **Shows** status in UI

**No manual kubectl needed!**

## ArgoCD CLI

### Install CLI

```bash
curl -sSL -o argocd-linux-amd64 https://github.com/argoproj/argo-cd/releases/latest/download/argocd-linux-amd64
sudo install -m 555 argocd-linux-amd64 /usr/local/bin/argocd
rm argocd-linux-amd64
```

### Login

```bash
# Port-forward first
kubectl port-forward svc/argocd-server -n argocd 8080:443 &

# Login
argocd login localhost:8080
# Username: admin
# Password: (from install)
```

### Useful Commands

```bash
# List applications
argocd app list

# Get application details
argocd app get qalitrack-platform

# Sync application manually
argocd app sync qalitrack-platform

# View application logs
argocd app logs qalitrack-platform

# Rollback to previous version
argocd app rollback qalitrack-platform

# Delete application
argocd app delete qalitrack-platform
```

## Application Management

### Check Sync Status

```bash
# CLI
argocd app get qalitrack-platform

# kubectl
kubectl get application qalitrack-platform -n argocd -o yaml
```

### Manual Sync

If auto-sync is disabled:

```bash
argocd app sync qalitrack-platform
```

Or in UI: Click "SYNC" button

### Refresh (Re-check Git)

```bash
argocd app get qalitrack-platform --refresh
```

### View Diff

See what changed:

```bash
argocd app diff qalitrack-platform
```

## Rollback

### Via UI

1. Go to application
2. Click "HISTORY AND ROLLBACK"
3. Select previous revision
4. Click "ROLLBACK"

### Via CLI

```bash
# List history
argocd app history qalitrack-platform

# Rollback to specific revision
argocd app rollback qalitrack-platform 5
```

## Multi-Environment Setup

### Create Staging Application

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: qalitrack-staging
  namespace: argocd
spec:
  source:
    repoURL: https://github.com/Qalitrack/qalitrackservices.git
    targetRevision: staging  # Different branch
    path: kubernetes/helm-charts/qalitrack-platform
    helm:
      valueFiles:
        - values-staging.yaml  # Different values
  destination:
    server: https://kubernetes.default.svc
    namespace: qalitrack-staging
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
```

### Create Dev Application

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: qalitrack-dev
  namespace: argocd
spec:
  source:
    repoURL: https://github.com/Qalitrack/qalitrackservices.git
    targetRevision: develop  # Dev branch
    path: kubernetes/helm-charts/qalitrack-platform
    helm:
      valueFiles:
        - values-dev.yaml
  destination:
    server: https://kubernetes.default.svc
    namespace: qalitrack-dev
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
```

## Sync Policies

### Automatic Sync

```yaml
syncPolicy:
  automated:
    prune: true        # Delete resources removed from Git
    selfHeal: true     # Auto-fix manual changes
    allowEmpty: false  # Don't allow empty sync
```

### Manual Sync

```yaml
syncPolicy:
  automated: null  # Disable auto-sync
```

Useful for production where you want manual control.

### Sync Waves

Control order of resource deployment:

```yaml
metadata:
  annotations:
    argocd.argoproj.io/sync-wave: "1"  # Deploy first
```

Example:
- Wave 0: Namespaces, CRDs
- Wave 1: Secrets, ConfigMaps
- Wave 2: Databases
- Wave 3: Applications

## Health Assessment

ArgoCD automatically checks health:

```yaml
spec:
  health:
    timeout: 10m
```

Custom health checks:

```yaml
resource.customizations: |
  apps/Deployment:
    health.lua: |
      hs = {}
      if obj.status.availableReplicas == obj.spec.replicas then
        hs.status = "Healthy"
        hs.message = "All replicas available"
      else
        hs.status = "Progressing"
        hs.message = "Waiting for replicas"
      end
      return hs
```

## Notifications

### Slack Integration

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: argocd-notifications-cm
  namespace: argocd
data:
  service.slack: |
    token: $slack-token
  template.app-deployed: |
    message: |
      Application {{.app.metadata.name}} is now running new version.
  trigger.on-deployed: |
    - when: app.status.operationState.phase in ['Succeeded']
      send: [app-deployed]
```

### Email Notifications

```yaml
data:
  service.email.gmail: |
    username: $email-username
    password: $email-password
    host: smtp.gmail.com
    port: 587
```

## Monitoring ArgoCD

### Prometheus Metrics

ArgoCD exports metrics on port 8082:

```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: argocd-metrics
  namespace: qalitrack-monitoring
spec:
  selector:
    matchLabels:
      app.kubernetes.io/name: argocd-server
  endpoints:
  - port: metrics
```

### Grafana Dashboard

Import dashboard ID `14584` in Grafana for ArgoCD metrics.

## Troubleshooting

### Application OutOfSync

```bash
# Check diff
argocd app diff qalitrack-platform

# Common causes:
# 1. Manual kubectl changes (self-heal will fix)
# 2. HPA changed replicas (add to ignoreDifferences)
# 3. Git not pushed (push changes)
```

### Sync Failed

```bash
# View error
argocd app get qalitrack-platform

# Check logs
argocd app logs qalitrack-platform

# Common causes:
# 1. Invalid YAML
# 2. Missing secrets
# 3. Resource quotas exceeded
```

### Application Stuck Progressing

```bash
# Check pod status
kubectl get pods -n qalitrack-prod

# Describe application
kubectl describe application qalitrack-platform -n argocd

# Common causes:
# 1. Image pull error
# 2. Health check failing
# 3. Resource limits
```

## Best Practices

### 1. Use Separate Branches

- `main` → Production
- `staging` → Staging
- `develop` → Development

### 2. Protect Git Branches

- Require pull requests for `main`
- Enable branch protection
- Require code reviews

### 3. Sync Windows

Limit when syncs can happen:

```yaml
syncWindows:
  - kind: allow
    schedule: '0 9-17 * * MON-FRI'  # Business hours only
    duration: 8h
```

### 4. Ignore HPA Replicas

```yaml
ignoreDifferences:
  - group: apps
    kind: Deployment
    jsonPointers:
      - /spec/replicas
```

### 5. Use App of Apps Pattern

Manage multiple apps with one app:

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: qalitrack-apps
spec:
  source:
    path: kubernetes/argocd/applications
  destination:
    server: https://kubernetes.default.svc
    namespace: argocd
```

## Security

### Change Admin Password

```bash
# Via CLI
argocd account update-password

# Or in UI: User Info → Update Password
```

### Create Users

Edit `argocd-cm`:

```yaml
data:
  accounts.developer: apiKey,login
  accounts.viewer: login
```

Set password:

```bash
argocd account update-password --account developer
```

### RBAC

See `argocd-config.yaml` for RBAC examples.

## Cleanup

```bash
# Delete application
kubectl delete application qalitrack-platform -n argocd

# Uninstall ArgoCD
kubectl delete -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/v2.9.3/manifests/install.yaml

# Delete namespace
kubectl delete namespace argocd
```

## Resources

- [ArgoCD Documentation](https://argo-cd.readthedocs.io/)
- [Best Practices](https://argo-cd.readthedocs.io/en/stable/user-guide/best_practices/)
- [GitOps Principles](https://opengitops.dev/)

---

**Last Updated:** March 2026
**Version:** 1.0.0
**ArgoCD Version:** 2.9.3
