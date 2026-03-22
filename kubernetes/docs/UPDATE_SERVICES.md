# Updating Services (Gateway, Microservices, etc.)

Complete guide to updating existing services when you make code changes.

## Overview

When you make code changes to any service (Gateway, User Service, etc.), here's the deployment flow:

```
Code Changes → GitHub Push → GitHub Actions Build → Container Image → Kubernetes Update
```

## Quick Update Process

### TL;DR (Too Long; Didn't Read)

```bash
# 1. Make your code changes
cd packages/qalitrack-gateway/src
# ... edit files ...

# 2. Commit and push
git add .
git commit -m "Update gateway authentication logic"
git push origin main

# 3. GitHub Actions automatically builds new image

# 4. Update Kubernetes (choose one):

# Option A: ArgoCD auto-sync (recommended - automatic)
# Nothing needed, ArgoCD detects and deploys automatically

# Option B: Manual restart (forces pull of latest image)
kubectl rollout restart deployment/gateway-service -n qalitrack-prod

# Option C: Helm upgrade (if you changed Helm values)
cd kubernetes/helm-charts/qalitrack-platform
helm upgrade qalitrack-platform . -n qalitrack-prod
```

---

## Detailed Update Process

### Step 1: Make Code Changes

```bash
# Navigate to your service
cd packages/qalitrack-gateway/src
# or
cd packages/microservices/masterdata/user-service/src/UserService.Api

# Make your changes
vim Program.cs
# ... edit files ...
```

### Step 2: Test Locally (Optional but Recommended)

```bash
# Run locally
dotnet run

# Or with Docker
docker build -t gateway-service:test .
docker run -p 7000:7000 gateway-service:test
```

### Step 3: Commit and Push

```bash
# Add changes
git add .

# Commit with descriptive message
git commit -m "fix(gateway): Update JWT validation logic"

# Push to GitHub
git push origin main
```

### Step 4: GitHub Actions Builds New Image

GitHub Actions automatically:
1. Detects changes in the service path
2. Builds new Docker image
3. Tags it with:
   - `latest` (always latest build)
   - `main-<commit-sha>` (specific version)
   - `sha-<commit-sha>` (git commit)
4. Pushes to `ghcr.io/qalitrack/qalitrackservices/<service-name>`

**Monitor the build:**

```bash
# Via GitHub UI
# Go to: https://github.com/Qalitrack/qalitrackservices/actions

# Or via CLI (if you have gh CLI)
gh run list --workflow="Build Gateway Service"
gh run watch
```

### Step 5: Update Kubernetes

You have three options:

---

## Option A: ArgoCD Auto-Sync (Recommended)

**If you're using ArgoCD**, it automatically detects changes and deploys.

### How It Works

1. ArgoCD polls your Git repo every 3 minutes
2. Detects changes to Helm charts or values
3. Automatically syncs and deploys new image

### Check Sync Status

```bash
# View ArgoCD application
kubectl get application qalitrack-platform -n argocd

# Check sync status
argocd app get qalitrack-platform

# Watch sync progress
argocd app sync qalitrack-platform --watch
```

### Force Sync

If ArgoCD doesn't pick up changes immediately:

```bash
# Force sync
argocd app sync qalitrack-platform

# Or via kubectl
kubectl patch application qalitrack-platform -n argocd \
  --type merge \
  -p '{"operation":{"initiatedBy":{"username":"manual"},"sync":{}}}'
```

---

## Option B: Manual Rollout Restart (Quick)

**Fastest way** to pull the latest image without changing Helm values.

```bash
# Restart specific service
kubectl rollout restart deployment/gateway-service -n qalitrack-prod

# Watch rollout progress
kubectl rollout status deployment/gateway-service -n qalitrack-prod

# Or restart all services
kubectl rollout restart deployment -n qalitrack-prod --selector=app.kubernetes.io/part-of=qalitrack
```

### What This Does

1. Kubernetes creates new pods
2. New pods pull `latest` image from registry
3. Waits for new pods to be healthy
4. Terminates old pods
5. **Zero downtime** (rolling update)

