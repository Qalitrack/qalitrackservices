# GitHub Actions Workflows

Automated CI/CD pipelines for building and deploying Qalitrack services.

## Overview

All service workflows follow the same pattern:

1. **Build & Push** - Build Docker images and push to GitHub Container Registry
2. **Deploy (Optional)** - Deploy to Docker Compose or Kubernetes

## Available Workflows

### Service Workflows

| Workflow | Service | Image | Kubernetes Deploy |
|----------|---------|-------|-------------------|
| `build-gateway.yml` | Gateway Service | `ghcr.io/qalitrack/qalitrackservices/gateway-service` | ✅ |
| `build-user-service.yml` | User Service | `ghcr.io/qalitrack/qalitrackservices/user-service` | ✅ |
| `build-masterdata-service.yml` | MasterData Service | `ghcr.io/qalitrack/qalitrackservices/masterdata-service` | ✅ |
| `build-transaction-service.yml` | Transaction Service | `ghcr.io/qalitrack/qalitrackservices/transaction-service` | ✅ |
| `build-backup-service.yml` | Backup Service | `ghcr.io/qalitrack/qalitrackservices/backup-service` | ✅ |

### Reusable Workflows

| Workflow | Purpose |
|----------|---------|
| `deploy-to-kubernetes.yml` | Reusable Kubernetes deployment logic |

## How It Works

### Build Process

When you push code to `main`:

```
Code Push (main branch)
   ↓
GitHub Actions detects changes
   ↓
Runs tests (if configured)
   ↓
Builds Docker image
   ↓
Tags: latest, main-<sha>, sha-<sha>
   ↓
Pushes to ghcr.io
```

### Deployment Process (Optional)

After successful build:

```
Image pushed to registry
   ↓
Check if K8S deploy enabled
   ↓
If enabled:
  - Configure kubectl
  - Restart deployment
  - Verify rollout
  - Show logs
```

## Enabling Kubernetes Deployment

Kubernetes deployment is **disabled by default**. To enable it:

### 1. Add Repository Secrets

Go to: **Settings → Secrets and variables → Actions → New repository secret**

Add:

**`KUBECONFIG`** (Required)
```bash
# On your local machine with kubectl configured:
cat ~/.kube/config | base64 -w 0

# Copy the output and paste as the secret value
```

### 2. Enable Kubernetes Deployment

Go to: **Settings → Secrets and variables → Actions → Variables → New repository variable**

Add:

**`ENABLE_K8S_DEPLOY`**
```
Value: true
```

### 3. Trigger Deployment

```bash
# Option A: Push code changes
git push origin main

# Option B: Manually trigger workflow
# Go to Actions → Select workflow → Run workflow
```

## Workflow Configuration

### Deployment Conditions

Kubernetes deployment only runs when **ALL** of the following are true:

1. Branch is `main`
2. Event is `push` (not pull request)
3. Variable `ENABLE_K8S_DEPLOY` is set to `true`
4. Secret `KUBECONFIG` exists

### Customize Deployment

Edit the workflow file to change deployment settings:

```yaml
deploy-kubernetes:
  uses: ./.github/workflows/deploy-to-kubernetes.yml
  with:
    service-name: gateway-service    # Service name
    namespace: qalitrack-prod        # Kubernetes namespace
    timeout: 5m                      # Rollout timeout
  secrets:
    kubeconfig: ${{ secrets.KUBECONFIG }}
```

## Secrets & Variables

### Required Secrets

| Secret | Description | How to Get |
|--------|-------------|------------|
| `KUBECONFIG` | Base64-encoded kubeconfig | `cat ~/.kube/config \| base64 -w 0` |

### Optional Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ENABLE_K8S_DEPLOY` | Enable Kubernetes deployment | `false` |

## Manual Triggers

All workflows support manual triggering:

1. Go to **Actions** tab
2. Select the workflow
3. Click **Run workflow**
4. Choose branch (usually `main`)
5. Click **Run workflow** button

## Monitoring Workflows

### View Workflow Runs

```bash
# Install GitHub CLI
gh auth login

# List recent runs
gh run list

# Watch a specific run
gh run watch

# View logs
gh run view <run-id> --log
```

### Check Build Status

Badges show build status:

```markdown
![Build Gateway Service](https://github.com/Qalitrack/qalitrackservices/actions/workflows/build-gateway.yml/badge.svg)
```

