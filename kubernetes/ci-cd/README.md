# CI/CD Auto-Deployment for Qalitrack

Automatic deployment to Kubernetes after Docker images are built by GitHub Actions.

## Overview

```
Code Change → GitHub Actions Build → Docker Image → Auto-Deploy to K8s
```

When you merge code to `main`:
1. GitHub Actions build new Docker images
2. Images pushed to ghcr.io/qalitrack/qalitrackservices
3. Kubernetes automatically pulls new images and restarts pods

## Setup Options

### Option 1: GitHub Actions Auto-Deployment (Recommended)

Automatically deploy after every successful image build.

#### Step 1: Create kubeconfig Secret

```bash
# Generate base64 encoded kubeconfig
kubectl config view --raw --minify | base64

# Copy the output and add as GitHub Secret named KUBE_CONFIG
```

#### Step 2: Add GitHub Secret

1. Go to your repository settings
2. Navigate to: Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Name: `KUBE_CONFIG`
5. Value: Paste the base64 encoded kubeconfig
6. Click "Add secret"

#### Step 3: Add Workflow

**For MAIN repository only** (not forks):

```bash
# Copy the template to GitHub Actions workflows
cp kubernetes/ci-cd/deploy-workflow-template.yml .github/workflows/deploy-to-kubernetes.yml

# Commit and push
git add .github/workflows/deploy-to-kubernetes.yml
git commit -m "Add auto-deployment workflow"
git push
```

#### How It Works

```
PR Merged → Build Workflow Runs → On Success → Deploy Workflow Triggers
                                                      ↓
                                                kubectl rollout restart
                                                      ↓
                                                Pods restart with new image
```

The workflow:
1. Waits for build workflows to complete successfully
2. Determines which service was built
3. Triggers `kubectl rollout restart` for that service
4. Waits for rollout to complete
5. Verifies deployment success

### Option 2: Manual Deployment Script

Manually trigger deployment updates when needed.

#### Usage

```bash
cd kubernetes/ci-cd

# Restart a single service
./rollout-restart.sh gateway

# Restart all services
./rollout-restart.sh all

# Restart and wait for completion
./rollout-restart.sh --wait user

# Restart in different namespace
./rollout-restart.sh -n qalitrack-staging transaction
```

#### Services

- `gateway` - Gateway Service
- `user` - User Service
- `masterdata` - MasterData Service
- `transaction` - Transaction Service
- `backup` - Backup Service
- `all` - All services

### Option 3: Image Updater with ArgoCD/Flux

For advanced GitOps workflows, use image automation controllers.

#### With Flux Image Automation

```bash
# Install Flux
flux install

# Create ImageRepository
flux create image repository gateway-service \
  --image=ghcr.io/qalitrack/qalitrackservices/gateway-service \
  --interval=1m

# Create ImagePolicy (watch for new tags)
flux create image policy gateway-service \
  --image-ref=gateway-service \
  --select-semver=">=1.0.0"

# Auto-update values file
flux create image update qalitrack-platform \
  --git-repo-ref=qalitrack-repo \
  --git-repo-path="./kubernetes/helm-charts/qalitrack-platform" \
  --checkout-branch=main \
  --push-branch=main \
  --author-name=flux \
  --author-email=flux@qalibrated.co.ke \
  --commit-template="Update image to {{range .Updated.Images}}{{println .}}{{end}}"
```

## How Rollout Restart Works

```bash
kubectl rollout restart deployment/gateway-service -n qalitrack-prod
```

This command:
1. Adds an annotation to the deployment's pod template
2. Kubernetes sees the change and starts a rolling update
3. New pods are created with updated image pull
4. Old pods are terminated after new ones are ready
5. Zero downtime (if configured properly)

**Key points:**
- Uses `imagePullPolicy: Always` (already configured in Helm charts)
- Pulls latest image tag from registry
- Rolling update ensures no downtime
- Health checks prevent unhealthy pods from receiving traffic

## Verification

### Check Rollout Status

```bash
# View rollout status
kubectl rollout status deployment/gateway-service -n qalitrack-prod

# View rollout history
kubectl rollout history deployment/gateway-service -n qalitrack-prod
```

### Check Image Version

```bash
# Check which image version is running
kubectl get pods -n qalitrack-prod \
  -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.spec.containers[*].image}{"\n"}{end}'
```

### Check Pod Events

```bash
# See recent events
kubectl get events -n qalitrack-prod --field-selector involvedObject.kind=Pod --sort-by='.lastTimestamp'
```

## Rollback

If a deployment fails, rollback to previous version:

```bash
# Rollback to previous version
kubectl rollout undo deployment/gateway-service -n qalitrack-prod

# Rollback to specific revision
kubectl rollout undo deployment/gateway-service -n qalitrack-prod --to-revision=3

# Check rollout history to see revisions
kubectl rollout history deployment/gateway-service -n qalitrack-prod
```