### Use When

- You pushed code changes
- Image tag is `latest`
- No Helm configuration changes
- Want quick deployment

---

## Option C: Helm Upgrade (For Config Changes)

**Use when** you changed Helm values, added environment variables, or changed resources.

```bash
cd kubernetes/helm-charts/qalitrack-platform

# Update Helm dependencies (if needed)
helm dependency update

# Upgrade platform
helm upgrade qalitrack-platform . \
  --namespace qalitrack-prod \
  --install \
  --wait \
  --timeout 10m

# Or upgrade specific service
cd kubernetes/helm-charts/gateway-service
helm upgrade gateway-service . \
  --namespace qalitrack-prod \
  --wait
```

### Use When

- Changed `values.yaml`
- Added new environment variables
- Changed resource limits
- Modified service configuration
- Added new volumes/mounts

---

## Option D: Update Image Tag (Version Pinning)

**Use when** you want to deploy a specific version instead of `latest`.

### Find Available Tags

```bash
# List available image tags
gh api /orgs/qalitrack/packages/container/qalitrackservices%2Fgateway-service/versions

# Or check GitHub Container Registry
# https://github.com/orgs/Qalitrack/packages
```

### Update to Specific Tag

Edit `kubernetes/helm-charts/qalitrack-platform/values.yaml`:

```yaml
gateway:
  image:
    tag: "sha-abc1234"  # Change from "latest"
```

Then upgrade:

```bash
helm upgrade qalitrack-platform . -n qalitrack-prod
```

---

## Flagger Canary Deployment (Automatic Rollback)

**If you have Flagger enabled**, updates happen gradually with automatic rollback on failure.

### How It Works

```
Deploy new version
   ↓
Canary: 0% → 10% traffic
   ↓ (check metrics: success rate, latency)
   ✓ Metrics OK
   ↓
Canary: 10% → 20% traffic
   ↓ (check metrics again)
   ✓ Metrics OK
   ↓
Continue to 50%
   ↓
   ✓ All checks pass
   ↓
Promote to 100% (new version becomes primary)
```

If metrics fail at any step → **Automatic rollback**

### Watch Canary Progress

```bash
# Watch canary status
kubectl get canary gateway-service -n qalitrack-prod -w

# Check canary events
kubectl describe canary gateway-service -n qalitrack-prod

# View Flagger logs
kubectl logs -n flagger-system deployment/flagger -f
```

### Trigger Canary Deployment

Just do a normal update (Option A, B, or C above). Flagger intercepts and manages the rollout.

---

## Verification After Update

Always verify after deploying:

### 1. Check Pod Status

```bash
# Check if new pods are running
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=gateway-service

# Should see new pods in Running state
# Old pods should be Terminating or gone
```

### 2. Check Rollout Status

```bash
kubectl rollout status deployment/gateway-service -n qalitrack-prod

# Should show: "deployment "gateway-service" successfully rolled out"
```

### 3. Check Logs

```bash
# View logs of new pods
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service --tail=100

# Follow logs in real-time
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service -f

# Check for errors
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service | grep -i error
```

### 4. Test Service

```bash
# Port-forward to service
kubectl port-forward -n qalitrack-prod svc/gateway-service 7000:7000

# Test health endpoint
curl http://localhost:7000/health

# Test your changes
curl http://localhost:7000/api/your-endpoint
```

### 5. Check Metrics (Grafana)

```bash
# Port-forward to Grafana
kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80

# Open: http://localhost:3000
# Check:
# - Request rate (should be normal)
# - Error rate (should be low)
# - Latency (should be similar to before)
```

---

## Common Update Scenarios

### Scenario 1: Fix a Bug in Gateway

```bash
# 1. Fix bug in code
vim packages/qalitrack-gateway/src/Middleware/AuthMiddleware.cs

# 2. Commit and push
git add .
git commit -m "fix(gateway): Fix JWT expiration validation"
git push origin main

# 3. Wait for GitHub Actions to build (2-5 minutes)

# 4. Restart deployment
kubectl rollout restart deployment/gateway-service -n qalitrack-prod

# 5. Verify
kubectl rollout status deployment/gateway-service -n qalitrack-prod
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service --tail=50
```

