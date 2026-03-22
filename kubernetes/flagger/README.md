# Flagger - Progressive Delivery & Automatic Rollbacks

Automated canary deployments and automatic rollbacks for Qalitrack using Flagger.

## What is Flagger?

Flagger automates the release process for applications running on Kubernetes by:
- **Progressive traffic shifting** - Gradually increase traffic to new version
- **Automated rollbacks** - Roll back if metrics fail
- **Canary analysis** - Monitor success rate, latency, custom metrics
- **Load testing** - Automatically test new version
- **Notifications** - Alert on rollback or promotion

## Why Use Flagger?

**Without Flagger:**
```
Deploy new version → Hope it works → Manual rollback if it breaks
```

**With Flagger:**
```
Deploy new version → Flagger tests gradually → Auto-rollback if metrics fail → Auto-promote if successful
```

**Benefits:**
- Catch bugs before full deployment
- Zero-downtime deployments
- Automatic rollback on failure
- Confidence in deployments

## Architecture

```
New Deployment Triggered
   ↓
Flagger Creates Canary
   ↓
0% → 10% → 20% → 30% → 40% → 50% traffic to canary
│     │      │      │      │      │
└─────┴──────┴──────┴──────┴──────┴─→ Check metrics at each step
                                      ↓
                             Metrics OK? ──Yes──→ Promote to 100%
                                      │
                                     No
                                      ↓
                               Automatic Rollback
```

## Installation

### Prerequisites

- Prometheus installed (for metrics)
- Your services must expose `/metrics` endpoint

### Quick Install

```bash
cd kubernetes/flagger
./install.sh
```

### Manual Installation

```bash
# Add Helm repo
helm repo add flagger https://flagger.app
helm repo update

# Install Flagger
helm install flagger flagger/flagger \
  --namespace flagger-system \
  --create-namespace \
  --set meshProvider=kubernetes \
  --set metricsServer=http://prometheus-kube-prometheus-prometheus.qalitrack-monitoring:9090

# Install Loadtester
helm install flagger-loadtester flagger/loadtester \
  --namespace flagger-system
```

## Setup Canary Deployments

### Apply Canary Configurations

```bash
# All services at once
kubectl apply -f all-services-canary.yaml

# Or individual services
kubectl apply -f gateway-canary.yaml
```

### Verify Setup

```bash
# Check canary resources
kubectl get canary -n qalitrack-prod

# Should show:
# NAME                   STATUS      WEIGHT
# gateway-service        Initialized 0
# user-service          Initialized 0
# masterdata-service    Initialized 0
# transaction-service   Initialized 0
# backup-service        Initialized 0
```

## How It Works

### 1. Initial State

```
Primary (100% traffic) ← All traffic
Canary (0% traffic)    ← No traffic
```

### 2. Trigger Deployment

Update the deployment image:

```bash
kubectl set image deployment/gateway-service \
  gateway-service=ghcr.io/qalitrack/qalitrackservices/gateway-service:v2.0.0 \
  -n qalitrack-prod
```

### 3. Flagger Takes Over

```
Minute 0:  Primary (90%) ← Canary (10%)  ← Check metrics
Minute 1:  Primary (80%) ← Canary (20%)  ← Check metrics
Minute 2:  Primary (70%) ← Canary (30%)  ← Check metrics
Minute 3:  Primary (60%) ← Canary (40%)  ← Check metrics
Minute 4:  Primary (50%) ← Canary (50%)  ← Check metrics
```

### 4a. Success Path

If all metrics pass:
```
Minute 5: Canary promoted to primary (100%)
Old version terminated
```

### 4b. Failure Path

If metrics fail at any step:
```
Immediate rollback to primary (100%)
Canary scaled to zero
Deployment marked as failed
Alert sent (if configured)
```

## Metrics Monitored

### Request Success Rate

```yaml
- name: request-success-rate
  thresholdRange:
    min: 99  # Must maintain 99% success rate
  interval: 1m
```