## Troubleshooting

### Issue: Workflow not triggering

**Cause:** Changes not in monitored paths

**Solution:** Check workflow `paths` filter:

```yaml
on:
  push:
    paths:
      - 'packages/microservices/QalitrackGateWay**'  # Only triggers for these paths
```

### Issue: Docker build fails

**Common causes:**
1. Dockerfile syntax error
2. Missing dependencies
3. Build context incorrect

**Check logs:**
```bash
gh run view <run-id> --log
```

### Issue: Kubernetes deployment fails

**Common causes:**
1. `KUBECONFIG` secret not set or invalid
2. `ENABLE_K8S_DEPLOY` variable not set to `true`
3. Kubernetes cluster unreachable
4. Service not yet deployed in Kubernetes

**Fix:**
1. Verify secrets/variables are set
2. Test kubectl locally:
   ```bash
   echo "$KUBECONFIG_SECRET" | base64 -d > /tmp/kubeconfig
   kubectl --kubeconfig=/tmp/kubeconfig get pods -n qalitrack-prod
   ```

### Issue: Image pull fails in Kubernetes

**Cause:** Image not public or no pull secret configured

**Solution:**

For public images (current setup):
```yaml
# Images are public by default at ghcr.io
# No imagePullSecrets needed
```

For private images:
```bash
# Create pull secret
kubectl create secret docker-registry ghcr-secret \
  --docker-server=ghcr.io \
  --docker-username=<github-username> \
  --docker-password=<github-token> \
  -n qalitrack-prod

# Add to Helm values
imagePullSecrets:
  - name: ghcr-secret
```

## Workflow Structure

### Standard Service Workflow

```yaml
name: Build <Service> Service

on:
  push:
    branches: [ main ]
    paths:
      - 'path/to/service/**'
  workflow_dispatch:

env:
  REGISTRY: ghcr.io
  SERVICE_NAME: service-name
  SERVICE_PATH: path/to/service

jobs:
  # Optional: Run tests
  test:
    runs-on: ubuntu-latest
    steps:
      - # Test steps

  # Required: Build and push image
  build-and-push:
    needs: test  # Only if test job exists
    runs-on: ubuntu-latest
    steps:
      - # Build and push steps

  # Optional: Deploy to Docker Compose
  deploy-compose:
    needs: build-and-push
    if: github.ref == 'refs/heads/main'
    steps:
      - # Docker Compose deployment

  # Optional: Deploy to Kubernetes
  deploy-kubernetes:
    needs: build-and-push
    if: |
      github.ref == 'refs/heads/main' &&
      vars.ENABLE_K8S_DEPLOY == 'true'
    uses: ./.github/workflows/deploy-to-kubernetes.yml
    with:
      service-name: service-name
      namespace: qalitrack-prod
    secrets:
      kubeconfig: ${{ secrets.KUBECONFIG }}
```

## Best Practices

### 1. Test Before Merging

Run tests in pull requests:

```yaml
on:
  pull_request:
    branches: [ main ]
```

### 2. Use Workflow Dispatch

Allow manual triggering:

```yaml
on:
  workflow_dispatch:
```

### 3. Cache Dependencies

Speed up builds with caching:

```yaml
- uses: docker/build-push-action@v5
  with:
    cache-from: type=gha
    cache-to: type=gha,mode=max
```

### 4. Separate Test and Deploy

Only deploy if tests pass:

```yaml
deploy:
  needs: test  # Deploy only after successful tests
```

### 5. Use Environments

For approval-based deployments:

```yaml
deploy-production:
  environment: production  # Requires manual approval
```

## Adding New Service Workflows

1. Copy existing workflow:
   ```bash
   cp build-gateway.yml build-new-service.yml
   ```

2. Update configuration:
   ```yaml
   env:
     SERVICE_NAME: new-service
     SERVICE_PATH: path/to/new-service
   ```

3. Update paths filter:
   ```yaml
   on:
     push:
       paths:
         - 'path/to/new-service/**'
   ```

4. Commit and push:
   ```bash
   git add .github/workflows/build-new-service.yml
   git commit -m "ci: Add workflow for new-service"
   git push origin main
   ```

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Workflow Syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [Docker Build Push Action](https://github.com/docker/build-push-action)
- [kubectl Setup Action](https://github.com/azure/setup-kubectl)

---

**Last Updated:** March 2026
**Version:** 1.0.0