### Scenario 2: Add New Feature to User Service

```bash
# 1. Add feature
vim packages/microservices/masterdata/user-service/src/UserService.Api/Controllers/UserController.cs

# 2. Commit and push
git add .
git commit -m "feat(user-service): Add user profile photo upload"
git push origin main

# 3. Wait for build

# 4. Update (with Flagger canary if enabled)
kubectl rollout restart deployment/user-service -n qalitrack-prod

# 5. Watch canary progress (if Flagger enabled)
kubectl get canary user-service -n qalitrack-prod -w

# 6. Test new endpoint
kubectl port-forward svc/user-service 7001:7001
curl -X POST http://localhost:7001/api/users/profile/photo -F "file=@photo.jpg"
```

### Scenario 3: Update Environment Variables

```bash
# 1. Edit Helm values
vim kubernetes/helm-charts/qalitrack-platform/values.yaml

# Change:
gateway:
  env:
    NEW_FEATURE_ENABLED: "true"  # Add new env var

# 2. Upgrade via Helm
cd kubernetes/helm-charts/qalitrack-platform
helm upgrade qalitrack-platform . -n qalitrack-prod

# 3. Verify new env var
kubectl get pod -n qalitrack-prod -l app.kubernetes.io/name=gateway-service -o yaml | grep NEW_FEATURE_ENABLED
```

### Scenario 4: Increase Resources (Memory/CPU)

```bash
# 1. Edit Helm values
vim kubernetes/helm-charts/qalitrack-platform/values.yaml

# Change:
gateway:
  resources:
    limits:
      memory: "512Mi"  # Increase from 256Mi

# 2. Upgrade
helm upgrade qalitrack-platform . -n qalitrack-prod

# 3. Verify
kubectl describe pod -n qalitrack-prod -l app.kubernetes.io/name=gateway-service | grep -A5 Limits
```

---

## Rollback if Something Goes Wrong

### Quick Rollback

```bash
# Rollback to previous version
kubectl rollout undo deployment/gateway-service -n qalitrack-prod

# Rollback to specific revision
kubectl rollout history deployment/gateway-service -n qalitrack-prod
kubectl rollout undo deployment/gateway-service -n qalitrack-prod --to-revision=3
```

### Helm Rollback

```bash
# List releases
helm history qalitrack-platform -n qalitrack-prod

# Rollback to previous
helm rollback qalitrack-platform -n qalitrack-prod

# Rollback to specific revision
helm rollback qalitrack-platform 3 -n qalitrack-prod
```

### Flagger Automatic Rollback

If using Flagger, rollback happens automatically when:
- Error rate > 1%
- p99 latency > 500ms
- Health checks fail

Check Flagger logs:
```bash
kubectl logs -n flagger-system deployment/flagger | grep gateway-service
```

---

## CI/CD Pipeline Overview

Your current setup:

```
┌─────────────────────────────────────────────────────────┐
│                    Your Workflow                         │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  1. Code Changes                                         │
│     └─ Edit files locally                                │
│                                                          │
│  2. Git Push                                             │
│     └─ git push origin main                              │
│                                                          │
│  3. GitHub Actions (Automatic)                           │
│     ├─ Detects changes                                   │
│     ├─ Runs tests (if configured)                        │
│     ├─ Builds Docker image                               │
│     ├─ Tags: latest, sha-xxx                             │
│     └─ Pushes to ghcr.io                                 │
│                                                          │
│  4. Deployment (Choose one):                             │
│     │                                                    │
│     ├─ A. ArgoCD Auto-Sync                              │
│     │   └─ Automatic deployment (recommended)           │
│     │                                                    │
│     ├─ B. Manual Restart                                │
│     │   └─ kubectl rollout restart                      │
│     │                                                    │
│     └─ C. Helm Upgrade                                  │
│         └─ helm upgrade                                  │
│                                                          │
│  5. Flagger (Optional)                                   │
│     ├─ Gradual traffic shift                            │
│     ├─ Metric monitoring                                │
│     └─ Auto-rollback if issues                          │
│                                                          │
│  6. Monitoring                                           │
│     ├─ Prometheus collects metrics                      │
│     ├─ Grafana displays dashboards                      │
│     └─ Alerts on issues                                 │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## Best Practices

### 1. Commit Messages

Use conventional commits:

```bash
# Format: type(scope): description

