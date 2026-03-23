#!/bin/bash
# QaliTrack ArgoCD Full Installation Script
# Run from: kubernetes/argocd/
# Usage: bash install.sh [--github-token YOUR_TOKEN]
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GITHUB_TOKEN=""

# Parse args
while [[ $# -gt 0 ]]; do
  case $1 in
    --github-token) GITHUB_TOKEN="$2"; shift 2 ;;
    *) echo "Unknown arg: $1"; exit 1 ;;
  esac
done

echo "=================================================="
echo "  QaliTrack ArgoCD Full Setup"
echo "=================================================="

# 1. Namespace
echo "[1/8] Creating argocd namespace..."
kubectl create namespace argocd --dry-run=client -o yaml | kubectl apply -f -

# 2. Install ArgoCD
echo "[2/8] Installing ArgoCD v2.9.3..."
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/v2.9.3/manifests/install.yaml

echo "      Waiting for ArgoCD to be ready (up to 5 min)..."
kubectl wait --for=condition=ready pod \
  -l app.kubernetes.io/name=argocd-server \
  -n argocd \
  --timeout=300s

# 3. Apply configuration (admin password, RBAC, server settings)
echo "[3/8] Applying ArgoCD configuration..."
kubectl apply -f "$SCRIPT_DIR/argocd-config.yaml"

# 4. Apply notifications (email alerts)
echo "[4/8] Applying notifications configuration..."
kubectl apply -f "$SCRIPT_DIR/argocd-notifications.yaml"

# 5. Apply ingress (argocd.qalibrated.co.ke)
echo "[5/8] Applying ingress..."
kubectl apply -f "$SCRIPT_DIR/argocd-ingress.yaml"

# 6. GitHub repo secret (so ArgoCD can pull from private repo)
echo "[6/8] Configuring GitHub repository access..."
if [ -n "$GITHUB_TOKEN" ]; then
  kubectl create secret generic argocd-repo-qalitrackservices \
    --namespace argocd \
    --from-literal=type=git \
    --from-literal=url=https://github.com/Qalitrack/qalitrackservices.git \
    --from-literal=username=x-access-token \
    --from-literal=password="$GITHUB_TOKEN" \
    --dry-run=client -o yaml | kubectl apply -f -

  kubectl label secret argocd-repo-qalitrackservices \
    -n argocd \
    argocd.argoproj.io/secret-type=repository \
    --overwrite
  echo "      GitHub token applied."
else
  echo "      ⚠️  No --github-token provided."
  echo "         If repo is private, run:"
  echo "         bash install.sh --github-token \$(gh auth token)"
fi

# 7. Restart ArgoCD server to pick up config changes
echo "[7/8] Restarting ArgoCD components to pick up config..."
kubectl rollout restart deployment argocd-server -n argocd
kubectl rollout restart deployment argocd-notifications-controller -n argocd
kubectl rollout status deployment argocd-server -n argocd --timeout=120s

# 8. Deploy Qalitrack application
echo "[8/8] Deploying Qalitrack ArgoCD application..."
kubectl apply -f "$SCRIPT_DIR/qalitrack-application.yaml"

echo ""
echo "=================================================="
echo "  ✅ ArgoCD Setup Complete!"
echo "=================================================="
echo ""
echo "  UI:       https://argocd.qalibrated.co.ke"
echo "  Username: admin"
echo "  Password: Qalitrack@2024!"
echo ""
echo "  ArgoCD will auto-sync from:"
echo "  https://github.com/Qalitrack/qalitrackservices.git"
echo "  Path: kubernetes/helm-charts/qalitrack-platform"
echo ""
echo "  Email alerts → joshuaiska@gmail.com"
echo "=================================================="