**What it checks:** HTTP 2xx,3xx vs 4xx,5xx responses

**Fails if:** Error rate > 1%

### Request Duration

```yaml
- name: request-duration
  thresholdRange:
    max: 500  # p99 latency must be < 500ms
  interval: 1m
```

**What it checks:** 99th percentile response time

**Fails if:** p99 latency > 500ms

### Custom Metrics

Add your own:

```yaml
- name: cpu-usage
  thresholdRange:
    max: 80  # CPU must be < 80%
  interval: 1m
  query: |
    sum(rate(container_cpu_usage_seconds_total{pod=~"gateway-service-.*"}[1m])) /
    sum(container_spec_cpu_quota{pod=~"gateway-service-.*"}) * 100
```

## Canary Deployment Example

### Step 1: Current State

```bash
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=gateway-service

# gateway-service-primary-xxxxx    1/1   Running   # v1.0.0
# gateway-service-primary-yyyyy    1/1   Running   # v1.0.0
```

### Step 2: Deploy New Version

```bash
kubectl set image deployment/gateway-service \
  gateway-service=ghcr.io/qalitrack/qalitrackservices/gateway-service:v2.0.0 \
  -n qalitrack-prod
```

### Step 3: Watch Canary Progress

```bash
watch kubectl get canary gateway-service -n qalitrack-prod

# STATUS        WEIGHT   LASTTRANSITIONTIME
# Progressing   10       2026-03-22T10:01:00Z
# Progressing   20       2026-03-22T10:02:00Z
# Progressing   30       2026-03-22T10:03:00Z
# Progressing   40       2026-03-22T10:04:00Z
# Progressing   50       2026-03-22T10:05:00Z
# Promoting     0        2026-03-22T10:06:00Z
# Succeeded     0        2026-03-22T10:07:00Z
```

### Step 4: Verify New Version

```bash
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=gateway-service

# gateway-service-primary-zzzzz    1/1   Running   # v2.0.0
# gateway-service-primary-aaaaa    1/1   Running   # v2.0.0
```

## Automatic Rollback Example

### Scenario: New Version Has Bugs

```bash
# Deploy buggy version
kubectl set image deployment/user-service \
  user-service=ghcr.io/qalitrack/qalitrackservices/user-service:v2.0.0-buggy \
  -n qalitrack-prod

# Watch canary
kubectl get canary user-service -n qalitrack-prod -w

# STATUS        WEIGHT   LASTTRANSITIONTIME
# Progressing   10       2026-03-22T10:01:00Z  ← 10% traffic to buggy version
# Progressing   10       2026-03-22T10:02:00Z  ← Metrics failing, staying at 10%
# Progressing   10       2026-03-22T10:03:00Z  ← Still failing
# Progressing   10       2026-03-22T10:04:00Z  ← Still failing
# Progressing   10       2026-03-22T10:05:00Z  ← 5 failures reached threshold
# Failed        0        2026-03-22T10:06:00Z  ← AUTOMATIC ROLLBACK!
```

**Result:**
- Canary terminated
- Primary (v1.0.0) still serving 100% traffic
- Only 10% of users saw the bug
- No manual intervention needed

## Load Testing

Flagger can automatically test new version:

```yaml
webhooks:
  - name: load-test
    url: http://flagger-loadtester.flagger-system/
    timeout: 5s
    metadata:
      type: cmd
      cmd: "hey -z 1m -q 10 -c 2 http://gateway-service-canary:7000/health"
```

**What happens:**
- Flagger triggers load test against canary
- Runs for 1 minute, 10 req/s, 2 connections
- Metrics collected during test
- Canary promoted only if test passes

## Notifications

### Slack Alerts

```bash
# Create Slack webhook secret
kubectl create secret generic slack-webhook \
  --from-literal=address=https://hooks.slack.com/services/YOUR/WEBHOOK/URL \
  -n qalitrack-prod

# Apply alert provider
kubectl apply -f gateway-canary.yaml
```