# Examples:
git commit -m "feat(gateway): Add rate limiting"
git commit -m "fix(user-service): Fix password reset bug"
git commit -m "refactor(transaction): Optimize query performance"
git commit -m "docs(readme): Update deployment instructions"
```

Types:
- `feat` - New feature
- `fix` - Bug fix
- `refactor` - Code refactoring
- `perf` - Performance improvement
- `test` - Adding tests
- `docs` - Documentation
- `chore` - Maintenance

### 2. Test Before Pushing

```bash
# Run tests locally
dotnet test

# Build Docker image locally
docker build -t gateway-service:test .

# Run locally to verify
docker run -p 7000:7000 gateway-service:test
```

### 3. Small, Incremental Changes

- Push small changes frequently
- Easier to debug if something breaks
- Faster rollbacks if needed

### 4. Monitor After Deployment

- Check Grafana dashboards
- Watch error rates
- Monitor response times
- Check logs for warnings

### 5. Use Feature Flags

For risky changes:

```csharp
// In appsettings.json
{
  "FeatureFlags": {
    "NewFeatureEnabled": false
  }
}

// In code
if (_configuration.GetValue<bool>("FeatureFlags:NewFeatureEnabled"))
{
    // New feature code
}
```

Deploy with feature disabled, then enable via config change.

---

## Troubleshooting

### Issue: New pods not pulling latest image

```bash
# Check image pull policy
kubectl get deployment gateway-service -n qalitrack-prod -o yaml | grep imagePullPolicy

# Should be "Always" to always pull latest

# Force recreate pods
kubectl delete pods -n qalitrack-prod -l app.kubernetes.io/name=gateway-service
```

### Issue: Pods crashing after update

```bash
# Check pod logs
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service --previous

# Check events
kubectl get events -n qalitrack-prod --sort-by='.lastTimestamp' | grep gateway

# Rollback immediately
kubectl rollout undo deployment/gateway-service -n qalitrack-prod
```

### Issue: Service not responding

```bash
# Check service endpoints
kubectl get endpoints gateway-service -n qalitrack-prod

# Check pod status
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=gateway-service

# Check pod details
kubectl describe pod <pod-name> -n qalitrack-prod
```

### Issue: GitHub Actions build failed

```bash
# Check workflow status
gh run list --workflow="Build Gateway Service"

# View failure details
gh run view <run-id>

# Common causes:
# - Dockerfile syntax error
# - Build errors in code
# - Missing dependencies
# - Registry authentication issues
```

---

## Quick Reference Commands

```bash
# === Viewing Status ===
kubectl get pods -n qalitrack-prod
kubectl get deployments -n qalitrack-prod
kubectl rollout status deployment/<service> -n qalitrack-prod

# === Updating Services ===
kubectl rollout restart deployment/<service> -n qalitrack-prod
helm upgrade qalitrack-platform . -n qalitrack-prod

# === Viewing Logs ===
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=<service>
kubectl logs -n qalitrack-prod <pod-name> -f

# === Rollback ===
kubectl rollout undo deployment/<service> -n qalitrack-prod
helm rollback qalitrack-platform -n qalitrack-prod

# === ArgoCD ===
argocd app sync qalitrack-platform
argocd app get qalitrack-platform

# === Flagger ===
kubectl get canary -n qalitrack-prod
kubectl describe canary <service> -n qalitrack-prod
```

---

**Last Updated:** March 2026
**Version:** 1.0.0
