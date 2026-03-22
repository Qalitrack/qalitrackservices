#!/bin/bash
set -e

echo "🚀 Installing ArgoCD for GitOps..."

# Create namespace
echo "📦 Creating argocd namespace..."
kubectl create namespace argocd --dry-run=client -o yaml | kubectl apply -f -

# Install ArgoCD
echo "📥 Installing ArgoCD v2.9.3..."
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/v2.9.3/manifests/install.yaml

# Wait for ArgoCD to be ready
echo "⏳ Waiting for ArgoCD pods to be ready..."
kubectl wait --for=condition=ready pod \
  -l app.kubernetes.io/name=argocd-server \
  -n argocd \
  --timeout=300s

echo "✅ ArgoCD installed successfully"

# Get initial admin password
echo ""
echo "📋 ArgoCD Initial Setup:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

ARGOCD_PASSWORD=$(kubectl -n argocd get secret argocd-initial-admin-secret \
  -o jsonpath="{.data.password}" | base64 -d)

echo "Username: admin"
echo "Password: $ARGOCD_PASSWORD"
echo ""
echo "⚠️  IMPORTANT: Change this password after first login!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Expose ArgoCD server
echo "🌐 Exposing ArgoCD server..."

# Option 1: Port-forward (for testing)
echo ""
echo "Access ArgoCD UI:"
echo "  kubectl port-forward svc/argocd-server -n argocd 8080:443"
echo "  Then open: https://localhost:8080"
echo "  (Accept self-signed certificate warning)"
echo ""

# Apply custom configuration
if [ -f "argocd-config.yaml" ]; then
    echo "⚙️  Applying custom configuration..."
    kubectl apply -f argocd-config.yaml
fi

echo "🎉 ArgoCD setup complete!"
echo ""
echo "Next steps:"
echo "1. Access ArgoCD UI (see above)"
echo "2. Login with admin credentials"
echo "3. Change admin password"
echo "4. Create Qalitrack application:"
echo "   kubectl apply -f qalitrack-application.yaml"
echo ""
echo "Install ArgoCD CLI (optional):"
echo "  curl -sSL -o argocd-linux-amd64 https://github.com/argoproj/argo-cd/releases/latest/download/argocd-linux-amd64"
echo "  sudo install -m 555 argocd-linux-amd64 /usr/local/bin/argocd"
echo "  argocd login localhost:8080"