## Troubleshooting

### Deployment Stuck

```bash
# Check deployment status
kubectl get deployment gateway-service -n qalitrack-prod

# Check pod status
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=gateway-service

# Check pod logs
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service --tail=50

# Describe pod for events
kubectl describe pod <pod-name> -n qalitrack-prod
```

### Image Pull Errors

```bash
# Check if image exists in registry
docker pull ghcr.io/qalitrack/qalitrackservices/gateway-service:latest

# Check imagePullSecrets (if using private registry)
kubectl get secrets -n qalitrack-prod

# Check pod events
kubectl describe pod <pod-name> -n qalitrack-prod | grep -A 10 Events
```

### Health Check Failures

```bash
# Check liveness/readiness probe status
kubectl describe pod <pod-name> -n qalitrack-prod | grep -A 5 "Liveness\|Readiness"

# Check pod logs during startup
kubectl logs -n qalitrack-prod <pod-name> --previous
```

## Best Practices

### 1. Tag Docker Images with Versions

Instead of always using `latest`, use version tags:

```yaml
image:
  tag: "v1.2.3"  # Specific version
```

Benefits:
- Easier rollbacks
- Know exactly which version is running
- Better audit trail

### 2. Use Blue-Green Deployments

For critical updates:

```bash
# Create new deployment with different name
kubectl create -f gateway-service-v2.yaml

# Test new version
kubectl port-forward deployment/gateway-service-v2 8080:7000

# Switch traffic (update Service selector)
kubectl patch service gateway-service -p '{"spec":{"selector":{"version":"v2"}}}'

# Delete old deployment
kubectl delete deployment gateway-service-v1
```

### 3. Canary Deployments

Gradually roll out to subset of users:

```yaml
apiVersion: flagger.app/v1beta1
kind: Canary
metadata:
  name: gateway-service
spec:
  targetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: gateway-service
  service:
    port: 7000
  analysis:
    interval: 1m
    threshold: 5
    maxWeight: 50
    stepWeight: 10
    metrics:
    - name: request-success-rate
      thresholdRange:
        min: 99
```

### 4. Automated Testing Before Deploy

Add testing steps to workflow:

```yaml
- name: Run integration tests
  run: |
    kubectl port-forward svc/gateway-service 7000:7000 &
    sleep 5
    curl -f http://localhost:7000/health || exit 1
```

## Security

### Kubeconfig Security

- **DO NOT** commit kubeconfig to Git
- Store as encrypted GitHub Secret
- Use service account with minimal permissions
- Rotate credentials regularly

### Create Limited Service Account

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: github-actions-deployer
  namespace: qalitrack-prod
---
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: deployment-updater
  namespace: qalitrack-prod
rules:
- apiGroups: ["apps"]
  resources: ["deployments"]
  verbs: ["get", "list", "patch"]
- apiGroups: [""]
  resources: ["pods"]
  verbs: ["get", "list"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: github-actions-deployment
  namespace: qalitrack-prod
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: Role
  name: deployment-updater
subjects:
- kind: ServiceAccount
  name: github-actions-deployer
  namespace: qalitrack-prod
```

## Monitoring Deployments

### Prometheus Alerts

```yaml
apiVersion: monitoring.coreos.com/v1
kind: PrometheusRule
metadata:
  name: deployment-alerts
  namespace: qalitrack-monitoring
spec:
  groups:
  - name: deployments
    rules:
    - alert: DeploymentReplicasMismatch
      expr: |
        kube_deployment_status_replicas != kube_deployment_spec_replicas
      for: 10m
      labels:
        severity: warning
      annotations:
        summary: "Deployment replicas mismatch"

    - alert: PodCrashLooping
      expr: |
        rate(kube_pod_container_status_restarts_total[15m]) > 0
      for: 5m
      labels:
        severity: critical
      annotations:
        summary: "Pod is crash looping"
```

### Slack Notifications

Add to GitHub Actions workflow:

```yaml
- name: Notify Slack on success
  if: success()
  uses: slackapi/slack-github-action@v1
  with:
    webhook-url: ${{ secrets.SLACK_WEBHOOK }}
    payload: |
      {
        "text": "✅ Deployed ${{ steps.service.outputs.service }} to production"
      }
```

## Resources

- [Kubernetes Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [kubectl rollout](https://kubernetes.io/docs/reference/generated/kubectl/kubectl-commands#rollout)
- [GitHub Actions Workflows](https://docs.github.com/en/actions/using-workflows)
- [GitOps with Flux](https://fluxcd.io/docs/)

---

**Last Updated:** March 2026
**Version:** 1.0.0