**You'll receive Slack messages for:**
- Canary started
- Canary progressing
- Canary promoted
- Canary failed (rollback)

### Custom Webhooks

```yaml
webhooks:
  - name: send-notification
    type: post-rollout
    url: http://your-webhook-service/notify
    metadata:
      message: "Deployment successful"
```

## Advanced Configurations

### Blue-Green Deployment

Instead of gradual shift, switch all at once:

```yaml
analysis:
  maxWeight: 100
  stepWeight: 100  # Jump straight to 100%
```

### A/B Testing

Route based on headers:

```yaml
analysis:
  match:
    - headers:
        user-agent:
          regex: ".*Chrome.*"
```

### Mirroring

Send traffic to canary but don't return response to user:

```yaml
analysis:
  mirror: true
  maxWeight: 100
```

## Monitoring Flagger

### Check Canary Status

```bash
# List all canaries
kubectl get canary -A

# Describe specific canary
kubectl describe canary gateway-service -n qalitrack-prod

# Watch events
kubectl get events -n qalitrack-prod --field-selector involvedObject.kind=Canary
```

### Flagger Logs

```bash
kubectl logs -n flagger-system deployment/flagger -f
```

### Prometheus Metrics

Flagger exports metrics:

```
flagger_canary_total
flagger_canary_status
flagger_canary_weight
flagger_canary_duration_seconds
```

### Grafana Dashboard

Import dashboard ID `16738` for Flagger metrics.

## Troubleshooting

### Canary Stuck

```bash
# Check canary status
kubectl describe canary gateway-service -n qalitrack-prod

# Common causes:
# 1. Metrics not available - check Prometheus
# 2. Pod not ready - check pod status
# 3. Analysis paused - check canary spec
```

### Metrics Not Working

```bash
# Test Prometheus query
kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090

# Open http://localhost:9090
# Run query: rate(http_request_duration_seconds_count[1m])

# If empty:
# 1. Service not exposing /metrics
# 2. ServiceMonitor not created
# 3. Prometheus not scraping
```

### Rollback Not Happening

```bash
# Check threshold setting
kubectl get canary gateway-service -n qalitrack-prod -o yaml | grep threshold

# Increase sensitivity by lowering threshold
kubectl patch canary gateway-service -n qalitrack-prod --type merge -p '{"spec":{"analysis":{"threshold":3}}}'
```

## Best Practices

### 1. Start Conservative

```yaml
analysis:
  interval: 2m      # Longer intervals
  threshold: 3      # Fewer failures allowed
  stepWeight: 5     # Smaller steps
```

### 2. Monitor Dashboards

Always watch during first canary:
- Grafana dashboards
- Flagger logs
- Application logs

### 3. Test in Staging First

```bash
# Apply canaries to staging
kubectl apply -f all-services-canary.yaml -n qalitrack-staging
```

### 4. Gradual Rollout

Don't enable all services at once:
1. Start with one service (gateway)
2. Run for a week
3. Add more services gradually

### 5. Set Realistic Thresholds

```yaml
# Too strict (will fail often)
min: 99.9

# Realistic for most apps
min: 99

# For critical services
min: 99.5
```

## Cleanup

```bash
# Delete canaries
kubectl delete canary --all -n qalitrack-prod

# Uninstall Flagger
helm uninstall flagger -n flagger-system
helm uninstall flagger-loadtester -n flagger-system

# Delete namespace
kubectl delete namespace flagger-system
```

## Resources

- [Flagger Documentation](https://docs.flagger.app/)
- [Progressive Delivery](https://flagger.app/tutorials/progressive-delivery/)
- [Canary Deployments](https://martinfowler.com/bliki/CanaryRelease.html)

---

**Last Updated:** March 2026
**Version:** 1.0.0
**Flagger Version:** Latest
