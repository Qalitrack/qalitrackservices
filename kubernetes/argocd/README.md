# ArgoCD GitOps for QaliTrack

Continuous delivery and GitOps automation for QaliTrack using ArgoCD.

## How it Works

```
Developer pushes code
       ↓
GitHub Actions builds Docker image → ghcr.io
       ↓
CI commits new image tag to values.yaml [skip ci]
       ↓
ArgoCD detects git change (180s poll)
       ↓
ArgoCD syncs cluster automatically
       ↓
Email notification → joshuaiska@gmail.com
```

Self-heal is on: any manual `kubectl` changes are automatically reverted to match Git.

## Quick Install (Fresh Server)

```bash
cd kubernetes/argocd
bash install.sh --github-token $(gh auth token)
```

That single command:
1. Creates the `argocd` namespace
2. Installs ArgoCD v2.9.3
3. Applies config (admin password, RBAC, server settings)
4. Sets up email notifications (Gmail SMTP)
5. Creates the Traefik ingress for HTTPS
6. Registers the GitHub repo secret
7. Restarts ArgoCD components
8. Deploys the `qalitrack` Application

## Access

| Item | Value |
|------|-------|
| URL | https://argocd.qalibrated.co.ke |
| Username | `admin` |
| Password | `Qalitrack@2024!` |

The admin password is permanent — survives restarts, reinstalls, and redeployments.

## Files

| File | Purpose |
|------|---------|
| `install.sh` | One-command full setup |
| `argocd-config.yaml` | ConfigMaps: settings, RBAC, cmd params + admin Secret |
| `argocd-notifications.yaml` | Email notifications (Secret + ConfigMap) |
| `argocd-ingress.yaml` | Traefik ingress with Let's Encrypt TLS |
| `qalitrack-application.yaml` | ArgoCD Application + AppProject |

## Sync Settings

The `qalitrack` Application is configured with:
- **Auto-sync**: on — ArgoCD syncs when git changes
- **Self-heal**: on — reverts manual cluster changes
- **Prune**: on — removes resources deleted from git
- **ServerSideApply**: on — required for large CRDs (Grafana, Prometheus)
- **Retry**: 5 attempts with exponential backoff (5s → 3m)

## Email Notifications

Notifications go to `joshuaiska@gmail.com` via Gmail SMTP for:
- **Deploy success** — app synced and healthy
- **Health degraded** — pods crash-looping or failing
- **Sync failed** — ArgoCD couldn't apply changes

SMTP credentials are in `argocd-notifications.yaml` (app password, not the account password).

## GitHub Repo Access

The cluster needs a secret to pull from the private GitHub repo:

```bash
kubectl create secret generic argocd-repo-qalitrackservices \
  --namespace argocd \
  --from-literal=type=git \
  --from-literal=url=https://github.com/Qalitrack/qalitrackservices.git \
  --from-literal=username=x-access-token \
  --from-literal=password=$(gh auth token) \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl label secret argocd-repo-qalitrackservices \
  -n argocd argocd.argoproj.io/secret-type=repository --overwrite
```

The `install.sh --github-token` flag handles this automatically.

## GHCR Image Pull Secret

All services pull images from `ghcr.io/qalitrack/qalitrackservices/*`. The cluster needs:

```bash
kubectl create secret docker-registry ghcr-secret \
  --namespace qalitrack-prod \
  --docker-server=ghcr.io \
  --docker-username=<github-username> \
  --docker-password=$(gh auth token)
```

This secret is referenced by `imagePullSecrets: [{name: ghcr-secret}]` in all Helm chart values.

## Excluded Resources

The following are excluded from ArgoCD management (too large / externally managed):
- `cilium.io/CiliumIdentity`
- `monitoring.grafana.com/*` (GrafanaAgent, LogsInstance, MetricsInstance)

## Useful Commands

```bash
# Check app status
kubectl get application qalitrack -n argocd

# Force a manual sync
kubectl patch application qalitrack -n argocd \
  --type merge -p '{"operation":{"initiatedBy":{"username":"admin"},"sync":{}}}'

# View ArgoCD logs
kubectl logs -n argocd -l app.kubernetes.io/name=argocd-application-controller --tail=50

# View notification logs
kubectl logs -n argocd -l app.kubernetes.io/name=argocd-notifications-controller --tail=50

# Check pod health
kubectl get pods -n qalitrack-prod
```
